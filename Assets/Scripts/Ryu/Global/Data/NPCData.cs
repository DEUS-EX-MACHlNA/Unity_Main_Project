using System;

/// <summary>
/// NPC 상태 정보
/// </summary>
[Serializable]
public class NPCStatus
{
    public NPCType npcType;
    public float affection;         // 호감도 (0~100)
    public float humanity;          // NPC 인간성 (-100~100, 새엄마는 -100 고정)
    public bool isAvailable;       // 대화 가능 여부
    public bool isDisabled;        // 무력화 상태 (수면제 등)
    public int disabledRemainingTurns; // 무력화 남은 턴 수
    public string disabledReason;  // 무력화 이유
}

