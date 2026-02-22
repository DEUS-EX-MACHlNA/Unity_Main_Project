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

        // 씬 전환 후에도 턴 수 유지: GameStateManager(DontDestroyOnLoad)에서 동기화
        if (GameStateManager.Instance != null)
            remainingTurns = GameStateManager.Instance.GetRemainingTurns();
        else
            remainingTurns = initialTurns;
        UpdateTurnsDisplay();
    }

    /// <summary>
    /// 남은 턴수를 1 차감합니다. GameStateManager에 반영해 씬 전환 후에도 턴 수가 유지됩니다.
    /// </summary>
    public void ConsumeTurn()
    {
        if (GameStateManager.Instance == null)
        {
            // GameStateManager 없을 때만 로컬 턴 사용
            if (remainingTurns <= 0)
            {
                Debug.LogWarning("[RoomTurnManager] 남은 턴수가 0입니다.");
                return;
            }
            remainingTurns--;
            UpdateTurnsDisplay();
            Debug.Log($"[RoomTurnManager] 턴 소모. 남은 턴수: {remainingTurns}");
            ApplyTurnEndLogic();
            return;
        }

        if (!GameStateManager.Instance.ConsumeTurn(1))
        {
            Debug.LogWarning("[RoomTurnManager] 남은 턴수가 0입니다.");
            return;
        }

        remainingTurns = GameStateManager.Instance.GetRemainingTurns();
        UpdateTurnsDisplay();
        Debug.Log($"[RoomTurnManager] 턴 소모. 남은 턴수: {remainingTurns}");
        ApplyTurnEndLogic();
    }

    /// <summary>
    /// 턴 종료 시 공통 처리 (Night 씬 전환 등).
    /// NPC 무력화 차감은 GameStateManager.ConsumeTurn() 내부에서 이미 한 번 수행되므로 여기서 호출하지 않음.
    /// (중복 호출 시 disabled_remaining_turns가 턴당 2씩 줄어드는 버그 방지)
    /// </summary>
    private void ApplyTurnEndLogic()
    {
        if (remainingTurns == 0)
        {
            Debug.Log("[RoomTurnManager] 턴수가 0이 되었습니다. Night 씬으로 전환합니다.");
            if (fadeManager != null)
                fadeManager.LoadSceneWithFade(nightSceneName, fadeDuration);
            else
            {
                Debug.LogWarning("[RoomTurnManager] SceneFadeManager가 연결되지 않았습니다. 페이드 없이 씬을 전환합니다.");
                SceneManager.LoadScene(nightSceneName);
            }
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
    /// 턴수를 초기값으로 리셋합니다. GameStateManager가 있으면 해당 값과 동기화합니다.
    /// </summary>
    public void ResetTurns()
    {
        if (GameStateManager.Instance != null)
            remainingTurns = GameStateManager.Instance.GetRemainingTurns();
        else
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

