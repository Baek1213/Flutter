using UnityEngine;
using TMPro;
public class UI_Fixer : MonoBehaviour
{
    
    public TMP_Text scoreText;
    public TMP_Text BestScoreText;
    public TMP_Text GameoverText;
    private int maxscore=99999900;
    void Start()
    {
        ScoreItem.score = 0;
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
}
