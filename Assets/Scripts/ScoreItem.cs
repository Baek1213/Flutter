using UnityEngine;

public class ScoreItem : MonoBehaviour
{
    public static int score = 0;
    public static int BestScore = 0;

    private string playerTag = "Player";
    private string itemTag1 = "heal_item";
    private string itemTag2 = "score_item";

    void Update()
    {
        if (BestScore <= ScoreItem.score)
            BestScore = ScoreItem.score;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (!collision.CompareTag(playerTag)) return;
        
        if (CompareTag(itemTag1)) score += 300;
        if (CompareTag(itemTag2)) score += 500;
        // 먹는 연출: 애니 트리거 or 파티클
        var anim = GetComponent<Animator>();
        if (anim) anim.SetTrigger("Collect");

        // 콜라이더 끄고 잠깐 뒤 제거(애니 길이만큼)
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        Destroy(gameObject, 0.5f); // 애니 길이에 맞게

    }
}
