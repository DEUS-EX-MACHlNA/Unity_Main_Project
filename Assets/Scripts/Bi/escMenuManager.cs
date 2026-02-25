using UnityEngine;
using UnityEngine.EventSystems;

public class EscMenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject menuPanel;         // 최상위 메뉴 패널
    [SerializeField] private GameObject menuButtonsPanel;  // 버튼들 있는 패널
    [SerializeField] private GameObject saveLoadPanel;
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private GameObject langPanel;

    private void Awake()
    {
        if (menuPanel != null)
            menuPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("[EscMenuManager] ESC 눌림!");

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);

            if (menuPanel != null && menuPanel.activeSelf)
                CloseMenu();
            else
                ShowMenuButtons();
        }
    }

    // ========== 패널 전환 ==========

    public void ShowMenuButtons()
    {
        menuPanel.SetActive(true);
        HideSubPanels();
        if (menuButtonsPanel != null) menuButtonsPanel.SetActive(true);
    }

    public void ShowSaveLoadPanel()
    {
        HideSubPanels();
        if (saveLoadPanel != null) saveLoadPanel.SetActive(true);
        if (menuButtonsPanel != null) menuButtonsPanel.SetActive(false);
    }

    public void ShowHowToPlayPanel()
    {
        HideSubPanels();
        if (howToPlayPanel != null) howToPlayPanel.SetActive(true);
        if (menuButtonsPanel != null) menuButtonsPanel.SetActive(false);
    }

    public void ShowLangPanel()
    {
        HideSubPanels();
        if (langPanel != null) langPanel.SetActive(true);
        if (menuButtonsPanel != null) menuButtonsPanel.SetActive(false);
    }

    public void BackToMenu()
    {
        ShowMenuButtons();
    }

    public void CloseMenu()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
    }

    void HideSubPanels()
    {
        if (saveLoadPanel != null) saveLoadPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (langPanel != null) langPanel.SetActive(false);
    }
}