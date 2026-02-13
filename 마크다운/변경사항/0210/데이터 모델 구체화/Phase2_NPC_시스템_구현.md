# Phase 2: GameStateManager NPC 시스템 구현

## 개요
NPC 상태 관리 시스템을 구현합니다. NPC 호감도, NPC 인간성, NPC 무력화 상태, NPC 위치 추적 등을 포함합니다.

## 목표
- NPC 상태 관리 메서드 구현 완료
- NPC 상태 초기화 로직 구현 완료
- NPC 관련 이벤트 발생 로직 구현 완료

## 작업 내용

### 1. NPC 상태 초기화

#### 1.1 Awake()에서 NPC 상태 초기화
```csharp
private void InitializeNPCStatuses()
{
    npcStatuses = new Dictionary<NPCType, NPCStatus>();
    
    // 새엄마 (엘리노어) - 최종보스, 인간성 불가
    npcStatuses[NPCType.NewMother] = new NPCStatus
    {
        npcType = NPCType.NewMother,
        affection = 50f,        // 기본 호감도
        humanity = -100f,       // 매우 큰 음수 - 절대 인간화 불가 (최종보스)
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
        humanity = -50f,      // 인간의 기억이 거의 사라진 상태 (0이 되면 기억 회복)
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
        humanity = -20f,      // 최근 개조되어 일부 기억이 남아있음 (0이 되면 기억 회복)
        isAvailable = true,
        isDisabled = false,
        disabledRemainingTurns = 0,
        disabledReason = ""
    };
    
    // 강아지 (바론)
    npcStatuses[NPCType.Dog] = new NPCStatus
    {
        npcType = NPCType.Dog,
        affection = 80f,      // 인간 상태의 주인공에게 기본 호감
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
        humanity = -100f,       // 생명력이 거의 다 빠진 상태
        isAvailable = false,   // 특수 조건 필요
        isDisabled = false,
        disabledRemainingTurns = 0,
        disabledReason = ""
    };
}
```

#### 1.2 NPC 위치 초기화
```csharp
private void InitializeNPCLocations()
{
    npcLocations = new Dictionary<NPCType, GameLocation>();
    
    // 기본 위치 설정 (씬에 따라 변경 가능)
    npcLocations[NPCType.NewMother] = GameLocation.Kitchen;
    npcLocations[NPCType.NewFather] = GameLocation.LivingRoom;
    npcLocations[NPCType.Sibling] = GameLocation.SiblingsRoom;
    npcLocations[NPCType.Dog] = GameLocation.Backyard;
    npcLocations[NPCType.Grandmother] = GameLocation.Basement;
}
```

### 2. NPC 호감도 관리 메서드

#### 2.1 ModifyAffection
```csharp
/// <summary>
/// NPC 호감도를 변경합니다.
/// 백엔드에서 받은 변화량을 적용합니다.
/// </summary>
/// <param name="npc">NPC 타입</param>
/// <param name="changeAmount">변화량 (양수: 증가, 음수: 감소) - 백엔드에서 제공</param>
public void ModifyAffection(NPCType npc, float changeAmount)
{
    if (!npcStatuses.ContainsKey(npc))
    {
        Debug.LogWarning($"[GameStateManager] NPC 상태를 찾을 수 없습니다: {npc}");
        return;
    }
    
    NPCStatus status = npcStatuses[npc];
    float oldAffection = status.affection;
    status.affection = Mathf.Clamp(status.affection + changeAmount, 0f, 100f);
    
    OnNPCStatusChanged?.Invoke(npc, status);
    Debug.Log($"[GameStateManager] {npc} 호감도 변경: {oldAffection:F1} → {status.affection:F1} (변화량: {changeAmount:F1})");
}
```

#### 2.2 GetAffection
```csharp
/// <summary>
/// NPC 호감도를 반환합니다.
/// </summary>
/// <param name="npc">NPC 타입</param>
/// <returns>호감도 (0~100)</returns>
public float GetAffection(NPCType npc)
{
    if (!npcStatuses.ContainsKey(npc))
    {
        Debug.LogWarning($"[GameStateManager] NPC 상태를 찾을 수 없습니다: {npc}");
        return 0f;
    }
    
    return npcStatuses[npc].affection;
}
```

### 3. NPC 인간성 관리 메서드

