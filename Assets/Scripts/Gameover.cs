using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Gameover : MonoBehaviour
{
    public Transform target;
    public static bool gameover;

    public Button RestartButton;

    Camera cam;

    void Start()
    {
        gameover = false;
        RestartButton.onClick.AddListener(RestartButtonClick);
        cam = Camera.main; // 추가
    }

    void Update()
    {
        if (!target || !cam) return; // 추가

        Vector3 vp = cam.WorldToViewportPoint(target.position); // 

        // 고정좌표 대신 "화면 밖" 판정으로 
        if (vp.x < -0.1f || vp.x > 1.1f || vp.y < -0.1f || vp.y > 1.1f)
        {
            gameover = true;
            Time.timeScale = 0f;
        }
    }

    void RestartButtonClick()
    {
        SceneManager.LoadScene("main");
    }
}
