# Phase 3: GameStateManager 아이템 시스템 구현

## 개요
아이템 인벤토리 시스템과 월드 아이템 상태 관리 시스템을 구현합니다. 아이템 획득, 사용, 매일 리스폰 등을 포함합니다.

## 목표
- 아이템 인벤토리 관리 메서드 구현 완료
- 월드 아이템 상태 관리 메서드 구현 완료
- 아이템 초기화 로직 구현 완료
- 아이템 관련 이벤트 발생 로직 구현 완료

## 작업 내용

### 1. 아이템 시스템 초기화

#### 1.1 Awake()에서 아이템 시스템 초기화
```csharp
private void InitializeInventory()
{
    inventory = new Dictionary<ItemType, int>();
    worldItemStates = new Dictionary<ItemType, WorldItemState>();
    
    // 월드 아이템 상태 초기화
    InitializeWorldItemStates();
}
```

#### 1.2 월드 아이템 상태 초기화
```csharp
private void InitializeWorldItemStates()
{
    // 수면제 - 주방 찬장
    worldItemStates[ItemType.SleepingPill] = new WorldItemState
    {
        itemType = ItemType.SleepingPill,
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
    worldItemStates[ItemType.OilBottle] = new WorldItemState
    {
        itemType = ItemType.OilBottle,
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
    worldItemStates[ItemType.SiblingsToy] = new WorldItemState
    {
        itemType = ItemType.SiblingsToy,
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
```

### 2. 인벤토리 관리 메서드

#### 2.1 AddItem
```csharp
/// <summary>
/// 인벤토리에 아이템을 추가합니다.
/// 백엔드에서 받은 아이템 획득 정보를 적용합니다.
/// </summary>
/// <param name="item">아이템 타입</param>
/// <param name="count">추가할 개수 (기본값: 1) - 백엔드에서 제공</param>
public void AddItem(ItemType item, int count = 1)
{
    if (count <= 0)
    {
        Debug.LogWarning($"[GameStateManager] 잘못된 아이템 개수: {count}");
        return;
    }
    
    if (!inventory.ContainsKey(item))
    {
        inventory[item] = 0;
    }
    
    int oldCount = inventory[item];
    inventory[item] += count;
    
    // 월드 아이템 상태 업데이트
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
    
    OnInventoryChanged?.Invoke(item, inventory[item]);
    Debug.Log($"[GameStateManager] 아이템 추가: {item} x{count} (총 {inventory[item]}개)");
}
```

#### 2.2 RemoveItem
```csharp
/// <summary>
/// 인벤토리에서 아이템을 제거합니다.
/// 백엔드에서 받은 아이템 사용/소모 정보를 적용합니다.
/// </summary>
/// <param name="item">아이템 타입</param>
/// <param name="count">제거할 개수 (기본값: 1) - 백엔드에서 제공</param>
/// <returns>제거 성공 여부</returns>
public bool RemoveItem(ItemType item, int count = 1)
{
    if (count <= 0)
    {
        Debug.LogWarning($"[GameStateManager] 잘못된 아이템 개수: {count}");
        return false;
    }
    
    if (!inventory.ContainsKey(item) || inventory[item] < count)
    {
        Debug.LogWarning($"[GameStateManager] 아이템이 부족합니다: {item} (보유: {inventory.GetValueOrDefault(item, 0)}, 요구: {count})");
        return false;
    }
    
    int oldCount = inventory[item];
    inventory[item] -= count;
    
    if (inventory[item] <= 0)
    {
        inventory.Remove(item);
    }
    
    OnInventoryChanged?.Invoke(item, inventory.GetValueOrDefault(item, 0));
    Debug.Log($"[GameStateManager] 아이템 제거: {item} x{count} (남은 개수: {inventory.GetValueOrDefault(item, 0)})");
    
    return true;
}
```