#### 3.1 ModifyNPCHumanity
```csharp
/// <summary>
/// NPC 인간성을 변경합니다. 새엄마는 변경 불가합니다.
/// 백엔드에서 받은 변화량을 적용합니다.
/// </summary>
/// <param name="npc">NPC 타입</param>
/// <param name="changeAmount">변화량 (양수: 증가, 음수: 감소) - 백엔드에서 제공</param>
public void ModifyNPCHumanity(NPCType npc, float changeAmount)
{
    if (npc == NPCType.NewMother)
    {
        Debug.LogWarning("[GameStateManager] 새엄마의 인간성은 변경할 수 없습니다.");
        return;
    }
    
    if (!npcStatuses.ContainsKey(npc))
    {
        Debug.LogWarning($"[GameStateManager] NPC 상태를 찾을 수 없습니다: {npc}");
        return;
    }
    
    NPCStatus status = npcStatuses[npc];
    float oldHumanity = status.humanity;
    status.humanity = Mathf.Clamp(status.humanity + changeAmount, -100f, 100f);
    
    OnNPCStatusChanged?.Invoke(npc, status);
    Debug.Log($"[GameStateManager] {npc} 인간성 변경: {oldHumanity:F1} → {status.humanity:F1} (변화량: {changeAmount:F1})");
}
```

#### 3.2 GetNPCHumanity
```csharp
/// <summary>
/// NPC 인간성을 반환합니다.
/// </summary>
/// <param name="npc">NPC 타입</param>
/// <returns>NPC 인간성 (-100~100)</returns>
public float GetNPCHumanity(NPCType npc)
{
    if (!npcStatuses.ContainsKey(npc))
    {
        Debug.LogWarning($"[GameStateManager] NPC 상태를 찾을 수 없습니다: {npc}");
        return 0f;
    }
    
    return npcStatuses[npc].humanity;
}
```

### 4. NPC 상태 조회 메서드

#### 4.1 GetNPCStatus
```csharp
/// <summary>
/// NPC의 전체 상태를 반환합니다.
/// </summary>
/// <param name="npc">NPC 타입</param>
/// <returns>NPC 상태 (복사본)</returns>
public NPCStatus GetNPCStatus(NPCType npc)
{
    if (!npcStatuses.ContainsKey(npc))
    {
        Debug.LogWarning($"[GameStateManager] NPC 상태를 찾을 수 없습니다: {npc}");
        return null;
    }
    
    // 복사본 반환 (원본 수정 방지)
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
```

### 5. NPC 무력화 상태 관리

#### 5.1 SetNPCDisabled
```csharp
/// <summary>
/// NPC를 무력화 상태로 설정합니다 (수면제 등).
/// 백엔드에서 받은 무력화 상태 정보를 적용합니다.
/// </summary>
/// <param name="npc">NPC 타입</param>
/// <param name="turns">무력화 지속 턴 수 (백엔드에서 제공)</param>
/// <param name="reason">무력화 이유 (백엔드에서 제공)</param>
public void SetNPCDisabled(NPCType npc, int turns, string reason)
{
    if (!npcStatuses.ContainsKey(npc))
    {
        Debug.LogWarning($"[GameStateManager] NPC 상태를 찾을 수 없습니다: {npc}");
        return;
    }
    
    NPCStatus status = npcStatuses[npc];
    status.isDisabled = true;
    status.disabledRemainingTurns = turns;
    status.disabledReason = reason;
    status.isAvailable = false;
    
    OnNPCStatusChanged?.Invoke(npc, status);
    Debug.Log($"[GameStateManager] {npc} 무력화: {turns}턴 동안 ({reason})");
}
```

#### 5.2 UpdateNPCDisabledStates
```csharp
/// <summary>
/// 턴 경과에 따라 NPC 무력화 상태를 업데이트합니다.
/// ConsumeTurn()에서 호출됩니다.
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
                status.isAvailable = true;
                status.disabledReason = "";
                Debug.Log($"[GameStateManager] {kvp.Key} 무력화 해제");
            }
            
            OnNPCStatusChanged?.Invoke(kvp.Key, status);
        }
    }
}
```

### 6. NPC 위치 관리 메서드

#### 6.1 SetNPCLocation
```csharp
/// <summary>
/// NPC의 현재 위치를 설정합니다.
/// 백엔드에서 받은 위치 정보를 적용합니다.
/// 백엔드 응답이 항상 우선: 백엔드에서 NPC 위치를 제공하면 씬 전환 업데이트보다 우선 적용됩니다.
/// </summary>
/// <param name="npc">NPC 타입</param>
/// <param name="location">위치 (백엔드에서 제공)</param>
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
            Debug.Log($"[GameStateManager] {npc} 위치 변경: {oldLocation} → {location} (백엔드 응답)");
        }
    }
}
```

#### 6.2 GetNPCLocation
```csharp
/// <summary>
/// NPC의 현재 위치를 반환합니다.
/// </summary>
/// <param name="npc">NPC 타입</param>
/// <returns>위치</returns>
public GameLocation GetNPCLocation(NPCType npc)
{
    if (!npcLocations.ContainsKey(npc))
    {
        Debug.LogWarning($"[GameStateManager] NPC 위치를 찾을 수 없습니다: {npc}");
        return GameLocation.Hallway; // 기본값
    }
    
    return npcLocations[npc];
}
```

