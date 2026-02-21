using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NPC 상태를 관리하는 매니저입니다.
/// </summary>
public class NPCManager
{
    private Dictionary<NPCType, NPCStatus> npcStatuses;
    private Dictionary<NPCType, GameLocation> npcLocations;

    /// <summary>
    /// NPC 상태 변경 시 호출되는 이벤트입니다.
    /// </summary>
    public event System.Action<NPCType, NPCStatus> OnNPCStatusChanged;

    /// <summary>
    /// 초기화합니다.
    /// </summary>
    public void Initialize()
    {
        npcStatuses = new Dictionary<NPCType, NPCStatus>();
        npcLocations = new Dictionary<NPCType, GameLocation>();
        
        InitializeNPCStatuses();
        InitializeNPCLocations();
    }

    /// <summary>
    /// NPC 상태를 초기화합니다.
    /// </summary>
    private void InitializeNPCStatuses()
    {
        // 새엄마 (엘리노어) - 최종보스, 인간성 불가
        npcStatuses[NPCType.NewMother] = new NPCStatus
        {
            npcType = NPCType.NewMother,
            affection = 50f,
            humanity = -999f,
            isAvailable = true,
            isDisabled = false,
            disabledRemainingTurns = 0,
            disabledReason = ""
        };
        
        // 새아빠 (아더)
        npcStatuses[NPCType.NewFather] = new NPCStatus
        {
            npcType = NPCType.NewFather,
            affection = 50f,
            humanity = -50f,
            isAvailable = true,
            isDisabled = false,
            disabledRemainingTurns = 0,
            disabledReason = ""
        };
        
        // 동생 (루카스)
        npcStatuses[NPCType.Sibling] = new NPCStatus
        {
            npcType = NPCType.Sibling,
            affection = 50f,
            humanity = -20f,
            isAvailable = true,
            isDisabled = false,
            disabledRemainingTurns = 0,
            disabledReason = ""
        };
        
        // 강아지 (바론)
        npcStatuses[NPCType.Dog] = new NPCStatus
        {
            npcType = NPCType.Dog,
            affection = 80f,
            humanity = 0f,
            isAvailable = true,
            isDisabled = false,
            disabledRemainingTurns = 0,
            disabledReason = ""
        };
        
        // 할머니 (마가렛)
        npcStatuses[NPCType.Grandmother] = new NPCStatus
        {
            npcType = NPCType.Grandmother,
            affection = 0f,
            humanity = -999f,
            isAvailable = false,
            isDisabled = false,
            disabledRemainingTurns = 0,
            disabledReason = ""
        };
    }

    /// <summary>
    /// NPC 위치를 초기화합니다.
    /// </summary>
    private void InitializeNPCLocations()
    {
        npcLocations[NPCType.NewMother] = GameLocation.Kitchen;
        npcLocations[NPCType.NewFather] = GameLocation.LivingRoom;
        npcLocations[NPCType.Sibling] = GameLocation.SiblingsRoom;
        npcLocations[NPCType.Dog] = GameLocation.Backyard;
        npcLocations[NPCType.Grandmother] = GameLocation.Basement;
    }

    /// <summary>
    /// NPC 호감도를 변경합니다.
    /// </summary>
    public void ModifyAffection(NPCType npc, float changeAmount)
    {
        if (!npcStatuses.ContainsKey(npc))
        {
            Debug.LogWarning($"[NPCManager] NPC 상태를 찾을 수 없습니다: {npc}");
            return;
        }
        
        NPCStatus status = npcStatuses[npc];
        float oldAffection = status.affection;
        status.affection = Mathf.Clamp(status.affection + changeAmount, 0f, 100f);
        
        OnNPCStatusChanged?.Invoke(npc, status);
        Debug.Log($"[NPCManager] {npc} 호감도 변경: {oldAffection:F1} → {status.affection:F1} (변화량: {changeAmount:F1})");
    }

    /// <summary>
    /// NPC 호감도를 반환합니다.
    /// </summary>
    public float GetAffection(NPCType npc)
    {
        if (!npcStatuses.ContainsKey(npc))
        {
            Debug.LogWarning($"[NPCManager] NPC 상태를 찾을 수 없습니다: {npc}");
            return 0f;
        }
        
        return npcStatuses[npc].affection;
    }

    /// <summary>
    /// NPC 인간성을 변경합니다. 새엄마는 변경 불가합니다.
    /// </summary>
    public void ModifyNPCHumanity(NPCType npc, float changeAmount)
    {
        if (npc == NPCType.NewMother)
        {
            Debug.LogWarning("[NPCManager] 새엄마의 인간성은 변경할 수 없습니다.");
            return;
        }
        
        if (!npcStatuses.ContainsKey(npc))
        {
            Debug.LogWarning($"[NPCManager] NPC 상태를 찾을 수 없습니다: {npc}");
            return;
        }
        
        NPCStatus status = npcStatuses[npc];
        float oldHumanity = status.humanity;
        status.humanity = Mathf.Clamp(status.humanity + changeAmount, -100f, 100f);
        
        OnNPCStatusChanged?.Invoke(npc, status);
        Debug.Log($"[NPCManager] {npc} 인간성 변경: {oldHumanity:F1} → {status.humanity:F1} (변화량: {changeAmount:F1})");
    }

