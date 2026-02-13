# Json.NET (Newtonsoft.Json) 도입 가이드

## 개요
백엔드 API 응답 형식이 Dictionary 구조를 포함하고 있어, Unity의 기본 `JsonUtility`로는 파싱이 불가능합니다. Json.NET을 도입하여 백엔드 응답을 정상적으로 파싱할 수 있도록 합니다.

## 목표
- Json.NET 패키지 설치 완료
- 백엔드 응답 구조체 정의 완료
- ApiClient에서 Json.NET 사용하도록 수정 완료
- 백엔드 응답을 현재 구조로 변환하는 로직 구현 완료

## 배경: 왜 Json.NET이 필요한가?

### 문제점
백엔드 응답 형식:
```json
{
  "narrative": "...",
  "ending_info": null,
  "state_delta": {
    "npc_stats": {           // ← Dictionary<string, NPCStats>
      "stepmother": {...},
      "brother": {...}
    },
    "flags": {               // ← Dictionary<string, bool>
      "met_mother": true,
      "heard_rumor": true
    },
    "locks": {               // ← Dictionary<string, bool>
      "basement_door": false
    },
    "vars": {                // ← Dictionary<string, float>
      "investigation_progress": 10
    }
  }
}
```

Unity의 `JsonUtility`는 **Dictionary를 지원하지 않습니다**. 따라서 위와 같은 구조를 파싱할 수 없습니다.

### 해결책
Json.NET (Newtonsoft.Json)은 Dictionary를 완벽하게 지원하며, Unity에서도 사용 가능한 공식 NuGet 패키지입니다.

## 1단계: Json.NET 패키지 설치

### 방법 A: Unity Package Manager 사용 (권장)

1. Unity 에디터에서 `Window` → `Package Manager` 열기
2. 왼쪽 상단 `+` 버튼 클릭 → `Add package by name...` 선택
3. 패키지 이름 입력:
   ```
   com.unity.nuget.newtonsoft-json
   ```
4. `Add` 버튼 클릭
5. Unity가 자동으로 패키지를 다운로드하고 설치합니다

### 방법 B: manifest.json 직접 수정

`Packages/manifest.json` 파일의 `dependencies` 섹션에 다음 줄 추가:

```json
{
  "dependencies": {
    "com.unity.nuget.newtonsoft-json": "3.2.1",
    // ... 기존 패키지들 ...
  }
}
```

Unity가 자동으로 패키지를 다운로드합니다.

### 설치 확인
설치가 완료되면 Unity 에디터에서 다음 코드가 컴파일되어야 합니다:
```csharp
using Newtonsoft.Json;
```

## 2단계: 백엔드 요청/응답 구조체 정의

### 2.0 백엔드 요청 구조체

백엔드로 전송하는 요청 형식은 기존 `ApiClient.cs`의 `StepRequest`와 동일합니다:

```csharp
[Serializable]
private class StepRequest
{
    public string chat_input;   // 사용자 입력 텍스트
    public string npc_name;    // NPC 이름 (선택적, null 가능)
    public string item_name;   // 아이템 이름 (선택적, null 가능)
}
```

**참고**: 요청은 Dictionary가 없으므로 기존 `JsonUtility`를 사용해도 됩니다.

### 2.1 백엔드 응답 구조체 정의

`GameDataTypes.cs` 파일에 백엔드 응답 구조체를 추가합니다.

### 2.1 백엔드 NPC 통계 정보

```csharp
/// <summary>
/// 백엔드 응답의 NPC 통계 정보
/// </summary>
[Serializable]
public class BackendNPCStats
{
    public float trust;         // 신뢰도 (→ affection으로 매핑)
    public float suspicion;     // 의심도 (새로운 개념)
    public float fear;          // 공포도 (새로운 개념)
}
```

**중요**: 백엔드 응답에서 일부 NPC는 일부 필드만 포함할 수 있습니다. 예를 들어:
- `stepmother`: `trust`, `suspicion`만 있고 `fear`는 없음
- `brother`: `fear`만 있고 `trust`, `suspicion`는 없음

Json.NET은 없는 필드를 기본값(0)으로 처리하므로, 변환 로직에서 값이 0이 아닐 때만 적용하도록 처리해야 합니다.

### 2.2 백엔드 엔딩 정보

```csharp
/// <summary>
/// 백엔드 응답의 엔딩 정보
/// </summary>
[Serializable]
public class BackendEndingInfo
{
    public string ending_type;  // 예: "stealth_exit"
    public string description;  // 선택적
}
```