### 7. Awake() 통합

Awake() 메서드에 초기화 호출 추가:
```csharp
private void Awake()
{
    // ... 기존 싱글톤 초기화 코드 ...
    
    // NPC 시스템 초기화
    InitializeNPCStatuses();
    InitializeNPCLocations();
}
```

### 8. 백엔드 API 연동

#### 8.1 ApiClient 확장 (GameResponse 구조체 수정)

`Assets/Scripts/Ryu/Global/ApiClient.cs`의 `GameResponse` 클래스를 확장하여 NPC 변화량, NPC 무력화 상태, NPC 위치를 받아옵니다:

**참고:** 구조체 정의는 Phase1 문서(2.6 섹션)를 참고하세요.

```csharp
[Serializable]
private class GameResponse
{
    public string response;
    public float humanity_change; // 플레이어 인간성 변화량
    
    // NPC 변화량 (백엔드에서 제공, 선택적)
    public NPCAffectionChanges npc_affection_changes;
    public NPCHumanityChanges npc_humanity_changes;
    
    // NPC 무력화 상태 (백엔드에서 제공, 선택적)
    public NPCDisabledStates npc_disabled_states;
    
    // NPC 위치 (백엔드에서 제공, 선택적)
    public NPCLocations npc_locations;
}

// Phase1 문서(2.6 섹션) 참고:
// - NPCAffectionChanges: NPC 호감도 변화량
// - NPCHumanityChanges: NPC 인간성 변화량
// - NPCDisabledState, NPCDisabledStates: NPC 무력화 상태
// - NPCLocations: NPC 위치
```

**구조체 정의 위치:**
- `NPCAffectionChanges`, `NPCHumanityChanges`, `NPCDisabledState`, `NPCDisabledStates`, `NPCLocations`는 Phase1 문서에 정의됨
- `GameResponse`는 `ApiClient.cs` 내부에 정의
- 또는 공통 구조체는 별도 파일로 분리 가능

#### 8.2 ApiClient SendMessage 메서드 확장

`SendMessage` 메서드의 콜백 시그니처를 확장하여 NPC 변화량, NPC 무력화 상태, NPC 위치도 전달:

**참고:** Phase3, Phase4와 통합 시 최종 시그니처는 `Action<string, float, NPCAffectionChanges, NPCHumanityChanges, ItemChanges, string>` (ending_trigger 포함)가 됩니다. Phase2에서는 NPC 관련 필드만 추가합니다.

```csharp
/// <summary>
/// 백엔드 서버에 메시지를 전송하고 응답을 콜백으로 반환합니다.
/// </summary>
/// <param name="chatInput">사용자 입력 텍스트</param>
/// <param name="onSuccess">성공 콜백 (response, humanityChange, npcAffectionChanges, npcHumanityChanges, npcDisabledStates, npcLocations)</param>
/// <param name="onError">에러 콜백</param>
public Coroutine SendMessage(
    string chatInput, 
    Action<string, float, NPCAffectionChanges, NPCHumanityChanges, NPCDisabledStates, NPCLocations> onSuccess, 
    Action<string> onError)
{
    return StartCoroutine(SendMessageCoroutine(chatInput, onSuccess, onError));
}

private IEnumerator SendMessageCoroutine(
    string chatInput, 
    Action<string, float, NPCAffectionChanges, NPCHumanityChanges, NPCDisabledStates, NPCLocations> onSuccess, 
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
        
        // NPC 변화량 추출 (null 체크 포함, 없으면 새 인스턴스 생성)
        NPCAffectionChanges affectionChanges = gameResponse.npc_affection_changes ?? new NPCAffectionChanges();
        NPCHumanityChanges humanityChanges = gameResponse.npc_humanity_changes ?? new NPCHumanityChanges();
        
        // NPC 무력화 상태 추출 (null 체크 포함, 없으면 null)
        NPCDisabledStates disabledStates = gameResponse.npc_disabled_states;
        
        // NPC 위치 추출 (null 체크 포함, 없으면 null)
        NPCLocations npcLocations = gameResponse.npc_locations;
        
        onSuccess?.Invoke(response, humanityChange, affectionChanges, humanityChanges, disabledStates, npcLocations);
    }
    catch (Exception e)
    {
        Debug.LogError($"[ApiClient] JSON 파싱 에러: {e.Message}");
        onError?.Invoke($"응답 파싱 실패: {e.Message}");
    }
}
```

