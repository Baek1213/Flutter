using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class ColorFilter : MonoBehaviour
{
    [Header("필터 적용 대상 (SpriteRenderer / Tilemap 둘 다 가능)")]
    [SerializeField] private GameObject[] targetObjects;

    private SpriteRenderer[] spriteRenderers;
    private Tilemap[] tilemaps;

    private Color[] originalSpriteColors;
    private Color[] originalTilemapColors;


    [SerializeField] private FilterMode currentMode = FilterMode.White;
    public FilterMode CurrentMode => currentMode;

    private void Awake()
    {
        spriteRenderers = new SpriteRenderer[targetObjects.Length];
        tilemaps = new Tilemap[targetObjects.Length];

        originalSpriteColors = new Color[targetObjects.Length];
        originalTilemapColors = new Color[targetObjects.Length];

        for (int i = 0; i < targetObjects.Length; i++)
        {
            if (!targetObjects[i]) continue;

            // SpriteRenderer
            spriteRenderers[i] = targetObjects[i].GetComponent<SpriteRenderer>();
            if (spriteRenderers[i] != null)
                originalSpriteColors[i] = spriteRenderers[i].color;

            // Tilemap
            tilemaps[i] = targetObjects[i].GetComponent<Tilemap>();
            if (tilemaps[i] != null)
                originalTilemapColors[i] = tilemaps[i].color;
        }
    }
    void Start()
    {
        
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.aKey.wasPressedThisFrame)
            CycleFilter();
    }

    private void CycleFilter()
    {
        currentMode = (FilterMode)(((int)currentMode + 1) % 4);
        ApplyCurrentFilter();
        RefreshColorWalls();
    }

    private void ApplyCurrentFilter()
    {
        for (int i = 0; i < targetObjects.Length; i++)
        {
            // --- SpriteRenderer 적용 ---
            if (spriteRenderers[i] != null)
            {
                var o = originalSpriteColors[i];
                spriteRenderers[i].color = Filtered(o);
            }

            // --- Tilemap 적용 ---
            if (tilemaps[i] != null)
            {
                var o = originalTilemapColors[i];
                tilemaps[i].color = Filtered(o);
            }
        }
    }
    private void RefreshColorWalls()
    {
        var walls = FindObjectsOfType<ColorWallState>();
        for (int i = 0; i < walls.Length; i++)
            walls[i].Refresh(currentMode);
    }
    private Color Filtered(Color original)
    {
        switch (currentMode)
        {
            case FilterMode.White: return original;
            case FilterMode.Red: return new Color(original.r, 0f, 0f, original.a);
            case FilterMode.Blue: return new Color(0f, 0f, original.b, original.a);
            case FilterMode.Green: return new Color(0f, original.g, 0f, original.a);
            default: return original;
        }
    }
}
