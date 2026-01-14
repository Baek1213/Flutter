using UnityEngine;
using UnityEngine.InputSystem; 
using System.Collections;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Collider2D coll;
    public Transform target;
    private bool isCollision;
    [SerializeField] private Animator anim;
    [SerializeField] private float Speed = 0.1f;
    [SerializeField] private float FlyPow = 8f;
    [SerializeField] private float DivePow = -15f;     
    [SerializeField] private float diveSeconds = 0.5f;
    

    private Coroutine diveCo;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
    }

    void Update()
    {
        //Movement();
        Go();
        Fly();
        Dive();
    }
    

    private void OnCollisionEnter2D(Collision2D col)
    {
        isCollision = true;
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        isCollision = false;
    }
    private void Go() 
    {
        if (isCollision) Speed = 0f;
        else Speed = 1f;

        if(target.position.x <= -8)
            rb.linearVelocity = new Vector2(Speed, rb.linearVelocity.y);
        else
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
    /*private void Movement()
    {
        float moveInput = 0f;

        if (Keyboard.current.leftArrowKey.isPressed)
        {
            moveInput = -1f;
            if(LR)transform.Rotate(0f, 180f, 0f);
            LR = false;
        }
        else if (Keyboard.current.rightArrowKey.isPressed)
        {
            moveInput = 1f;
            if (!LR) transform.Rotate(0f, 180f, 0f);
            LR = true;        }

        rb.linearVelocity = new Vector2(moveInput * Speed, rb.linearVelocity.y);
    }*/

    private void Fly()
    {
        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, FlyPow);
        }
    }
    private void Dive()
    {
        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            // 이미 다이브 중이면 시간 리셋(원하면 return으로 막아도 됨)
            if (diveCo != null) StopCoroutine(diveCo);

            // 물리 처리
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, DivePow);

            // 애니메이션 처리 (추천 파라미터: Trigger + Bool)
            anim.SetBool("Dive",true);        // 시작 신호
            

            diveCo = StartCoroutine(DiveTimer());
        }
    }
    private IEnumerator DiveTimer()
    {
        yield return new WaitForSeconds(diveSeconds);
        anim.SetBool("Dive", false);
        diveCo = null;
    }
}