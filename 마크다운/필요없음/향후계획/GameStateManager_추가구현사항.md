# GameStateManager 추가 구현 사항

## 개요
시나리오 문서를 기반으로 `GameStateManager.cs`에 추가해야 할 기능들을 정리한 문서입니다.

---

## ✅ 이미 구현된 기능

### 1. 플레이어 인간성 시스템
- ✅ 인간성 수치 관리 (0~100)
- ✅ 인간성 변경 메서드 (`ModifyHumanity`, `SetHumanity`)
- ✅ 인간성 0% 도달 시 게임 오버 트리거
- ✅ 인간성 변경 이벤트 (`OnHumanityChanged`, `OnHumanityReachedZero`)

### 2. 날짜 시스템
- ✅ 현재 날짜 관리 (1~5일차)
- ✅ 다음 날 진행 기능 (`AdvanceToNextDay`, `AdvanceDay`)
- ✅ 날짜 변경 이벤트 (`OnDayChanged`)
- ✅ 날짜 경과 시 인간성 10% 자동 감소

### 3. 게임 오버 시스템
- ✅ 인간성 0% 도달 시 배드 엔딩 트리거
- ✅ SceneFadeManager를 통한 씬 전환

### 4. 싱글톤 패턴
- ✅ 싱글톤 인스턴스 관리
- ✅ DontDestroyOnLoad 설정
- ✅ 인스턴스 초기화 메서드 (`ClearInstance`)

---

## ❌ 구현해야 할 기능

### 1. NPC 상태 관리 시스템

#### 1.1 NPC 호감도 시스템 (Affection)
**요구사항:**
- 각 NPC별 호감도 관리 (0~100)
- NPC 목록: 새엄마(엘리노어), 새아빠(아더), 동생(루카스), 강아지(바론), 할머니(마가렛)

**구현 필요 사항:**
```csharp
// NPC 타입 열거형
public enum NPCType
{
    NewMother,    // 새엄마 (엘리노어)
    NewFather,    // 새아빠 (아더)
    Sibling,      // 동생 (루카스)
    Dog,          // 강아지 (바론)
    Grandmother   // 할머니 (마가렛)
}

// NPC별 호감도 딕셔너리
private Dictionary<NPCType, float> npcAffection;

// 호감도 변경 메서드
public void ModifyAffection(NPCType npc, float changeAmount);
public float GetAffection(NPCType npc);

// 호감도 변경 이벤트
public event Action<NPCType, float> OnAffectionChanged;
```

**호감도 영향:**
- 높음: 부탁 수락, 힌트 제공, 아이템 위치 알림
- 낮음: 압박, 아이템 수집 제지, 밤의 대화에서 부정적 평가

#### 1.2 NPC 인간성 시스템 (Humanity)
**요구사항:**
- NPC별 인간성 관리 (-100~100)
- 새엄마는 인간성 불가 (최종보스)
- 과거 기억이 남아있는 정도를 나타냄

**구현 필요 사항:**
```csharp
// NPC별 인간성 딕셔너리
private Dictionary<NPCType, float> npcHumanity;

// 인간성 변경 메서드
public void ModifyNPCHumanity(NPCType npc, float changeAmount);
public float GetNPCHumanity(NPCType npc);

// 인간성 변경 이벤트
public event Action<NPCType, float> OnNPCHumanityChanged;
```

**인간성 영향:**
- 높음: 탈출 조력자로 포섭 가능, 새엄마와 논쟁
- 낮음: 새엄마 명령에만 복종, 기계적 반응

---

### 2. 행동력(턴) 시스템

**현재 상태:**
- ⚠️ **별도 매니저로 구현됨**: `TurnManager` 클래스가 별도로 존재하며 기본 기능은 구현되어 있음
- ❌ **GameStateManager 통합 필요**: 턴 시스템이 `GameStateManager`에 통합되지 않아 저장/로드 시 턴 상태가 포함되지 않음

**현재 구현 (`TurnManager.cs`):**
- ✅ 일일 10턴 제한
- ✅ 행동 시 턴 소모 (`ConsumeTurn()`)
- ✅ 턴 소진 시 밤으로 전환
- ✅ 날짜 변경 시 턴 리셋
- ✅ UI 표시 (HUD_TopLeft의 TurnsText)

**GameStateManager에 추가 필요:**
```csharp
// 턴 관련 변수 (GameStateManager에 통합)
private int currentTurn = 0;
private const int MAX_TURNS_PER_DAY = 10;

// 턴 관리 메서드
public bool ConsumeTurn(int amount = 1);
public int GetRemainingTurns();
public bool HasRemainingTurns();

// 턴 변경 이벤트
public event Action<int> OnTurnChanged;
public event Action OnTurnsExhausted; // 턴 소진 시 밤으로 전환
```

