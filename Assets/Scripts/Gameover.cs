using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Gameover : MonoBehaviour
{
    public Transform target;
    public static bool gameover;

    public Button RestartButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Time.timeScale = 1f;
        gameover = false;
        RestartButton.onClick.AddListener(RestartButtonClick);
    }

    // Update is called once per frame
    void Update()
    {
        if (target.position.x < -13.5 || target.position.y<-5.5)
        {
            gameover=true;
            Time.timeScale = 0f;
        }
        
    }
    void RestartButtonClick() 
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("main");
    }
}
