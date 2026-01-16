using UnityEngine;
using UnityEngine.Tilemaps;

public class ColorWallState : MonoBehaviour
{
    [SerializeField] private FilterMode wallColor = FilterMode.Red;

    private Collider2D col;  

    void Awake()
    {
        
        col = (Collider2D)GetComponent<CompositeCollider2D>();
        if (col == null) col = GetComponent<TilemapCollider2D>();
    }

    public void Init(FilterMode color)
    {
        wallColor = color;
    }

    public void Refresh(FilterMode playerMode)
    {
        if (col == null) return;
        col.isTrigger = (playerMode == wallColor);
    }
}