### 2.3 백엔드 상태 변화량 (State Delta)

```csharp
/// <summary>
/// 백엔드 응답의 상태 변화량 (State Delta)
/// </summary>
[Serializable]
public class BackendStateDelta
{
    [JsonProperty("npc_stats")]
    public Dictionary<string, BackendNPCStats> npc_stats;  // Dictionary 지원!
    
    [JsonProperty("flags")]
    public Dictionary<string, bool> flags;  // Dictionary 지원!
    
    [JsonProperty("inventory_add")]
    public List<string> inventory_add;  // string[] 대신 List 사용 권장
    
    [JsonProperty("inventory_remove")]
    public List<string> inventory_remove;
    
    [JsonProperty("locks")]
    public Dictionary<string, bool> locks;  // 새로운 필드
    
    [JsonProperty("vars")]
    public Dictionary<string, float> vars;  // 새로운 필드
    
    [JsonProperty("turn_increment")]
    public int turn_increment;  // 새로운 필드
}
```

**중요**: `[JsonProperty]` 속성을 사용하여 JSON 필드 이름과 C# 필드 이름을 매핑합니다.

### 2.4 백엔드 디버그 정보

```csharp
/// <summary>
/// 백엔드 응답의 디버그 정보
/// </summary>
[Serializable]
public class BackendDebugInfo
{
    [JsonProperty("game_id")]
    public int game_id;
    
    [JsonProperty("steps")]
    public List<BackendStepInfo> steps;
    
    [JsonProperty("turn_after")]
    public int turn_after;
}

/// <summary>
/// 백엔드 응답의 스텝 정보
/// </summary>
[Serializable]
public class BackendStepInfo
{
    [JsonProperty("step")]
    public string step;
    
    [JsonProperty("state_delta")]
    public object state_delta;  // 동적 타입
    
    [JsonProperty("reached")]
    public bool reached;
    
    [JsonProperty("newly_unlocked")]
    public List<string> newly_unlocked;
}
```

### 2.5 백엔드 전체 응답 구조

```csharp
/// <summary>
/// 백엔드 전체 응답 구조
/// </summary>
[Serializable]
public class BackendGameResponse
{
    [JsonProperty("narrative")]
    public string narrative;  // → response로 매핑
    
    [JsonProperty("ending_info")]
    public BackendEndingInfo ending_info;  // → ending_trigger로 변환
    
    [JsonProperty("state_delta")]
    public BackendStateDelta state_delta;
    
    [JsonProperty("debug")]
    public BackendDebugInfo debug;  // 선택적 (로깅용)
}
```

## 3단계: ApiClient.cs 수정

### 3.1 네임스페이스 추가

`ApiClient.cs` 파일 상단에 다음 using 문 추가:

```csharp
using Newtonsoft.Json;  // Json.NET 네임스페이스 추가
```

### 3.2 응답 파싱 로직 수정

`SendMessageCoroutine` 메서드의 응답 파싱 부분을 수정:

```csharp
private IEnumerator SendMessageCoroutine(
    string chatInput, 
    Action<string, float, NPCAffectionChanges, NPCHumanityChanges, NPCDisabledStates, NPCLocations, ItemChanges, EventFlags, string> onSuccess, 
    Action<string> onError)
{
    // ... 기존 요청 코드 (JsonUtility 사용) ...
    
    string responseText = request.downloadHandler.text;
    Debug.Log($"[ApiClient] 응답 수신: {responseText}");

    try
    {
        // Json.NET을 사용하여 백엔드 응답 파싱
        BackendGameResponse backendResponse = JsonConvert.DeserializeObject<BackendGameResponse>(responseText);
        
        // 백엔드 응답을 현재 구조로 변환
        ConvertBackendResponseToCurrentFormat(
            backendResponse,
            out string response,
            out float humanityChange,
            out NPCAffectionChanges affectionChanges,
            out NPCHumanityChanges humanityChanges,
            out NPCDisabledStates disabledStates,
            out NPCLocations npcLocations,
            out ItemChanges itemChanges,
            out EventFlags eventFlags,
            out string endingTrigger
        );
        
        onSuccess?.Invoke(response, humanityChange, affectionChanges, humanityChanges, disabledStates, npcLocations, itemChanges, eventFlags, endingTrigger);
    }
    catch (JsonException e)
    {
        Debug.LogError($"[ApiClient] JSON 파싱 에러: {e.Message}");
        Debug.LogError($"[ApiClient] JSON 내용: {responseText}");
        onError?.Invoke($"응답 파싱 실패: {e.Message}");
    }
    catch (Exception e)
    {
        Debug.LogError($"[ApiClient] 예상치 못한 에러: {e.Message}");
        onError?.Invoke($"처리 실패: {e.Message}");
    }
}
```

