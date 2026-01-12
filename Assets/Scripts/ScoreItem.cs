using UnityEngine;

public class ScoreItem : MonoBehaviour
{
    public int score = 1;
    public string playerTag = "Player";

    bool used;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (used) return;
        if (!other.CompareTag(playerTag)) return;

        used = true;

        // 점수 올리기 (너 프로젝트 방식에 맞게 바꿔)
        

        // 먹는 연출: 애니 트리거 or 파티클
        var anim = GetComponent<Animator>();
        if (anim) anim.SetTrigger("Collect");

        // 콜라이더 끄고 잠깐 뒤 제거(애니 길이만큼)
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        Destroy(gameObject, 0.5f); // 애니 길이에 맞게
    }
}
