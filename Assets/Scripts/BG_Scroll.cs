using UnityEngine;

public class BG_Scroll : MonoBehaviour
{
    [Header("Speed Ramp")]
    public float startScrollSpeed = 4f;   // 시작 속도
    public float maxScrollSpeed = 8f;     // 최대 속도
    public float timeToMaxSpeed = 600f;   // 몇 초에 max까지 갈지
    public AnimationCurve speedCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Background")]
    public Transform[] pieces;            // BG1, BG2
    public float pieceWidthWorld = 26.4f;
    public Camera cam;

    private float scrollSpeed;            // 현재 속도
    private float elapsed;                // 경과 시간

    void Awake()
    {
        if (!cam) cam = Camera.main;
        elapsed = 0f;
        scrollSpeed = startScrollSpeed;
    }

    void Update()
    {
        // 게임이 시작되지 않았으면 배경도 멈춤
        if (!UI_Fixer.isGameStarted) return;

        // 속도 증가 계산 (ChunkSpawner와 동일한 로직)
        elapsed += Time.deltaTime;
        float t = (timeToMaxSpeed <= 0f) ? 1f : Mathf.Clamp01(elapsed / timeToMaxSpeed);
        float k = speedCurve.Evaluate(t);
        scrollSpeed = Mathf.Lerp(startScrollSpeed, maxScrollSpeed, k);

        float dx = scrollSpeed * Time.deltaTime;

        // 전부 왼쪽으로 이동
        pieces[0].position += Vector3.left * dx;
        pieces[1].position += Vector3.left * dx;

        float camLeft = cam.transform.position.x - cam.orthographicSize * cam.aspect;

        // 왼쪽 밖으로 나간 조각을 오른쪽 끝으로 이동
        for (int i = 0; i < pieces.Length; i++)
        {
            float rightEdge = pieces[i].position.x + pieceWidthWorld * 0.5f;
            if (rightEdge < camLeft)
            {
                // 현재 조각들 중 가장 오른쪽 조각 찾기
                Transform rightmost = pieces[0];
                for (int j = 1; j < pieces.Length; j++)
                    if (pieces[j].position.x > rightmost.position.x)
                        rightmost = pieces[j];

                pieces[i].position = new Vector3(
                    rightmost.position.x + pieceWidthWorld,
                    pieces[i].position.y,
                    pieces[i].position.z
                );
            }
        }
    }
}