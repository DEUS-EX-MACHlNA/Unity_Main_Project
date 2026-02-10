# Phase 4: GameStateManager 엔딩 시스템 및 이벤트 플래그 구현

## 개요
엔딩 조건 추적 시스템과 이벤트 플래그 시스템을 구현합니다. 각 엔딩의 달성 조건을 실시간으로 체크하고, 게임 이벤트를 추적합니다.

## 목표
- 엔딩 조건 체크 로직 구현 완료
- 엔딩 트리거 메서드 구현 완료
- 이벤트 플래그 관리 메서드 구현 완료
- 시간대 및 턴 시스템 통합 완료
- 5일차 엔딩 체크 로직 통합 완료

## 작업 내용

### 1. 엔딩 시스템 초기화

#### 1.1 Awake()에서 엔딩 시스템 초기화
```csharp
private void InitializeEndingConditions()
{
    endingConditions = new Dictionary<EndingType, EndingCondition>();
    
    // 완벽한 기만 (StealthExit)
    endingConditions[EndingType.StealthExit] = new EndingCondition
    {
        endingType = EndingType.StealthExit,
        hasRequiredItems = false,
        hasRequiredNPCStatus = false,
        hasRequiredLocation = false,
        hasRequiredTime = false,
        customFlags = new Dictionary<string, bool>()
    };
    
    // 혼돈의 밤 (ChaoticBreakout)
    endingConditions[EndingType.ChaoticBreakout] = new EndingCondition
    {
        endingType = EndingType.ChaoticBreakout,
        hasRequiredItems = false,
        hasRequiredNPCStatus = false,
        hasRequiredLocation = false,
        hasRequiredTime = false,
        customFlags = new Dictionary<string, bool>()
    };
    
    // 조력자의 희생 (SiblingsHelp)
    endingConditions[EndingType.SiblingsHelp] = new EndingCondition
    {
        endingType = EndingType.SiblingsHelp,
        hasRequiredItems = false,
        hasRequiredNPCStatus = false,
        hasRequiredLocation = false,
        hasRequiredTime = false,
        customFlags = new Dictionary<string, bool>()
    };
    
    // 불완전한 박제 (UnfinishedDoll) - 이미 구현됨
    endingConditions[EndingType.UnfinishedDoll] = new EndingCondition
    {
        endingType = EndingType.UnfinishedDoll,
        hasRequiredItems = false,
        hasRequiredNPCStatus = false,
        hasRequiredLocation = false,
        hasRequiredTime = false,
        customFlags = new Dictionary<string, bool>()
    };
    
    // 영원한 식사 시간 (EternalDinner)
    endingConditions[EndingType.EternalDinner] = new EndingCondition
    {
        endingType = EndingType.EternalDinner,
        hasRequiredItems = false,
        hasRequiredNPCStatus = false,
        hasRequiredLocation = false,
        hasRequiredTime = false,
        customFlags = new Dictionary<string, bool>()
    };
}
```

#### 1.2 이벤트 플래그 초기화
```csharp
private void InitializeEventFlags()
{
    eventFlags = new EventFlags
    {
        grandmotherCooperation = false,
        holeUnlocked = false,
        fireStarted = false,
        familyAsleep = false,
        teaWithSleepingPill = false,
        keyStolen = false,
        caughtByFather = false,
        caughtByMother = false,
        imprisonmentDay = -1,
        customEvents = new Dictionary<string, bool>()
    };
}
```

### 2. 시간대 및 턴 시스템 구현

#### 2.1 SetTimeOfDay
```csharp
/// <summary>
/// 현재 시간대를 설정합니다.
/// </summary>
/// <param name="time">시간대</param>
public void SetTimeOfDay(TimeOfDay time)
{
    if (currentTimeOfDay == time)
        return;
    
    TimeOfDay oldTime = currentTimeOfDay;
    currentTimeOfDay = time;
    
    OnTimeOfDayChanged?.Invoke(time);
    Debug.Log($"[GameStateManager] 시간대 변경: {oldTime} → {time}");
}
```

#### 2.2 GetCurrentTimeOfDay
```csharp
/// <summary>
/// 현재 시간대를 반환합니다.
/// </summary>
/// <returns>현재 시간대</returns>
public TimeOfDay GetCurrentTimeOfDay()
{
    return currentTimeOfDay;
}
```

#### 2.3 ConsumeTurn
```csharp
/// <summary>
/// 턴을 소모합니다.
/// </summary>
/// <param name="amount">소모할 턴 수 (기본값: 1)</param>
/// <returns>턴 소모 성공 여부</returns>
public bool ConsumeTurn(int amount = 1)
{
    if (currentTurn + amount > MAX_TURNS_PER_DAY)
    {
        Debug.LogWarning($"[GameStateManager] 턴 수가 부족합니다. (현재: {currentTurn}, 최대: {MAX_TURNS_PER_DAY})");
        return false;
    }
    
    currentTurn += amount;
    OnTurnChanged?.Invoke(GetRemainingTurns());
    
    // NPC 무력화 상태 업데이트
    UpdateNPCDisabledStates();
    
    Debug.Log($"[GameStateManager] 턴 소모: {amount} (사용된 턴: {currentTurn}/{MAX_TURNS_PER_DAY})");
    
    // 턴 소진 체크
    if (currentTurn >= MAX_TURNS_PER_DAY)
    {
        OnTurnsExhausted?.Invoke();
        
        // 밤의 대화 전에 엔딩 조건 체크 (수면제 엔딩 등)
        // 백엔드에서 엔딩 트리거를 받아오는 경우 이 체크는 폴백(fallback) 용도
        EndingType preNightEnding = CheckEndingConditions();
        if (preNightEnding != EndingType.None && 
            preNightEnding != EndingType.EternalDinner &&
            preNightEnding != EndingType.UnfinishedDoll)
        {
            // 엔딩 트리거 (밤의 대화 전에 엔딩으로 진입)
            TriggerEnding(preNightEnding);
            return true; // 턴 소모 성공, 엔딩으로 진입
        }
        
        SetTimeOfDay(TimeOfDay.Night);
    }
    
    return true;
}
```

