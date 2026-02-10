# Phase 1: GameStateManager 데이터 모델 정의

## 개요
GameStateManager.cs에 필요한 모든 열거형(Enum)과 직렬화 가능한 데이터 구조체를 정의합니다. 이는 모든 시스템의 기반이 되는 데이터 모델입니다.

## 목표
- 모든 열거형 정의 완료
- 모든 직렬화 가능한 구조체 정의 완료
- GameStateManager에 필드 선언 완료

## 작업 내용

### 1. 열거형(Enum) 정의

#### 1.1 NPCType
```csharp
public enum NPCType
{
    NewMother,      // 새엄마 (엘리노어) - 최종보스, 인간성 불가
    NewFather,      // 새아빠 (아더)
    Sibling,        // 동생 (루카스)
    Dog,            // 강아지 (바론)
    Grandmother     // 할머니 (마가렛)
}
```

#### 1.2 ItemType
```csharp
public enum ItemType
{
    SleepingPill,           // 보라색 라벨의 약병 (수면제)
    EarlGreyTea,           // 홍차
    RealFamilyPhoto,       // 훼손된 가족 사진 (진짜 가족 사진)
    OilBottle,             // 고래기름 통 (기름병)
    SilverLighter,         // 은색 지포 라이터
    SiblingsToy,          // 낡은 태엽 로봇 (동생의 장난감)
    BrassKey              // 황동 열쇠 (마스터 키)
}
```

#### 1.3 ItemState
```csharp
public enum ItemState
{
    InWorld,        // 월드에 존재 (획득 가능)
    InInventory,    // 인벤토리에 있음
    Used            // 사용됨 (소모품)
}
```

#### 1.4 TimeOfDay
```csharp
public enum TimeOfDay
{
    Day,            // 낮 (탐색 및 대화 가능)
    Night           // 밤 (NPC 회의 진행)
}
```

#### 1.5 GameLocation
```csharp
public enum GameLocation
{
    PlayersRoom,        // 주인공의 방 (The Birdcage)
    Hallway,            // 복도와 계단 (The Veins)
    LivingRoom,         // 거실 (The Showroom)
    Kitchen,            // 주방 및 식당 (The Modification Lab)
    SiblingsRoom,       // 동생의 놀이방 (The Glitch Room)
    Basement,           // 지하실 (The Archive / 할머니의 방)
    Backyard            // 뒷마당과 정원 (The Gateway)
}
```

#### 1.6 EndingType
```csharp
public enum EndingType
{
    None,                   // 엔딩 없음
    StealthExit,            // 완벽한 기만 (루트 1)
    ChaoticBreakout,        // 혼돈의 밤 (루트 2)
    SiblingsHelp,           // 조력자의 희생 (루트 4)
    UnfinishedDoll,         // 불완전한 박제 (배드 루트 1)
    EternalDinner           // 영원한 식사 시간 (배드 루트 2)
}
```

### 2. 직렬화 가능한 데이터 구조체

#### 2.1 NPCStatus
```csharp
[Serializable]
public class NPCStatus
{
    public NPCType npcType;
    public float affection;         // 호감도 (0~100)
    public float humanity;           // NPC 인간성 (-100~100, 새엄마는 -100 고정)
    public bool isAvailable;        // 대화 가능 여부
    public bool isDisabled;         // 무력화 상태 (수면제 등)
    public int disabledRemainingTurns; // 무력화 남은 턴 수
    public string disabledReason;   // 무력화 이유
}
```

#### 2.2 ItemLocation
```csharp
[Serializable]
public class ItemLocation
{
    public GameLocation location;   // 게임 구역
    public string sceneName;        // 씬 이름
    public string locationId;       // 구체적 위치 식별자 (예: "Hallway_PhotoFrame", "Garden_DogHouse")
}
```

#### 2.3 WorldItemState
```csharp
[Serializable]
public class WorldItemState
{
    public ItemType itemType;
    public ItemState state;
    public ItemLocation location;   // null이면 여러 위치 가능 또는 위치 불명
    public bool isRespawnable;      // 매일 리스폰 여부 (홍차 등)
}
```