**중요**: 요청 전송은 기존 `JsonUtility`를 사용해도 됩니다 (Dictionary가 없으므로).

### 3.3 변환 메서드 구현

백엔드 응답을 현재 구조로 변환하는 메서드를 추가:

```csharp
/// <summary>
/// 백엔드 응답을 현재 구조로 변환합니다.
/// </summary>
private void ConvertBackendResponseToCurrentFormat(
    BackendGameResponse backendResponse,
    out string response,
    out float humanityChange,
    out NPCAffectionChanges affectionChanges,
    out NPCHumanityChanges humanityChanges,
    out NPCDisabledStates disabledStates,
    out NPCLocations npcLocations,
    out ItemChanges itemChanges,
    out EventFlags eventFlags,
    out string endingTrigger)
{
    // 1. narrative → response
    response = !string.IsNullOrEmpty(backendResponse.narrative) 
        ? backendResponse.narrative 
        : MOCK_RESPONSE;

    // 2. ending_info → ending_trigger
    endingTrigger = backendResponse.ending_info?.ending_type;

    // 3. humanity_change는 백엔드 응답에 없으므로 0으로 설정
    // (또는 state_delta.vars에서 가져올 수 있다면 그렇게 처리)
    humanityChange = 0f;
    if (backendResponse.state_delta?.vars != null 
        && backendResponse.state_delta.vars.ContainsKey("humanity_change"))
    {
        humanityChange = backendResponse.state_delta.vars["humanity_change"];
    }

    // 4. state_delta.npc_stats → npc_affection_changes, npc_humanity_changes
    affectionChanges = new NPCAffectionChanges();
    humanityChanges = new NPCHumanityChanges();

    if (backendResponse.state_delta?.npc_stats != null)
    {
        foreach (var kvp in backendResponse.state_delta.npc_stats)
        {
            string npcName = kvp.Key;
            BackendNPCStats stats = kvp.Value;

            // NPC 이름 매핑 (백엔드 이름 → Unity enum)
            switch (npcName.ToLower())
            {
                case "stepmother":
                case "new_mother":
                    // trust를 affection으로 매핑 (값이 0이 아닐 때만 적용)
                    if (stats.trust != 0f)
                        affectionChanges.new_mother = stats.trust;
                    // suspicion은 현재 구조에 없으므로 무시하거나 로깅
                    if (stats.suspicion != 0f)
                        Debug.Log($"[ApiClient] stepmother suspicion: {stats.suspicion} (현재 구조에 없어 무시됨)");
                    break;

                case "new_father":
                case "father":
                    if (stats.trust != 0f)
                        affectionChanges.new_father = stats.trust;
                    if (stats.fear != 0f)
                        humanityChanges.new_father = stats.fear;  // fear를 humanity로 매핑 (임시)
                    break;

                case "sibling":
                case "brother":
                    // 백엔드 예시: brother는 fear만 있음
                    if (stats.trust != 0f)
                        affectionChanges.sibling = stats.trust;
                    if (stats.fear != 0f)
                        humanityChanges.sibling = stats.fear;
                    break;

                case "dog":
                case "baron":
                    affectionChanges.dog = stats.trust;
                    humanityChanges.dog = stats.fear;
                    break;

                case "grandmother":
                    affectionChanges.grandmother = stats.trust;
                    humanityChanges.grandmother = stats.fear;
                    break;

                default:
                    Debug.LogWarning($"[ApiClient] 알 수 없는 NPC 이름: {npcName}");
                    break;
            }
        }
    }

    // 5. state_delta.flags → eventFlags
    eventFlags = null;
    if (backendResponse.state_delta?.flags != null && backendResponse.state_delta.flags.Count > 0)
    {
        eventFlags = new EventFlags();
        
        // Dictionary의 각 플래그를 EventFlags의 고정 필드에 매핑
        foreach (var kvp in backendResponse.state_delta.flags)
        {
            string flagName = kvp.Key;
            bool flagValue = kvp.Value;

            switch (flagName.ToLower())
            {
                case "met_mother":
                case "metmother":
                    // 새로운 플래그 - customEvents에 저장하거나 EventFlags에 필드 추가 필요
                    if (eventFlags.customEvents == null)
                        eventFlags.customEvents = new Dictionary<string, bool>();
                    eventFlags.customEvents["met_mother"] = flagValue;
                    Debug.Log($"[ApiClient] 플래그 설정: met_mother = {flagValue}");
                    break;

                case "heard_rumor":
                case "heardrumor":
                    // 새로운 플래그 - customEvents에 저장하거나 EventFlags에 필드 추가 필요
                    if (eventFlags.customEvents == null)
                        eventFlags.customEvents = new Dictionary<string, bool>();
                    eventFlags.customEvents["heard_rumor"] = flagValue;
                    Debug.Log($"[ApiClient] 플래그 설정: heard_rumor = {flagValue}");
                    break;

                case "grandmother_cooperation":
                case "grandmothercooperation":
                    eventFlags.grandmotherCooperation = flagValue;
                    break;

                case "hole_unlocked":
                case "holeunlocked":
                    eventFlags.holeUnlocked = flagValue;
                    break;

                case "fire_started":
                case "firestarted":
                    eventFlags.fireStarted = flagValue;
                    break;

                case "family_asleep":
                case "familyasleep":
                    eventFlags.familyAsleep = flagValue;
                    break;

                case "tea_with_sleeping_pill":
                case "teawithsleepingpill":
                    eventFlags.teaWithSleepingPill = flagValue;
                    break;

                case "key_stolen":
                case "keystolen":
                    eventFlags.keyStolen = flagValue;
                    break;

                case "caught_by_father":
                case "caughtbyfather":
                    eventFlags.caughtByFather = flagValue;
                    break;

                case "caught_by_mother":
                case "caughtbymother":
                    eventFlags.caughtByMother = flagValue;
                    break;

                default:
                    // 커스텀 플래그는 customEvents Dictionary에 저장
                    if (eventFlags.customEvents == null)
                        eventFlags.customEvents = new Dictionary<string, bool>();
                    eventFlags.customEvents[flagName] = flagValue;
                    Debug.Log($"[ApiClient] 커스텀 플래그 설정: {flagName} = {flagValue}");
                    break;
            }
        }
    }

    // 6. state_delta.inventory_add/remove → itemChanges
    itemChanges = new ItemChanges();
    
    if (backendResponse.state_delta?.inventory_add != null && backendResponse.state_delta.inventory_add.Count > 0)
    {
        List<ItemAcquisition> acquisitions = new List<ItemAcquisition>();
        foreach (string itemName in backendResponse.state_delta.inventory_add)
        {
            acquisitions.Add(new ItemAcquisition
            {
                item_name = itemName,
                count = 1  // 기본값 1 (백엔드에서 개수 제공 시 수정)
            });
        }
        itemChanges.acquired_items = acquisitions.ToArray();
    }

    if (backendResponse.state_delta?.inventory_remove != null && backendResponse.state_delta.inventory_remove.Count > 0)
    {
        List<ItemConsumption> consumptions = new List<ItemConsumption>();
        foreach (string itemName in backendResponse.state_delta.inventory_remove)
        {
            consumptions.Add(new ItemConsumption
            {
                item_name = itemName,
                count = 1  // 기본값 1
            });
        }
        itemChanges.consumed_items = consumptions.ToArray();
    }

    // 7. npc_locations, disabled_states는 백엔드 응답에 없으므로 null
    // (필요 시 state_delta에 추가 요청)
    npcLocations = null;
    disabledStates = null;

    // 8. turn_increment 처리 (필요 시 TurnManager에 전달)
    if (backendResponse.state_delta?.turn_increment > 0)
    {
        Debug.Log($"[ApiClient] 턴 증가량: {backendResponse.state_delta.turn_increment}");
        // TurnManager에 전달하는 로직 추가 필요
    }

    // 9. locks, vars 처리 (새로운 필드 - GameStateManager에 추가 필요)
    if (backendResponse.state_delta?.locks != null)
    {
        Debug.Log($"[ApiClient] 잠금 상태 변경: {JsonConvert.SerializeObject(backendResponse.state_delta.locks)}");
        // GameStateManager에 locks 필드 추가 후 처리
    }

    if (backendResponse.state_delta?.vars != null)
    {
        Debug.Log($"[ApiClient] 변수 변경: {JsonConvert.SerializeObject(backendResponse.state_delta.vars)}");
        // GameStateManager에 vars 필드 추가 후 처리
    }
}
```

