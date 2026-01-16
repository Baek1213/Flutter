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
        if (!UI_Fixer.isGameStarted) return;
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
    private void OnTriggerExit2D(Collider2D col)//색깔벽에 딱붙어있다가 트리거 발생으로 색깔벽을 넘어갈시
    {
        isCollision = false;
    }
    private void Go() 
    {
        if (isCollision) Speed = 0f;
        else Speed = 1f;

        if(target.position.x <= -6)
            rb.linearVelocity = new Vector2(Speed, rb.linearVelocity.y);
        else
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }
   

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
            // 이미 다이브 중이면 시간 리셋
            if (diveCo != null) StopCoroutine(diveCo);

            // 물리 처리
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, DivePow);

            // 애니메이션 처리 
            anim.SetBool("Dive",true);
            

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