#### 2.4 GetRemainingTurns
```csharp
/// <summary>
/// 남은 턴 수를 반환합니다.
/// </summary>
/// <returns>남은 턴 수</returns>
public int GetRemainingTurns()
{
    return Mathf.Max(0, MAX_TURNS_PER_DAY - currentTurn);
}
```

#### 2.5 HasRemainingTurns
```csharp
/// <summary>
/// 남은 턴이 있는지 확인합니다.
/// </summary>
/// <returns>남은 턴 존재 여부</returns>
public bool HasRemainingTurns()
{
    return GetRemainingTurns() > 0;
}
```

### 3. 위치 관리 메서드

#### 3.1 SetCurrentLocation
```csharp
/// <summary>
/// 플레이어의 현재 위치를 설정합니다.
/// </summary>
/// <param name="location">위치</param>
public void SetCurrentLocation(GameLocation location)
{
    if (currentLocation == location)
        return;
    
    GameLocation oldLocation = currentLocation;
    currentLocation = location;
    
    OnLocationChanged?.Invoke(location);
    Debug.Log($"[GameStateManager] 위치 변경: {oldLocation} → {location}");
}
```

#### 3.2 GetCurrentLocation
```csharp
/// <summary>
/// 플레이어의 현재 위치를 반환합니다.
/// </summary>
/// <returns>현재 위치</returns>
public GameLocation GetCurrentLocation()
{
    return currentLocation;
}
```

### 4. 엔딩 조건 체크 메서드

#### 4.1 CheckEndingItemCondition
```csharp
/// <summary>
/// 엔딩의 아이템 조건을 체크합니다.
/// </summary>
/// <param name="ending">엔딩 타입</param>
/// <returns>조건 달성 여부</returns>
private bool CheckEndingItemCondition(EndingType ending)
{
    switch (ending)
    {
        case EndingType.StealthExit:
            // 수면제 보유 + 홍차에 수면제를 탔다는 조건 필요
            return HasItem(ItemType.SleepingPill) && eventFlags.teaWithSleepingPill;
            
        case EndingType.ChaoticBreakout:
            // 기름병 + 라이터 보유 필요
            return HasItem(ItemType.OilBottle) && HasItem(ItemType.SilverLighter);
            
        case EndingType.SiblingsHelp:
            // 가족 사진 + 동생의 장난감 보유 필요
            return HasItem(ItemType.RealFamilyPhoto) && HasItem(ItemType.SiblingsToy);
            
        case EndingType.UnfinishedDoll:
            // 인간성 0% (이미 TriggerGameOver에서 처리)
            return humanity <= MIN_HUMANITY;
            
        case EndingType.EternalDinner:
            // 5일차 저녁 식사 전 탈출 엔딩 미달성 (AdvanceToNextDay에서 처리)
            return false;
            
        default:
            return false;
    }
}
```

#### 4.2 CheckEndingNPCCondition
```csharp
/// <summary>
/// 엔딩의 NPC 조건을 체크합니다.
/// </summary>
/// <param name="ending">엔딩 타입</param>
/// <returns>조건 달성 여부</returns>
private bool CheckEndingNPCCondition(EndingType ending)
{
    switch (ending)
    {
        case EndingType.StealthExit:
            // 새엄마 호감도 중간 이상 (50 이상)
            return GetAffection(NPCType.NewMother) >= 50f;
            
        case EndingType.ChaoticBreakout:
            // 할머니 협력 상태 (조건부)
            return eventFlags.grandmotherCooperation;
            
        case EndingType.SiblingsHelp:
            // 동생 호감도 최대 (90 이상)
            return GetAffection(NPCType.Sibling) >= 90f;
            
        default:
            return true;
    }
}
```

#### 4.3 CheckEndingLocationCondition
```csharp
/// <summary>
/// 엔딩의 위치 조건을 체크합니다.
/// </summary>
/// <param name="ending">엔딩 타입</param>
/// <returns>조건 달성 여부</returns>
private bool CheckEndingLocationCondition(EndingType ending)
{
    switch (ending)
    {
        case EndingType.StealthExit:
            // 위치 무관 (홍차에 수면제를 탔다는 조건으로 대체)
            return true;
            
        case EndingType.ChaoticBreakout:
            // 거실 또는 지하실
            return currentLocation == GameLocation.LivingRoom || 
                   currentLocation == GameLocation.Basement;
            
        case EndingType.SiblingsHelp:
            // 동생의 방 (단둘이 있을 때)
            return currentLocation == GameLocation.SiblingsRoom &&
                   GetNPCLocation(NPCType.Sibling) == GameLocation.SiblingsRoom;
            
        default:
            return true;
    }
}
```

#### 4.4 CheckEndingTimeCondition
```csharp
/// <summary>
/// 엔딩의 시간 조건을 체크합니다.
/// </summary>
/// <param name="ending">엔딩 타입</param>
/// <returns>조건 달성 여부</returns>
private bool CheckEndingTimeCondition(EndingType ending)
{
    switch (ending)
    {
        case EndingType.StealthExit:
            // 시간대 무관 (홍차에 수면제를 탔다는 조건으로 대체)
            return true;
            
        case EndingType.ChaoticBreakout:
            // 시간대 무관
            return true;
            
        case EndingType.SiblingsHelp:
            // 낮 시간
            return currentTimeOfDay == TimeOfDay.Day;
            
        default:
            return true;
    }
}
```

