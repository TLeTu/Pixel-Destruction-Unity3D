using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public GameObject mainMenuPanel;
    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        GameManager.instance.OnMainMenu += ShowMainMenu;
        GameManager.instance.OnGameStarted += ShowGameUI;
    }
    public void ShowMainMenu()
    {
        // Implement logic to show main menu UI
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }
    public void ShowGameUI()
    {
        // Implement logic to show in-game UI
    }
    public void MenuPlayButton()
    {
        // Implement logic for play button in main menu
        ShowGameUI();
        GameManager.instance.SetGameState(GameState.Playing);
    }
}