**폴백 전략:**
- `npc_affection_changes`, `npc_humanity_changes`: null이면 빈 인스턴스 생성 (모든 값 0)
- `npc_disabled_states`, `npc_locations`: null이면 null로 전달 (이전 상태 유지)

#### 8.3 InputHandler 연동

`Assets/Scripts/Ryu/Tutorial/InputHandler.cs`의 `OnApiSuccess` 메서드를 수정하여 NPC 변화량, NPC 무력화 상태, NPC 위치를 처리:

```csharp
private void OnApiSuccess(
    string response, 
    float humanityChange, 
    NPCAffectionChanges npcAffectionChanges, 
    NPCHumanityChanges npcHumanityChanges,
    NPCDisabledStates npcDisabledStates,
    NPCLocations npcLocations)
{
    Debug.Log($"[InputHandler] 응답 수신: {response}");
    Debug.Log($"[InputHandler] 인간성 변화량: {humanityChange:F1}");

    // 응답 텍스트 표시
    resultText.text = response;

    // 플레이어 인간성 변화량 적용
    if (gameStateManager != null)
    {
        gameStateManager.ModifyHumanity(humanityChange);
        
        // NPC 호감도 변화량 적용
        if (npcAffectionChanges != null)
        {
            if (npcAffectionChanges.new_mother != 0f)
                gameStateManager.ModifyAffection(NPCType.NewMother, npcAffectionChanges.new_mother);
            if (npcAffectionChanges.new_father != 0f)
                gameStateManager.ModifyAffection(NPCType.NewFather, npcAffectionChanges.new_father);
            if (npcAffectionChanges.sibling != 0f)
                gameStateManager.ModifyAffection(NPCType.Sibling, npcAffectionChanges.sibling);
            if (npcAffectionChanges.dog != 0f)
                gameStateManager.ModifyAffection(NPCType.Dog, npcAffectionChanges.dog);
            if (npcAffectionChanges.grandmother != 0f)
                gameStateManager.ModifyAffection(NPCType.Grandmother, npcAffectionChanges.grandmother);
        }
        
        // NPC 인간성 변화량 적용
        if (npcHumanityChanges != null)
        {
            if (npcHumanityChanges.new_father != 0f)
                gameStateManager.ModifyNPCHumanity(NPCType.NewFather, npcHumanityChanges.new_father);
            if (npcHumanityChanges.sibling != 0f)
                gameStateManager.ModifyNPCHumanity(NPCType.Sibling, npcHumanityChanges.sibling);
            if (npcHumanityChanges.dog != 0f)
                gameStateManager.ModifyNPCHumanity(NPCType.Dog, npcHumanityChanges.dog);
            if (npcHumanityChanges.grandmother != 0f)
                gameStateManager.ModifyNPCHumanity(NPCType.Grandmother, npcHumanityChanges.grandmother);
        }
        
        // NPC 무력화 상태 적용 (백엔드에서 제공 시만)
        if (npcDisabledStates != null)
        {
            ApplyNPCDisabledStates(npcDisabledStates);
        }
        
        // NPC 위치 업데이트 (백엔드에서 제공 시만, 백엔드 응답이 항상 우선)
        if (npcLocations != null)
        {
            ApplyNPCLocations(npcLocations);
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
    else
    {
        Debug.LogWarning("[InputHandler] TurnManager가 연결되지 않았습니다.");
    }
}

// NPC 무력화 상태 적용 헬퍼 메서드
private void ApplyNPCDisabledStates(NPCDisabledStates disabledStates)
{
    if (disabledStates.new_father != null && disabledStates.new_father.is_disabled)
    {
        gameStateManager.SetNPCDisabled(
            NPCType.NewFather, 
            disabledStates.new_father.remaining_turns, 
            disabledStates.new_father.reason
        );
    }
    
    if (disabledStates.sibling != null && disabledStates.sibling.is_disabled)
    {
        gameStateManager.SetNPCDisabled(
            NPCType.Sibling, 
            disabledStates.sibling.remaining_turns, 
            disabledStates.sibling.reason
        );
    }
    
    if (disabledStates.dog != null && disabledStates.dog.is_disabled)
    {
        gameStateManager.SetNPCDisabled(
            NPCType.Dog, 
            disabledStates.dog.remaining_turns, 
            disabledStates.dog.reason
        );
    }
    
    if (disabledStates.grandmother != null && disabledStates.grandmother.is_disabled)
    {
        gameStateManager.SetNPCDisabled(
            NPCType.Grandmother, 
            disabledStates.grandmother.remaining_turns, 
            disabledStates.grandmother.reason
        );
    }
    
    // 새엄마는 무력화 불가 (최종보스)
}

// NPC 위치 적용 헬퍼 메서드 (위치 이름 매핑 사용)
private void ApplyNPCLocations(NPCLocations npcLocations)
{
    // 위치 이름 매핑 헬퍼 사용 (Phase1 문서 2.6.12 섹션 참고)
    if (!string.IsNullOrEmpty(npcLocations.new_mother))
    {
        GameLocation location = ConvertLocationNameToType(npcLocations.new_mother);
        gameStateManager.SetNPCLocation(NPCType.NewMother, location);
    }
    
    if (!string.IsNullOrEmpty(npcLocations.new_father))
    {
        GameLocation location = ConvertLocationNameToType(npcLocations.new_father);
        gameStateManager.SetNPCLocation(NPCType.NewFather, location);
    }
    
    if (!string.IsNullOrEmpty(npcLocations.sibling))
    {
        GameLocation location = ConvertLocationNameToType(npcLocations.sibling);
        gameStateManager.SetNPCLocation(NPCType.Sibling, location);
    }
    
    if (!string.IsNullOrEmpty(npcLocations.dog))
    {
        GameLocation location = ConvertLocationNameToType(npcLocations.dog);
        gameStateManager.SetNPCLocation(NPCType.Dog, location);
    }
    
    if (!string.IsNullOrEmpty(npcLocations.grandmother))
    {
        GameLocation location = ConvertLocationNameToType(npcLocations.grandmother);
        gameStateManager.SetNPCLocation(NPCType.Grandmother, location);
    }
}

// 위치 이름을 GameLocation enum으로 변환하는 헬퍼 메서드
// (Phase1 문서 2.6.12 섹션의 위치 이름 매핑 사용)
private GameLocation ConvertLocationNameToType(string locationName)
{
    // Phase1 문서의 locationNameMapping 딕셔너리 사용
    // 또는 직접 매핑
    switch (locationName?.ToLower())
    {
        case "players_room": return GameLocation.PlayersRoom;
        case "hallway": return GameLocation.Hallway;
        case "living_room": return GameLocation.LivingRoom;
        case "kitchen": return GameLocation.Kitchen;
        case "siblings_room": return GameLocation.SiblingsRoom;
        case "basement": return GameLocation.Basement;
        case "backyard": return GameLocation.Backyard;
        default:
            Debug.LogWarning($"[InputHandler] 알 수 없는 위치 이름: {locationName}");
            return GameLocation.Hallway; // 기본값
    }
}
```

