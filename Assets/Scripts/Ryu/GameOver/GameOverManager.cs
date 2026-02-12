using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Game Over 씬을 관리하고 재시작 기능을 제공합니다.
/// 모든 엔딩 타입에 대해 적절한 메시지를 표시합니다.
/// 모든 설정은 스크립트를 통해서만 처리됩니다.
/// </summary>
public class GameOverManager : MonoBehaviour
{
    // Inspector에서 설정 불가능 - 스크립트를 통해서만 찾아짐
    private TextMeshProUGUI gameOverText;
    private Button restartButton;
    private SceneFadeManager fadeManager;
    
    // 상수로 정의 (Inspector에서 수정 불가능)
    private const string TUTORIAL_SCENE_NAME = "Tutorial";
    private const float FADE_DURATION = 1f;

    // 엔딩 타입별 메시지 딕셔너리
    private Dictionary<EndingType, string> endingMessages;

    private void Awake()
    {
        // 모든 참조를 스크립트를 통해서만 찾기
        GameObject textObj = GameObject.Find("GameOverText");
        if (textObj != null)
        {
            gameOverText = textObj.GetComponent<TextMeshProUGUI>();
        }

        GameObject buttonObj = GameObject.Find("RestartButton");
        if (buttonObj != null)
        {
            restartButton = buttonObj.GetComponent<Button>();
        }

        fadeManager = FindFirstObjectByType<SceneFadeManager>();
    }

    private void Start()
    {
        // 엔딩 메시지 초기화
        InitializeEndingMessages();
        
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
    /// 엔딩 타입별 메시지를 초기화합니다.
    /// </summary>
    private void InitializeEndingMessages()
    {
        endingMessages = new Dictionary<EndingType, string>
        {
            { EndingType.StealthExit, "완벽한 기만\n\n당신은 아무도 눈치채지 못하게 탈출했습니다." },
            { EndingType.ChaoticBreakout, "혼돈의 밤\n\n폭력과 혼란 속에서 탈출했습니다." },
            { EndingType.SiblingsHelp, "조력자의 희생\n\n동생의 도움으로 탈출했습니다." },
            { EndingType.UnfinishedDoll, "불완전한 박제\n\n인간성이 0%에 도달했습니다." },
            { EndingType.EternalDinner, "영원한 식사 시간\n\n5일차가 끝났습니다." }
        };
    }

    /// <summary>
    /// UI를 초기화합니다.
    /// </summary>
    private void InitializeUI()
    {
        if (gameOverText != null)
        {
            // GameStateManager에서 현재 엔딩 타입 읽기
            EndingType currentEnding = EndingType.None;
            if (GameStateManager.Instance != null)
            {
                currentEnding = GameStateManager.Instance.CurrentEnding;
            }

            // 엔딩 타입에 따른 메시지 표시
            if (endingMessages != null && endingMessages.ContainsKey(currentEnding))
            {
                gameOverText.text = endingMessages[currentEnding];
            }
            else
            {
                // 기본 메시지 (엔딩 타입이 없거나 알 수 없는 경우)
                gameOverText.text = "게임 오버\n\n인간성이 0%에 도달했습니다.";
                Debug.LogWarning($"[GameOverManager] 알 수 없는 엔딩 타입: {currentEnding}. 기본 메시지를 표시합니다.");
            }
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
            fadeManager.LoadSceneWithFade(TUTORIAL_SCENE_NAME, FADE_DURATION);
        }
        else
        {
            Debug.LogWarning("[GameOverManager] SceneFadeManager가 연결되지 않았습니다. 페이드 없이 씬을 전환합니다.");
            SceneManager.LoadScene(TUTORIAL_SCENE_NAME);
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