#### 2.3 HasItem
```csharp
/// <summary>
/// 인벤토리에 아이템이 있는지 확인합니다.
/// </summary>
/// <param name="item">아이템 타입</param>
/// <returns>보유 여부</returns>
public bool HasItem(ItemType item)
{
    return inventory.ContainsKey(item) && inventory[item] > 0;
}
```

#### 2.4 GetItemCount
```csharp
/// <summary>
/// 인벤토리에 있는 아이템의 개수를 반환합니다.
/// </summary>
/// <param name="item">아이템 타입</param>
/// <returns>아이템 개수</returns>
public int GetItemCount(ItemType item)
{
    return inventory.GetValueOrDefault(item, 0);
}
```

### 3. 월드 아이템 상태 관리 메서드

#### 3.1 SetItemState
```csharp
/// <summary>
/// 아이템의 상태를 설정합니다.
/// 백엔드에서 받은 아이템 상태 변경 정보를 적용합니다.
/// </summary>
/// <param name="item">아이템 타입</param>
/// <param name="state">새로운 상태 (백엔드에서 제공)</param>
/// <param name="location">위치 정보 (선택적)</param>
public void SetItemState(ItemType item, ItemState state, ItemLocation location = null)
{
    if (!worldItemStates.ContainsKey(item))
    {
        Debug.LogWarning($"[GameStateManager] 월드 아이템 상태를 찾을 수 없습니다: {item}");
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
    Debug.Log($"[GameStateManager] 아이템 상태 변경: {item} {oldState} → {state}");
}
```

#### 3.2 GetItemState
```csharp
/// <summary>
/// 아이템의 현재 상태를 반환합니다.
/// </summary>
/// <param name="item">아이템 타입</param>
/// <returns>아이템 상태</returns>
public ItemState GetItemState(ItemType item)
{
    if (!worldItemStates.ContainsKey(item))
    {
        Debug.LogWarning($"[GameStateManager] 월드 아이템 상태를 찾을 수 없습니다: {item}");
        return ItemState.InWorld;
    }
    
    return worldItemStates[item].state;
}
```

#### 3.3 RespawnDailyItems
```csharp
/// <summary>
/// 매일 리스폰되는 아이템을 처리합니다 (날짜 변경 시 호출).
/// </summary>
/// <param name="currentDay">현재 날짜</param>
public void RespawnDailyItems(int currentDay)
{
    foreach (var kvp in worldItemStates)
    {
        WorldItemState itemState = kvp.Value;
        
        // 리스폰 가능하고 인벤토리에 없거나 사용된 경우 리스폰
        if (itemState.isRespawnable && 
            (itemState.state == ItemState.Used || 
             (!HasItem(kvp.Key) && itemState.state != ItemState.InWorld)))
        {
            itemState.state = ItemState.InWorld;
            worldItemStates[kvp.Key] = itemState;
            
            OnItemStateChanged?.Invoke(kvp.Key, itemState);
            Debug.Log($"[GameStateManager] 아이템 리스폰: {kvp.Key}");
        }
    }
}
```

### 4. AdvanceToNextDay() 통합

날짜 변경 시 아이템 복구 및 리스폰 처리:
```csharp
public bool AdvanceToNextDay()
{
    if (currentDay < MAX_DAY)
    {
        currentDay++;
        CurrentDay = currentDay;
        
        // 인간성 감소
        float oldHumanity = humanity;
        ModifyHumanity(-10f);
        
        // 게임 오버 체크
        bool gameOverOccurred = humanity <= MIN_HUMANITY && oldHumanity > MIN_HUMANITY;
        
        if (!gameOverOccurred)
        {
            // 아이템 리스폰
            RespawnDailyItems(CurrentDay);
            
            OnDayChanged?.Invoke(CurrentDay);
            Debug.Log($"[GameStateManager] 다음 날로 진행: {CurrentDay}일차 (최대 {MAX_DAY}일차)");
        }
        
        return gameOverOccurred;
    }
    // ... 기존 코드 ...
}
```

### 5. 백엔드 API 연동

