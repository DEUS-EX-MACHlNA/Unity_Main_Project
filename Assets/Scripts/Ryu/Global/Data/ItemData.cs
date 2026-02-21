using System;

/// <summary>
/// 아이템 위치 정보
/// </summary>
[Serializable]
public class ItemLocation
{
    public GameLocation location;   // 게임 구역
    public string locationId;      // 구체적 위치 식별자 (예: "Hallway_PhotoFrame", "Garden_DogHouse")
}

/// <summary>
/// 월드 아이템 상태
/// </summary>
[Serializable]
public class WorldItemState
{
    public ItemType itemType;
    public ItemState state;
    public ItemLocation location;   // null이면 여러 위치 가능 또는 위치 불명
}