**폴백 전략:**
- `npcDisabledStates`가 null이면 이전 무력화 상태 유지
- `npcLocations`가 null이면 이전 NPC 위치 유지
- 백엔드에서 NPC 위치를 제공하면 **백엔드 응답이 항상 우선** (씬 전환 업데이트보다 우선)
- 위치 이름이 알 수 없는 경우 기본값(`GameLocation.Hallway`) 사용

#### 8.4 백엔드 응답 예시

백엔드 API는 다음과 같은 JSON 형식으로 응답해야 합니다:

**NPC 변화량만 있는 경우:**
```json
{
  "response": "엘리노어가 당신을 빤히 쳐다본다...",
  "humanity_change": -5.0,
  "npc_affection_changes": {
    "new_mother": -10.0,
    "new_father": 0.0,
    "sibling": 5.0,
    "dog": 0.0,
    "grandmother": 0.0
  },
  "npc_humanity_changes": {
    "new_father": 0.0,
    "sibling": 10.0,
    "dog": 0.0,
    "grandmother": 0.0
  }
}
```

**NPC 무력화 상태가 있는 경우:**
```json
{
  "response": "모두에게 차를 권하며 수면제를 탄다...",
  "humanity_change": 0.0,
  "npc_disabled_states": {
    "new_father": {
      "is_disabled": true,
      "remaining_turns": 3,
      "reason": "수면제"
    },
    "sibling": {
      "is_disabled": true,
      "remaining_turns": 3,
      "reason": "수면제"
    }
  }
}
```

**NPC 위치가 있는 경우:**
```json
{
  "response": "거실로 이동한다...",
  "humanity_change": 0.0,
  "npc_locations": {
    "new_mother": "kitchen",
    "new_father": "living_room",
    "sibling": "siblings_room",
    "dog": "backyard",
    "grandmother": "basement"
  }
}
```

**모든 필드가 포함된 경우:**
```json
{
  "response": "엘리노어가 당신을 빤히 쳐다본다...",
  "humanity_change": -5.0,
  "npc_affection_changes": {
    "new_mother": -10.0,
    "new_father": 0.0,
    "sibling": 5.0,
    "dog": 0.0,
    "grandmother": 0.0
  },
  "npc_humanity_changes": {
    "new_father": 0.0,
    "sibling": 10.0,
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
  }
}
```

