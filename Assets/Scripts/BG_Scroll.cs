using UnityEngine;

public class BG_Scroll : MonoBehaviour
{
    public float scrollSpeed = 8f;     // 배경 속도 
    public Transform[] pieces;         // BG1, BG2
    public float pieceWidthWorld = 26.4f; 
    public Camera cam;

    void Awake()
    {
        
    }

    void Update()
    {
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
