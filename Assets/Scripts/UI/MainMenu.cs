using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainPanel;
    public GameObject inventoryPanel;
    public GameObject battlePassPanel;
    public GameObject settingsPanel;
    
    [Header("Stats Display")]
    public TextMeshProUGUI cookiesText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI xpText;
    
    void Start()
    {
        ShowMainPanel();
        UpdateStats();
    }

    public void ShowMainPanel()
    {
        HideAllPanels();
        mainPanel.SetActive(true);
        UpdateStats();
    }

    public void ShowInventory()
    {
        HideAllPanels();
        inventoryPanel.SetActive(true);
    }

    public void ShowBattlePass()
    {
        HideAllPanels();
        battlePassPanel.SetActive(true);
    }

    public void ShowSettings()
    {
        HideAllPanels();
        settingsPanel.SetActive(true);
    }

    public void PlayGame()
    {
        // Load game scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    void HideAllPanels()
    {
        mainPanel.SetActive(false);
        inventoryPanel.SetActive(false);
        battlePassPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    void UpdateStats()
    {
        if (PlayerProgression.Instance != null)
        {
            PlayerData data = PlayerProgression.Instance.GetData();
            if (cookiesText != null)
                cookiesText.text = "🍪 " + data.cookies.ToString();
            if (levelText != null)
                levelText.text = "LVL " + data.level.ToString();
            if (xpText != null)
                xpText.text = "XP: " + data.xp.ToString();
        }
    }
}