**참고:**
- 모든 필드는 **선택적(optional)**이며, 백엔드에서 제공하지 않을 수 있음
- 변화량이 0이면 해당 필드를 생략하거나 0으로 전달 가능
- 새엄마의 humanity는 항상 변경 불가 (백엔드에서도 전달하지 않음)
- 새엄마는 무력화 불가 (최종보스)
- 백엔드에서 필드를 제공하지 않으면 **이전 상태를 유지**
- 초기 상태이거나 이전 상태가 없으면 **프론트엔드에서 기본값 사용**
- **백엔드 응답이 항상 우선**: NPC 위치는 백엔드에서 제공하면 씬 전환 업데이트보다 우선 적용

### 9. 실제 코드 수정 계획

#### 9.1 ApiClient.cs 수정 (우선순위: 높음)

**파일 위치:** `Assets/Scripts/Ryu/Global/ApiClient.cs`

**수정 단계:**

1. **구조체 정의 확인**
   - Phase1 문서(2.6 섹션)에 정의된 구조체 사용:
     - `NPCAffectionChanges`
     - `NPCHumanityChanges`
     - `NPCDisabledState`, `NPCDisabledStates`
     - `NPCLocations`
   - 또는 `ApiClient.cs`에 직접 정의 (Phase1 문서 참고)

2. **GameResponse 클래스 확장**
   - `GameResponse`에 다음 필드 추가 (모두 선택적):
     - `NPCAffectionChanges npc_affection_changes`
     - `NPCHumanityChanges npc_humanity_changes`
     - `NPCDisabledStates npc_disabled_states`
     - `NPCLocations npc_locations`

3. **SendMessage 메서드 시그니처 변경**
   - 기존: `Action<string, float> onSuccess`
   - 변경: `Action<string, float, NPCAffectionChanges, NPCHumanityChanges, NPCDisabledStates, NPCLocations> onSuccess`
   - 타임아웃 시 목업 데이터도 새 시그니처에 맞게 수정

4. **SendMessageCoroutine 메서드 수정**
   - JSON 파싱 후 NPC 변화량, NPC 무력화 상태, NPC 위치 추출
   - null 체크 및 기본값 처리
   - `npc_disabled_states`, `npc_locations`는 null일 수 있음 (이전 상태 유지)
   - 콜백 호출 시 모든 NPC 관련 데이터 포함

**주의사항:**
- 기존 코드와의 호환성을 위해 하위 호환성 고려 필요
- 타임아웃 시 목업 데이터에서 NPC 변화량은 모두 0으로 설정
- NPC 무력화 상태와 NPC 위치는 null로 설정 (이전 상태 유지)

#### 9.2 InputHandler.cs 수정 (우선순위: 높음)

**파일 위치:** `Assets/Scripts/Ryu/Tutorial/InputHandler.cs`

**수정 단계:**

1. **OnApiSuccess 메서드 시그니처 변경**
   - 기존: `OnApiSuccess(string response, float humanityChange)`
   - 변경: `OnApiSuccess(string response, float humanityChange, NPCAffectionChanges npcAffectionChanges, NPCHumanityChanges npcHumanityChanges, NPCDisabledStates npcDisabledStates, NPCLocations npcLocations)`

2. **NPC 변화량 처리 로직 추가**
   - `GameStateManager` 참조 확인
   - NPC 호감도 변화량 적용 (5명)
   - NPC 인간성 변화량 적용 (4명, 새엄마 제외)
   - null 체크 및 0 값 체크

3. **NPC 무력화 상태 처리 로직 추가**
   - `ApplyNPCDisabledStates()` 헬퍼 메서드 추가
   - `npcDisabledStates`가 null이 아닐 때만 처리
   - 각 NPC별 null 체크 및 `is_disabled` 체크
   - 새엄마는 무력화 불가 (최종보스)

4. **NPC 위치 처리 로직 추가**
   - `ApplyNPCLocations()` 헬퍼 메서드 추가
   - `ConvertLocationNameToType()` 헬퍼 메서드 추가 (위치 이름 매핑)
   - `npcLocations`가 null이 아닐 때만 처리
   - 각 NPC별 위치 문자열 null/빈 문자열 체크
   - 백엔드 응답이 항상 우선 (씬 전환 업데이트보다 우선)

5. **디버그 로그 추가**
   - NPC 변화량 로그 출력 (변화가 있는 경우만)
   - NPC 무력화 상태 로그 출력
   - NPC 위치 변경 로그 출력

**주의사항:**
- `GameStateManager`가 null인 경우 경고 로그만 출력하고 계속 진행
- 변화량이 0인 경우 불필요한 호출 방지
- `npcDisabledStates`, `npcLocations`가 null이면 이전 상태 유지
- 위치 이름이 알 수 없는 경우 기본값(`GameLocation.Hallway`) 사용

