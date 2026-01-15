using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ChunkSpawner : MonoBehaviour
{
    [Header("Grid / Camera")]
    public Grid grid;
    public Camera cam;

    [Header("Tiles")]
    public TileBase wallTile;
    public TileBase ColorwallTile;

    [Header("Items (sprite prefabs)")]
    public GameObject cherryPrefab;
    public GameObject gemPrefab;

    [Header("Chunk Prefab (base Tilemap)")]
    public Tilemap chunkPrefab;

    [Header("Chunk Size (cells)")]
    public int chunkWidth = 27;
    public int chunkHeight = 10;

    [Header("Scroll")]
    public float scrollSpeed;
    public float chunkYOffset = 0f;
    public float baseInterval = 4f;
    private float _nextSpawnTime = 0f;


    [Header("Speed Ramp")]
    public float startScrollSpeed = 4f;
    public float maxScrollSpeed = 7f;
    public float timeToMaxSpeed = 600f;   // 몇 초에 max까지 갈지
    public AnimationCurve speedCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    float elapsed;
    [SerializeField] private float minChunkGapWorld = 5f;

    [Header("Spawn")]
    public float spawnBufferWorld = 2f;
    [Tooltip("게임 시작 시 청크가 바로 보이지 않게, 초기 생성 X를 +방향으로 밀어줌(월드 유닛).")]
    public float initialSpawnOffsetWorld = 15f;

    [Header("WFC Wall Rule")]
    [Tooltip("앞/뒤 벽 x좌표 간 최소 거리")]
    public int minWallXDistance = 6;
    [Tooltip("빈칸(통로) 크기 고정")]
    public int gapSize = 4;

    [Header("Color Wall Rule")]
    [Tooltip("색깔벽은 그냥벽(2개)과 다른 X 중 1개를 선택")]
    public bool spawnColorWall = true;

    [Tooltip("한 청크당 아이템 최대 개수")]
    public int itemsPerChunk = 6;
    [Tooltip("아이템 스폰 시 제외할 y(셀 좌표)들. 기본: -5, 5")]
    public int[] forbiddenItemYs = new[] { -5, 5 };

    // 살아있는 청크들
    readonly List<Tilemap> alive = new();

    float ChunkWorldWidth => chunkWidth * (grid ? grid.cellSize.x : 1f);

    // SOLID: 책임 분리(생성/페인트/아이템)
    IWallPatternGenerator wallGen;
    IColorWallSpawner colorWallSpawner;
    IItemSpawner itemSpawner;

    void Awake()
    {
        
        if (!cam) cam = Camera.main;
        if (!grid) grid = GetComponent<Grid>();
        wallGen = new WfcWallPatternGenerator();
        colorWallSpawner = new SimpleColorWallSpawner();
        itemSpawner = new FixedCountItemSpawner();
        
    }

    void Start()
    {
        gameObject.SetActive(false);
        if (!chunkPrefab || !cam) return;

        float left = CamLeftX() - spawnBufferWorld;

        //  1) 시작 시 청크가 바로 보이는 어색함 제거: 초기 생성 X를 +로 민다
        float x = left + initialSpawnOffsetWorld;

        while (x < CamRightX() + spawnBufferWorld)
        {
            alive.Add(CreateChunkAt(x));
            x += ChunkWorldWidth;
        }
        elapsed = 0f;
        scrollSpeed = startScrollSpeed;
    }

    void FixedUpdate()
    {
        if (!chunkPrefab || !cam) return;

        elapsed += Time.fixedDeltaTime;
        float t = (timeToMaxSpeed <= 0f) ? 1f : Mathf.Clamp01(elapsed / timeToMaxSpeed);
        float k = speedCurve.Evaluate(t);
        scrollSpeed = Mathf.Lerp(startScrollSpeed, maxScrollSpeed, k);
        float dx = scrollSpeed * Time.fixedDeltaTime;

        // 이동
        for (int i = 0; i < alive.Count; i++)
        {
            var tm = alive[i];
            if (!tm) continue;
            tm.transform.position += Vector3.left * dx;
        }

        // 제거
        float leftBound = CamLeftX() - spawnBufferWorld;
        for (int i = alive.Count - 1; i >= 0; i--)
        {
            var tm = alive[i];
            if (!tm) { alive.RemoveAt(i); continue; }
            float rightEdge = tm.transform.position.x + ChunkWorldWidth;
            if (rightEdge < leftBound)
            {
                Destroy(tm.gameObject);
                alive.RemoveAt(i);
            }
        }

        // 생성 (공간 간격만 사용)
        float needRight = CamRightX() + spawnBufferWorld;
        while (RightmostEdgeX() + minChunkGapWorld < needRight)
        {
            float spawnX = (alive.Count == 0)
                ? (CamLeftX() - spawnBufferWorld + initialSpawnOffsetWorld)
                : RightmostEdgeX() + minChunkGapWorld;
            alive.Add(CreateChunkAt(spawnX));
        }
    }

    Tilemap CreateChunkAt(float worldX)
    {
        var baseTm = Instantiate(chunkPrefab, transform);
        baseTm.name = $"Chunk_{Time.frameCount}";
        baseTm.transform.position = new Vector3(worldX, chunkYOffset, 0f);
        baseTm.transform.rotation = Quaternion.identity;
        baseTm.transform.localScale = Vector3.one;

        //  2) WFC 기반 "그냥벽" 2개 생성
        var ctx = new ChunkContext(chunkWidth, chunkHeight, minWallXDistance, gapSize, forbiddenItemYs);
        var wallPattern = wallGen.Generate(ctx);

        // base tilemap에 그냥벽을 그린다
        PaintWallColumns(baseTm, wallPattern.wallXs, wallPattern.gapCells, wallTile, ctx);

        //  3) 색깔벽(독립 레이어) 생성: 그냥벽 2개 제외한 X 중 1개
        var colorLayer = (spawnColorWall)
            ? colorWallSpawner.TrySpawnColorWallLayer(baseTm, wallPattern.wallXs, ColorwallTile, ctx)
            : null;

        //  4) 아이템: (그냥벽/색깔벽 다 그린 후) y=5, y=-5 제외 + 한 청크 6개 고정
        itemSpawner.Spawn(baseTm, colorLayer, cherryPrefab, gemPrefab, itemsPerChunk, ctx);

        return baseTm;
    }

    // --------------------
    // Tile Painting helpers
    // --------------------

    static void PaintWallColumns(Tilemap baseTm, int[] wallXs, HashSet<Vector3Int> gapCells, TileBase wall, ChunkContext ctx)
    {
        baseTm.ClearAllTiles();

        for (int i = 0; i < wallXs.Length; i++)
        {
            int x = wallXs[i];
            for (int y = ctx.yMin; y <= ctx.yMax; y++)
            {
                var cell = new Vector3Int(x, y, 0);
                if (gapCells.Contains(cell)) continue;
                baseTm.SetTile(cell, wall);
            }
        }
    }

    float RightmostEdgeX()
    {
        float max = float.NegativeInfinity;
        for (int i = 0; i < alive.Count; i++)
        {
            var tm = alive[i];
            if (!tm) continue;

            float edge = tm.transform.position.x + ChunkWorldWidth;
            if (edge > max) max = edge;
        }
        return (max == float.NegativeInfinity) ? (CamLeftX() - spawnBufferWorld + initialSpawnOffsetWorld) : max;
    }

    float CamLeftX()
    {
        float halfW = cam.orthographicSize * cam.aspect;
        return cam.transform.position.x - halfW;
    }

    float CamRightX()
    {
        float halfW = cam.orthographicSize * cam.aspect;
        return cam.transform.position.x + halfW;
    }

    // =========================
    // SOLID: small components
    // =========================

    // 컨텍스트(규칙/좌표계)
    sealed class ChunkContext
    {
        public readonly int width, height;
        public readonly int minXDist;
        public readonly int gapSize;
        public readonly HashSet<int> forbiddenItemY;

        // y좌표는 "중앙 기준"으로 쓰는 편이 (y=-5 같은 규칙) 맞아서:
        // height=10이면 yMin=-5, yMax=4
        public readonly int yMin;
        public readonly int yMax;

        public int Half => width / 2;

        public ChunkContext(int w, int h, int minDist, int gapSize, int[] forbiddenYs)
        {
            width = w;
            height = h;
            minXDist = minDist;
            this.gapSize = gapSize;

            yMin = -(h / 2);
            yMax = yMin + h - 1;

            forbiddenItemY = new HashSet<int>();
            if (forbiddenYs != null)
            {
                for (int i = 0; i < forbiddenYs.Length; i++)
                    forbiddenItemY.Add(forbiddenYs[i]);
            }
        }

        public bool IsWithinY(int y) => y >= yMin && y <= yMax;
    }

    // 벽 패턴 결과
    readonly struct WallPattern
    {
        public readonly int[] wallXs;                 // 2개
        public readonly HashSet<Vector3Int> gapCells; // 통로(비우는 셀들)

        public WallPattern(int[] wallXs, HashSet<Vector3Int> gapCells)
        {
            this.wallXs = wallXs;
            this.gapCells = gapCells;
        }
    }

    interface IWallPatternGenerator
    {
        WallPattern Generate(ChunkContext ctx);
    }

    //  2) WFC 느낌(의존 규칙) 구현:
    // - 청크를 반으로 나눔
    // - 첫 벽은 한쪽 절반에서 랜덤 선택
    // - 두 번째 벽은 반대 절반에서 선택하되, 첫 벽과 최소 6 이상 떨어진 값(재시도)
    // - 통로(gap)는 반드시 연속 4칸, 중간값(예: 0)에서 두 패턴(1,0,-1,-2) / (2,1,0,-1) 중 랜덤
    sealed class WfcWallPatternGenerator : IWallPatternGenerator
    {
        public WallPattern Generate(ChunkContext ctx)
        {
            // 1) 벽 X 2개 뽑기
            int firstHalf = UnityEngine.Random.value < 0.5f ? 0 : 1;

            int x1 = PickXInHalf(ctx, firstHalf);
            int x2 = PickXInOtherHalfWithMinDist(ctx, 1 - firstHalf, x1);

            // 2) gap(연속 4칸) y 만들기
            //    gapYSeed는 전체 범위에서 하나 뽑고, 그 seed를 포함하는 4연속을 2가지 방식으로 결정
            int seedY = UnityEngine.Random.Range(ctx.yMin, ctx.yMax + 1);

            // Option A: seed, seed-1, seed-2, seed-3  (예: 5,4,3,2)
            // Option B: seed+1, seed, seed-1, seed-2  (예: 2,1,0,-1 when seed=1 OR 2,1,0,-1 느낌)
            var optA = new int[] { seedY, seedY - 1, seedY - 2, seedY - 3 };
            var optB = new int[] { seedY + 1, seedY, seedY - 1, seedY - 2 };

            bool aValid = IsAllWithin(ctx, optA);
            bool bValid = IsAllWithin(ctx, optB);

            int[] gapYs;
            if (aValid && bValid) gapYs = (UnityEngine.Random.value < 0.5f) ? optA : optB;
            else if (aValid) gapYs = optA;
            else if (bValid) gapYs = optB;
            else
            {
                // 둘 다 안맞으면(극단 경계) 강제로 clamp해서 연속 4칸 생성
                int top = Mathf.Clamp(seedY, ctx.yMin + 3, ctx.yMax);
                gapYs = new int[] { top, top - 1, top - 2, top - 3 };
            }

            // 3) 두 벽 각각에 같은 gapYs를 적용(요청대로)
            var gapCells = new HashSet<Vector3Int>();
            for (int i = 0; i < gapYs.Length; i++)
            {
                gapCells.Add(new Vector3Int(x1, gapYs[i], 0));
                gapCells.Add(new Vector3Int(x2, gapYs[i], 0));
            }

            return new WallPattern(new[] { x1, x2 }, gapCells);
        }

        static int PickXInHalf(ChunkContext ctx, int halfIndex)
        {
            int start = (halfIndex == 0) ? 0 : ctx.Half;
            int endExclusive = (halfIndex == 0) ? ctx.Half : ctx.width; // [start, endExclusive)
            return UnityEngine.Random.Range(start, endExclusive);
        }

        static int PickXInOtherHalfWithMinDist(ChunkContext ctx, int otherHalf, int x1)
        {
            int start = (otherHalf == 0) ? 0 : ctx.Half;
            int endExclusive = (otherHalf == 0) ? ctx.Half : ctx.width;

            // 재시도
            const int maxTry = 50;
            for (int t = 0; t < maxTry; t++)
            {
                int x2 = UnityEngine.Random.Range(start, endExclusive);
                if (Mathf.Abs(x2 - x1) >= ctx.minXDist) return x2;
            }

            // 최악: 조건을 만족하는 후보들 중 랜덤
            List<int> candidates = new List<int>();
            for (int x = start; x < endExclusive; x++)
                if (Mathf.Abs(x - x1) >= ctx.minXDist) candidates.Add(x);

            if (candidates.Count > 0)
                return candidates[UnityEngine.Random.Range(0, candidates.Count)];

            // 진짜 불가능하면 그냥 아무거나(설정이 이상한 경우)
            return UnityEngine.Random.Range(start, endExclusive);
        }

        static bool IsAllWithin(ChunkContext ctx, int[] ys)
        {
            for (int i = 0; i < ys.Length; i++)
                if (!ctx.IsWithinY(ys[i])) return false;
            return true;
        }
    }

    //  3) 색깔벽 레이어 생성(독립 Tilemap)
    interface IColorWallSpawner
    {
        Tilemap TrySpawnColorWallLayer(Tilemap baseTm, int[] excludedWallXs, TileBase tile, ChunkContext ctx);
    }

    sealed class SimpleColorWallSpawner : IColorWallSpawner
    {
        public Tilemap TrySpawnColorWallLayer(Tilemap baseTm, int[] excludedWallXs, TileBase tile, ChunkContext ctx)
        {
            // 후보 X: 중앙-1, 중앙, 중앙+1 중 하나
            int center = ctx.width / 2; // 27이면 13
            List<int> xs = new List<int>(3) { center - 1, center, center + 1 };

            // 그냥벽이 중앙 3칸 중 하나를 차지하면 제거(최대 1개만 걸린다는 전제)
            xs.Remove(excludedWallXs[0]);
            xs.Remove(excludedWallXs[1]);

            if (xs.Count == 0) return null;
            int pickX = xs[UnityEngine.Random.Range(0, xs.Count)];

            // 벽 색 선택(4종): Red, Green, Blue, White
            FilterMode wallMode = PickWallMode();
            Color wallColor = ModeToColor(wallMode);

            // 독립 타일맵 레이어 생성 (baseTm 아래 자식으로)
            GameObject go = new GameObject("ColorWall_" + wallMode);
            go.transform.SetParent(baseTm.transform, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            var layer = go.AddComponent<Tilemap>();
            var rend = go.AddComponent<TilemapRenderer>();
            rend.sortingOrder = 1;

            AddTilemapColliders(go, useComposite: true);

            // 타일맵 색 지정
            layer.color = wallColor;

            // x너비 1, y너비 height 전부 채움
            for (int y = ctx.yMin; y <= ctx.yMax; y++)
                layer.SetTile(new Vector3Int(pickX, y, 0), tile);

            // 벽 색 저장 + 초기 트리거 상태 갱신
            var state = go.AddComponent<ColorWallState>();
            state.Init(wallMode);

            // 현재 필터 모드로 초기 Refresh(필터 없으면 기본은 막힘)
            var filter = UnityEngine.Object.FindObjectOfType<ColorFilter>();
            if (filter != null)
                state.Refresh(filter.CurrentMode);

            return layer;
        }

        static void AddTilemapColliders(GameObject go, bool useComposite)
        {
            var rb = go.GetComponent<Rigidbody2D>();
            if (!rb) rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            rb.simulated = true;

            var tmCol = go.GetComponent<TilemapCollider2D>();
            if (!tmCol) tmCol = go.AddComponent<TilemapCollider2D>();
            tmCol.isTrigger = false;

            if (useComposite)
            {
                var comp = go.GetComponent<CompositeCollider2D>();
                if (!comp) comp = go.AddComponent<CompositeCollider2D>();
                tmCol.usedByComposite = true; 
                comp.isTrigger = false; // 추가: 초기값
            }
        }

        static FilterMode PickWallMode()
        {
            int r = UnityEngine.Random.Range(0, 4);
            if (r == 0) return FilterMode.Red;
            if (r == 1) return FilterMode.Green;
            if (r == 2) return FilterMode.Blue;
            return FilterMode.White;
        }

        static Color ModeToColor(FilterMode mode)
        {
            switch (mode)
            {
                case FilterMode.Red: return new Color(1f, 0f, 0f, 1f);
                case FilterMode.Green: return new Color(0f, 1f, 0f, 1f);
                case FilterMode.Blue: return new Color(0f, 0f, 1f, 1f);
                case FilterMode.White: return new Color(1f, 1f, 1f, 1f);
                default: return new Color(1f, 1f, 1f, 1f);
            }
        }
    }


    //  4) 아이템 스폰(한 청크 6개 고정 + y=±5 제외 + 벽 위 금지)
    interface IItemSpawner
    {
        void Spawn(Tilemap baseTm, Tilemap colorLayer, GameObject cherry, GameObject gem, int count, ChunkContext ctx);
    }

    sealed class FixedCountItemSpawner : IItemSpawner
    {
        public void Spawn(Tilemap baseTm, Tilemap colorLayer, GameObject cherry, GameObject gem, int count, ChunkContext ctx)
        {
            if (!cherry && !gem) return;
            if (count <= 0) return;

            // “빈 칸” 랜덤으로 count개 뽑기 (충돌/제외 규칙)
            int placed = 0;
            int tries = 0;
            int maxTries = 500;

            while (placed < count && tries++ < maxTries)
            {
                int x = UnityEngine.Random.Range(0, ctx.width);
                int y = UnityEngine.Random.Range(ctx.yMin, ctx.yMax + 1);

                // y=5, y=-5 등 제외
                if (ctx.forbiddenItemY.Contains(y)) continue;

                var cell = new Vector3Int(x, y, 0);

                // 벽(그냥벽) 위 금지
                if (baseTm.HasTile(cell)) continue;

                // 색깔벽 위도 금지
                if (colorLayer && colorLayer.HasTile(cell)) continue;

                // 프리팹 선택
                GameObject prefab = (UnityEngine.Random.value < 0.5f) ? cherry : gem;
                if (!prefab) prefab = cherry ? cherry : gem;
                if (!prefab) continue;

                Vector3 world = baseTm.CellToWorld(cell) + baseTm.cellSize * 0.5f;
                UnityEngine.Object.Instantiate(prefab, world, Quaternion.identity, baseTm.transform);

                placed++;
            }
        }
    }
}
