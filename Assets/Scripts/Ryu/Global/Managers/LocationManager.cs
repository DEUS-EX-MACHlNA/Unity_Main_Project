using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 및 NPC 위치를 관리하는 매니저입니다.
/// </summary>
public class LocationManager
{
    private GameLocation currentLocation;
    private NPCManager npcManager;

    /// <summary>
    /// 위치 변경 시 호출되는 이벤트입니다.
    /// </summary>
    public event System.Action<GameLocation> OnLocationChanged;

    /// <summary>
    /// 초기화합니다.
    /// </summary>
    public void Initialize(NPCManager npcMgr)
    {
        npcManager = npcMgr;
    }

    /// <summary>
    /// 플레이어의 현재 위치를 설정합니다.
    /// </summary>
    public void SetCurrentLocation(GameLocation location)
    {
        if (currentLocation == location)
            return;
        
        GameLocation oldLocation = currentLocation;
        currentLocation = location;
        
        OnLocationChanged?.Invoke(location);
        Debug.Log($"[LocationManager] 위치 변경: {oldLocation} → {location}");
    }

    /// <summary>
    /// 플레이어의 현재 위치를 반환합니다.
    /// </summary>
    public GameLocation GetCurrentLocation()
    {
        return currentLocation;
    }

    /// <summary>
    /// NPC의 현재 위치를 설정합니다.
    /// </summary>
    public void SetNPCLocation(NPCType npc, GameLocation location)
    {
        if (npcManager != null)
        {
            npcManager.SetNPCLocation(npc, location);
        }
    }

    /// <summary>
    /// NPC의 현재 위치를 반환합니다.
    /// </summary>
    public GameLocation GetNPCLocation(NPCType npc)
    {
        if (npcManager != null)
        {
            return npcManager.GetNPCLocation(npc);
        }
        return GameLocation.Hallway;
    }
}