#### 5.1 ApiClient 확장 (GameResponse 구조체 수정)

`Assets/Scripts/Ryu/Global/ApiClient.cs`의 `GameResponse` 클래스를 확장하여 아이템 변화량을 받아옵니다:

**참고:** 구조체 정의는 Phase1 문서(2.6 섹션)를 참고하세요.

**Phase2에서 이미 추가된 NPC 변화량, NPC 무력화 상태, NPC 위치 필드와 함께 아이템 변화량도 추가:**

```csharp
[Serializable]
private class GameResponse
{
    public string response;
    public float humanity_change; // 플레이어 인간성 변화량
    
    // NPC 변화량 (Phase2에서 추가, 선택적)
    public NPCAffectionChanges npc_affection_changes;
    public NPCHumanityChanges npc_humanity_changes;
    public NPCDisabledStates npc_disabled_states;
    public NPCLocations npc_locations;
    
    // 아이템 변화량 (백엔드에서 제공, 선택적)
    public ItemChanges item_changes;
}

// Phase1 문서(2.6 섹션) 참고:
// - ItemAcquisition: 아이템 획득
// - ItemConsumption: 아이템 사용/소모
// - ItemStateChange: 아이템 상태 변경
// - ItemChanges: 아이템 변화량 통합 구조체
```

**구조체 정의 위치:**
- `ItemAcquisition`, `ItemConsumption`, `ItemStateChange`, `ItemChanges`는 Phase1 문서에 정의됨
- `GameResponse`는 `ApiClient.cs` 내부에 정의
- 또는 공통 구조체는 별도 파일로 분리 가능

#### 5.2 ApiClient SendMessage 메서드 확장

Phase2에서 확장한 `SendMessage` 메서드에 아이템 변화량도 추가:

**참고:** Phase4와 통합 시 최종 시그니처는 `Action<string, float, NPCAffectionChanges, NPCHumanityChanges, NPCDisabledStates, NPCLocations, ItemChanges, string>` (ending_trigger 포함)가 됩니다. Phase3에서는 아이템 변화량만 추가합니다.

```csharp
/// <summary>
/// 백엔드 서버에 메시지를 전송하고 응답을 콜백으로 반환합니다.
/// </summary>
/// <param name="chatInput">사용자 입력 텍스트</param>
/// <param name="onSuccess">성공 콜백 (response, humanityChange, npcAffectionChanges, npcHumanityChanges, npcDisabledStates, npcLocations, itemChanges)</param>
/// <param name="onError">에러 콜백</param>
public Coroutine SendMessage(
    string chatInput, 
    Action<string, float, NPCAffectionChanges, NPCHumanityChanges, NPCDisabledStates, NPCLocations, ItemChanges> onSuccess, 
    Action<string> onError)
{
    return StartCoroutine(SendMessageCoroutine(chatInput, onSuccess, onError));
}

private IEnumerator SendMessageCoroutine(
    string chatInput, 
    Action<string, float, NPCAffectionChanges, NPCHumanityChanges, NPCDisabledStates, NPCLocations, ItemChanges> onSuccess, 
    Action<string> onError)
{
    // ... 기존 요청 코드 ...
    
    try
    {
        GameResponse gameResponse = JsonUtility.FromJson<GameResponse>(responseText);
        
        string response = !string.IsNullOrEmpty(gameResponse.response) 
            ? gameResponse.response 
            : MOCK_RESPONSE;
        
        float humanityChange = gameResponse.humanity_change;
        
        // NPC 변화량 추출 (Phase2, null 체크 포함)
        NPCAffectionChanges affectionChanges = gameResponse.npc_affection_changes ?? new NPCAffectionChanges();
        NPCHumanityChanges humanityChanges = gameResponse.npc_humanity_changes ?? new NPCHumanityChanges();
        NPCDisabledStates disabledStates = gameResponse.npc_disabled_states;
        NPCLocations npcLocations = gameResponse.npc_locations;
        
        // 아이템 변화량 추출 (null 체크 포함, 없으면 새 인스턴스 생성)
        ItemChanges itemChanges = gameResponse.item_changes ?? new ItemChanges();
        
        onSuccess?.Invoke(response, humanityChange, affectionChanges, humanityChanges, disabledStates, npcLocations, itemChanges);
    }
    catch (Exception e)
    {
        Debug.LogError($"[ApiClient] JSON 파싱 에러: {e.Message}");
        onError?.Invoke($"응답 파싱 실패: {e.Message}");
    }
}
```