## 4단계: Json.NET vs JsonUtility 비교

### JsonUtility (Unity 기본)
- **장점**: Unity에 내장되어 있음, 가벼움
- **단점**: Dictionary 미지원, 제한적인 기능
- **사용 시기**: 간단한 구조체 파싱

### Json.NET (Newtonsoft.Json)
- **장점**: Dictionary 완벽 지원, 유연한 파싱, 풍부한 기능
- **단점**: 별도 패키지 필요, 약간 무거움
- **사용 시기**: 복잡한 구조(Dictionary 포함) 파싱

### 사용 가이드
- **요청 전송**: JsonUtility 사용 가능 (Dictionary 없음)
- **응답 파싱**: Json.NET 사용 (Dictionary 포함)

## 5단계: 테스트 및 디버깅

### 디버깅 팁

#### 1. JSON 응답 로깅
```csharp
Debug.Log($"[ApiClient] 원본 JSON: {responseText}");
```

#### 2. 파싱된 객체 확인
```csharp
Debug.Log($"[ApiClient] 파싱된 객체: {JsonConvert.SerializeObject(backendResponse, Formatting.Indented)}");
```

#### 3. Dictionary 내용 확인
```csharp
if (backendResponse.state_delta?.npc_stats != null)
{
    foreach (var kvp in backendResponse.state_delta.npc_stats)
    {
        Debug.Log($"[ApiClient] NPC: {kvp.Key}, Trust: {kvp.Value.trust}, Suspicion: {kvp.Value.suspicion}");
    }
}
```

