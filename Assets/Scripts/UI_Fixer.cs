using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class UI_Fixer : MonoBehaviour
{
    
    public TMP_Text scoreText;
    public TMP_Text BestScoreText;
    public TMP_Text GameoverText;
    public TMP_Text StartText;
    public Button startButton;
    public Grid grid;
    private int maxscore=99999900;
    void Start()
    {
        Time.timeScale = 0f;
        ScoreItem.score = 0;
        StartText.gameObject.SetActive(true);
        startButton.onClick.AddListener(StartButtonClick);
    }
    
    // Update is called once per frame
    void Update()
    {
        if (ScoreItem.score >= maxscore) scoreText.text = "Score: MAX";
        else scoreText.text = "Score: " + ScoreItem.score;

        BestScoreText.text = "Best: " + ScoreItem.BestScore;

        if (Gameover.gameover) GameoverText.gameObject.SetActive(true);
        else GameoverText.gameObject.SetActive(false);

        
    }
    void StartButtonClick() 
    {
        StartText.gameObject.SetActive(false);
        grid.gameObject.SetActive(true);
        Time.timeScale = 1f;
    }
}
