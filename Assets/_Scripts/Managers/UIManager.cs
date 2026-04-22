using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public Slider xpBar;
    public GameObject mainMenuPanel;
    public GameObject inGamePanel;
    public GameObject worldCanvas;
    public GameObject weaponSlotButtonPrefab;
    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        GameManager.instance.OnMainMenu += () => ShowPanel(mainMenuPanel);
    }

    public void SetUpXPBar(float minXP, float maxXP)
    {
        if (xpBar != null)
        {
            xpBar.minValue = minXP;
            xpBar.maxValue = maxXP;
        }
    }
    public void UpdateXPBar(float currentXP)
    {
        if (xpBar != null)
        {
            xpBar.value = currentXP;
        }
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
    public void SetUpWeaponSlotButton(GameObject obstacle)
    {
        GameObject newButton = Instantiate(weaponSlotButtonPrefab, inGamePanel.transform);
        WeaponSlotController controller = newButton.GetComponent<WeaponSlotController>();
        controller.obstacle = obstacle;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(obstacle.transform.position);
        newButton.transform.position = screenPos;
    }
    public void MenuPlayButton()
    {
        HidePanel(mainMenuPanel);
        ShowPanel(inGamePanel);
        GameManager.instance.SetGameState(GameState.Playing);
    }
}
