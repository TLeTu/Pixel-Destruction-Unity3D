using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public Slider xpBar;
    public GameObject mainMenuPanel;
    public GameObject inGamePanel;
    public GameObject placeWeaponPanel;
    public GameObject weaponSlotButtonPrefab;
    public GameObject chooseUpgradePanel;
    public GameObject upgradeBtn1;
    public GameObject upgradeBtn2;
    void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.OnGameStateChanged += HandleGameStateChanged;
            HandleGameStateChanged(GameManager.instance.gameState);
        }
    }

    private void OnDisable()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.OnGameStateChanged -= HandleGameStateChanged;
        }
    }

    private void HandleGameStateChanged(GameState state)
    {
        HideAllPanels();

        switch (state)
        {
            case GameState.MainMenu:
                ShowPanel(mainMenuPanel);
                break;
            case GameState.Playing:
                ShowPanel(inGamePanel);
                break;
            case GameState.PlaceWeapon:
                ShowPanel(placeWeaponPanel);
                break;
            case GameState.ChooseUpgrade:
                ShowPanel(chooseUpgradePanel);
                break;
            case GameState.GameWin:
                break;
            default:
                throw new System.ArgumentOutOfRangeException(nameof(state), state, null);
        }
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
        if (panel != null)
        {
            Debug.Log("Showing panel: " + panel.name);
            panel.SetActive(true);
        }
    }
    private void HidePanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    private void HideAllPanels()
    {
        HidePanel(mainMenuPanel);
        HidePanel(inGamePanel);
        HidePanel(placeWeaponPanel);
        HidePanel(chooseUpgradePanel);
    }
    public void SetUpgradeButtons(WeaponUpgrade upgrade1, WeaponUpgrade upgrade2)
    {
        upgradeBtn1.GetComponent<UpgradeBtnController>().upgrade = upgrade1;
        upgradeBtn2.GetComponent<UpgradeBtnController>().upgrade = upgrade2;
    }

    public void SetUpWeaponSlotButton(GameObject obstacle)
    {
        GameObject newButton = Instantiate(weaponSlotButtonPrefab, placeWeaponPanel.transform);
        WeaponSlotController controller = newButton.GetComponent<WeaponSlotController>();
        controller.obstacle = obstacle;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(obstacle.transform.position);
        newButton.transform.position = screenPos;
    }
    public void MenuPlayButton()
    {
        GameManager.instance.SetGameState(GameState.Playing);
    }
}
