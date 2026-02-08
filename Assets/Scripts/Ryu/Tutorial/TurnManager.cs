using UnityEngine;
using TMPro;

/// <summary>
/// 남은 턴수를 관리하고 HUD_TopLeft의 TurnsText에 표시합니다.
/// </summary>
public class TurnManager : MonoBehaviour
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

    private void Start()
    {
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
            Debug.Log($"[TurnManager] 턴 소모. 남은 턴수: {remainingTurns}");

            // 턴수가 0이 되면 Night 씬으로 전환
            if (remainingTurns == 0)
            {
                Debug.Log("[TurnManager] 턴수가 0이 되었습니다. Night 씬으로 전환합니다.");
                if (fadeManager != null)
                {
                    fadeManager.LoadSceneWithFade(nightSceneName, fadeDuration);
                }
                else
                {
                    Debug.LogWarning("[TurnManager] SceneFadeManager가 연결되지 않았습니다. 페이드 없이 씬을 전환합니다.");
                    UnityEngine.SceneManagement.SceneManager.LoadScene(nightSceneName);
                }
            }
        }
        else
        {
            Debug.LogWarning("[TurnManager] 남은 턴수가 0입니다.");
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
        Debug.Log($"[TurnManager] 턴수 설정: {remainingTurns}");
    }

    /// <summary>
    /// 턴수를 초기값으로 리셋합니다.
    /// </summary>
    public void ResetTurns()
    {
        remainingTurns = initialTurns;
        UpdateTurnsDisplay();
        Debug.Log($"[TurnManager] 턴수 리셋: {remainingTurns}");
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
}
