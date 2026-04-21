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
    void Start()
    {
        UIManager.instance.SetUpXPBar(0f, currentThreshold);
    }
    public void UpdateScore(int points)
    {
        currentScore += points;
        Debug.Log("Score updated: " + currentScore);
        UIManager.instance.UpdateXPBar(currentScore);
        CheckForThresholds();

    }
    private void CheckForThresholds()
    {
        for (int i = scoreThresholds.Length - 1; i >= 0; i--)
        {
            if (currentScore >= scoreThresholds[i])
            {
                Debug.Log("Reached score threshold: " + scoreThresholds[i]);
                // Setup the xp
                if (i < scoreThresholds.Length - 1)
                {
                    currentThreshold = scoreThresholds[i + 1];
                    UIManager.instance.SetUpXPBar(scoreThresholds[i], currentThreshold);
                }
                break;
            }
        }
    }
}
