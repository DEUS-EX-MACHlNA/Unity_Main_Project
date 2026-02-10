using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 백엔드 이름을 Unity enum으로 변환하는 정적 유틸리티 클래스입니다.
/// </summary>
public static class NameMapper
{
    // 아이템 이름 매핑
    private static Dictionary<string, ItemType> itemNameMapping = new Dictionary<string, ItemType>
    {
        { "sleeping_pill", ItemType.SleepingPill },
        { "earl_grey_tea", ItemType.EarlGreyTea },
        { "real_family_photo", ItemType.RealFamilyPhoto },
        { "oil_bottle", ItemType.OilBottle },
        { "silver_lighter", ItemType.SilverLighter },
        { "siblings_toy", ItemType.SiblingsToy },
        { "brass_key", ItemType.BrassKey }
    };

    // 위치 이름 매핑
    private static Dictionary<string, GameLocation> locationNameMapping = new Dictionary<string, GameLocation>
    {
        { "players_room", GameLocation.PlayersRoom },
        { "hallway", GameLocation.Hallway },
        { "living_room", GameLocation.LivingRoom },
        { "kitchen", GameLocation.Kitchen },
        { "siblings_room", GameLocation.SiblingsRoom },
        { "basement", GameLocation.Basement },
        { "backyard", GameLocation.Backyard }
    };

    // 엔딩 이름 매핑
    private static Dictionary<string, EndingType> endingNameMapping = new Dictionary<string, EndingType>
    {
        { "stealth_exit", EndingType.StealthExit },
        { "chaotic_breakout", EndingType.ChaoticBreakout },
        { "siblings_help", EndingType.SiblingsHelp },
        { "unfinished_doll", EndingType.UnfinishedDoll },
        { "eternal_dinner", EndingType.EternalDinner },
        { "none", EndingType.None }
    };

    // 아이템 상태 이름 매핑
    private static Dictionary<string, ItemState> itemStateNameMapping = new Dictionary<string, ItemState>
    {
        { "in_world", ItemState.InWorld },
        { "in_inventory", ItemState.InInventory },
        { "used", ItemState.Used }
    };

    /// <summary>
    /// 백엔드 아이템 이름을 ItemType enum으로 변환합니다.
    /// </summary>
    public static ItemType ConvertItemNameToType(string itemName)
    {
        if (string.IsNullOrEmpty(itemName))
            return ItemType.None;
        
        if (itemNameMapping.TryGetValue(itemName.ToLower(), out ItemType itemType))
            return itemType;
        
        Debug.LogWarning($"[NameMapper] 알 수 없는 아이템 이름: {itemName}");
        return ItemType.None;
    }

    /// <summary>
    /// 백엔드 위치 이름을 GameLocation enum으로 변환합니다.
    /// </summary>
    public static GameLocation ConvertLocationNameToType(string locationName)
    {
        if (string.IsNullOrEmpty(locationName))
            return GameLocation.Hallway; // 기본값
        
        if (locationNameMapping.TryGetValue(locationName.ToLower(), out GameLocation location))
            return location;
        
        Debug.LogWarning($"[NameMapper] 알 수 없는 위치 이름: {locationName}");
        return GameLocation.Hallway; // 기본값
    }

    /// <summary>
    /// 백엔드 엔딩 이름을 EndingType enum으로 변환합니다.
    /// </summary>
    public static EndingType ConvertEndingNameToType(string endingName)
    {
        if (string.IsNullOrEmpty(endingName))
            return EndingType.None;
        
        if (endingNameMapping.TryGetValue(endingName.ToLower(), out EndingType endingType))
            return endingType;
        
        Debug.LogWarning($"[NameMapper] 알 수 없는 엔딩 이름: {endingName}");
        return EndingType.None;
    }

    /// <summary>
    /// 백엔드 아이템 상태 이름을 ItemState enum으로 변환합니다.
    /// </summary>
    public static ItemState ConvertItemStateNameToType(string stateName)
    {
        if (string.IsNullOrEmpty(stateName))
            return ItemState.InWorld; // 기본값
        
        if (itemStateNameMapping.TryGetValue(stateName.ToLower(), out ItemState state))
            return state;
        
        Debug.LogWarning($"[NameMapper] 알 수 없는 아이템 상태 이름: {stateName}");
        return ItemState.InWorld; // 기본값
    }
}

