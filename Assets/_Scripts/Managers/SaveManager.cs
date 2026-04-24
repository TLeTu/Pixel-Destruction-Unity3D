using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    private const string LevelIndexKey = "SavedLevelIndex";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveLevelIndex(int levelIndex)
    {
        int safeIndex = Mathf.Max(0, levelIndex);
        PlayerPrefs.SetInt(LevelIndexKey, safeIndex);
        PlayerPrefs.Save();
        Debug.Log("Saved level index: " + safeIndex);
    }

    public int LoadLevelIndex()
    {
        int savedIndex = PlayerPrefs.GetInt(LevelIndexKey, 0);
        return Mathf.Max(0, savedIndex);
    }
}
