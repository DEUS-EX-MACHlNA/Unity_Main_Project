using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// 남은 턴수를 관리하고 HUD_TopLeft의 TurnsText에 표시합니다.
/// 모든 방 씬에서 공통으로 사용되는 TurnManager입니다.
/// </summary>
public class RoomTurnManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI turnsText;

    [Header("Turn Settings")]
    [SerializeField] private int initialTurns = 10;

    [Header("Scene Transition")]
    [SerializeField] private SceneFadeManager fadeManager;
    [SerializeField] private string nightSceneName = "Night";
    [SerializeField] private float fadeDuration = 1f;

    private int remainingTurns;
    private string currentSceneName;

    private void OnEnable()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnDayChanged += OnDayChanged;
        }
    }

    private void OnDisable()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnDayChanged -= OnDayChanged;
        }
    }

    private void Start()
    {
        // 현재 씬 이름 저장
        currentSceneName = SceneManager.GetActiveScene().name;
        
        remainingTurns = initialTurns;
        UpdateTurnsDisplay();
    }

    /// <summary>
    /// 남은 턴수를 1 차감합니다.
    /// </summary>
    public void ConsumeTurn()
    {
        if (remainingTurns > 0)
        {
            remainingTurns--;
            UpdateTurnsDisplay();
            Debug.Log($"[RoomTurnManager] 턴 소모. 남은 턴수: {remainingTurns}");

            // 턴이 지날 때마다 NPC 무력화 남은 턴 차감 및 0이면 해제
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.UpdateNPCDisabledStates();

            // 턴수가 0이 되면 Night 씬으로 전환
            if (remainingTurns == 0)
            {
                Debug.Log("[RoomTurnManager] 턴수가 0이 되었습니다. Night 씬으로 전환합니다.");
                if (fadeManager != null)
                {
                    fadeManager.LoadSceneWithFade(nightSceneName, fadeDuration);
                }
                else
                {
                    Debug.LogWarning("[RoomTurnManager] SceneFadeManager가 연결되지 않았습니다. 페이드 없이 씬을 전환합니다.");
                    SceneManager.LoadScene(nightSceneName);
                }
            }
        }
        else
        {
            Debug.LogWarning("[RoomTurnManager] 남은 턴수가 0입니다.");
        }
    }

    /// <summary>
    /// 남은 턴수를 반환합니다.
    /// </summary>
    public int GetRemainingTurns()
    {
        return remainingTurns;
    }

    /// <summary>
    /// 남은 턴수를 설정합니다.
    /// </summary>
    public void SetRemainingTurns(int turns)
    {
        remainingTurns = Mathf.Max(0, turns);
        UpdateTurnsDisplay();
        Debug.Log($"[RoomTurnManager] 턴수 설정: {remainingTurns}");
    }

    /// <summary>
    /// 턴수를 초기값으로 리셋합니다.
    /// </summary>
    public void ResetTurns()
    {
        remainingTurns = initialTurns;
        UpdateTurnsDisplay();
        Debug.Log($"[RoomTurnManager] 턴수 리셋: {remainingTurns}");
    }

    /// <summary>
    /// TurnsText UI를 업데이트합니다.
    /// </summary>
    private void UpdateTurnsDisplay()
    {
        if (turnsText != null)
        {
            turnsText.text = $"남은 턴수 : {remainingTurns}";
        }
    }

    private void OnDayChanged(int newDay)
    {
        Debug.Log($"[RoomTurnManager] 날짜 변경 감지: {newDay}일차. 턴수를 리셋합니다.");
        ResetTurns();
    }

    /// <summary>
    /// 새로운 날을 시작합니다. 날짜를 진행하고 턴수를 초기화하며 현재 씬으로 전환합니다.
    /// </summary>
    public void StartNewDay()
    {
        Debug.Log("[RoomTurnManager] 새로운 날 시작");

        // GameStateManager를 통해 다음 날 진행 (인간성 감소 포함)
        bool gameOverOccurred = false;
        if (GameStateManager.Instance != null)
        {
            gameOverOccurred = GameStateManager.Instance.AdvanceToNextDay();
        }
        else
        {
            Debug.LogWarning("[RoomTurnManager] GameStateManager.Instance를 찾을 수 없습니다.");
        }

        // 게임 오버가 발생했으면 씬 전환을 건너뜀 (TriggerGameOver에서 이미 처리됨)
        if (gameOverOccurred)
        {
            Debug.Log("[RoomTurnManager] 게임 오버가 발생했습니다. 씬 전환을 건너뜁니다.");
            return;
        }

        // 턴수 초기화 (OnDayChanged 이벤트로도 처리되지만 명시적으로 호출)
        ResetTurns();

        // 현재 씬 이름이 없으면 다시 가져오기
        if (string.IsNullOrEmpty(currentSceneName))
        {
            currentSceneName = SceneManager.GetActiveScene().name;
        }

        // 현재 씬으로 전환 (페이드 효과)
        if (fadeManager != null)
        {
            fadeManager.LoadSceneWithFade(currentSceneName, fadeDuration);
        }
        else
        {
            Debug.LogWarning("[RoomTurnManager] SceneFadeManager가 연결되지 않았습니다. 페이드 없이 씬을 전환합니다.");
            SceneManager.LoadScene(currentSceneName);
        }
    }
}