**폴백 전략:**
- `item_changes`: null이면 빈 인스턴스 생성 (모든 배열 null)
- `acquired_items`, `consumed_items`, `state_changes`: null이면 null로 전달 (이전 상태 유지)

#### 5.3 InputHandler 연동

`Assets/Scripts/Ryu/Tutorial/InputHandler.cs`의 `OnApiSuccess` 메서드를 수정하여 아이템 변화량을 처리:

```csharp
private void OnApiSuccess(
    string response, 
    float humanityChange, 
    NPCAffectionChanges npcAffectionChanges, 
    NPCHumanityChanges npcHumanityChanges,
    NPCDisabledStates npcDisabledStates,
    NPCLocations npcLocations,
    ItemChanges itemChanges)
{
    Debug.Log($"[InputHandler] 응답 수신: {response}");
    Debug.Log($"[InputHandler] 인간성 변화량: {humanityChange:F1}");

    // 응답 텍스트 표시
    resultText.text = response;

    // 플레이어 인간성 변화량 적용
    if (gameStateManager != null)
    {
        gameStateManager.ModifyHumanity(humanityChange);
        
        // NPC 변화량 적용 (Phase2)
        // ... NPC 처리 코드 ...
        
        // 아이템 변화량 적용
        if (itemChanges != null)
        {
            ApplyItemChanges(itemChanges);
        }
    }
    else
    {
        Debug.LogWarning("[InputHandler] GameStateManager가 연결되지 않았습니다.");
    }

    // 턴 소모
    if (turnManager != null)
    {
        turnManager.ConsumeTurn();
    }
}

// 아이템 변화량 적용 헬퍼 메서드
private void ApplyItemChanges(ItemChanges itemChanges)
{
    // 아이템 획득 처리
    if (itemChanges.acquired_items != null)
    {
        foreach (var acquisition in itemChanges.acquired_items)
        {
            ItemType itemType = ConvertItemNameToType(acquisition.item_name);
            if (itemType != ItemType.None) // None이 아닌 경우만 처리
            {
                gameStateManager.AddItem(itemType, acquisition.count);
                Debug.Log($"[InputHandler] 아이템 획득: {itemType} x{acquisition.count}");
            }
        }
    }
    
    // 아이템 사용/소모 처리
    if (itemChanges.consumed_items != null)
    {
        foreach (var consumption in itemChanges.consumed_items)
        {
            ItemType itemType = ConvertItemNameToType(consumption.item_name);
            if (itemType != ItemType.None)
            {
                gameStateManager.RemoveItem(itemType, consumption.count);
                Debug.Log($"[InputHandler] 아이템 사용/소모: {itemType} x{consumption.count}");
            }
        }
    }
    
    // 아이템 상태 변경 처리
    if (itemChanges.state_changes != null)
    {
        foreach (var stateChange in itemChanges.state_changes)
        {
            ItemType itemType = ConvertItemNameToType(stateChange.item_name);
            ItemState newState = ConvertItemStateNameToType(stateChange.new_state);
            if (itemType != ItemType.None)
            {
                gameStateManager.SetItemState(itemType, newState);
                Debug.Log($"[InputHandler] 아이템 상태 변경: {itemType} → {newState}");
            }
        }
    }
}

// 아이템 이름을 ItemType으로 변환하는 헬퍼 메서드
// (Phase1 문서 2.6.12 섹션의 아이템 이름 매핑 사용)
private ItemType ConvertItemNameToType(string itemName)
{
    // Phase1 문서의 itemNameMapping 딕셔너리 사용
    // 또는 직접 매핑
    switch (itemName?.ToLower())
    {
        case "sleeping_pill": return ItemType.SleepingPill;
        case "earl_grey_tea": return ItemType.EarlGreyTea;
        case "real_family_photo": return ItemType.RealFamilyPhoto;
        case "oil_bottle": return ItemType.OilBottle;
        case "silver_lighter": return ItemType.SilverLighter;
        case "siblings_toy": return ItemType.SiblingsToy;
        case "brass_key": return ItemType.BrassKey;
        default:
            Debug.LogWarning($"[InputHandler] 알 수 없는 아이템 이름: {itemName}");
            return ItemType.None; // 또는 기본값
    }
}

// 아이템 상태 이름을 ItemState enum으로 변환하는 헬퍼 메서드
// (Phase1 문서 2.6.11 섹션의 아이템 상태 이름 매핑 사용)
private ItemState ConvertItemStateNameToType(string stateName)
{
    switch (stateName?.ToLower())
    {
        case "in_world": return ItemState.InWorld;
        case "in_inventory": return ItemState.InInventory;
        case "used": return ItemState.Used;
        default:
            Debug.LogWarning($"[InputHandler] 알 수 없는 아이템 상태 이름: {stateName}");
            return ItemState.InWorld; // 기본값
    }
}
```

