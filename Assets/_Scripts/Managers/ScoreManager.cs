using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;
    private float[] scoreThresholds = { 0f, 100f, 250f, 500f, 1000f };
    private int currentScore = 0;
    private float currentThreshold = 100f;
    void Awake()
    {
        instance = this;
    }
    public void CleanUp()
    {
        currentScore = 0;
        currentThreshold = 0;
        scoreThresholds = null;
    }
    public void UpdateScore(int points)
    {
        currentScore += points;
        UIManager.instance.UpdateXPBar(currentScore);
        CheckForThresholds();

    }
    public void SetupScoreManager(int currentScore, float[] thresholds)
    {
        this.currentScore = currentScore;
        SetupThresholds(thresholds);
    }
    private void SetupThresholds(float[] thresholds)
    {
        scoreThresholds = thresholds;
        currentThreshold = scoreThresholds[0];
        UIManager.instance.SetUpXPBar(0, currentThreshold);
    }
    private void CheckForThresholds()
    {
        for (int i = scoreThresholds.Length - 1; i >= 0; i--)
        {
            if (currentScore >= scoreThresholds[i])
            {
                if (i < scoreThresholds.Length - 1)
                {
                    currentThreshold = scoreThresholds[i + 1];
                }
                break;
            }
        }
    }
}
