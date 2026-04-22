using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameState gameState;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public event Action OnMainMenu;
    public event Action OnGameStarted;
    public event Action OnGamePaused;
    public event Action OnGameResumed;
    public event Action OnGameWin;
    [SerializeField] private string levelFolder = "Data Levels";
    private List<LevelConfig> levelConfigs = new List<LevelConfig>();
    public IReadOnlyList<LevelConfig> LevelConfigs => levelConfigs;
    void Awake()
    {
        instance = this;
        LoadLevelConfigs();
    }
    void Start()
    {
        SetGameState(GameState.MainMenu);
    }
    private void LoadLevelConfigs()
    {
        levelConfigs = Resources.LoadAll<LevelConfig>(levelFolder)
            .OrderBy(x => x.name)
            .ToList();

        if (levelConfigs.Count == 0)
        {
            Debug.LogWarning("No LevelConfig found in Resources/" + levelFolder);
        }
    }
    private void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levelConfigs.Count)
        {
            Debug.LogError("Invalid level index: " + levelIndex);
            return;
        }
        LevelConfig config = levelConfigs[levelIndex];
        // LevelManager.instance.levelConfig = config;
        LevelManager.instance.LoadLevel(config);
        ScoreManager.instance.SetupScoreManager(0, config.scoreThreshold);
        foreach (Vector3 obstaclePos in config.obstaclePositions)
        {
            ObstacleManager.instance.SpawnObstacle(obstaclePos);
        }
    }
    private void PauseGame(bool shouldPause)
    {
        ObstacleManager.instance.PauseWeapons(shouldPause);
        LevelManager.instance.PauseLevel(shouldPause);
    }
    public void SetGameState(GameState newState)
    {
        GameState currentState = gameState;
        switch (newState)
        {
            case GameState.MainMenu:
                OnMainMenu?.Invoke();
                break;
            case GameState.Playing:
                if (currentState == GameState.MainMenu)
                {
                    LoadLevel(0);
                    OnGameStarted?.Invoke();
                }
                else if (currentState == GameState.ChooseUpgrade)
                {
                    OnGameResumed?.Invoke();
                }
                break;
            case GameState.ChooseUpgrade:
                if (currentState == GameState.Playing)
                {
                    OnGamePaused?.Invoke();
                }
                break;
            case GameState.GameWin:
                if (currentState == GameState.Playing)
                {
                    OnGameWin?.Invoke();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
        gameState = newState;
    }
}

public enum GameState
{
    MainMenu,
    Playing,
    ChooseUpgrade,
    GameWin
}