#### 2.4 EndingCondition
```csharp
[Serializable]
public class EndingCondition
{
    public EndingType endingType;
    public bool hasRequiredItems;       // 필수 아이템 보유 여부
    public bool hasRequiredNPCStatus;  // 필수 NPC 상태 달성 여부
    public bool hasRequiredLocation;   // 필수 장소 도달 여부
    public bool hasRequiredTime;      // 필수 시간대 여부
    public Dictionary<string, bool> customFlags; // 커스텀 플래그 (예: "GrandmotherCooperation")
}
```

#### 2.5 EventFlags
```csharp
[Serializable]
public class EventFlags
{
    public bool grandmotherCooperation;    // 할머니 협력 상태
    public bool holeUnlocked;              // 개구멍 개방 상태
    public bool fireStarted;               // 화재 발생 상태
    public bool familyAsleep;              // 가족 수면 상태
    public bool teaWithSleepingPill;       // 홍차에 수면제를 탔는지 여부 (StealthExit 엔딩 조건)
    public bool keyStolen;                 // 열쇠 탈취 상태
    public bool caughtByFather;            // 새아빠에게 발각됨
    public bool caughtByMother;            // 새엄마에게 발각됨
    public int imprisonmentDay;            // 감금된 날짜 (-1이면 감금 아님)
    public Dictionary<string, bool> customEvents; // 기타 커스텀 이벤트
}
```

### 2.6 백엔드 API 응답 구조체

백엔드에서 받아오는 게임 상태 변화를 위한 직렬화 가능한 구조체들입니다. 모든 필드는 **선택적(optional)**이며, 백엔드에서 제공하지 않을 수 있습니다.

#### 2.6.1 NPCAffectionChanges
```csharp
[Serializable]
public class NPCAffectionChanges
{
    public float new_mother;    // 새엄마 (엘리노어) 호감도 변화량
    public float new_father;    // 새아빠 (아더) 호감도 변화량
    public float sibling;       // 동생 (루카스) 호감도 변화량
    public float dog;           // 강아지 (바론) 호감도 변화량
    public float grandmother;   // 할머니 (마가렛) 호감도 변화량
}
```

#### 2.6.2 NPCHumanityChanges
```csharp
[Serializable]
public class NPCHumanityChanges
{
    // 새엄마는 인간성 변경 불가 (최종보스)
    public float new_father;    // 새아빠 (아더) 인간성 변화량
    public float sibling;       // 동생 (루카스) 인간성 변화량
    public float dog;           // 강아지 (바론) 인간성 변화량
    public float grandmother;   // 할머니 (마가렛) 인간성 변화량
}
```

#### 2.6.3 NPCDisabledState
```csharp
[Serializable]
public class NPCDisabledState
{
    public bool is_disabled;        // 무력화 여부
    public int remaining_turns;     // 무력화 남은 턴 수
    public string reason;           // 무력화 이유
}
```

#### 2.6.4 NPCDisabledStates
```csharp
[Serializable]
public class NPCDisabledStates
{
    // Unity JsonUtility는 Dictionary를 지원하지 않으므로, 각 NPC별로 개별 필드 사용
    // 백엔드에서 제공하지 않는 NPC는 null 또는 기본값으로 처리
    public NPCDisabledState new_mother;     // 새엄마 무력화 상태 (선택적)
    public NPCDisabledState new_father;     // 새아빠 무력화 상태 (선택적)
    public NPCDisabledState sibling;        // 동생 무력화 상태 (선택적)
    public NPCDisabledState dog;            // 강아지 무력화 상태 (선택적)
    public NPCDisabledState grandmother;    // 할머니 무력화 상태 (선택적)
}
```

**참고:** 
- Unity의 `JsonUtility`는 Dictionary를 지원하지 않으므로, 각 NPC별로 개별 필드를 사용합니다.
- 백엔드에서 특정 NPC의 무력화 상태를 제공하지 않으면 해당 필드는 null이거나 기본값입니다.
- 프론트엔드에서 null 체크 후 적용합니다.

