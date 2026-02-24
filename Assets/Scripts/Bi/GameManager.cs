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
    public GameObject firstPanel;
    public GameObject menuPanel;
    public GameObject saveLoadPanel;
    public GameObject howToPlayPanel;
    public Image fadePanel;

    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;

    [Header("Game Start Settings")]
    [SerializeField] private int scenarioId = 5;
    [SerializeField] private int userId = 1;

    private const string PLAYERS_ROOM_SCENE_NAME = "pg";

    private ApiClient apiClient;
    

    void Start()
    {
        ShowFirstPanel();
        apiClient = ApiClient.Instance;
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
        if (apiClient == null)
        {
            Debug.LogError("[GameManager] ApiClient를 찾을 수 없습니다.");
            return;
        }

        Debug.Log($"[GameManager] 시나리오 시작: scenarioId={scenarioId}, userId={userId}");

        apiClient.StartScenario(scenarioId, userId,
            (gameId) =>
            {
                Debug.Log($"[GameManager] 게임 시작 성공: gameId={gameId}");
                StartCoroutine(FadeAndLoadSceneByName(PLAYERS_ROOM_SCENE_NAME));
            },
            (error) =>
            {
                Debug.LogError($"[GameManager] 게임 시작 실패: {error}");
            }
        );
    }

    public void BackToTitle()
    {
        StartCoroutine(FadeAndLoadScene(titleSceneBuildIndex));
    }

    public void QuitGame()
    {
        StartCoroutine(FadeAndQuit());
    }

    // 씬 로드 (빌드 인덱스 사용)
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

    // 씬 로드 (씬 이름 사용)
    IEnumerator FadeAndLoadSceneByName(string sceneName)
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

        SceneManager.LoadScene(sceneName);
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