#### 4.5 CanAchieveEnding
```csharp
/// <summary>
/// 특정 엔딩을 달성할 수 있는지 확인합니다.
/// </summary>
/// <param name="ending">엔딩 타입</param>
/// <returns>달성 가능 여부</returns>
public bool CanAchieveEnding(EndingType ending)
{
    // 1일차는 엔딩 진입 불가
    if (CurrentDay == 1)
        return false;
    
    // 불완전한 박제는 이미 처리됨
    if (ending == EndingType.UnfinishedDoll)
        return humanity <= MIN_HUMANITY;
    
    // 영원한 식사 시간은 5일차 종료 시 처리
    if (ending == EndingType.EternalDinner)
        return false;
    
    bool hasItems = CheckEndingItemCondition(ending);
    bool hasNPCStatus = CheckEndingNPCCondition(ending);
    bool hasLocation = CheckEndingLocationCondition(ending);
    bool hasTime = CheckEndingTimeCondition(ending);
    
    return hasItems && hasNPCStatus && hasLocation && hasTime;
}
```

#### 4.6 CheckEndingConditions
```csharp
/// <summary>
/// 현재 상태에서 달성 가능한 엔딩을 체크합니다.
/// 주의: 백엔드에서 엔딩 트리거를 받아오는 경우 이 메서드는 폴백(fallback) 용도로만 사용됩니다.
/// </summary>
/// <returns>달성 가능한 엔딩 타입 (없으면 None)</returns>
public EndingType CheckEndingConditions()
{
    // 불완전한 박제 체크 (최우선)
    if (humanity <= MIN_HUMANITY)
    {
        return EndingType.UnfinishedDoll;
    }
    
    // 1일차는 엔딩 진입 불가
    if (CurrentDay == 1)
    {
        return EndingType.None;
    }
    
    // 각 엔딩 체크 (우선순위: StealthExit > ChaoticBreakout > SiblingsHelp)
    if (CanAchieveEnding(EndingType.StealthExit))
    {
        return EndingType.StealthExit;
    }
    
    if (CanAchieveEnding(EndingType.ChaoticBreakout))
    {
        return EndingType.ChaoticBreakout;
    }
    
    if (CanAchieveEnding(EndingType.SiblingsHelp))
    {
        return EndingType.SiblingsHelp;
    }
    
    return EndingType.None;
}
```

#### 4.7 TriggerEnding
```csharp
/// <summary>
/// 엔딩을 트리거합니다.
/// 백엔드에서 받은 엔딩 트리거 정보를 적용합니다.
/// </summary>
/// <param name="ending">엔딩 타입 - 백엔드에서 제공</param>
public void TriggerEnding(EndingType ending)
{
    if (ending == EndingType.None)
    {
        Debug.LogWarning("[GameStateManager] 유효하지 않은 엔딩 타입입니다.");
        return;
    }
    
    currentEnding = ending;
    OnEndingTriggered?.Invoke(ending);
    
    Debug.Log($"[GameStateManager] 엔딩 트리거: {ending}");
    
    // 엔딩별 씬 전환 (씬 이름은 Inspector에서 설정)
    string endingSceneName = GetEndingSceneName(ending);
    if (!string.IsNullOrEmpty(endingSceneName))
    {
        SceneFadeManager fadeManager = FindObjectOfType<SceneFadeManager>();
        if (fadeManager != null)
        {
            fadeManager.LoadSceneWithFade(endingSceneName, gameOverFadeDuration);
        }
        else
        {
            SceneManager.LoadScene(endingSceneName);
        }
    }
}

private string GetEndingSceneName(EndingType ending)
{
    // TODO: Inspector에서 설정 가능하도록 수정
    switch (ending)
    {
        case EndingType.StealthExit:
            return "Ending_StealthExit";
        case EndingType.ChaoticBreakout:
            return "Ending_ChaoticBreakout";
        case EndingType.SiblingsHelp:
            return "Ending_SiblingsHelp";
        case EndingType.UnfinishedDoll:
            return "GameOver";
        case EndingType.EternalDinner:
            return "Ending_EternalDinner";
        default:
            return "";
    }
}
```

### 5. 이벤트 플래그 관리 메서드

#### 5.1 SetEventFlag
```csharp
/// <summary>
/// 이벤트 플래그를 설정합니다.
/// 백엔드에서 받은 이벤트 플래그 정보를 적용합니다.
/// </summary>
/// <param name="flagName">플래그 이름</param>
/// <param name="value">값 (백엔드에서 제공)</param>
public void SetEventFlag(string flagName, bool value)
{
    switch (flagName.ToLower())
    {
        case "grandmothercooperation":
            eventFlags.grandmotherCooperation = value;
            break;
        case "holeunlocked":
            eventFlags.holeUnlocked = value;
            break;
        case "firestarted":
            eventFlags.fireStarted = value;
            break;
        case "familyasleep":
            eventFlags.familyAsleep = value;
            break;
        case "teawithsleepingpill":
            eventFlags.teaWithSleepingPill = value;
            break;
        case "keystolen":
            eventFlags.keyStolen = value;
            break;
        case "caughtbyfather":
            eventFlags.caughtByFather = value;
            break;
        case "caughtbymother":
            eventFlags.caughtByMother = value;
            break;
        default:
            Debug.LogWarning($"[GameStateManager] 알 수 없는 플래그 이름: {flagName}");
            return;
    }
    
    Debug.Log($"[GameStateManager] 이벤트 플래그 설정: {flagName} = {value}");
}
```

#### 5.2 GetEventFlag
```csharp
/// <summary>
/// 이벤트 플래그 값을 반환합니다.
/// </summary>
/// <param name="flagName">플래그 이름</param>
/// <returns>플래그 값</returns>
public bool GetEventFlag(string flagName)
{
    switch (flagName.ToLower())
    {
        case "grandmothercooperation":
            return eventFlags.grandmotherCooperation;
        case "holeunlocked":
            return eventFlags.holeUnlocked;
        case "firestarted":
            return eventFlags.fireStarted;
        case "familyasleep":
            return eventFlags.familyAsleep;
        case "teawithsleepingpill":
            return eventFlags.teaWithSleepingPill;
        case "keystolen":
            return eventFlags.keyStolen;
        case "caughtbyfather":
            return eventFlags.caughtByFather;
        case "caughtbymother":
            return eventFlags.caughtByMother;
        default:
            Debug.LogWarning($"[GameStateManager] 알 수 없는 플래그 이름: {flagName}");
            return false;
    }
}
```