#### 2.6.5 ItemAcquisition
```csharp
[Serializable]
public class ItemAcquisition
{
    public string item_name;  // 백엔드에서 사용하는 아이템 이름 (예: "sleeping_pill")
    public int count;         // 획득 개수
}
```

#### 2.6.6 ItemConsumption
```csharp
[Serializable]
public class ItemConsumption
{
    public string item_name;  // 백엔드에서 사용하는 아이템 이름 (예: "sleeping_pill")
    public int count;         // 소모 개수
}
```

#### 2.6.7 ItemStateChange
```csharp
[Serializable]
public class ItemStateChange
{
    public string item_name;  // 백엔드에서 사용하는 아이템 이름
    public string new_state;  // 새로운 상태 (예: "used", "in_inventory")
}
```

#### 2.6.8 ItemChanges
```csharp
[Serializable]
public class ItemChanges
{
    public ItemAcquisition[] acquired_items;   // 획득된 아이템 목록 (선택적)
    public ItemConsumption[] consumed_items;   // 사용/소모된 아이템 목록 (선택적)
    public ItemStateChange[] state_changes;    // 아이템 상태 변경 목록 (선택적)
}
```

#### 2.6.9 NPCLocations
```csharp
[Serializable]
public class NPCLocations
{
    // Unity JsonUtility는 Dictionary를 지원하지 않으므로, 각 NPC별로 개별 필드 사용
    // 백엔드에서 제공하지 않는 NPC는 null 또는 빈 문자열로 처리
    public string new_mother;     // 새엄마 위치 (예: "kitchen", 선택적)
    public string new_father;     // 새아빠 위치 (예: "living_room", 선택적)
    public string sibling;        // 동생 위치 (예: "siblings_room", 선택적)
    public string dog;            // 강아지 위치 (예: "backyard", 선택적)
    public string grandmother;     // 할머니 위치 (예: "basement", 선택적)
}
```

**참고:** 
- 위치 이름은 백엔드에서 문자열로 제공되며, 프론트엔드에서 `GameLocation` enum으로 변환해야 합니다.
- 백엔드에서 특정 NPC의 위치를 제공하지 않으면 해당 필드는 null이거나 빈 문자열입니다.
- 프론트엔드에서 null/빈 문자열 체크 후, 이전 상태를 유지하거나 기본값을 사용합니다.

#### 2.6.10 GameResponse (ApiClient 내부용)
```csharp
// ApiClient.cs 내부에 정의 (GameStateManager.cs가 아닌 ApiClient.cs에)
[Serializable]
private class GameResponse
{
    public string response;                           // 응답 텍스트
    public float humanity_change;                     // 플레이어 인간성 변화량
    public NPCAffectionChanges npc_affection_changes;    // NPC 호감도 변화량 (선택적)
    public NPCHumanityChanges npc_humanity_changes;       // NPC 인간성 변화량 (선택적)
    public NPCDisabledStates npc_disabled_states;        // NPC 무력화 상태 (선택적)
    public ItemChanges item_changes;                      // 아이템 변화량 (선택적)
    public EventFlags event_flags;                        // 이벤트 플래그 (선택적)
    public NPCLocations npc_locations;                    // NPC 위치 (선택적)
    public string ending_trigger;                        // 엔딩 트리거 (선택적, null 가능)
}
```

**참고:**
- `GameResponse`는 `ApiClient.cs`에 정의되어야 하며, `GameStateManager.cs`에는 정의하지 않습니다.
- 모든 필드는 선택적(optional)이며, 백엔드에서 제공하지 않을 수 있습니다.
- 백엔드 응답이 없으면 이전 상태를 유지하거나 프론트엔드에서 기본값을 사용합니다.

#### 2.6.11 이름 매핑 헬퍼 (ApiClient 또는 GameStateManager)

백엔드에서 사용하는 이름과 Unity의 enum을 매핑하는 헬퍼 메서드가 필요합니다.

