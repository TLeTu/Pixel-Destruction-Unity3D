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
    public event Action OnGameResumed;
    public event Action OnGamePaused;
    public event Action OnGameWin;
    [SerializeField] private string levelFolder = "Data Levels";
    private List<LevelConfig> levelConfigs = new List<LevelConfig>();
    public IReadOnlyList<LevelConfig> LevelConfigs => levelConfigs;
    private int currentLevelIndex = 0;
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
        currentLevelIndex = levelIndex;
        LevelConfig config = levelConfigs[levelIndex];
        // LevelManager.instance.levelConfig = config;
        ScoreManager.instance.SetupScoreManager(0, config.scoreThreshold);
        foreach (Vector3 obstaclePos in config.obstaclePositions)
        {
            ObstacleManager.instance.SpawnObstacle(obstaclePos);
        }
        List<GameObject> obstaclesWithoutWeapons = ObstacleManager.instance.GetObstaclesWithoutWeapons();
        foreach (GameObject obstacle in obstaclesWithoutWeapons)
        {
            UIManager.instance.SetUpWeaponSlotButton(obstacle);
        }
        ObstacleManager.instance.LoadWeaponPrefab(config.weaponPrefab);
        StartPlaceWeapon(config.initialWeaponCount);
        LevelManager.instance.LoadLevel(config);
    }
    public void StartPlaceWeapon(int numberOfWeaponsToPlace)
    {
        Debug.Log("Starting Place Weapon phase with quota: " + numberOfWeaponsToPlace);
        ObstacleManager.instance.StartPlacingSession(numberOfWeaponsToPlace);

        SetGameState(GameState.PlaceWeapon);
    }
    private void PauseGame(bool shouldPause)
    {
        Debug.Log((shouldPause ? "Pausing" : "Resuming") + " game. Pausing weapons and level.");
        ObstacleManager.instance.PauseWeapons(shouldPause);
        LevelManager.instance.PauseLevel(shouldPause);
    }
    public void SetGameState(GameState newState)
    {
        GameState currentState = gameState;
        gameState = newState;
        switch (newState)
        {
            case GameState.MainMenu:
                OnMainMenu?.Invoke();
                InputManager.instance.DisableInput();
                break;
            case GameState.Playing:
                if (currentState == GameState.MainMenu)
                {
                    OnGameStarted?.Invoke();
                    LoadLevel(0);
                    InputManager.instance.EnableInput();
                }
                else
                {
                    OnGameResumed?.Invoke();
                    InputManager.instance.EnableInput();
                    PauseGame(false);
                }
                break;
            case GameState.ChooseUpgrade:
                OnGamePaused?.Invoke();
                InputManager.instance.DisableInput();
                PauseGame(true);
                break;
            case GameState.GameWin:
                OnGameWin?.Invoke();
                InputManager.instance.DisableInput();
                break;
            case GameState.PlaceWeapon:
                OnGamePaused?.Invoke();
                InputManager.instance.EnableInput();
                PauseGame(true);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }
}

public enum GameState
{
    MainMenu,
    Playing,
    ChooseUpgrade,
    PlaceWeapon,
    GameWin
}