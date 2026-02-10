using UnityEngine;

/// <summary>
/// 이벤트 플래그 적용을 담당하는 클래스입니다.
/// 표준 이벤트 플래그 및 커스텀 이벤트를 처리합니다.
/// </summary>
public static class EventFlagApplier
{
    /// <summary>
    /// 이벤트 플래그를 적용합니다.
    /// </summary>
    /// <param name="manager">GameStateManager 인스턴스</param>
    /// <param name="flags">이벤트 플래그</param>
    public static void ApplyEventFlags(GameStateManager manager, EventFlags flags)
    {
        if (manager == null || flags == null)
            return;

        // 표준 이벤트 플래그 적용 (EventFlags 구조체는 bool 필드를 직접 가지고 있으므로 직접 사용)
        manager.SetEventFlag("grandmotherCooperation", flags.grandmotherCooperation);
        manager.SetEventFlag("holeUnlocked", flags.holeUnlocked);
        manager.SetEventFlag("fireStarted", flags.fireStarted);
        manager.SetEventFlag("familyAsleep", flags.familyAsleep);
        manager.SetEventFlag("teaWithSleepingPill", flags.teaWithSleepingPill);
        manager.SetEventFlag("keyStolen", flags.keyStolen);
        manager.SetEventFlag("caughtByFather", flags.caughtByFather);
        manager.SetEventFlag("caughtByMother", flags.caughtByMother);

        // 커스텀 이벤트 적용
        if (flags.customEvents != null)
        {
            foreach (var customEvent in flags.customEvents)
            {
                manager.SetCustomEvent(customEvent.Key, customEvent.Value);
            }
        }

        Debug.Log($"[EventFlagApplier] 이벤트 플래그 적용 완료");
    }
}