**아이템 이름 매핑:**
```csharp
// ApiClient.cs 또는 GameStateManager.cs에 추가
private Dictionary<string, ItemType> itemNameMapping = new Dictionary<string, ItemType>
{
    { "sleeping_pill", ItemType.SleepingPill },
    { "earl_grey_tea", ItemType.EarlGreyTea },
    { "real_family_photo", ItemType.RealFamilyPhoto },
    { "oil_bottle", ItemType.OilBottle },
    { "silver_lighter", ItemType.SilverLighter },
    { "siblings_toy", ItemType.SiblingsToy },
    { "brass_key", ItemType.BrassKey }
};

private ItemType ConvertItemNameToType(string itemName)
{
    if (string.IsNullOrEmpty(itemName))
        return ItemType.None; // 또는 기본값
    
    if (itemNameMapping.TryGetValue(itemName.ToLower(), out ItemType itemType))
        return itemType;
    
    Debug.LogWarning($"[ApiClient] 알 수 없는 아이템 이름: {itemName}");
    return ItemType.None; // 또는 기본값
}
```

**위치 이름 매핑:**
```csharp
// ApiClient.cs 또는 GameStateManager.cs에 추가
private Dictionary<string, GameLocation> locationNameMapping = new Dictionary<string, GameLocation>
{
    { "players_room", GameLocation.PlayersRoom },
    { "hallway", GameLocation.Hallway },
    { "living_room", GameLocation.LivingRoom },
    { "kitchen", GameLocation.Kitchen },
    { "siblings_room", GameLocation.SiblingsRoom },
    { "basement", GameLocation.Basement },
    { "backyard", GameLocation.Backyard }
};

private GameLocation ConvertLocationNameToType(string locationName)
{
    if (string.IsNullOrEmpty(locationName))
        return GameLocation.Hallway; // 기본값
    
    if (locationNameMapping.TryGetValue(locationName.ToLower(), out GameLocation location))
        return location;
    
    Debug.LogWarning($"[ApiClient] 알 수 없는 위치 이름: {locationName}");
    return GameLocation.Hallway; // 기본값
}
```

**엔딩 이름 매핑:**
```csharp
// ApiClient.cs 또는 GameStateManager.cs에 추가
private Dictionary<string, EndingType> endingNameMapping = new Dictionary<string, EndingType>
{
    { "stealth_exit", EndingType.StealthExit },
    { "chaotic_breakout", EndingType.ChaoticBreakout },
    { "siblings_help", EndingType.SiblingsHelp },
    { "unfinished_doll", EndingType.UnfinishedDoll },
    { "eternal_dinner", EndingType.EternalDinner },
    { "none", EndingType.None }
};

private EndingType ConvertEndingNameToType(string endingName)
{
    if (string.IsNullOrEmpty(endingName))
        return EndingType.None;
    
    if (endingNameMapping.TryGetValue(endingName.ToLower(), out EndingType endingType))
        return endingType;
    
    Debug.LogWarning($"[ApiClient] 알 수 없는 엔딩 이름: {endingName}");
    return EndingType.None;
}
```

**아이템 상태 이름 매핑:**
```csharp
// ApiClient.cs 또는 GameStateManager.cs에 추가
private Dictionary<string, ItemState> itemStateNameMapping = new Dictionary<string, ItemState>
{
    { "in_world", ItemState.InWorld },
    { "in_inventory", ItemState.InInventory },
    { "used", ItemState.Used }
};

private ItemState ConvertItemStateNameToType(string stateName)
{
    if (string.IsNullOrEmpty(stateName))
        return ItemState.InWorld; // 기본값
    
    if (itemStateNameMapping.TryGetValue(stateName.ToLower(), out ItemState state))
        return state;
    
    Debug.LogWarning($"[ApiClient] 알 수 없는 아이템 상태 이름: {stateName}");
    return ItemState.InWorld; // 기본값
}
```

### 3. GameStateManager에 필드 선언

