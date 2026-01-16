using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_Fixer : MonoBehaviour
{
    public TMP_Text scoreText;
    public TMP_Text BestScoreText;
    public TMP_Text GameoverText;
    public TMP_Text StartText;
    public RawImage gamename;
    public RawImage dummy;
    public Button startButton;
    public Grid grid;
    public AudioSource bgmSource;

    public static bool isGameStarted = false;  // 게임 시작 여부

    private int maxscore = 99999900;

    void Start()
    {
        Time.timeScale = 0f;
        ScoreItem.score = 0;
        StartText.gameObject.SetActive(true);
        startButton.onClick.AddListener(StartButtonClick);

        // Grid는 처음부터 활성화하되 보이지 않게
        grid.gameObject.SetActive(true);
    }

    void Update()
    {
        if (ScoreItem.score >= maxscore) scoreText.text = "Score: MAX";
        else scoreText.text = "Score: " + ScoreItem.score;

        BestScoreText.text = "Best: " + ScoreItem.BestScore;

        if (Gameover.gameover) 
        {
            GameoverText.gameObject.SetActive(true);
            bgmSource.Pause();
        }
        else GameoverText.gameObject.SetActive(false);
    }

    void StartButtonClick()
    {
        StartText.gameObject.SetActive(false);
        isGameStarted = true;  // 게임 시작 플래그 설정
        Time.timeScale = 1f;
        bgmSource.Play();
        dummy.enabled = false;
        gamename.enabled = false;
    }
}