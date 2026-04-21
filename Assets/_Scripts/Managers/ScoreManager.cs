using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;
    private float[] scoreThresholds = { 0f, 100f, 250f, 500f, 1000f };
    private int currentScore = 0;
    void Awake()
    {
        instance = this;
    }
    private void UpdateScore(int points)
    {
        currentScore += points;
        CheckForThresholds();

    }
    private void CheckForThresholds()
    {
        for (int i = scoreThresholds.Length - 1; i >= 0; i--)
        {
            if (currentScore >= scoreThresholds[i])
            {
                Debug.Log("Reached score threshold: " + scoreThresholds[i]);
                break;
            }
        }
    }
}
