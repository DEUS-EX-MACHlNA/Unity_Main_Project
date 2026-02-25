using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameOverManager : MonoBehaviour
{
    private TextMeshProUGUI gameOverText;
    private Button restartButton;
    private Button exitButton;
    private SceneFadeManager fadeManager;
    
    private const string PLAYERS_ROOM_SCENE_NAME = "PlayersRoom";
    private const float FADE_DURATION = 1f;

    private Dictionary<EndingType, string> endingMessages;

    private void Awake()
    {
        GameObject textObj = GameObject.Find("GameOverText");
        if (textObj != null)
            gameOverText = textObj.GetComponent<TextMeshProUGUI>();

        GameObject buttonObj = GameObject.Find("RestartButton");
        if (buttonObj != null)
            restartButton = buttonObj.GetComponent<Button>();

        GameObject exitObj = GameObject.Find("ExitButton");
        if (exitObj != null)
            exitButton = exitObj.GetComponent<Button>();

        fadeManager = FindFirstObjectByType<SceneFadeManager>();
    }

    private void Start()
    {
        InitializeEndingMessages();
        InitializeUI();

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        else
            Debug.LogWarning("[GameOverManager] RestartButton을 찾을 수 없습니다.");

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitButtonClicked);
        else
            Debug.LogWarning("[GameOverManager] ExitButton을 찾을 수 없습니다.");
    }

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

    private void InitializeUI()
    {
        if (gameOverText != null)
        {
            EndingType currentEnding = EndingType.None;
            if (GameStateManager.Instance != null)
                currentEnding = GameStateManager.Instance.CurrentEnding;

            if (endingMessages != null && endingMessages.ContainsKey(currentEnding))
                gameOverText.text = endingMessages[currentEnding];
            else
            {
                gameOverText.text = "게임 오버\n\n인간성이 0%에 도달했습니다.";
                Debug.LogWarning($"[GameOverManager] 알 수 없는 엔딩 타입: {currentEnding}. 기본 메시지를 표시합니다.");
            }
        }
    }

    private void OnRestartButtonClicked()
    {
        Debug.Log("[GameOverManager] 게임 재시작");
        ResetGameState();

        if (fadeManager != null)
            fadeManager.LoadSceneWithFade(PLAYERS_ROOM_SCENE_NAME, FADE_DURATION);
        else
        {
            Debug.LogWarning("[GameOverManager] SceneFadeManager가 연결되지 않았습니다. 페이드 없이 씬을 전환합니다.");
            SceneManager.LoadScene(PLAYERS_ROOM_SCENE_NAME);
        }
    }

    private void OnExitButtonClicked()
    {
        Debug.Log("[GameOverManager] 게임 종료");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ResetGameState()
    {
        Debug.Log("[GameOverManager] GameStateManager 재생성 시작");
        GameStateManager.ClearInstance();

        GameObject gameStateManagerObj = new GameObject("GameStateManager");
        GameStateManager newManager = gameStateManagerObj.AddComponent<GameStateManager>();
        DontDestroyOnLoad(gameStateManagerObj);
        
#if UNITY_EDITOR
        VerifyGameStateManagerRecreation();
#else
        if (GameStateManager.Instance != null)
            Debug.Log($"[GameOverManager] GameStateManager 재생성 완료 - Instance ID: {GameStateManager.Instance.GetInstanceID()}");
        else
            Debug.LogWarning("[GameOverManager] GameStateManager 재생성 후 Instance가 null입니다.");
#endif
    }

#if UNITY_EDITOR
    private void VerifyGameStateManagerRecreation()
    {
        if (GameStateManager.Instance != null)
        {
            Debug.Log($"[GameOverManager] GameStateManager 재생성 완료 - Instance ID: {GameStateManager.Instance.GetInstanceID()}");
            Debug.Log($"[GameOverManager] 초기 인간성: {GameStateManager.Instance.GetHumanity()}%");
            Debug.Log($"[GameOverManager] 초기 날짜: {GameStateManager.Instance.GetCurrentDay()}일차");
        }
        else
            Debug.LogWarning("[GameOverManager] GameStateManager 재생성 후 Instance가 null입니다.");
    }
#endif
}