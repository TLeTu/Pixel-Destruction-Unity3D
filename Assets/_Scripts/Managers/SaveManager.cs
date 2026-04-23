using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    private const string LevelIndexKey = "SavedLevelIndex";
    [SerializeField] private int debugLevelIndex = 0;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
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

    [ContextMenu("Debug/Set Saved Level Index")]
    private void DebugSetSavedLevelIndex()
    {
        SaveLevelIndex(debugLevelIndex);
        Debug.Log("Debug set SavedLevelIndex = " + debugLevelIndex);
    }

    [ContextMenu("Debug/Clear Saved Level Index")]
    private void DebugClearSavedLevelIndex()
    {
        PlayerPrefs.DeleteKey(LevelIndexKey);
        PlayerPrefs.Save();
        Debug.Log("Debug cleared SavedLevelIndex");
    }
}
