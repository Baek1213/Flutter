using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ChunkSpawner : MonoBehaviour
{
    public Transform player;
    public Tilemap chunkPrefab;     // ★ ChunkPrefab(=Tilemap 프리팹) 넣기
    public int chunkWidth = 32;
    public int chunkHeight = 18;

    public int keepAhead = 3;
    public int keepBehind = 2;

    public TileBase wallTile;

    // 아이템(스프라이트 프리팹들)
    public GameObject cherryPrefab;
    public GameObject gemPrefab;
    public float itemChance = 0.15f;

    readonly Dictionary<int, Tilemap> chunks = new();

    void Update()
    {
        if (!player || !chunkPrefab) return;

        int playerChunk = Mathf.FloorToInt(player.position.x / chunkWidth);

        // 생성
        for (int i = playerChunk - keepBehind; i <= playerChunk + keepAhead; i++)
        {
            if (!chunks.ContainsKey(i))
                chunks[i] = CreateChunk(i);
        }

        // 삭제
        var remove = new List<int>();
        foreach (var kv in chunks)
        {
            if (kv.Key < playerChunk - keepBehind - 1)
                remove.Add(kv.Key);
        }
        foreach (int idx in remove)
        {
            Destroy(chunks[idx].gameObject);
            chunks.Remove(idx);
        }
    }

    Tilemap CreateChunk(int idx)
    {
        // Grid(=이 스크립트 붙은 오브젝트) 밑에 생성
        var tm = Instantiate(chunkPrefab, transform);
        tm.name = $"Chunk_{idx:D3}";
        tm.transform.localPosition = new Vector3(idx * chunkWidth, -5, 0);
        tm.transform.localRotation = Quaternion.identity;
        tm.transform.localScale = Vector3.one;

        PaintObstacles(tm);
        SpawnItems(tm, tm.transform); // 청크의 자식으로 아이템 스폰

        return tm;
    }

    void PaintObstacles(Tilemap tm)
    {
        tm.ClearAllTiles();

        // 테스트용: 플래피 기둥 2개
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

            // 체리/젬 랜덤
            GameObject prefab = (Random.value < 0.5f) ? cherryPrefab : gemPrefab;
            if (!prefab) continue;

            Instantiate(prefab, world, Quaternion.identity, parent);
        }
    }
}
