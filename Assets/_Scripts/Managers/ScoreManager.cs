using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;
    private int scoreThresholds = 500;
    private int currentScore = 0;
    private int currentThreshold = 100;
    private bool isReady = false;
    void Awake()
    {
        instance = this;
    }
    public void CleanUp()
    {
        currentScore = 0;
        currentThreshold = 0;
        scoreThresholds = 0;
        isReady = false;
    }
    public void UpdateScore(int points)
    {
        if (!isReady)
        {
            return;
        }

        currentScore += points;
        UIManager.instance.UpdateXPBar(currentScore);
        CheckForThresholds();

    }
    public void SetupScoreManager(int currentScore, int thresholds)
    {
        this.currentScore = currentScore;
        this.scoreThresholds = thresholds;
        this.currentThreshold = scoreThresholds;
        isReady = true;
        UIManager.instance.SetUpXPBar(0, currentThreshold);
    }
    private void CheckForThresholds()
    {
        if (scoreThresholds <= 0 || currentThreshold <= 0)
        {
            return;
        }

        while (currentScore >= currentThreshold)
        {
            currentThreshold += scoreThresholds;
            UIManager.instance.SetUpXPBar(currentScore, currentThreshold);
            GameManager.instance.SetGameState(GameState.ChooseUpgrade);
        }
    }
}
