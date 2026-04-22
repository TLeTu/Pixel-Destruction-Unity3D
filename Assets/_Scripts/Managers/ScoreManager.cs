using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;
    private int scoreThresholds = 500;
    private int currentScore = 0;
    private int currentThreshold = 100;
    void Awake()
    {
        instance = this;
    }
    public void CleanUp()
    {
        currentScore = 0;
        currentThreshold = 0;
        scoreThresholds = 0;
    }
    public void UpdateScore(int points)
    {
        currentScore += points;
        UIManager.instance.UpdateXPBar(currentScore);
        CheckForThresholds();

    }
    public void SetupScoreManager(int currentScore, int thresholds)
    {
        this.currentScore = currentScore;
        this.scoreThresholds = thresholds;
        this.currentThreshold = scoreThresholds;
        UIManager.instance.SetUpXPBar(0, currentThreshold);
    }
    private void CheckForThresholds()
    {
        while (currentScore >= currentThreshold)
        {
            currentThreshold += scoreThresholds;
        }
    }
}
