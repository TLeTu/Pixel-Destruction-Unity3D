using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
        UIManager.instance.SetLevelText(currentLevelIndex);
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
        int availableSlots = ObstacleManager.instance.AvailableWeaponSlots();
        int placementQuota = Mathf.Min(numberOfWeaponsToPlace, availableSlots);

        if (placementQuota <= 0)
        {
            Debug.Log("Skipping Place Weapon phase because there are no available obstacle slots.");
            SetGameState(GameState.Playing);
            return;
        }

        if (placementQuota < numberOfWeaponsToPlace)
        {
            Debug.Log("Reducing Place Weapon quota to available slots: " + placementQuota);
        }

        Debug.Log("Starting Place Weapon phase with quota: " + placementQuota);
        ObstacleManager.instance.StartPlacingSession(placementQuota);

        SetGameState(GameState.PlaceWeapon);
    }
    public void StartUpgrade()
    {
        if (ObstacleManager.instance.GetObstaclesCount() <= 0)
        {
            Debug.Log("Skipping upgrade phase because there are no obstacles.");
            SetGameState(GameState.Playing);
            return;
        }

        Debug.Log("Starting Upgrade phase. Presenting upgrade choices to player.");
        WeaponUpgrade upgrade1 = ObstacleManager.instance.GetRandomUpgrade();
        WeaponUpgrade upgrade2 = ObstacleManager.instance.GetRandomUpgrade();
        while (upgrade2 == upgrade1)
        {
            upgrade2 = ObstacleManager.instance.GetRandomUpgrade();
        }
        UIManager.instance.SetUpgradeButtons(upgrade1, upgrade2);
    }

    private void EndLevel()
    {
        Debug.Log("Ending level and cleaning up. Returning to main menu.");
        PauseGame(true);
        InputManager.instance.DisableInput();

        LevelManager.instance.CleanUpLevel();
        ObstacleManager.instance.CleanUp();
        PoolManager.instance.ReturnAllToPool();
        PoolManager.instance.ResetSpawnedPixelCount();
        ScoreManager.instance.CleanUp();
        UIManager.instance.ClearWeaponSlotButtons();
    }
    public void NextLevel()
    {
        int nextLevelIndex = currentLevelIndex + 1;
        if (nextLevelIndex >= levelConfigs.Count)
        {
            Debug.Log("All levels completed. Returning to main menu.");
            SetGameState(GameState.MainMenu);
            return;
        }

        LoadLevel(nextLevelIndex);
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
    public void ReplayLevel()
    {
        EndLevel();
        LoadLevel(currentLevelIndex);
    }
    public void StartGame()
    {
        int levelToLoad = 0;

        if (SaveManager.instance != null)
        {
            levelToLoad = SaveManager.instance.LoadLevelIndex();
        }

        int maxLevelIndex = Mathf.Max(0, levelConfigs.Count - 1);
        levelToLoad = Mathf.Clamp(levelToLoad, 0, maxLevelIndex);
        LoadLevel(levelToLoad);
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
                EndLevel();
                break;
            case GameState.Playing:
                InputManager.instance.EnableInput();
                PauseGame(false);
                break;
            case GameState.ChooseUpgrade:
                PauseGame(true);
                InputManager.instance.DisableInput();
                StartUpgrade();
                break;
            case GameState.GameWin:
                if (SaveManager.instance != null)
                {
                    int maxLevelIndex = Mathf.Max(0, levelConfigs.Count - 1);
                    int nextUnlockedLevelIndex = Mathf.Min(currentLevelIndex + 1, maxLevelIndex);
                    SaveManager.instance.SaveLevelIndex(nextUnlockedLevelIndex);
                }
                EndLevel();
                InputManager.instance.DisableInput();
                break;
            case GameState.PlaceWeapon:
                PauseGame(true);
                InputManager.instance.EnableInput();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
        Debug.Log("Game state changed to: " + newState);

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