using UnityEngine;

/// <summary>
/// 게임 날짜를 관리하는 매니저입니다.
/// </summary>
public class DayManager
{
    private int currentDay = 1;
    private const int MAX_DAY = 5;

    /// <summary>
    /// 날짜가 변경될 때 호출되는 이벤트입니다. (새로운 날짜)
    /// </summary>
    public event System.Action<int> OnDayChanged;

    /// <summary>
    /// 현재 날짜를 반환합니다 (1~5일차).
    /// </summary>
    public int CurrentDay 
    { 
        get { return currentDay; } 
        private set { currentDay = Mathf.Clamp(value, 1, MAX_DAY); }
    }

    /// <summary>
    /// 현재 날짜를 반환합니다.
    /// </summary>
    public int GetCurrentDay()
    {
        return CurrentDay;
    }

    /// <summary>
    /// 최대 일수를 반환합니다.
    /// </summary>
    public int GetMaxDay()
    {
        return MAX_DAY;
    }

    /// <summary>
    /// 다음 날로 진행합니다.
    /// </summary>
    /// <returns>최대 일수에 도달했으면 true, 아니면 false</returns>
    public bool AdvanceToNextDay()
    {
        if (currentDay < MAX_DAY)
        {
            currentDay++;
            CurrentDay = currentDay;
            
            OnDayChanged?.Invoke(CurrentDay);
            Debug.Log($"[DayManager] 다음 날로 진행: {CurrentDay}일차 (최대 {MAX_DAY}일차)");
            return false;
        }
        else
        {
            Debug.LogWarning($"[DayManager] 최대 일수({MAX_DAY}일)에 도달했습니다.");
            return true;
        }
    }

    /// <summary>
    /// 초기값을 설정합니다.
    /// </summary>
    public void Initialize(int initialDay = 1)
    {
        currentDay = initialDay;
        CurrentDay = currentDay;
    }
}

