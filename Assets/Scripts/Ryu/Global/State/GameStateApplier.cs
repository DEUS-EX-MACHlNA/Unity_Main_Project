using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 게임 상태 적용을 담당하는 클래스입니다.
/// 인간성 변경 및 엔딩 트리거 처리를 수행합니다.
/// </summary>
public static class GameStateApplier
{
    /// <summary>
    /// 플레이어 인간성 변화량을 적용합니다.
    /// </summary>
    /// <param name="manager">GameStateManager 인스턴스</param>
    /// <param name="change">인간성 변화량</param>
    public static void ApplyHumanityChange(GameStateManager manager, float change)
    {
        if (manager == null)
        {
            Debug.LogWarning("[GameStateApplier] GameStateManager가 null입니다.");
            return;
        }

        manager.ModifyHumanity(change);
        Debug.Log($"[GameStateApplier] 인간성 변화량 적용: {change:F1}");
    }

    /// <summary>
    /// 엔딩 트리거를 처리합니다.
    /// </summary>
    /// <param name="manager">GameStateManager 인스턴스</param>
    /// <param name="endingTrigger">엔딩 트리거 이름</param>
    /// <returns>엔딩이 트리거되었으면 true, 그렇지 않으면 false</returns>
    public static bool ApplyEndingTrigger(GameStateManager manager, string endingTrigger)
    {
        if (manager == null)
        {
            Debug.LogWarning("[GameStateApplier] GameStateManager가 null입니다.");
            return false;
        }

        if (string.IsNullOrEmpty(endingTrigger))
            return false;

        EndingType endingType = NameMapper.ConvertEndingNameToType(endingTrigger);
        if (endingType != EndingType.None)
        {
            manager.TriggerEnding(endingType);
            Debug.Log($"[GameStateApplier] 백엔드에서 엔딩 트리거 수신: {endingType}");
            return true;
        }

        return false;
    }
}