#### 9.3 GameStateManager.cs 수정 (우선순위: 중간)

**파일 위치:** `Assets/Scripts/Ryu/Global/GameStateManager.cs`

**확인 사항:**
- `ModifyAffection()` 메서드가 이미 구현되어 있는지 확인
- `ModifyNPCHumanity()` 메서드가 이미 구현되어 있는지 확인
- `SetNPCDisabled()` 메서드가 이미 구현되어 있는지 확인
- `SetNPCLocation()` 메서드가 이미 구현되어 있는지 확인
- `UpdateNPCDisabledStates()` 메서드가 이미 구현되어 있는지 확인 (턴 소모 시 자동 감소)
- NPC 상태 초기화가 완료되었는지 확인

**필요 시 추가 작업:**
- NPC 시스템이 아직 구현되지 않았다면 Phase2의 다른 섹션 먼저 구현
- `SetNPCDisabled()` 메서드는 백엔드에서 받은 `remaining_turns`를 그대로 사용
- `SetNPCLocation()` 메서드는 백엔드에서 받은 위치를 적용 (백엔드 응답 우선)

#### 9.4 수정 순서 권장사항

1. **1단계: Phase1 문서 확인**
   - 백엔드 API 응답 구조체 정의 확인 (2.6 섹션)
   - 구조체가 `ApiClient.cs` 또는 공통 파일에 정의되어 있는지 확인

2. **2단계: ApiClient.cs 수정**
   - GameResponse 구조체 확장 (NPC 무력화 상태, NPC 위치 추가)
   - SendMessage 메서드 시그니처 변경
   - SendMessageCoroutine에서 NPC 무력화 상태, NPC 위치 추출
   - 컴파일 에러 확인 및 수정

3. **3단계: InputHandler.cs 수정**
   - OnApiSuccess 시그니처 변경
   - NPC 변화량 처리 로직 추가
   - NPC 무력화 상태 처리 로직 추가 (`ApplyNPCDisabledStates` 헬퍼)
   - NPC 위치 처리 로직 추가 (`ApplyNPCLocations`, `ConvertLocationNameToType` 헬퍼)
   - 컴파일 에러 확인 및 수정

4. **4단계: 테스트**
   - 백엔드 응답이 없을 때 동작 확인 (null 처리, 이전 상태 유지)
   - NPC 변화량이 0일 때 동작 확인
   - NPC 무력화 상태가 null일 때 이전 상태 유지 확인
   - NPC 위치가 null일 때 이전 상태 유지 확인
   - 백엔드 NPC 위치와 씬 전환 업데이트 충돌 시 백엔드 우선 확인
   - 위치 이름 매핑 정상 작동 확인
   - 실제 API 호출 테스트

#### 9.5 하위 호환성 고려사항

**문제점:**
- 기존 코드에서 `SendMessage`를 호출하는 곳이 있을 수 있음
- 기존 시그니처와 새 시그니처가 다름

**해결 방안:**

**옵션 1: 오버로딩 사용 (권장)**
```csharp
// 기존 메서드 유지 (하위 호환성)
public Coroutine SendMessage(string chatInput, Action<string, float> onSuccess, Action<string> onError)
{
    return SendMessage(chatInput, 
        (response, humanity, aff, hum, disabled, locations) => onSuccess(response, humanity), 
        onError);
}

// 새 메서드 추가 (NPC 무력화 상태, NPC 위치 포함)
public Coroutine SendMessage(
    string chatInput, 
    Action<string, float, NPCAffectionChanges, NPCHumanityChanges, NPCDisabledStates, NPCLocations> onSuccess, 
    Action<string> onError)
{
    return StartCoroutine(SendMessageCoroutine(chatInput, onSuccess, onError));
}
```

**옵션 2: 기존 코드 모두 수정**
- 모든 호출부를 새 시그니처로 변경
- InputHandler만 수정하면 되는 경우 이 방법 사용 가능
- Phase3, Phase4와 통합 시 최종 시그니처로 확장 필요

#### 9.6 에러 처리 및 폴백 전략

1. **JSON 파싱 실패 시**
   - 기존처럼 에러 콜백 호출
   - NPC 변화량은 모두 0으로 처리
   - NPC 무력화 상태와 NPC 위치는 null로 처리 (이전 상태 유지)

2. **NPC 변화량 필드가 없는 경우**
   - null 체크 후 빈 객체로 처리
   - 모든 변화량을 0으로 간주

3. **NPC 무력화 상태 필드가 없는 경우**
   - null로 처리
   - 이전 무력화 상태 유지
   - 프론트엔드에서 자동으로 처리하지 않음