#### 3.1 NPC 상태 관리 필드
```csharp
private Dictionary<NPCType, NPCStatus> npcStatuses;
public event Action<NPCType, NPCStatus> OnNPCStatusChanged;
```

#### 3.2 아이템 시스템 필드
```csharp
private Dictionary<ItemType, int> inventory;  // 인벤토리 (아이템별 개수)
private Dictionary<ItemType, WorldItemState> worldItemStates;  // 월드 아이템 상태
public event Action<ItemType, int> OnInventoryChanged;
public event Action<ItemType, WorldItemState> OnItemStateChanged;
```

#### 3.3 시간대 및 턴 시스템 필드
```csharp
private TimeOfDay currentTimeOfDay = TimeOfDay.Day;
private int currentTurn = 0;
private const int MAX_TURNS_PER_DAY = 10;
public event Action<TimeOfDay> OnTimeOfDayChanged;
public event Action<int> OnTurnChanged;
public event Action OnTurnsExhausted;
```

#### 3.4 위치 추적 필드
```csharp
private GameLocation currentLocation;
private Dictionary<NPCType, GameLocation> npcLocations;  // NPC 현재 위치
public event Action<GameLocation> OnLocationChanged;
```

#### 3.5 엔딩 시스템 필드
```csharp
private Dictionary<EndingType, EndingCondition> endingConditions;
private EndingType currentEnding = EndingType.None;
public event Action<EndingType> OnEndingTriggered;
```

#### 3.6 이벤트 플래그 필드
```csharp
private EventFlags eventFlags;
```

### 4. 초기화 준비

Awake() 메서드에서 딕셔너리 초기화만 수행 (구체적인 초기값 설정은 다음 Phase에서):
```csharp
npcStatuses = new Dictionary<NPCType, NPCStatus>();
inventory = new Dictionary<ItemType, int>();
worldItemStates = new Dictionary<ItemType, WorldItemState>();
endingConditions = new Dictionary<EndingType, EndingCondition>();
npcLocations = new Dictionary<NPCType, GameLocation>();
eventFlags = new EventFlags();
```

## 완료 조건
- [ ] 모든 열거형이 GameStateManager.cs에 정의됨
- [ ] 모든 직렬화 가능한 구조체가 GameStateManager.cs에 정의됨
- [ ] 백엔드 API 응답 구조체가 정의됨
  - [ ] `NPCAffectionChanges` 구조체 정의 (ApiClient.cs 또는 GameStateManager.cs)
  - [ ] `NPCHumanityChanges` 구조체 정의
  - [ ] `NPCDisabledState`, `NPCDisabledStates` 구조체 정의
  - [ ] `ItemAcquisition`, `ItemConsumption`, `ItemStateChange`, `ItemChanges` 구조체 정의
  - [ ] `NPCLocations` 구조체 정의
  - [ ] `GameResponse` 구조체 정의 (ApiClient.cs 내부)
- [ ] 이름 매핑 헬퍼 메서드 정의
  - [ ] 아이템 이름 매핑 딕셔너리 및 변환 메서드
  - [ ] 위치 이름 매핑 딕셔너리 및 변환 메서드
  - [ ] 엔딩 이름 매핑 딕셔너리 및 변환 메서드
  - [ ] 아이템 상태 이름 매핑 딕셔너리 및 변환 메서드
- [ ] 모든 필드가 GameStateManager.cs에 선언됨
- [ ] 모든 이벤트가 선언됨
- [ ] Awake()에서 딕셔너리 초기화 완료
- [ ] 컴파일 에러 없음

## 참고 파일
- [Assets/Scripts/Ryu/Global/GameStateManager.cs](Assets/Scripts/Ryu/Global/GameStateManager.cs)
- [Assets/Scripts/Ryu/Global/ApiClient.cs](Assets/Scripts/Ryu/Global/ApiClient.cs)
- [마크다운/시나리오/시나리오.md](마크다운/시나리오/시나리오.md)
- [마크다운/변경사항/0210/동적_상태_관리_정책.md](마크다운/변경사항/0210/동적_상태_관리_정책.md)

