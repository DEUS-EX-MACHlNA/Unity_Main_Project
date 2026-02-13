using System;
using System.Collections.Generic;

/// <summary>
/// 엔딩 조건
/// </summary>
[Serializable]
public class EndingCondition
{
    public EndingType endingType;
    public bool hasRequiredItems;      // 필수 아이템 보유 여부
    public bool hasRequiredNPCStatus; // 필수 NPC 상태 달성 여부
    public bool hasRequiredLocation;   // 필수 장소 도달 여부
    public bool hasRequiredTime;       // 필수 시간대 여부
    // Unity JsonUtility는 Dictionary를 지원하지 않으므로, 직렬화가 필요한 경우 별도 처리 필요
    public Dictionary<string, bool> customFlags; // 커스텀 플래그 (예: "GrandmotherCooperation")
}

