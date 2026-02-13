using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 이벤트 플래그를 관리하는 매니저입니다.
/// </summary>
public class EventFlagManager
{
    private EventFlags eventFlags;

    /// <summary>
    /// 초기화합니다.
    /// </summary>
    public void Initialize()
    {
        eventFlags = new EventFlags
        {
            grandmotherCooperation = false,
            holeUnlocked = false,
            fireStarted = false,
            familyAsleep = false,
            teaWithSleepingPill = false,
            keyStolen = false,
            customEvents = new Dictionary<string, bool>()
        };
    }

    /// <summary>
    /// 이벤트 플래그를 설정합니다.
    /// </summary>
    public void SetEventFlag(string flagName, bool value)
    {
        switch (flagName.ToLower())
        {
            case "grandmothercooperation":
                eventFlags.grandmotherCooperation = value;
                break;
            case "holeunlocked":
                eventFlags.holeUnlocked = value;
                break;
            case "firestarted":
                eventFlags.fireStarted = value;
                break;
            case "familyasleep":
                eventFlags.familyAsleep = value;
                break;
            case "teawithsleepingpill":
                eventFlags.teaWithSleepingPill = value;
                break;
            case "keystolen":
                eventFlags.keyStolen = value;
                break;
            default:
                Debug.LogWarning($"[EventFlagManager] 알 수 없는 플래그 이름: {flagName}");
                return;
        }
        
        Debug.Log($"[EventFlagManager] 이벤트 플래그 설정: {flagName} = {value}");
    }

    /// <summary>
    /// 이벤트 플래그 값을 반환합니다.
    /// </summary>
    public bool GetEventFlag(string flagName)
    {
        switch (flagName.ToLower())
        {
            case "grandmothercooperation":
                return eventFlags.grandmotherCooperation;
            case "holeunlocked":
                return eventFlags.holeUnlocked;
            case "firestarted":
                return eventFlags.fireStarted;
            case "familyasleep":
                return eventFlags.familyAsleep;
            case "teawithsleepingpill":
                return eventFlags.teaWithSleepingPill;
            case "keystolen":
                return eventFlags.keyStolen;
            default:
                Debug.LogWarning($"[EventFlagManager] 알 수 없는 플래그 이름: {flagName}");
                return false;
        }
    }

    /// <summary>
    /// 커스텀 이벤트를 설정합니다.
    /// </summary>
    public void SetCustomEvent(string eventName, bool value)
    {
        if (eventFlags.customEvents == null)
        {
            eventFlags.customEvents = new Dictionary<string, bool>();
        }
        
        eventFlags.customEvents[eventName] = value;
        Debug.Log($"[EventFlagManager] 커스텀 이벤트 설정: {eventName} = {value}");
    }

    /// <summary>
    /// 커스텀 이벤트 값을 반환합니다.
    /// </summary>
    public bool GetCustomEvent(string eventName)
    {
        if (eventFlags.customEvents == null || !eventFlags.customEvents.ContainsKey(eventName))
        {
            return false;
        }
        
        return eventFlags.customEvents[eventName];
    }

    /// <summary>
    /// EventFlags 인스턴스를 반환합니다.
    /// </summary>
    public EventFlags GetEventFlags()
    {
        return eventFlags;
    }
}

