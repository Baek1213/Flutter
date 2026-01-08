using UnityEngine;
using UnityEngine.InputSystem; // 추가!

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Collider2D coll;
    [SerializeField] private float Speed = 5f;
    [SerializeField] private float FlyPow = 8f;
    [SerializeField] private LayerMask ground;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
    }

    void Update()
    {
        Movement();
        Fly();
    }
    private void Movement()
    {
        float moveInput = 0f;

        if (Keyboard.current.leftArrowKey.isPressed)
            moveInput = -1f;
        else if (Keyboard.current.rightArrowKey.isPressed)
            moveInput = 1f;

        rb.linearVelocity = new Vector2(moveInput * Speed, rb.linearVelocity.y);
    }

    private void Fly()
    {
        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, FlyPow);
        }
    }
}