4. **NPC 위치 필드가 없는 경우**
   - null로 처리
   - 이전 NPC 위치 유지
   - 초기 상태이거나 이전 위치가 없으면 프론트엔드 기본값 사용

5. **위치 이름 매핑 실패 시**
   - 알 수 없는 위치 이름인 경우 기본값(`GameLocation.Hallway`) 사용
   - 경고 로그 출력

6. **타임아웃 시**
   - 목업 응답 반환
   - NPC 변화량은 모두 0으로 설정
   - NPC 무력화 상태와 NPC 위치는 null로 설정 (이전 상태 유지)

#### 9.7 테스트 체크리스트

- [ ] ApiClient 컴파일 에러 없음
- [ ] InputHandler 컴파일 에러 없음
- [ ] 기존 API 호출 동작 확인 (하위 호환성)
- [ ] NPC 변화량이 있는 응답 처리 확인
- [ ] NPC 변화량이 없는 응답 처리 확인 (null 체크)
- [ ] NPC 무력화 상태가 있는 응답 처리 확인
- [ ] NPC 무력화 상태가 없는 응답 처리 확인 (null 체크, 이전 상태 유지)
- [ ] NPC 위치가 있는 응답 처리 확인
- [ ] NPC 위치가 없는 응답 처리 확인 (null 체크, 이전 상태 유지)
- [ ] 백엔드 NPC 위치와 씬 전환 업데이트 충돌 시 백엔드 우선 확인
- [ ] 위치 이름 매핑 정상 작동 확인
- [ ] 알 수 없는 위치 이름 처리 확인 (기본값 사용)
- [ ] 타임아웃 시 동작 확인
- [ ] 새엄마 humanity 변경 시도 시 차단 확인
- [ ] 새엄마 무력화 시도 시 차단 확인
- [ ] NPC 변화량 로그 출력 확인
- [ ] NPC 무력화 상태 로그 출력 확인
- [ ] NPC 위치 변경 로그 출력 확인

## 완료 조건
- [ ] InitializeNPCStatuses() 구현 완료
- [ ] InitializeNPCLocations() 구현 완료
- [ ] ModifyAffection() 구현 완료
- [ ] GetAffection() 구현 완료
- [ ] ModifyNPCHumanity() 구현 완료
- [ ] GetNPCHumanity() 구현 완료
- [ ] GetNPCStatus() 구현 완료
- [ ] SetNPCDisabled() 구현 완료
- [ ] UpdateNPCDisabledStates() 구현 완료
- [ ] SetNPCLocation() 구현 완료
- [ ] GetNPCLocation() 구현 완료
- [ ] ApiClient의 GameResponse에 NPC 변화량 필드 추가 완료
- [ ] ApiClient의 GameResponse에 NPC 무력화 상태 필드 추가 완료
- [ ] ApiClient의 GameResponse에 NPC 위치 필드 추가 완료
- [ ] ApiClient의 SendMessage 콜백 시그니처 확장 완료 (NPC 무력화 상태, NPC 위치 포함)
- [ ] InputHandler의 OnApiSuccess에서 NPC 변화량 처리 완료
- [ ] InputHandler의 OnApiSuccess에서 NPC 무력화 상태 처리 완료
- [ ] InputHandler의 OnApiSuccess에서 NPC 위치 처리 완료
- [ ] 위치 이름 매핑 헬퍼 메서드 구현 완료
- [ ] 백엔드 응답이 없을 때 폴백 전략 구현 완료 (이전 상태 유지)
- [ ] 백엔드 응답 우선순위 로직 구현 완료 (NPC 위치)
- [ ] 모든 메서드에서 이벤트 발생 확인
- [ ] 컴파일 에러 없음

## 참고 파일
- [Assets/Scripts/Ryu/Global/GameStateManager.cs](Assets/Scripts/Ryu/Global/GameStateManager.cs)
- [Assets/Scripts/Ryu/Global/ApiClient.cs](Assets/Scripts/Ryu/Global/ApiClient.cs)
- [Assets/Scripts/Ryu/Tutorial/InputHandler.cs](Assets/Scripts/Ryu/Tutorial/InputHandler.cs)
- [마크다운/시나리오/시나리오.md](마크다운/시나리오/시나리오.md)
- [마크다운/변경사항/0208/humanity_system_implementation.md](마크다운/변경사항/0208/humanity_system_implementation.md)
- [마크다운/변경사항/0210/Phase1_데이터_모델_정의.md](마크다운/변경사항/0210/Phase1_데이터_모델_정의.md)
- [마크다운/변경사항/0210/동적_상태_관리_정책.md](마크다운/변경사항/0210/동적_상태_관리_정책.md)