#### 4. 에러 처리
```csharp
catch (JsonException e)
{
    Debug.LogError($"[ApiClient] JSON 파싱 에러: {e.Message}");
    Debug.LogError($"[ApiClient] JSON 내용: {responseText}");
    // 에러 발생 시 원본 JSON을 확인하여 문제 파악
}
```

## 6단계: 추가 고려사항

### 6.1 NPC Stats 매핑 전략

백엔드의 `trust`, `suspicion`, `fear`를 현재 구조에 매핑하는 방법:

**옵션 1: 직접 매핑**
```csharp
affectionChanges.new_mother = stats.trust;
```

**옵션 2: 조합 계산**
```csharp
affectionChanges.new_mother = stats.trust - (stats.suspicion * 0.5f);
```

**옵션 3: fear를 humanity로 매핑**
```csharp
humanityChanges.sibling = stats.fear;
```

**권장**: 백엔드와 협의하여 명확한 매핑 규칙 정의

### 6.2 새로운 필드 처리

백엔드 응답에 포함된 새로운 필드들:

- **locks**: 잠금 상태 관리 (예: `basement_door`)
- **vars**: 게임 변수 (예: `investigation_progress`)
- **turn_increment**: 턴 증가량

이 필드들을 처리하려면:

1. `GameStateManager`에 해당 필드 추가
2. 변환 로직에서 처리
3. `InputHandler`에서 적용

### 6.3 humanity_change 필드 누락

백엔드 응답에 `humanity_change` 필드가 없습니다. 해결 방법:

**옵션 1**: 백엔드에 `humanity_change` 필드 추가 요청
**옵션 2**: `state_delta.vars`에서 `humanity_change` 키로 제공받기
**옵션 3**: 현재는 0으로 설정하고 나중에 추가

### 6.4 npc_locations, disabled_states 누락

백엔드 응답에 이 필드들이 없습니다. 해결 방법:

**옵션 1**: 백엔드에 필드 추가 요청
**옵션 2**: `state_delta` 내부로 이동 요청
**옵션 3**: 현재는 null로 처리하고 나중에 추가

## 7단계: 체크리스트

### 설치 및 설정
- [ ] Json.NET 패키지 설치 완료
- [ ] `using Newtonsoft.Json;` 추가 완료
- [ ] 컴파일 에러 없음 확인

### 구조체 정의
- [ ] `BackendNPCStats` 정의 완료
- [ ] `BackendEndingInfo` 정의 완료
- [ ] `BackendStateDelta` 정의 완료
- [ ] `BackendDebugInfo` 정의 완료
- [ ] `BackendGameResponse` 정의 완료

### 코드 수정
- [ ] `ApiClient.cs`에 Json.NET 사용하도록 수정 완료
- [ ] `ConvertBackendResponseToCurrentFormat` 메서드 구현 완료
- [ ] NPC 이름 매핑 로직 구현 완료
- [ ] 플래그 매핑 로직 구현 완료
- [ ] 아이템 변화량 변환 로직 구현 완료