**폴백 전략:**
- `itemChanges`가 null이면 아이템 변화량 처리하지 않음 (이전 상태 유지)
- `acquired_items`, `consumed_items`, `state_changes`가 null이면 해당 처리 생략
- 아이템 이름이 알 수 없는 경우 경고 로그 출력 및 처리 생략
- 아이템 상태 이름이 알 수 없는 경우 기본값(`ItemState.InWorld`) 사용

#### 5.4 백엔드 응답 예시

백엔드 API는 다음과 같은 JSON 형식으로 응답해야 합니다:

**아이템 획득만 있는 경우:**
```json
{
  "response": "찬장을 열어보니 보라색 라벨의 작은 약병을 발견했다...",
  "humanity_change": 0.0,
  "item_changes": {
    "acquired_items": [
      {
        "item_name": "sleeping_pill",
        "count": 1
      }
    ],
    "consumed_items": [],
    "state_changes": []
  }
}
```

**아이템 사용/소모가 있는 경우:**
```json
{
  "response": "홍차에 수면제를 탄다...",
  "humanity_change": 0.0,
  "item_changes": {
    "acquired_items": [],
    "consumed_items": [
      {
        "item_name": "sleeping_pill",
        "count": 1
      }
    ],
    "state_changes": [
      {
        "item_name": "sleeping_pill",
        "new_state": "used"
      }
    ]
  }
}
```

**아이템 상태 변경이 있는 경우:**
```json
{
  "response": "라이터를 이용해 불을 지른다...",
  "humanity_change": 0.0,
  "item_changes": {
    "acquired_items": [],
    "consumed_items": [
      {
        "item_name": "silver_lighter",
        "count": 1
      }
    ],
    "state_changes": [
      {
        "item_name": "silver_lighter",
        "new_state": "used"
      }
    ]
  }
}
```

**모든 필드가 포함된 경우:**
```json
{
  "response": "찬장을 열어보니 보라색 라벨의 작은 약병을 발견했다...",
  "humanity_change": 0.0,
  "item_changes": {
    "acquired_items": [
      {
        "item_name": "sleeping_pill",
        "count": 1
      }
    ],
    "consumed_items": [],
    "state_changes": [
      {
        "item_name": "sleeping_pill",
        "new_state": "in_inventory"
      }
    ]
  }
}
```

