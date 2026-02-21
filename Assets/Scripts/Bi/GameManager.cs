using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Scene Settings")]
    public int titleSceneBuildIndex = 0;
    public int nextSceneBuildIndex = 1;
    
    [Header("Panels")]
    public GameObject firstPanel;      // 시작 화면 추가!
    public GameObject menuPanel;
    public GameObject saveLoadPanel;
    public GameObject howToPlayPanel;
    public Image fadePanel;
    
    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;
    
    void Start()
    {
        // 시작할 때 첫 화면만 표시
        ShowFirstPanel();
    }
    
    // ========== 패널 전환 ==========
    
    public void ShowFirstPanel()
    {
        HideAllPanels();
        firstPanel.SetActive(true);
    }
    
    public void ShowMenuPanel()
    {
        Debug.Log("=== ShowMenuPanel 호출됨! ===");
        HideAllPanels();
        menuPanel.SetActive(true);
    }
    
    public void ShowSaveLoadPanel()
    {
        HideAllPanels();
        saveLoadPanel.SetActive(true);
    }
    
    public void ShowHowToPlayPanel()
    {
        HideAllPanels();
        howToPlayPanel.SetActive(true);
    }
    
    void HideAllPanels()
    {
        firstPanel.SetActive(false);
        menuPanel.SetActive(false);
        saveLoadPanel.SetActive(false);
        howToPlayPanel.SetActive(false);
    }
    
    // ========== 게임 시작/종료 ==========
    
    public void StartGame()
    {
        StartCoroutine(FadeAndLoadScene(nextSceneBuildIndex));
    }
    
    public void BackToTitle()
    {
        StartCoroutine(FadeAndLoadScene(titleSceneBuildIndex));
    }
    
    public void QuitGame()
    {
        StartCoroutine(FadeAndQuit());
    }
    
    // 씬 로드
    IEnumerator FadeAndLoadScene(int sceneIndex)
    {
        float elapsed = 0f;
        Color color = fadePanel.color;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsed / fadeDuration);
            fadePanel.color = color;
            yield return null;
        }
        
        SceneManager.LoadScene(sceneIndex);
    }
    
    // 게임 종료
    IEnumerator FadeAndQuit()
    {
        float elapsed = 0f;
        Color color = fadePanel.color;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsed / fadeDuration);
            fadePanel.color = color;
            yield return null;
        }
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}