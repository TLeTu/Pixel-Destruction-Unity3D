using System;
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
    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
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
                    OnGameStarted?.Invoke();
                }
                else if (currentState == GameState.Paused)
                {
                    OnGameResumed?.Invoke();
                }
                break;
            case GameState.Paused:
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
    Paused,
    GameWin
}