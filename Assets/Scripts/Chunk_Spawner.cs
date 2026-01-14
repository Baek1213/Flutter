using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ChunkSpawner : MonoBehaviour
{
   
    public Grid grid;

    [Header("Chunk Prefab")]
    public Tilemap chunkPrefab;

    [Header("Chunk Size (cells)")]
    public int chunkWidth = 27;
    public int chunkHeight = 10;

    [Header("Scroll")]
    public float scrollSpeed = 5f;      // 청크가 왼쪽으로 흐르는 속도(월드 유닛/초)
    public float chunkYOffset = -5f;    // 너가 쓰던 y 오프셋 유지

    [Header("Camera")]
    public Camera cam;
    public float spawnBufferWorld = 2f; // 화면 오른쪽 여유분

    [Header("Tiles")]
    public TileBase wallTile;

    [Header("Items (sprite prefabs)")]
    public GameObject cherryPrefab;
    public GameObject gemPrefab;
    public float itemChance = 0.15f;

    // 살아있는 청크들(왼->오 순서로 관리)
    readonly List<Tilemap> alive = new();

    float ChunkWorldWidth => chunkWidth * (grid ? grid.cellSize.x : 1f);

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!grid) grid = GetComponent<Grid>();
    }

    void Start()
    {
        if (!chunkPrefab) return;

        // 시작 시 화면을 청크로 채워놓기
        float left = CamLeftX() - spawnBufferWorld;
        float x = left;

        // 최소 3~6개 정도는 깔려 있어야 안정적
        while (x < CamRightX() + spawnBufferWorld)
        {
            alive.Add(CreateChunkAt(x));
            x += ChunkWorldWidth;
        }
    }

    void FixedUpdate()
    {
        if (!chunkPrefab) return;

        // (선택) 플레이어 X 고정
        

        float dx = scrollSpeed * Time.fixedDeltaTime;

        // 1) 모든 청크를 왼쪽으로 이동
        for (int i = 0; i < alive.Count; i++)
        {
            var tm = alive[i];
            if (!tm) continue;

            // Rigidbody2D(Kinematic) 쓰면 여기 rb.MovePosition으로 바꾸는 게 더 안전함
            tm.transform.position += Vector3.left * dx;
        }

        // 2) 왼쪽 밖으로 나간 청크 삭제
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

        // 3) 오른쪽 끝이 비면 새 청크 생성
        float needRight = CamRightX() + spawnBufferWorld;
        while (RightmostEdgeX() < needRight)
        {
            float spawnX = (alive.Count == 0) ? (CamLeftX() - spawnBufferWorld) : RightmostEdgeX();
            alive.Add(CreateChunkAt(spawnX));
        }
    }

    Tilemap CreateChunkAt(float worldX)
    {
        var tm = Instantiate(chunkPrefab, transform);
        tm.name = $"Chunk_{Time.frameCount}";
        tm.transform.position = new Vector3(worldX, chunkYOffset, 0f);
        tm.transform.rotation = Quaternion.identity;
        tm.transform.localScale = Vector3.one;

        PaintObstacles(tm);
        SpawnItems(tm, tm.transform);

        return tm;
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
        return (max == float.NegativeInfinity) ? (CamLeftX() - spawnBufferWorld) : max;
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

    // ----------------------
    // 기존 로직(장애물/아이템)
    // ----------------------

    void PaintObstacles(Tilemap tm)
    {
        tm.ClearAllTiles();

        for (int p = 0; p < 2; p++)
        {
            int x = 8 + p * 16;
            int gapY = Random.Range(4, chunkHeight - 6);
            int gapSize = 4;

            for (int y = 0; y < chunkHeight; y++)
            {
                if (y >= gapY && y < gapY + gapSize) continue;
                tm.SetTile(new Vector3Int(x, y, 0), wallTile);
            }
        }
    }

    void SpawnItems(Tilemap tm, Transform parent)
    {
        if (!cherryPrefab && !gemPrefab) return;

        for (int x = 0; x < chunkWidth; x++)
        {
            if (Random.value > itemChance) continue;

            int y = Random.Range(2, chunkHeight - 2);
            Vector3 world = tm.CellToWorld(new Vector3Int(x, y, 0)) + tm.cellSize * 0.5f;

            GameObject prefab = (Random.value < 0.5f) ? cherryPrefab : gemPrefab;
            if (!prefab) continue;

            Instantiate(prefab, world, Quaternion.identity, parent);
        }
    }
}
