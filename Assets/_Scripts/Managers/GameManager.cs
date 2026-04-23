using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public event Action<GameState> OnGameStateChanged;
    public GameState gameState;
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
        InputManager.instance.SetTapDamage(config.damageRadius, config.maxTapDamage, config.minTapDamage);
        ScoreManager.instance.SetupScoreManager(0, config.scoreThreshold, config.targetDestroyCount);
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
        if (numberOfWeaponsToPlace <= 0)
        {
            Debug.Log("Skipping Place Weapon phase because quota is 0.");
            SetGameState(GameState.Playing);
            return;
        }

        Debug.Log("Starting Place Weapon phase with quota: " + numberOfWeaponsToPlace);
        ObstacleManager.instance.StartPlacingSession(numberOfWeaponsToPlace);

        SetGameState(GameState.PlaceWeapon);
    }
    public void StartUpgrade()
    {
        WeaponUpgrade upgrade1 = ObstacleManager.instance.GetRandomUpgrade();
        WeaponUpgrade upgrade2 = ObstacleManager.instance.GetRandomUpgrade();
        while (upgrade2 == upgrade1)        {
            upgrade2 = ObstacleManager.instance.GetRandomUpgrade();
        }
        UIManager.instance.SetUpgradeButtons(upgrade1, upgrade2);
    }

    public void EndLevel()
    {
        PauseGame(true);
        InputManager.instance.DisableInput();

        LevelManager.instance.CleanUpLevel();
        ObstacleManager.instance.CleanUp();
        PoolManager.instance.ReturnAllToPool();
        ScoreManager.instance.CleanUp();
        UIManager.instance.ClearWeaponSlotButtons();

        // int nextLevelIndex = currentLevelIndex + 1;
        // if (nextLevelIndex >= levelConfigs.Count)
        // {
        //     Debug.Log("All levels completed. Returning to main menu.");
        //     SetGameState(GameState.MainMenu);
        //     return;
        // }

        // LoadLevel(nextLevelIndex);
    }

    public void OnUpgradeSelected(WeaponUpgrade upgrade)
    {
        if (gameState != GameState.ChooseUpgrade)
        {
            return;
        }

        if (upgrade == WeaponUpgrade.MoreWeapons)
        {
            StartPlaceWeapon(1);
            return;
        }

        ObstacleManager.instance.ApplyUpgradeToWeapon(upgrade);
        SetGameState(GameState.Playing);
    }
    private void PauseGame(bool shouldPause)
    {
        Debug.Log((shouldPause ? "Pausing" : "Resuming") + " game. Pausing weapons and level.");
        ObstacleManager.instance.PauseWeapons(shouldPause);
        LevelManager.instance.PauseLevel(shouldPause);
    }
    public void SetGameState(GameState newState)
    {
        if (gameState == newState)
        {
            OnGameStateChanged?.Invoke(gameState);
            return;
        }

        GameState currentState = gameState;
        gameState = newState;
        switch (newState)
        {
            case GameState.MainMenu:
                InputManager.instance.DisableInput();
                break;
            case GameState.Playing:
                if (currentState == GameState.MainMenu)
                {
                    LoadLevel(0);
                    InputManager.instance.EnableInput();
                }
                else
                {
                    InputManager.instance.EnableInput();
                    PauseGame(false);
                }
                break;
            case GameState.ChooseUpgrade:
                InputManager.instance.DisableInput();
                StartUpgrade();
                PauseGame(true);
                break;
            case GameState.GameWin:
                InputManager.instance.DisableInput();
                break;
            case GameState.PlaceWeapon:
                InputManager.instance.EnableInput();
                PauseGame(true);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        OnGameStateChanged?.Invoke(gameState);
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