using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 게임 시작 씬을 관리하고 게임 시작 기능을 제공합니다.
/// </summary>
public class TitleManager : MonoBehaviour
{
    private TextMeshProUGUI titleText;
    private Button startButton;
    private SceneFadeManager fadeManager;
    private ApiClient apiClient;
    
    [Header("Game Start Settings")]
    [SerializeField] private int scenarioId = 1;
    [SerializeField] private int userId = 1;
    
    // 상수로 정의 (Inspector에서 수정 불가능)
    private const string PLAYERS_ROOM_SCENE_NAME = "PlayersRoom";
    private const float FADE_DURATION = 1f;

    private void Awake()
    {
        // 자동 참조 찾기
        GameObject textObj = GameObject.Find("TitleText");
        if (textObj != null)
        {
            titleText = textObj.GetComponent<TextMeshProUGUI>();
        }

        GameObject buttonObj = GameObject.Find("StartButton");
        if (buttonObj != null)
        {
            startButton = buttonObj.GetComponent<Button>();
        }

        fadeManager = FindFirstObjectByType<SceneFadeManager>();
        apiClient = FindFirstObjectByType<ApiClient>();
        
        // ApiClient가 없으면 생성
        if (apiClient == null)
        {
            GameObject apiClientObj = new GameObject("ApiClient");
            apiClient = apiClientObj.AddComponent<ApiClient>();
            DontDestroyOnLoad(apiClientObj);
        }
    }

    private void Start()
    {
        // 시작 버튼 이벤트 연결
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
        else
        {
            Debug.LogWarning("[TitleManager] StartButton을 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 게임 시작 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnStartButtonClicked()
    {
        Debug.Log("[TitleManager] 게임 시작");

        if (apiClient == null)
        {
            Debug.LogError("[TitleManager] ApiClient를 찾을 수 없습니다.");
            return;
        }

        // 시작 버튼 비활성화 (중복 클릭 방지)
        if (startButton != null)
        {
            startButton.interactable = false;
        }

        Debug.Log($"[TitleManager] 시나리오 시작: scenarioId={scenarioId}, userId={userId}");

        // 시나리오 시작 API 호출
        apiClient.StartScenario(scenarioId, userId,
            (gameId) => {
                Debug.Log($"[TitleManager] 게임 시작 성공: gameId={gameId}");
                // 씬 전환
                if (fadeManager != null)
                {
                    fadeManager.LoadSceneWithFade(PLAYERS_ROOM_SCENE_NAME, FADE_DURATION);
                }
                else
                {
                    Debug.LogWarning("[TitleManager] SceneFadeManager가 연결되지 않았습니다. 페이드 없이 씬을 전환합니다.");
                    SceneManager.LoadScene(PLAYERS_ROOM_SCENE_NAME);
                }
            },
            (error) => {
                Debug.LogError($"[TitleManager] 게임 시작 실패: {error}");
                // 에러 발생 시 버튼 다시 활성화
                if (startButton != null)
                {
                    startButton.interactable = true;
                }
                // 에러 메시지 표시 (선택적)
                if (titleText != null)
                {
                    titleText.text = $"게임 시작 실패\n{error}";
                }
            }
        );
    }
}