    /// <summary>
    /// NPC 인간성을 반환합니다.
    /// </summary>
    public float GetNPCHumanity(NPCType npc)
    {
        if (!npcStatuses.ContainsKey(npc))
        {
            Debug.LogWarning($"[NPCManager] NPC 상태를 찾을 수 없습니다: {npc}");
            return 0f;
        }
        
        return npcStatuses[npc].humanity;
    }

    /// <summary>
    /// NPC의 전체 상태를 반환합니다.
    /// </summary>
    public NPCStatus GetNPCStatus(NPCType npc)
    {
        if (!npcStatuses.ContainsKey(npc))
        {
            Debug.LogWarning($"[NPCManager] NPC 상태를 찾을 수 없습니다: {npc}");
            return null;
        }
        
        // 복사본 반환
        NPCStatus original = npcStatuses[npc];
        return new NPCStatus
        {
            npcType = original.npcType,
            affection = original.affection,
            humanity = original.humanity,
            isAvailable = original.isAvailable,
            isDisabled = original.isDisabled,
            disabledRemainingTurns = original.disabledRemainingTurns,
            disabledReason = original.disabledReason
        };
    }

    /// <summary>
    /// NPC를 무력화 상태로 설정합니다.
    /// </summary>
    public void SetNPCDisabled(NPCType npc, int turns, string reason)
    {
        if (!npcStatuses.ContainsKey(npc))
        {
            Debug.LogWarning($"[NPCManager] NPC 상태를 찾을 수 없습니다: {npc}");
            return;
        }
        
        NPCStatus status = npcStatuses[npc];
        status.isDisabled = true;
        status.disabledRemainingTurns = turns;
        status.disabledReason = reason;
        status.isAvailable = false;
        
        OnNPCStatusChanged?.Invoke(npc, status);
        Debug.Log($"[NPCManager] {npc} 무력화: {turns}턴 동안 ({reason})");
    }

    /// <summary>
    /// NPC 무력화 상태를 즉시 해제합니다. (에디터/테스트용)
    /// </summary>
    public void ClearNPCDisabled(NPCType npc)
    {
        if (!npcStatuses.ContainsKey(npc))
        {
            Debug.LogWarning($"[NPCManager] NPC 상태를 찾을 수 없습니다: {npc}");
            return;
        }

        NPCStatus status = npcStatuses[npc];
        if (!status.isDisabled)
            return;

        status.isDisabled = false;
        status.disabledRemainingTurns = 0;
        status.disabledReason = "";
        status.isAvailable = true;
        OnNPCStatusChanged?.Invoke(npc, status);
        Debug.Log($"[NPCManager] {npc} 무력화 해제 (수동)");
    }

    /// <summary>
    /// 턴 경과에 따라 NPC 무력화 상태를 업데이트합니다.
    /// </summary>
    public void UpdateNPCDisabledStates()
    {
        foreach (var kvp in npcStatuses)
        {
            NPCStatus status = kvp.Value;
            if (status.isDisabled && status.disabledRemainingTurns > 0)
            {
                status.disabledRemainingTurns--;
                
                if (status.disabledRemainingTurns <= 0)
                {
                    status.isDisabled = false;
                    status.disabledRemainingTurns = 0;
                    status.disabledReason = "";
                    status.isAvailable = true;
                    Debug.Log($"[NPCManager] {kvp.Key} 무력화 해제 (남은 턴 0)");
                }
                
                OnNPCStatusChanged?.Invoke(kvp.Key, status);
            }
        }
    }

    /// <summary>
    /// NPC의 현재 위치를 설정합니다.
    /// </summary>
    public void SetNPCLocation(NPCType npc, GameLocation location)
    {
        if (!npcLocations.ContainsKey(npc))
        {
            npcLocations[npc] = location;
        }
        else
        {
            GameLocation oldLocation = npcLocations[npc];
            npcLocations[npc] = location;
            
            if (oldLocation != location)
            {
                Debug.Log($"[NPCManager] {npc} 위치 변경: {oldLocation} → {location}");
            }
        }
    }

    /// <summary>
    /// NPC의 현재 위치를 반환합니다.
    /// </summary>
    public GameLocation GetNPCLocation(NPCType npc)
    {
        if (!npcLocations.ContainsKey(npc))
        {
            Debug.LogWarning($"[NPCManager] NPC 위치를 찾을 수 없습니다: {npc}");
            return GameLocation.Hallway;
        }
        
        return npcLocations[npc];
    }
}