### 테스트
- [ ] 실제 백엔드 응답으로 테스트 완료
- [ ] Dictionary 파싱 정상 작동 확인
- [ ] 변환 로직 정상 작동 확인
- [ ] 에러 처리 정상 작동 확인

## 8단계: 예상 문제 및 해결책

### 문제 1: Json.NET 설치 후 컴파일 에러
**원인**: 패키지가 제대로 설치되지 않음
**해결**: Unity를 재시작하고 Package Manager에서 다시 확인

### 문제 2: Dictionary가 null로 파싱됨
**원인**: JSON 필드 이름과 C# 필드 이름 불일치
**해결**: `[JsonProperty]` 속성으로 명시적 매핑

### 문제 3: NPC 이름 매핑 실패
**원인**: 백엔드에서 사용하는 NPC 이름과 예상한 이름 불일치
**해결**: switch 문에 모든 가능한 이름 추가, 디버그 로그로 확인

### 문제 4: 플래그 매핑 실패
**원인**: 백엔드 플래그 이름과 EventFlags 필드 이름 불일치
**해결**: switch 문에 모든 가능한 이름 추가, 커스텀 플래그는 `customEvents`에 저장

## 9단계: 실제 백엔드 예시와의 매핑

### 9.1 요청 예시

백엔드로 전송하는 요청:
```json
{
  "chat_input": "안녕하세요",
  "npc_name": "stepmother",
  "item_name": null
}
```

이미 `ApiClient.cs`의 `StepRequest` 클래스로 구현되어 있습니다.

### 9.2 응답 예시 분석

백엔드 응답 예시:
```json
{
  "narrative": "플레이어가 새엄마에게 말을 걸었습니다...",
  "ending_info": null,
  "state_delta": {
    "npc_stats": {
      "stepmother": {
        "trust": 2,
        "suspicion": 5
      },
      "brother": {
        "fear": -1
      }
    },
    "flags": {
      "met_mother": true,
      "heard_rumor": true
    },
    "inventory_add": ["old_key", "strange_note"],
    "inventory_remove": ["apple"],
    "locks": {
      "basement_door": false
    },
    "vars": {
      "investigation_progress": 10
    },
    "turn_increment": 1
  }
}
```

**주요 특징**:
1. `npc_stats`에서 일부 NPC만 포함됨 (stepmother, brother만)
2. 각 NPC의 일부 필드만 포함됨 (stepmother는 trust/suspicion만, brother는 fear만)
3. `flags`에 새로운 플래그 포함 (`met_mother`, `heard_rumor`)
4. `inventory_add/remove`는 문자열 배열
5. `locks`, `vars`는 Dictionary 형태

### 9.3 변환 결과 예시

위 응답이 변환되면:
- `response`: "플레이어가 새엄마에게 말을 걸었습니다..."
- `affectionChanges.new_mother`: 2 (trust 값)
- `humanityChanges.sibling`: -1 (brother의 fear 값)
- `eventFlags.customEvents["met_mother"]`: true
- `eventFlags.customEvents["heard_rumor"]`: true
- `itemChanges.acquired_items`: `["old_key", "strange_note"]`
- `itemChanges.consumed_items`: `["apple"]`

## 요약

1. **Json.NET 설치**: `com.unity.nuget.newtonsoft-json` 패키지 추가
2. **요청 구조체**: 기존 `StepRequest` 사용 (JsonUtility로 충분)
3. **응답 구조체 정의**: `GameDataTypes.cs`에 백엔드 응답 구조체 추가
4. **ApiClient 수정**: `JsonConvert.DeserializeObject` 사용
5. **변환 로직**: 백엔드 응답을 현재 구조로 변환하는 메서드 추가
   - 일부 필드만 있는 경우 처리 (0이 아닐 때만 적용)
   - 새로운 플래그는 `customEvents`에 저장
6. **테스트**: 실제 백엔드 응답으로 테스트

이 가이드를 따라하면 Dictionary를 포함한 백엔드 응답을 정상적으로 파싱할 수 있습니다.

## 참고 자료

- [Json.NET 공식 문서](https://www.newtonsoft.com/json/help/html/Introduction.htm)
- [Unity Package Manager 문서](https://docs.unity3d.com/Manual/upm-ui.html)
- [Json.NET Unity 패키지](https://github.com/jilleJr/Newtonsoft.Json-for-Unity)

