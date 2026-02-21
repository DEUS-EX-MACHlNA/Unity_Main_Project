using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 월드 아이템 상태를 관리하는 매니저입니다.
/// </summary>
public class ItemStateManager
{
    private Dictionary<ItemType, WorldItemState> worldItemStates;
    private InventoryManager inventoryManager;

    /// <summary>
    /// 아이템 상태 변경 시 호출되는 이벤트입니다.
    /// </summary>
    public event System.Action<ItemType, WorldItemState> OnItemStateChanged;

    /// <summary>
    /// 초기화합니다.
    /// </summary>
    public void Initialize(InventoryManager inventoryMgr)
    {
        inventoryManager = inventoryMgr;
        worldItemStates = new Dictionary<ItemType, WorldItemState>();
        InitializeWorldItemStates();
    }

    /// <summary>
    /// 월드 아이템 상태를 초기화합니다.
    /// </summary>
    private void InitializeWorldItemStates()
    {
        // 수면제 - 주방 찬장
        worldItemStates[ItemType.SleepingPills] = new WorldItemState
        {
            itemType = ItemType.SleepingPills,
            state = ItemState.InWorld,
            location = new ItemLocation
            {
                location = GameLocation.Kitchen,
                sceneName = "Tutorial",
                locationId = "Kitchen_Cabinet"
            },
            isRespawnable = false
        };
        
        // 홍차 - 주방 식탁 (매일 리스폰)
        worldItemStates[ItemType.EarlGreyTea] = new WorldItemState
        {
            itemType = ItemType.EarlGreyTea,
            state = ItemState.InWorld,
            location = new ItemLocation
            {
                location = GameLocation.Kitchen,
                sceneName = "Tutorial",
                locationId = "Kitchen_Table"
            },
            isRespawnable = true
        };
        
        // 진짜 가족 사진 - 뒷마당 개집 근처
        worldItemStates[ItemType.RealFamilyPhoto] = new WorldItemState
        {
            itemType = ItemType.RealFamilyPhoto,
            state = ItemState.InWorld,
            location = new ItemLocation
            {
                location = GameLocation.Backyard,
                sceneName = "Tutorial",
                locationId = "Backyard_DogHouse"
            },
            isRespawnable = false
        };
        
        // 고래기름 통 - 지하실 수술대 아래
        worldItemStates[ItemType.WhaleOilCan] = new WorldItemState
        {
            itemType = ItemType.WhaleOilCan,
            state = ItemState.InWorld,
            location = new ItemLocation
            {
                location = GameLocation.Basement,
                sceneName = "Tutorial",
                locationId = "Basement_SurgeryTable"
            },
            isRespawnable = false
        };
        
        // 은색 라이터 - 거실 소파 틈새
        worldItemStates[ItemType.SilverLighter] = new WorldItemState
        {
            itemType = ItemType.SilverLighter,
            state = ItemState.InWorld,
            location = new ItemLocation
            {
                location = GameLocation.LivingRoom,
                sceneName = "Tutorial",
                locationId = "LivingRoom_Sofa"
            },
            isRespawnable = false
        };
        
        // 동생의 장난감 - 동생의 방 인형의 집 모형
        worldItemStates[ItemType.OldRobotToy] = new WorldItemState
        {
            itemType = ItemType.OldRobotToy,
            state = ItemState.InWorld,
            location = new ItemLocation
            {
                location = GameLocation.SiblingsRoom,
                sceneName = "Tutorial",
                locationId = "SiblingsRoom_DollHouse"
            },
            isRespawnable = false
        };
        
        // 황동 열쇠 - 새엄마 목걸이 (항상 소지)
        worldItemStates[ItemType.BrassKey] = new WorldItemState
        {
            itemType = ItemType.BrassKey,
            state = ItemState.InWorld,
            location = new ItemLocation
            {
                location = GameLocation.Kitchen,
                sceneName = "Tutorial",
                locationId = "NewMother_Necklace"
            },
            isRespawnable = false
        };
    }

    /// <summary>
    /// 아이템의 상태를 설정합니다.
    /// </summary>
    public void SetItemState(ItemType item, ItemState state, ItemLocation location = null)
    {
        if (!worldItemStates.ContainsKey(item))
        {
            Debug.LogWarning($"[ItemStateManager] 월드 아이템 상태를 찾을 수 없습니다: {item}");
            return;
        }
        
        WorldItemState itemState = worldItemStates[item];
        ItemState oldState = itemState.state;
        itemState.state = state;
        
        if (location != null)
        {
            itemState.location = location;
        }
        
        worldItemStates[item] = itemState;
        
        OnItemStateChanged?.Invoke(item, itemState);
        Debug.Log($"[ItemStateManager] 아이템 상태 변경: {item} {oldState} → {state}");
    }

    /// <summary>
    /// 아이템의 현재 상태를 반환합니다.
    /// </summary>
    public ItemState GetItemState(ItemType item)
    {
        if (!worldItemStates.ContainsKey(item))
        {
            Debug.LogWarning($"[ItemStateManager] 월드 아이템 상태를 찾을 수 없습니다: {item}");
            return ItemState.InWorld;
        }
        
        return worldItemStates[item].state;
    }

    /// <summary>
    /// 매일 리스폰되는 아이템을 처리합니다.
    /// </summary>
    public void RespawnDailyItems(int currentDay)
    {
        foreach (var kvp in worldItemStates)
        {
            WorldItemState itemState = kvp.Value;
            
            // 리스폰 가능하고 인벤토리에 없거나 사용된 경우 리스폰
            if (itemState.isRespawnable && 
                (itemState.state == ItemState.Used || 
                 (!inventoryManager.HasItem(kvp.Key) && itemState.state != ItemState.InWorld)))
            {
                itemState.state = ItemState.InWorld;
                worldItemStates[kvp.Key] = itemState;
                
                OnItemStateChanged?.Invoke(kvp.Key, itemState);
                Debug.Log($"[ItemStateManager] 아이템 리스폰: {kvp.Key}");
            }
        }
    }

    /// <summary>
    /// 아이템이 인벤토리에 추가될 때 상태를 업데이트합니다.
    /// </summary>
    public void OnItemAddedToInventory(ItemType item)
    {
        if (worldItemStates.ContainsKey(item))
        {
            WorldItemState itemState = worldItemStates[item];
            if (itemState.state == ItemState.InWorld)
            {
                itemState.state = ItemState.InInventory;
                worldItemStates[item] = itemState;
                OnItemStateChanged?.Invoke(item, itemState);
            }
        }
    }
}