#### 5.3 SetCustomEvent
```csharp
/// <summary>
/// 커스텀 이벤트를 설정합니다.
/// </summary>
/// <param name="eventName">이벤트 이름</param>
/// <param name="value">값</param>
public void SetCustomEvent(string eventName, bool value)
{
    if (eventFlags.customEvents == null)
    {
        eventFlags.customEvents = new Dictionary<string, bool>();
    }
    
    eventFlags.customEvents[eventName] = value;
    Debug.Log($"[GameStateManager] 커스텀 이벤트 설정: {eventName} = {value}");
}
```

#### 5.4 GetCustomEvent
```csharp
/// <summary>
/// 커스텀 이벤트 값을 반환합니다.
/// </summary>
/// <param name="eventName">이벤트 이름</param>
/// <returns>이벤트 값 (없으면 false)</returns>
public bool GetCustomEvent(string eventName)
{
    if (eventFlags.customEvents == null || !eventFlags.customEvents.ContainsKey(eventName))
    {
        return false;
    }
    
    return eventFlags.customEvents[eventName];
}
```

### 6. AdvanceToNextDay() 통합

5일차 종료 시 엔딩 체크:
```csharp
public bool AdvanceToNextDay()
{
    if (currentDay < MAX_DAY)
    {
        // ... 기존 코드 ...
    }
    else
    {
        // 5일차 종료 시 엔딩 체크
        Debug.LogWarning($"[GameStateManager] 최대 일수({MAX_DAY}일)에 도달했습니다. 엔딩 조건을 체크합니다.");
        
        EndingType achievableEnding = CheckEndingConditions();
        
        if (achievableEnding != EndingType.None && 
            achievableEnding != EndingType.EternalDinner)
        {
            // 달성 가능한 엔딩이 있으면 트리거
            TriggerEnding(achievableEnding);
            return false;
        }
        else
        {
            // 달성 가능한 엔딩이 없으면 영원한 식사 시간 배드 엔딩
            TriggerEnding(EndingType.EternalDinner);
            return false;
        }
    }
}
```

### 7. 날짜 변경 시 턴 리셋

```csharp
public bool AdvanceToNextDay()
{
    // ... 기존 코드 ...
    
    if (!gameOverOccurred)
    {
        // 턴 수 리셋
        currentTurn = 0;
        OnTurnChanged?.Invoke(GetRemainingTurns());
        
        // 시간대를 Day로 설정
        SetTimeOfDay(TimeOfDay.Day);
        
        // ... 나머지 코드 ...
    }
}
```

### 8. 백엔드 API 연동 (엔딩 트리거)

#### 8.1 ApiClient 확장 (GameResponse 구조체 수정)

`Assets/Scripts/Ryu/Global/ApiClient.cs`의 `GameResponse` 클래스를 확장하여 엔딩 트리거 및 이벤트 플래그 정보를 받아옵니다:

**참고:** 구조체 정의는 Phase1 문서(2.6 섹션)를 참고하세요.

**Phase2, Phase3에서 이미 추가된 필드들과 함께 엔딩 트리거 및 이벤트 플래그도 추가:**

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
    
    // 아이템 변화량 (Phase3에서 추가, 선택적)
    public ItemChanges item_changes;
    
    // 이벤트 플래그 (백엔드에서 제공, 선택적)
    public EventFlags event_flags;
    
    // 엔딩 트리거 (백엔드에서 제공, 선택적)
    public string ending_trigger; // 엔딩 타입 문자열 (예: "stealth_exit", "chaotic_breakout", "siblings_help", "unfinished_doll", "eternal_dinner", null)
}

// Phase1 문서(2.6 섹션) 참고:
// - EventFlags: 이벤트 플래그 구조체
```

**구조체 정의 위치:**
- `EventFlags`는 Phase1 문서에 정의됨
- `GameResponse`는 `ApiClient.cs` 내부에 정의
- 또는 공통 구조체는 별도 파일로 분리 가능

#### 8.2 ApiClient SendMessage 메서드 확장

Phase2, Phase3에서 확장한 `SendMessage` 메서드에 이벤트 플래그 및 엔딩 트리거도 추가:

**최종 시그니처 (모든 Phase 통합):**

```csharp
/// <summary>
/// 백엔드 서버에 메시지를 전송하고 응답을 콜백으로 반환합니다.
/// </summary>
/// <param name="chatInput">사용자 입력 텍스트</param>
/// <param name="onSuccess">성공 콜백 (response, humanityChange, npcAffectionChanges, npcHumanityChanges, npcDisabledStates, npcLocations, itemChanges, eventFlags, endingTrigger)</param>
/// <param name="onError">에러 콜백</param>
public Coroutine SendMessage(
    string chatInput, 
    Action<string, float, NPCAffectionChanges, NPCHumanityChanges, NPCDisabledStates, NPCLocations, ItemChanges, EventFlags, string> onSuccess, 
    Action<string> onError)
{
    return StartCoroutine(SendMessageCoroutine(chatInput, onSuccess, onError));
}