**턴 소모 조건:**
- 조사 행동
- 아이템 습득
- NPC와 대화
- 이동

**통합 필요 이유:**
- 게임 저장/로드 시 턴 상태 포함 필요
- 다른 시스템에서 턴 상태 조회 필요
- 중앙 집중식 상태 관리

---

### 3. 낮/밤 상태 관리

**요구사항:**
- 낮: 탐색 및 대화 가능
- 밤: NPC 회의 진행, 대화 로그 노출

**구현 필요 사항:**
```csharp
// 시간대 열거형
public enum TimeOfDay
{
    Day,    // 낮
    Night   // 밤
}

// 현재 시간대
private TimeOfDay currentTimeOfDay = TimeOfDay.Day;

// 시간대 변경 메서드
public void SetTimeOfDay(TimeOfDay time);
public TimeOfDay GetCurrentTimeOfDay();

// 시간대 변경 이벤트
public event Action<TimeOfDay> OnTimeOfDayChanged;
```

**시간대별 동작:**
- 낮 → 밤: 턴 소진 시 자동 전환
- 밤 → 낮: 밤의 대화 종료 후 다음 날로 진행

---

### 4. 아이템 인벤토리 시스템

**요구사항:**
- 플레이어가 획득한 아이템 관리
- 아이템 사용 및 조합
- 월드 아이템 상태 관리 (위치, 획득 여부, 숨김 상태)

**주요 아이템 목록:**
- 진짜 가족사진
- 따뜻한 홍차 (엄마의 특별식) - 매일 리스폰
- 말린 허브 뭉치
- 강아지 간식
- 공업용 수면제
- 기름병 & 라이터
- 윤활유
- 엄마의 열쇠
- 동생의 장난감
- 인간의 증표(소지품)

**구현 필요 사항:**
```csharp
// 아이템 타입 열거형
public enum ItemType
{
    RealFamilyPhoto,
    WarmTea,
    DriedHerbs,
    DogTreat,
    SleepingPill,
    OilBottle,
    Lighter,
    Lubricant,
    MothersKey,
    SiblingsToy,
    HumanToken
}

// 아이템 상태 열거형
public enum ItemState
{
    InWorld,        // 월드에 존재 (획득 가능)
    InInventory,    // 인벤토리에 있음
    Used,           // 사용됨 (소모품)
    Hidden          // 숨겨짐 (새아빠가 제지한 경우 등)
}

// 인벤토리 (아이템별 개수)
private Dictionary<ItemType, int> inventory;

// 월드 아이템 상태 관리
private Dictionary<ItemType, WorldItemState> worldItemStates;

// 아이템 관리 메서드
public void AddItem(ItemType item, int count = 1);
public void RemoveItem(ItemType item, int count = 1);
public bool HasItem(ItemType item);
public int GetItemCount(ItemType item);

// 월드 아이템 상태 관리 메서드
public void SetItemState(ItemType item, ItemState state, ItemLocation location = null);
public ItemState GetItemState(ItemType item);
public void HideItem(ItemType item, int untilDay);  // 새아빠가 제지한 경우
public void RestoreHiddenItems(int currentDay);     // 다음 날 복구

// 인벤토리 변경 이벤트
public event Action<ItemType, int> OnInventoryChanged;
public event Action<ItemType, ItemState> OnItemStateChanged;
```

**월드 아이템 상태 관리 필요성:**
- **새아빠 호감도 낮음 시**: 일정 확률로 아이템 수집 제지 (해당 일자에 오브젝트 사라짐, 다음날 복구)
- **매일 리스폰 아이템**: 따뜻한 홍차는 매일 아침/저녁 주방 식탁에 리스폰
- **여러 위치 아이템**: 진짜 가족사진은 복도 액자 뒤 또는 정원 개집 근처 땅속에 존재 가능
- **아이템 위치 추적**: 플레이어가 어디서 아이템을 찾았는지, 현재 어디에 있는지 추적 필요

---

### 5. 엔딩 조건 추적 시스템

**요구사항:**
- 각 엔딩의 달성 조건 추적
- 5일차 종료 시 엔딩 체크

**엔딩 목록:**

#### Good/Normal Endings:
1. **완벽한 기만 (The Stealth Exit)**
   - 조건: 수면제 보유, 전 NPC 호감도 중간 이상

2. **혼돈의 밤 (The Chaotic Breakout)**
   - 조건: 기름병 보유, 할머니와 협력 상태

