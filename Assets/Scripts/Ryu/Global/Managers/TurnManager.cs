using UnityEngine;

/// <summary>
/// 턴 및 시간대를 관리하는 매니저입니다.
/// </summary>
public class TurnManager
{
    private TimeOfDay currentTimeOfDay = TimeOfDay.Day;
    private int currentTurn = 0;
    private const int MAX_TURNS_PER_DAY = 10;

    /// <summary>
    /// 시간대 변경 시 호출되는 이벤트입니다.
    /// </summary>
    public event System.Action<TimeOfDay> OnTimeOfDayChanged;

    /// <summary>
    /// 턴 수 변경 시 호출되는 이벤트입니다.
    /// </summary>
    public event System.Action<int> OnTurnChanged;

    /// <summary>
    /// 턴이 모두 소진되었을 때 호출되는 이벤트입니다.
    /// </summary>
    public event System.Action OnTurnsExhausted;

    /// <summary>
    /// 현재 시간대를 설정합니다.
    /// </summary>
    public void SetTimeOfDay(TimeOfDay time)
    {
        if (currentTimeOfDay == time)
            return;
        
        TimeOfDay oldTime = currentTimeOfDay;
        currentTimeOfDay = time;
        
        OnTimeOfDayChanged?.Invoke(time);
        Debug.Log($"[TurnManager] 시간대 변경: {oldTime} → {time}");
    }

    /// <summary>
    /// 현재 시간대를 반환합니다.
    /// </summary>
    public TimeOfDay GetCurrentTimeOfDay()
    {
        return currentTimeOfDay;
    }

    /// <summary>
    /// 턴을 소모합니다.
    /// </summary>
    public bool ConsumeTurn(int amount = 1)
    {
        if (currentTurn + amount > MAX_TURNS_PER_DAY)
        {
            Debug.LogWarning($"[TurnManager] 턴 수가 부족합니다. (현재: {currentTurn}, 최대: {MAX_TURNS_PER_DAY})");
            return false;
        }
        
        currentTurn += amount;
        OnTurnChanged?.Invoke(GetRemainingTurns());
        
        Debug.Log($"[TurnManager] 턴 소모: {amount} (사용된 턴: {currentTurn}/{MAX_TURNS_PER_DAY})");
        
        // 턴 소진 체크
        if (currentTurn >= MAX_TURNS_PER_DAY)
        {
            OnTurnsExhausted?.Invoke();
            SetTimeOfDay(TimeOfDay.Night);
        }
        
        return true;
    }

    /// <summary>
    /// 남은 턴 수를 반환합니다.
    /// </summary>
    public int GetRemainingTurns()
    {
        return Mathf.Max(0, MAX_TURNS_PER_DAY - currentTurn);
    }

    /// <summary>
    /// 남은 턴이 있는지 확인합니다.
    /// </summary>
    public bool HasRemainingTurns()
    {
        return GetRemainingTurns() > 0;
    }

    /// <summary>
    /// 턴 수를 리셋합니다.
    /// </summary>
    public void ResetTurns()
    {
        currentTurn = 0;
        OnTurnChanged?.Invoke(GetRemainingTurns());
    }
}

