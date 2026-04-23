using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public Slider xpBar;
    public GameObject mainMenuPanel;
    public GameObject inGamePanel;
    public GameObject placeWeaponPanel;
    public GameObject weaponSlotButtonPrefab;
    public GameObject chooseUpgradePanel;
    public GameObject gameWinPanel;
    public GameObject upgradeBtn1;
    public GameObject upgradeBtn2;
    public GameObject nextLevelBtn;
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
                ShowPanel(gameWinPanel);
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
        HidePanel(gameWinPanel);
    }
    public void SetUpgradeButtons(WeaponUpgrade upgrade1, WeaponUpgrade upgrade2)
    {
        upgradeBtn1.GetComponent<UpgradeBtnController>().upgrade = upgrade1;
        upgradeBtn2.GetComponent<UpgradeBtnController>().upgrade = upgrade2;

        SetButtonLabel(upgradeBtn1, upgrade1.ToString());
        SetButtonLabel(upgradeBtn2, upgrade2.ToString());
    }

    private void SetButtonLabel(GameObject buttonObj, string label)
    {
        TMP_Text tmpText = buttonObj.GetComponentInChildren<TMP_Text>();
        if (tmpText != null)
        {
            tmpText.text = label;
            return;
        }

        Text legacyText = buttonObj.GetComponentInChildren<Text>();
        if (legacyText != null)
        {
            legacyText.text = label;
            return;
        }

        Debug.LogWarning("No text component found on button: " + buttonObj.name);
    }

    public void SetUpWeaponSlotButton(GameObject obstacle)
    {
        GameObject newButton = Instantiate(weaponSlotButtonPrefab, placeWeaponPanel.transform);
        WeaponSlotController controller = newButton.GetComponent<WeaponSlotController>();
        controller.obstacle = obstacle;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(obstacle.transform.position);
        newButton.transform.position = screenPos;
    }

    public void ClearWeaponSlotButtons()
    {
        if (placeWeaponPanel == null)
        {
            return;
        }

        Transform panel = placeWeaponPanel.transform;
        for (int i = panel.childCount - 1; i >= 0; i--)
        {
            Transform child = panel.GetChild(i);
            if (child.GetComponent<WeaponSlotController>() != null)
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void MenuPlayButton()
    {
        GameManager.instance.StartGame();
    }
    public void NextLevelButton()
    {
        GameManager.instance.NextLevel();
    }
    public void BackToMenuButton()
    {
        GameManager.instance.SetGameState(GameState.MainMenu);
    }
}