private IEnumerator SendMessageCoroutine(
    string chatInput, 
    Action<string, float, NPCAffectionChanges, NPCHumanityChanges, NPCDisabledStates, NPCLocations, ItemChanges, EventFlags, string> onSuccess, 
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
        
        // 아이템 변화량 추출 (Phase3, null 체크 포함)
        ItemChanges itemChanges = gameResponse.item_changes ?? new ItemChanges();
        
        // 이벤트 플래그 추출 (null 체크 포함, 없으면 null)
        EventFlags eventFlags = gameResponse.event_flags;
        
        // 엔딩 트리거 추출 (null 체크 포함, 없으면 null)
        string endingTrigger = gameResponse.ending_trigger;
        
        onSuccess?.Invoke(response, humanityChange, affectionChanges, humanityChanges, disabledStates, npcLocations, itemChanges, eventFlags, endingTrigger);
    }
    catch (Exception e)
    {
        Debug.LogError($"[ApiClient] JSON 파싱 에러: {e.Message}");
        onError?.Invoke($"응답 파싱 실패: {e.Message}");
    }
}
```

**폴백 전략:**
- `event_flags`: null이면 null로 전달 (이전 상태 유지)
- `ending_trigger`: null이면 null로 전달 (엔딩 트리거 없음)

#### 8.3 InputHandler 연동

`Assets/Scripts/Ryu/Tutorial/InputHandler.cs`의 `OnApiSuccess` 메서드를 수정하여 이벤트 플래그 및 엔딩 트리거를 처리:

```csharp
private void OnApiSuccess(
    string response, 
    float humanityChange, 
    NPCAffectionChanges npcAffectionChanges, 
    NPCHumanityChanges npcHumanityChanges,
    NPCDisabledStates npcDisabledStates,
    NPCLocations npcLocations,
    ItemChanges itemChanges,
    EventFlags eventFlags,
    string endingTrigger)
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
        
        // 아이템 변화량 적용 (Phase3)
        // ... 아이템 처리 코드 ...
        
        // 이벤트 플래그 적용 (백엔드에서 제공 시만)
        if (eventFlags != null)
        {
            ApplyEventFlags(eventFlags);
        }
        
        // 엔딩 트리거 처리
        if (!string.IsNullOrEmpty(endingTrigger))
        {
            EndingType endingType = ConvertEndingNameToType(endingTrigger);
            if (endingType != EndingType.None)
            {
                // 백엔드에서 엔딩 트리거를 받았으므로 즉시 엔딩 진입
                gameStateManager.TriggerEnding(endingType);
                Debug.Log($"[InputHandler] 백엔드에서 엔딩 트리거 수신: {endingType}");
                return; // 엔딩 진입 시 더 이상 처리하지 않음
            }
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

// 이벤트 플래그 적용 헬퍼 메서드
private void ApplyEventFlags(EventFlags eventFlags)
{
    // 표준 이벤트 플래그 적용
    if (eventFlags.grandmother_cooperation.HasValue)
    {
        gameStateManager.SetEventFlag("grandmotherCooperation", eventFlags.grandmother_cooperation.Value);
    }
    
    if (eventFlags.hole_unlocked.HasValue)
    {
        gameStateManager.SetEventFlag("holeUnlocked", eventFlags.hole_unlocked.Value);
    }
    
    if (eventFlags.fire_started.HasValue)
    {
        gameStateManager.SetEventFlag("fireStarted", eventFlags.fire_started.Value);
    }
    
    if (eventFlags.family_asleep.HasValue)
    {
        gameStateManager.SetEventFlag("familyAsleep", eventFlags.family_asleep.Value);
    }
    
    if (eventFlags.tea_with_sleeping_pill.HasValue)
    {
        gameStateManager.SetEventFlag("teaWithSleepingPill", eventFlags.tea_with_sleeping_pill.Value);
    }
    
    if (eventFlags.key_stolen.HasValue)
    {
        gameStateManager.SetEventFlag("keyStolen", eventFlags.key_stolen.Value);
    }
    
    if (eventFlags.caught_by_father.HasValue)
    {
        gameStateManager.SetEventFlag("caughtByFather", eventFlags.caught_by_father.Value);
    }
    
    if (eventFlags.caught_by_mother.HasValue)
    {
        gameStateManager.SetEventFlag("caughtByMother", eventFlags.caught_by_mother.Value);
    }
    
    if (eventFlags.imprisonment_day.HasValue)
    {
        // 감금된 날짜는 별도 처리 필요 (EventFlags 구조체에 int 필드로 존재)
        // TODO: GameStateManager에 SetImprisonmentDay() 메서드 추가 필요
    }
    
    // 커스텀 이벤트 적용
    if (eventFlags.custom_events != null)
    {
        foreach (var customEvent in eventFlags.custom_events)
        {
            gameStateManager.SetCustomEvent(customEvent.Key, customEvent.Value);
        }
    }
    
    Debug.Log($"[InputHandler] 이벤트 플래그 적용 완료");
}

// 엔딩 이름을 EndingType으로 변환하는 헬퍼 메서드
// (Phase1 문서 2.6.12 섹션의 엔딩 이름 매핑 사용)
private EndingType ConvertEndingNameToType(string endingName)
{
    if (string.IsNullOrEmpty(endingName))
        return EndingType.None;
    
    // Phase1 문서의 endingNameMapping 딕셔너리 사용
    // 또는 직접 매핑
    switch (endingName.ToLower())
    {
        case "stealth_exit":
        case "stealthexit":
            return EndingType.StealthExit;
        case "chaotic_breakout":
        case "chaoticbreakout":
            return EndingType.ChaoticBreakout;
        case "siblings_help":
        case "siblingshelp":
            return EndingType.SiblingsHelp;
        case "unfinished_doll":
        case "unfinisheddoll":
            return EndingType.UnfinishedDoll;
        case "eternal_dinner":
        case "eternaldinner":
            return EndingType.EternalDinner;
        case "none":
        case "":
            return EndingType.None;
        default:
            Debug.LogWarning($"[InputHandler] 알 수 없는 엔딩 이름: {endingName}");
            return EndingType.None;
    }
}
```

**폴백 전략:**
- `eventFlags`가 null이면 이전 이벤트 플래그 상태 유지
- 각 이벤트 플래그 필드가 null이면 해당 플래그는 업데이트하지 않음 (이전 상태 유지)
- `endingTrigger`가 null이거나 빈 문자열이면 엔딩 트리거 없음

#### 8.4 백엔드 응답 예시

백엔드 API는 다음과 같은 JSON 형식으로 응답해야 합니다:

**이벤트 플래그만 있는 경우:**
```json
{
  "response": "라이터를 이용해 불을 지른다...",
  "humanity_change": 0.0,
  "event_flags": {
    "fire_started": true,
    "grandmother_cooperation": false,
    "hole_unlocked": false,
    "family_asleep": false,
    "tea_with_sleeping_pill": false,
    "key_stolen": false,
    "caught_by_father": false,
    "caught_by_mother": false,
    "imprisonment_day": -1,
    "custom_events": {}
  }
}
```

**엔딩 트리거가 있는 경우:**
```json
{
  "response": "모든 가족이 식탁에서 잠든 사이, 주인공은 새엄마의 목걸이에서 열쇠를 조심스럽게 빼낸다...",
  "humanity_change": 0.0,
  "event_flags": {
    "family_asleep": true,
    "tea_with_sleeping_pill": true,
    "key_stolen": true
  },
  "ending_trigger": "stealth_exit"
}
```

**이벤트 플래그와 엔딩 트리거가 모두 있는 경우:**
```json
{
  "response": "라이터를 이용해 불을 지른다...",
  "humanity_change": 0.0,
  "event_flags": {
    "fire_started": true,
    "grandmother_cooperation": true
  },
  "ending_trigger": "chaotic_breakout"
}
```

**모든 필드가 포함된 경우:**
```json
{
  "response": "모든 가족이 식탁에서 잠든 사이, 주인공은 새엄마의 목걸이에서 열쇠를 조심스럽게 빼낸다...",
  "humanity_change": 0.0,
  "npc_affection_changes": {
    "new_mother": 0.0,
    "new_father": 0.0,
    "sibling": 0.0,
    "dog": 0.0,
    "grandmother": 0.0
  },
  "npc_humanity_changes": {
    "new_father": 0.0,
    "sibling": 0.0,
    "dog": 0.0,
    "grandmother": 0.0
  },
  "npc_disabled_states": {
    "new_father": {
      "is_disabled": true,
      "remaining_turns": 3,
      "reason": "수면제"
    }
  },
  "npc_locations": {
    "new_mother": "kitchen",
    "new_father": "living_room"
  },
  "item_changes": {
    "acquired_items": [],
    "consumed_items": [
      {
        "item_name": "sleeping_pill",
        "count": 1
      }
    ]
  },
  "event_flags": {
    "family_asleep": true,
    "key_stolen": true
  },
  "ending_trigger": "stealth_exit"
}
```

**참고:**
- 모든 필드는 **선택적(optional)**이며, 백엔드에서 제공하지 않을 수 있음
- `event_flags`가 null이면 이전 이벤트 플래그 상태 유지
- 각 이벤트 플래그 필드가 null이면 해당 플래그는 업데이트하지 않음 (이전 상태 유지)
- `ending_trigger`가 `null`이거나 빈 문자열이면 엔딩 트리거 없음
- LLM이 플레이어의 자연어 입력을 분석하여 이벤트 플래그 및 엔딩 조건 충족 여부를 판단
- 시나리오 문서의 "엔딩 트리거 로직 테이블"을 참고하여 백엔드에서 판정

#### 8.5 엔딩 판정 우선순위

**백엔드에서 판정하는 경우:**
1. **불완전한 박제 (UnfinishedDoll)**: 인간성 0% 도달 시 (프론트엔드에서도 체크 가능)
2. **완벽한 기만 (StealthExit)**: 수면제 보유 + 홍차에 수면제를 탔다는 조건 + 새엄마 호감도 조건 (밤의 대화 전에 트리거)
3. **혼돈의 밤 (ChaoticBreakout)**: 화재 발생 + 할머니 협력 조건
4. **조력자의 희생 (SiblingsHelp)**: 동생 각성 + 희생 이벤트
5. **영원한 식사 시간 (EternalDinner)**: 5일차 종료 시 자동 (프론트엔드에서 처리)

**프론트엔드에서 체크하는 경우 (폴백):**
- `CheckEndingConditions()` 메서드는 백엔드 응답이 없거나 엔딩 트리거가 없을 때 사용
- 주로 5일차 종료 시 자동 체크용

#### 8.6 엔딩 트리거 판정 로직 (백엔드)

백엔드에서 엔딩 트리거를 판정할 때 고려사항:

1. **플레이어 행동 분석**
   - "수면제를 탄다", "모두에게 차를 권한다" → StealthExit 가능성
   - "불을 지른다", "벽난로에 기름을 붓는다" → ChaoticBreakout 가능성
   - "동생에게 사진을 보여준다", "로봇을 건네준다" → SiblingsHelp 가능성

2. **게임 상태 확인**
   - 현재 아이템 보유 상태
   - NPC 호감도/인간성 상태
   - 현재 위치 및 시간대
   - 이벤트 플래그 상태

3. **엔딩 조건 충족 여부**
   - 모든 조건이 충족되고 플레이어가 명확한 의도를 표현했을 때만 트리거
   - 조건이 일부만 충족된 경우는 트리거하지 않음

### 9. 실제 코드 수정 계획

#### 9.1 ApiClient.cs 수정 (우선순위: 높음)

**파일 위치:** `Assets/Scripts/Ryu/Global/ApiClient.cs`

**수정 단계:**

1. **구조체 정의 확인**
   - Phase1 문서(2.6 섹션)에 정의된 구조체 사용:
     - `EventFlags`
   - 또는 `ApiClient.cs`에 직접 정의 (Phase1 문서 참고)

2. **GameResponse 구조체에 이벤트 플래그 및 엔딩 트리거 필드 추가**
   ```csharp
   [Serializable]
   private class GameResponse
   {
       // ... 기존 필드들 (Phase2, Phase3 포함) ...
       public EventFlags event_flags; // 이벤트 플래그 (선택적)
       public string ending_trigger; // 엔딩 타입 문자열 (선택적)
   }
   ```

3. **SendMessage 메서드 시그니처 변경**
   - Phase2, Phase3에서 이미 확장된 시그니처에 `eventFlags`, `endingTrigger` 파라미터 추가
   - 최종 시그니처: `Action<string, float, NPCAffectionChanges, NPCHumanityChanges, NPCDisabledStates, NPCLocations, ItemChanges, EventFlags, string>`

4. **SendMessageCoroutine에서 이벤트 플래그 및 엔딩 트리거 추출 및 전달**
   ```csharp
   EventFlags eventFlags = gameResponse.event_flags;
   string endingTrigger = gameResponse.ending_trigger;
   onSuccess?.Invoke(response, humanityChange, affectionChanges, humanityChanges, disabledStates, npcLocations, itemChanges, eventFlags, endingTrigger);
   ```

**주의사항:**
- `event_flags`, `ending_trigger`가 `null`일 수 있으므로 null 체크 필요
- Phase2, Phase3의 변경사항과 함께 통합되어야 함

#### 9.2 InputHandler.cs 수정 (우선순위: 높음)

**파일 위치:** `Assets/Scripts/Ryu/Tutorial/InputHandler.cs`

**수정 단계:**

1. **OnApiSuccess 메서드 시그니처 변경**
   - Phase2, Phase3에서 이미 확장된 시그니처에 `eventFlags`, `endingTrigger` 파라미터 추가
   - 최종 시그니처: `OnApiSuccess(string response, float humanityChange, NPCAffectionChanges npcAffectionChanges, NPCHumanityChanges npcHumanityChanges, NPCDisabledStates npcDisabledStates, NPCLocations npcLocations, ItemChanges itemChanges, EventFlags eventFlags, string endingTrigger)`

2. **이벤트 플래그 처리 로직 추가**
   - `ApplyEventFlags()` 헬퍼 메서드 추가
   - `eventFlags`가 null이 아닐 때만 처리
   - 각 이벤트 플래그 필드가 null이 아닐 때만 업데이트 (이전 상태 유지)
   - 표준 이벤트 플래그 및 커스텀 이벤트 처리

3. **엔딩 트리거 처리 로직 추가**
   - `endingTrigger`가 null이 아니고 빈 문자열이 아닌 경우에만 처리
   - `ConvertEndingNameToType()` 헬퍼 메서드로 문자열을 `EndingType` enum으로 변환
   - `EndingType.None`이 아닌 경우 `GameStateManager.TriggerEnding()` 호출
   - 엔딩 트리거가 발생한 경우 `return`으로 이후 처리 중단 (턴 소모하지 않음)

4. **ConvertEndingNameToType() 헬퍼 메서드 추가**
   - 백엔드에서 받은 엔딩 이름 문자열을 `EndingType` enum으로 변환
   - 다양한 형식 지원 (snake_case, camelCase 등)
   - 알 수 없는 이름인 경우 `EndingType.None` 반환 및 경고 로그

**주의사항:**
- 이벤트 플래그가 null이면 이전 상태 유지
- 각 이벤트 플래그 필드가 null이면 해당 플래그는 업데이트하지 않음
- 엔딩 트리거가 발생하면 턴을 소모하지 않음 (이미 게임 종료)
- `GameStateManager`가 null인 경우 경고 로그만 출력
- 엔딩 트리거가 `None`인 경우 정상적으로 계속 진행

#### 9.3 GameStateManager.cs 확인 (우선순위: 낮음)

**파일 위치:** `Assets/Scripts/Ryu/Global/GameStateManager.cs`

**확인 사항:**
- `TriggerEnding(EndingType ending)` 메서드가 이미 구현되어 있는지 확인
- 엔딩 씬 전환 로직이 정상 작동하는지 확인
- `OnEndingTriggered` 이벤트가 제대로 발생하는지 확인

**필요 시 추가 작업:**
- 엔딩 시스템이 아직 구현되지 않았다면 Phase4의 다른 섹션 먼저 구현
- `TriggerEnding()` 메서드가 없으면 4.7 섹션의 코드를 참고하여 구현

#### 9.4 수정 순서 권장사항

1. **1단계: ApiClient.cs 수정**
   - `GameResponse` 구조체에 `ending_trigger` 필드 추가
   - `SendMessage` 메서드 시그니처 변경 (Phase2, Phase3와 통합)
   - `SendMessageCoroutine`에서 `ending_trigger` 추출 및 전달
   - 컴파일 에러 확인 및 수정

2. **2단계: InputHandler.cs 수정**
   - `OnApiSuccess` 시그니처 변경 (Phase2, Phase3와 통합)
   - `ConvertEndingNameToType()` 헬퍼 메서드 추가
   - 엔딩 트리거 처리 로직 추가
   - 컴파일 에러 확인 및 수정

3. **3단계: GameStateManager.cs 확인**
   - `TriggerEnding()` 메서드 존재 여부 확인
   - 필요 시 Phase4의 4.7 섹션 코드로 구현

4. **4단계: 테스트**
   - 백엔드 응답에 `ending_trigger`가 없을 때 동작 확인 (null 처리)
   - `ending_trigger`가 `null` 또는 빈 문자열일 때 정상 진행 확인
   - 각 엔딩 타입별 트리거 동작 확인
   - 알 수 없는 엔딩 이름 처리 확인
   - 엔딩 트리거 시 턴 소모가 중단되는지 확인

#### 9.5 하위 호환성 고려사항

**Phase2, Phase3와의 통합:**
- Phase2 (NPC 변화량), Phase3 (아이템 변화량), Phase4 (엔딩 트리거)는 모두 `ApiClient`와 `InputHandler`를 수정
- 각 Phase의 변경사항을 순차적으로 통합하거나, 한 번에 모두 통합 가능
- 통합 시 `SendMessage`와 `OnApiSuccess`의 시그니처가 최종적으로 다음과 같이 확장됨:
  ```csharp
  Action<string, float, NPCAffectionChanges, NPCHumanityChanges, ItemChanges, string>
  ```

**백엔드 응답 형식:**
- 백엔드에서 `ending_trigger` 필드를 제공하지 않는 경우 `null`로 처리
- 기존 백엔드와의 호환성을 위해 `ending_trigger`가 없어도 정상 동작해야 함

#### 9.6 에러 처리 및 폴백 전략

1. **이벤트 플래그가 null인 경우**
   - 이전 이벤트 플래그 상태 유지
   - 각 이벤트 플래그 필드가 null이면 해당 플래그는 업데이트하지 않음

2. **엔딩 트리거가 null인 경우**
   - 정상적으로 게임 진행
   - 프론트엔드의 `CheckEndingConditions()` 메서드로 폴백 체크 (5일차 종료 시)

3. **알 수 없는 엔딩 이름인 경우**
   - 경고 로그 출력
   - `EndingType.None`으로 처리하여 정상 진행

4. **GameStateManager가 null인 경우**
   - 경고 로그 출력
   - 이벤트 플래그 및 엔딩 트리거를 무시하고 정상 진행

5. **TriggerEnding() 호출 실패 시**
   - `GameStateManager` 내부에서 에러 처리
   - 엔딩 트리거는 받았지만 씬 전환 실패 시에도 게임은 계속 진행 가능하도록 설계

#### 9.7 테스트 체크리스트

- [ ] `event_flags`가 `null`인 경우 이전 상태 유지
- [ ] 각 이벤트 플래그 필드가 `null`인 경우 해당 플래그는 업데이트하지 않음
- [ ] 이벤트 플래그가 정상적으로 적용되는지 확인
- [ ] 커스텀 이벤트가 정상적으로 적용되는지 확인
- [ ] `ending_trigger`가 `null`인 경우 정상 진행
- [ ] `ending_trigger`가 빈 문자열인 경우 정상 진행
- [ ] `ending_trigger`가 "stealth_exit"인 경우 StealthExit 엔딩 트리거
- [ ] `ending_trigger`가 "chaotic_breakout"인 경우 ChaoticBreakout 엔딩 트리거
- [ ] `ending_trigger`가 "siblings_help"인 경우 SiblingsHelp 엔딩 트리거
- [ ] `ending_trigger`가 "unfinished_doll"인 경우 UnfinishedDoll 엔딩 트리거
- [ ] `ending_trigger`가 "eternal_dinner"인 경우 EternalDinner 엔딩 트리거
- [ ] `ending_trigger`가 "none"인 경우 정상 진행
- [ ] 알 수 없는 엔딩 이름인 경우 경고 로그 및 정상 진행
- [ ] 엔딩 트리거 발생 시 턴 소모가 중단되는지 확인
- [ ] 엔딩 트리거 발생 시 씬 전환이 정상 작동하는지 확인
- [ ] Phase2, Phase3와 통합된 시그니처가 정상 작동하는지 확인

## 완료 조건
- [ ] InitializeEndingConditions() 구현 완료
- [ ] InitializeEventFlags() 구현 완료
- [ ] SetTimeOfDay() 구현 완료
- [ ] GetCurrentTimeOfDay() 구현 완료
- [ ] ConsumeTurn() 구현 완료
- [ ] GetRemainingTurns() 구현 완료
- [ ] HasRemainingTurns() 구현 완료
- [ ] SetCurrentLocation() 구현 완료
- [ ] GetCurrentLocation() 구현 완료
- [ ] CheckEndingItemCondition() 구현 완료
- [ ] CheckEndingNPCCondition() 구현 완료
- [ ] CheckEndingLocationCondition() 구현 완료
- [ ] CheckEndingTimeCondition() 구현 완료
- [ ] CanAchieveEnding() 구현 완료
- [ ] CheckEndingConditions() 구현 완료 (폴백용)
- [ ] TriggerEnding() 구현 완료
- [ ] SetEventFlag() 구현 완료
- [ ] GetEventFlag() 구현 완료
- [ ] SetCustomEvent() 구현 완료
- [ ] GetCustomEvent() 구현 완료
- [ ] ApiClient의 GameResponse에 ending_trigger 필드 추가 완료
- [ ] ApiClient의 SendMessage 콜백 시그니처에 ending_trigger 추가 완료
- [ ] InputHandler의 OnApiSuccess에서 엔딩 트리거 처리 완료
- [ ] 엔딩 이름 매핑 딕셔너리 구현 완료
- [ ] AdvanceToNextDay()에 엔딩 체크 통합 완료 (5일차 폴백용)
- [ ] 날짜 변경 시 턴 리셋 통합 완료
- [ ] 컴파일 에러 없음

## 참고 파일
- [Assets/Scripts/Ryu/Global/GameStateManager.cs](Assets/Scripts/Ryu/Global/GameStateManager.cs)
- [Assets/Scripts/Ryu/Global/ApiClient.cs](Assets/Scripts/Ryu/Global/ApiClient.cs)
- [Assets/Scripts/Ryu/Tutorial/InputHandler.cs](Assets/Scripts/Ryu/Tutorial/InputHandler.cs)
- [마크다운/시나리오/시나리오.md](마크다운/시나리오/시나리오.md)
- [마크다운/변경사항/0210/Phase1_데이터_모델_정의.md](마크다운/변경사항/0210/Phase1_데이터_모델_정의.md)
- [마크다운/변경사항/0210/Phase2_NPC_시스템_구현.md](마크다운/변경사항/0210/Phase2_NPC_시스템_구현.md)
- [마크다운/변경사항/0210/Phase3_아이템_시스템_구현.md](마크다운/변경사항/0210/Phase3_아이템_시스템_구현.md)
- [마크다운/변경사항/0210/동적_상태_관리_정책.md](마크다운/변경사항/0210/동적_상태_관리_정책.md)