3. **조력자의 희생 (The Sibling's Help)**
   - 조건: 동생의 장난감 보유, 인간의 증표 보유, 동생 호감도 최대

#### Bad Endings:
1. **불완전한 박제 (The Unfinished Doll)**
   - 조건: 인간성 0% 도달 (✅ 이미 구현됨)

2. **영원한 식사 시간 (The Eternal Dinner)**
   - 조건: 5일차 저녁 식사 전 탈출 엔딩 미달성

**구현 필요 사항:**
```csharp
// 엔딩 타입 열거형
public enum EndingType
{
    None,
    StealthExit,        // 완벽한 기만
    ChaoticBreakout,    // 혼돈의 밤
    SiblingsHelp,       // 조력자의 희생
    UnfinishedDoll,     // 불완전한 박제 (이미 구현)
    EternalDinner       // 영원한 식사 시간
}

// 엔딩 조건 체크 메서드
public EndingType CheckEndingConditions();
public bool CanAchieveEnding(EndingType ending);

// 엔딩 트리거 메서드
public void TriggerEnding(EndingType ending);

// 엔딩 이벤트
public event Action<EndingType> OnEndingTriggered;
```

**5일차 종료 시 처리:**
- `AdvanceToNextDay()` 메서드에서 5일차 도달 시 엔딩 조건 체크
- 조건에 맞는 엔딩이 없으면 "영원한 식사 시간" 배드 엔딩

---

### 6. 게임 저장/로드 시스템

**요구사항:**
- 게임 상태 저장 및 불러오기
- PlayerPrefs 또는 JSON 파일 사용

**저장해야 할 데이터:**
- 현재 날짜
- 플레이어 인간성
- NPC 호감도 (5명)
- NPC 인간성 (4명, 새엄마 제외)
- **NPC 위치 정보** (어느 방/씬에 있는지)
- **NPC 무력화 상태** (수면제로 잠든 NPC, 남은 턴 수)
- 현재 턴 수
- 현재 시간대 (낮/밤)
- 인벤토리 상태
- **월드 아이템 상태** (아이템이 월드에 있는지, 인벤토리에 있는지, 사용되었는지)
- **아이템 위치 정보** (여러 위치에 있을 수 있는 아이템의 경우)
- **아이템 숨김 상태** (새아빠가 제지한 경우 해당 일자에 숨겨진 아이템)
- **이벤트 플래그** (할머니 협력 상태, 개구멍 상태, 화재 상태, 아이템 사용 이력 등)
- **엔딩 달성 플래그** (각 엔딩 조건 달성 여부)
- 현재 씬 이름

**구현 필요 사항:**
```csharp
// 저장/로드 메서드
public void SaveGame(string saveSlot = "default");
public bool LoadGame(string saveSlot = "default");
public void DeleteSave(string saveSlot = "default");
public bool HasSave(string saveSlot = "default");

// 아이템 상태 열거형
public enum ItemState
{
    InWorld,        // 월드에 존재 (획득 가능)
    InInventory,    // 인벤토리에 있음
    Used,           // 사용됨 (소모품)
    Hidden          // 숨겨짐 (새아빠가 제지한 경우 등)
}

// 아이템 위치 정보
[Serializable]
public class ItemLocation
{
    public string sceneName;    // 씬 이름
    public string locationId;  // 위치 식별자 (예: "Hallway_PhotoFrame", "Garden_DogHouse")
}

// 월드 아이템 상태 정보
[Serializable]
public class WorldItemState
{
    public ItemType itemType;
    public ItemState state;
    public ItemLocation location;  // null이면 여러 위치 가능 또는 위치 불명
    public int hiddenUntilDay;     // 숨겨진 경우 복구될 날짜 (-1이면 숨김 아님)
}

// NPC 위치 정보
[Serializable]
public class NPCLocation
{
    public NPCType npcType;
    public string sceneName;    // 현재 있는 씬
    public string locationId;    // 구체적 위치 (예: "LivingRoom_Sofa", "Kitchen_Table")
}

// NPC 무력화 상태
[Serializable]
public class NPCDisabledState
{
    public NPCType npcType;
    public int remainingTurns;  // 무력화 남은 턴 수 (0이면 정상)
    public string reason;        // 무력화 이유 ("SleepingPill" 등)
}

// 이벤트 플래그
[Serializable]
public class EventFlags
{
    public bool grandmotherCooperation;  // 할머니에게 생기를 나눠줌 (혼돈의 밤 엔딩)
    public bool holeOpened;              // 개구멍이 열렸는지
    public bool livingRoomOnFire;        // 거실에 불이 난 상태
    public bool kitchenOnFire;           // 주방에 불이 난 상태
    public List<NPCType> photoShownToNPCs;  // 진짜 가족사진을 보여준 NPC 목록
    public bool gaveToyToSibling;        // 동생에게 장난감과 사진을 건네줬는지
    public List<NPCType> sleepingPillUsedOn; // 수면제를 사용한 NPC 목록
}

// 엔딩 달성 플래그
[Serializable]
public class EndingFlags
{
    public bool canAchieveStealthExit;      // 완벽한 기만 (수면제 + 지도 보유 + 호감도 체크)
    public bool canAchieveChaoticBreakout;  // 혼돈의 밤 (기름병 보유 + 할머니 협력)
    public bool canAchieveSiblingsHelp;     // 조력자의 희생 (동생 호감도 최대 + 아이템 보유)
}

// 저장 데이터 구조체
[Serializable]
public class GameSaveData
{
    public int currentDay;
    public float playerHumanity;
    public Dictionary<NPCType, float> npcAffection;
    public Dictionary<NPCType, float> npcHumanity;
    public List<NPCLocation> npcLocations;           // NPC 위치 정보
    public List<NPCDisabledState> npcDisabledStates; // NPC 무력화 상태
    public int currentTurn;
    public TimeOfDay timeOfDay;
    public Dictionary<ItemType, int> inventory;
    public List<WorldItemState> worldItemStates;     // 월드 아이템 상태 목록
    public EventFlags eventFlags;                     // 이벤트 플래그
    public EndingFlags endingFlags;                   // 엔딩 달성 플래그
    public string currentScene;
}
```

---

### 7. 기타 추가 기능

#### 7.1 밤의 대화 시스템 연동
**요구사항:**
- 밤 시간대에 NPC 회의 진행
- 호감도 재계산
- 대화 로그 일부 노출

**구현 필요 사항:**
```csharp
// 밤의 대화 트리거
public void TriggerNightDialogue();

// 밤의 대화 완료 후 처리
public void OnNightDialogueComplete();
```

#### 7.2 아이템 효과 시스템
**요구사항:**
- 아이템 사용 시 효과 적용
- 예: 홍차 마시기 → 인간성 5% 감소, 기력 회복

**구현 필요 사항:**
```csharp
// 아이템 사용 메서드
public bool UseItem(ItemType item, params object[] parameters);

// 아이템 효과 정의 (ScriptableObject 또는 별도 클래스)
```

#### 7.3 NPC 상태 조회 API
**요구사항:**
- 다른 시스템에서 NPC 상태 조회 가능

**구현 필요 사항:**
```csharp
// NPC 상태 조회 메서드
public NPCStatus GetNPCStatus(NPCType npc);

// NPC 상태 구조체
[Serializable]
public struct NPCStatus
{
    public float affection;
    public float humanity;
    public bool isAvailable; // 대화 가능 여부
}
```

---

## 구현 우선순위

### Phase 1 (핵심 시스템)
1. ✅ 플레이어 인간성 시스템 (완료)
2. ✅ 날짜 시스템 (완료)
3. ⚠️ 행동력(턴) 시스템 (별도 매니저로 구현됨, GameStateManager 통합 필요)
4. ❌ 낮/밤 상태 관리

### Phase 2 (NPC 시스템)
5. ❌ NPC 호감도 시스템
6. ❌ NPC 인간성 시스템

### Phase 3 (아이템 및 엔딩)
7. ❌ 아이템 인벤토리 시스템
8. ❌ 엔딩 조건 추적 시스템

### Phase 4 (저장/로드)
9. ❌ 게임 저장/로드 시스템

---

## 참고 사항

1. **이벤트 기반 아키텍처**: 모든 상태 변경은 이벤트를 통해 알림
2. **데이터 영속성**: DontDestroyOnLoad로 씬 전환 시에도 상태 유지
3. **확장성**: 새로운 NPC나 아이템 추가 시 열거형 확장 용이
4. **성능**: Dictionary 사용으로 O(1) 조회 성능 보장
5. **디버깅**: 모든 상태 변경에 Debug.Log 추가 권장

---

## 연동 필요 시스템

- `NightDialogueManager`: 밤의 대화 시스템
- `TurnManager`: 턴 관리 시스템 (별도 매니저일 수 있음)
- `InventoryManager`: 인벤토리 UI 시스템 (별도 매니저일 수 있음)
- `SceneFadeManager`: 씬 전환 및 페이드 효과
- `NPCManager`: NPC 상호작용 시스템 (별도 매니저일 수 있음)

