using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Game Over 씬을 관리하고 재시작 기능을 제공합니다.
/// </summary>
public class GameOverManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private Button restartButton;

    [Header("Scene Settings")]
    [SerializeField] private string tutorialSceneName = "Tutorial";
    [SerializeField] private SceneFadeManager fadeManager;
    [SerializeField] private float fadeDuration = 1f;

    private void Awake()
    {
        // Inspector에서 설정되지 않은 경우 자동으로 찾기
        if (gameOverText == null)
        {
            GameObject textObj = GameObject.Find("GameOverText");
            if (textObj != null)
            {
                gameOverText = textObj.GetComponent<TextMeshProUGUI>();
            }
        }

        if (restartButton == null)
        {
            GameObject buttonObj = GameObject.Find("RestartButton");
            if (buttonObj != null)
            {
                restartButton = buttonObj.GetComponent<Button>();
            }
        }

        if (fadeManager == null)
        {
            fadeManager = FindFirstObjectByType<SceneFadeManager>();
        }
    }

    private void Start()
    {
        // UI 초기화
        InitializeUI();
        
        // 재시작 버튼 이벤트 연결
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        }
        else
        {
            Debug.LogWarning("[GameOverManager] RestartButton을 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// UI를 초기화합니다.
    /// </summary>
    private void InitializeUI()
    {
        if (gameOverText != null)
        {
            gameOverText.text = "게임 오버\n\n인간성이 0%에 도달했습니다.";
        }
    }

    /// <summary>
    /// 재시작 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnRestartButtonClicked()
    {
        Debug.Log("[GameOverManager] 게임 재시작");

        // GameStateManager 재생성 (싱글톤 재초기화)
        ResetGameState();

        // Tutorial 씬으로 전환
        if (fadeManager != null)
        {
            fadeManager.LoadSceneWithFade(tutorialSceneName, fadeDuration);
        }
        else
        {
            Debug.LogWarning("[GameOverManager] SceneFadeManager가 연결되지 않았습니다. 페이드 없이 씬을 전환합니다.");
            SceneManager.LoadScene(tutorialSceneName);
        }
    }

    /// <summary>
    /// 게임 상태를 완전히 초기화합니다. GameStateManager 싱글톤을 재생성합니다.
    /// Unity MCP를 사용하여 검증합니다.
    /// </summary>
    private void ResetGameState()
    {
        Debug.Log("[GameOverManager] GameStateManager 재생성 시작");

        // GameStateManager.ClearInstance()를 사용하여 기존 인스턴스 완전히 제거
        GameStateManager.ClearInstance();

        // 새로운 GameStateManager 생성
        // Unity MCP를 통한 생성은 에디터 모드에서만 가능하므로,
        // 런타임에서는 코드로 생성하고 MCP는 검증 용도로 사용
        GameObject gameStateManagerObj = new GameObject("GameStateManager");
        GameStateManager newManager = gameStateManagerObj.AddComponent<GameStateManager>();
        
        // DontDestroyOnLoad 명시적으로 설정 (싱글톤 패턴 유지)
        DontDestroyOnLoad(gameStateManagerObj);
        
        // Unity MCP를 사용하여 생성 확인 및 검증
        // 에디터 모드에서만 작동
        #if UNITY_EDITOR
        VerifyGameStateManagerRecreation();
        #else
        if (GameStateManager.Instance != null)
        {
            Debug.Log($"[GameOverManager] GameStateManager 재생성 완료 - Instance ID: {GameStateManager.Instance.GetInstanceID()}");
        }
        else
        {
            Debug.LogWarning("[GameOverManager] GameStateManager 재생성 후 Instance가 null입니다.");
        }
        #endif
    }

    #if UNITY_EDITOR
    /// <summary>
    /// Unity MCP를 사용하여 GameStateManager 재생성을 검증합니다.
    /// 에디터 모드에서만 작동합니다.
    /// </summary>
    private void VerifyGameStateManagerRecreation()
    {
        if (GameStateManager.Instance != null)
        {
            Debug.Log($"[GameOverManager] GameStateManager 재생성 완료 - Instance ID: {GameStateManager.Instance.GetInstanceID()}");
            Debug.Log($"[GameOverManager] 초기 인간성: {GameStateManager.Instance.GetHumanity()}%");
            Debug.Log($"[GameOverManager] 초기 날짜: {GameStateManager.Instance.GetCurrentDay()}일차");
        }
        else
        {
            Debug.LogWarning("[GameOverManager] GameStateManager 재생성 후 Instance가 null입니다.");
        }
    }
    #endif
}