**참고:**
- 모든 필드는 **선택적(optional)**이며, 백엔드에서 제공하지 않을 수 있음
- `acquired_items`, `consumed_items`, `state_changes`가 null이면 해당 처리 생략
- 백엔드에서 필드를 제공하지 않으면 **이전 상태를 유지**
- 초기 상태이거나 이전 상태가 없으면 **프론트엔드에서 기본값 사용**
- LLM이 플레이어의 자연어 입력을 분석하여 아이템 획득/사용/상태 변경 여부를 판단
- 인벤토리 UI 아이템 사용과 클릭 가능 오브젝트 클릭 모두 자연어 입력을 통해 LLM이 판정하므로 백엔드에서 관리

#### 5.5 아이템 획득/사용 판정 로직

**백엔드에서 판정해야 하는 경우:**
- 플레이어가 "찬장을 연다", "수면제를 찾는다" → 아이템 획득 판정
- 플레이어가 "홍차에 수면제를 탄다" → 아이템 사용 판정
- 플레이어가 "동생에게 장난감을 건네준다" → 아이템 소모 판정

**프론트엔드에서 직접 처리하는 경우:**
- 클릭 가능한 오브젝트를 클릭했을 때 (명확한 아이템 획득)
- 인벤토리 UI에서 아이템 사용 버튼 클릭 (명확한 아이템 사용)

**권장 방식:**
- **자유 서술 입력**: 백엔드에서 LLM이 판단하여 아이템 변화량 제공
- **명확한 상호작용**: 프론트엔드에서 직접 처리 가능 (선택적)

## 완료 조건
- [ ] InitializeInventory() 구현 완료
- [ ] InitializeWorldItemStates() 구현 완료
- [ ] AddItem() 구현 완료
- [ ] RemoveItem() 구현 완료
- [ ] HasItem() 구현 완료
- [ ] GetItemCount() 구현 완료
- [ ] SetItemState() 구현 완료
- [ ] GetItemState() 구현 완료
- [ ] RespawnDailyItems() 구현 완료
- [ ] AdvanceToNextDay()에 아이템 처리 통합 완료
- [ ] ApiClient의 GameResponse에 아이템 변화량 필드 추가 완료 (ItemChanges 구조체)
- [ ] ApiClient의 SendMessage 콜백 시그니처에 아이템 변화량 추가 완료 (Phase2 NPC 필드 포함)
- [ ] InputHandler의 OnApiSuccess에서 아이템 변화량 처리 완료
- [ ] InputHandler의 OnApiSuccess에서 아이템 상태 변경 처리 완료
- [ ] 아이템 이름 매핑 헬퍼 메서드 구현 완료 (ConvertItemNameToType)
- [ ] 아이템 상태 이름 매핑 헬퍼 메서드 구현 완료 (ConvertItemStateNameToType)
- [ ] 백엔드 응답이 없을 때 폴백 전략 구현 완료 (이전 상태 유지)
- [ ] 모든 메서드에서 이벤트 발생 확인
- [ ] 컴파일 에러 없음

## 참고 파일
- [Assets/Scripts/Ryu/Global/GameStateManager.cs](Assets/Scripts/Ryu/Global/GameStateManager.cs)
- [Assets/Scripts/Ryu/Global/ApiClient.cs](Assets/Scripts/Ryu/Global/ApiClient.cs)
- [Assets/Scripts/Ryu/Tutorial/InputHandler.cs](Assets/Scripts/Ryu/Tutorial/InputHandler.cs)
- [마크다운/시나리오/시나리오.md](마크다운/시나리오/시나리오.md)
- [마크다운/변경사항/0210/Phase1_데이터_모델_정의.md](마크다운/변경사항/0210/Phase1_데이터_모델_정의.md)
- [마크다운/변경사항/0210/Phase2_NPC_시스템_구현.md](마크다운/변경사항/0210/Phase2_NPC_시스템_구현.md)
- [마크다운/변경사항/0210/동적_상태_관리_정책.md](마크다운/변경사항/0210/동적_상태_관리_정책.md)

