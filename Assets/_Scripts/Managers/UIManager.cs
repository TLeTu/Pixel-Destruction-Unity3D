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
        GameManager.instance.OnMainMenu += () => ShowPanel(mainMenuPanel);
    }

    private void ShowPanel(GameObject panel)
    {
        if (panel != null && !panel.activeSelf)
        {
            panel.SetActive(true);
        }
    }
    private void HidePanel(GameObject panel)
    {
        if (panel != null && panel.activeSelf)
        {
            panel.SetActive(false);
        }
    }
    public void MenuPlayButton()
    {
        HidePanel(mainMenuPanel);
        GameManager.instance.SetGameState(GameState.Playing);
    }
}
