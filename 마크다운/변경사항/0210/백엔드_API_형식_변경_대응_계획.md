# 백엔드 API 형식 변경 대응 계획

## 개요
백엔드 API 응답 형식이 기존 구조에서 새로운 구조로 변경되었습니다. 이 문서는 새로운 JSON 형식에 맞춰 프론트엔드를 수정하기 위한 변경 계획을 정리합니다.

## 작업일: 2026-02-10

---

## 1. 변경 사항 요약

### 1.1 요청 형식 (프론트 → 백엔드)
**✅ 변경 없음 - 완전히 일치**
```json
{
  "chat_input": "string",
  "npc_name": "string",
  "item_name": "string"
}
```

### 1.2 응답 형식 (백엔드 → 프론트)

#### 기존 형식
```json
{
  "response": "string",
  "humanity_change": float,
  "npc_affection_changes": {...},
  "npc_humanity_changes": {...},
  "npc_disabled_states": {...},
  "item_changes": {...},
  "event_flags": {...},
  "npc_locations": {...},
  "ending_trigger": "string"
}
```

#### 새로운 형식
```json
{
  "narrative": "string",
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
    "inventory_add": [
      "old_key",
      "strange_note"
    ],
    "inventory_remove": [
      "apple"
    ],
    "locks": {
      "basement_door": false
    },
    "vars": {
      "investigation_progress": 10
    },
    "turn_increment": 1
  },
  "debug": {
    "game_id": 24,
    "steps": [...],
    "turn_after": 3
  }
}
```

---

## 2. 주요 변경 사항 분석

### 2.1 필드명 변경
| 기존 필드 | 새로운 필드 | 설명 |
|----------|------------|------|
| `response` | `narrative` | 응답 텍스트 (동일한 역할) |
| `ending_trigger` | `ending_info` | 엔딩 정보 (구조 변경 가능성) |

### 2.2 구조 통합
- **기존**: 평면적 구조 (각 상태 변화가 독립적인 필드)
- **새로운**: 중첩 구조 (`state_delta` 객체로 모든 상태 변화 통합)

### 2.3 NPC 상태 구조 변경
- **기존**: NPC별 고정 필드 (`new_mother`, `sibling` 등)
- **새로운**: 동적 Dictionary (`"stepmother": {"trust": 2, "suspicion": 5}`)
  - NPC별로 다른 속성을 가질 수 있음 (trust, suspicion, fear 등)
  - NPC 이름이 백엔드 형식으로 제공됨 (`"stepmother"`, `"brother"` 등)

### 2.4 인벤토리 처리 변경
- **기존**: `ItemChanges` 구조체 (acquired_items, consumed_items, state_changes)
- **새로운**: `inventory_add` / `inventory_remove` 배열 (단순 문자열 배열)

### 2.5 새로운 필드 추가
- `locks`: 잠금 상태 관리 (Dictionary<string, bool>)
- `vars`: 커스텀 변수 (Dictionary<string, number>)
- `turn_increment`: 턴 증가량 (number)
- `debug`: 디버그 정보 객체

### 2.6 Unity JsonUtility 제약사항
- Unity의 `JsonUtility`는 **Dictionary를 지원하지 않음**
- 새로운 형식은 Dictionary 기반 구조:
  - `npc_stats`: `Dictionary<string, Dictionary<string, number>>`
  - `flags`: `Dictionary<string, bool>`
  - `locks`: `Dictionary<string, bool>`
  - `vars`: `Dictionary<string, number>`

---

## 3. 해결 방안

### 3.1 JSON 파서 변경 (필수)

#### 옵션 1: Newtonsoft.Json (Json.NET) 사용 (권장)
- Unity Package Manager에서 설치 가능
- Dictionary 완벽 지원
- 유연한 구조 처리 가능
- 널리 사용되는 라이브러리

**설치 방법:**
1. Unity Package Manager 열기
2. `+` 버튼 → `Add package from git URL`
3. URL 입력: `https://github.com/jilleJr/Newtonsoft.Json-for-Unity.git?path=/src/Newtonsoft.Json.Unity3D`

#### 옵션 2: Unity JsonUtility + 커스텀 변환 로직
- 추가 라이브러리 불필요
- Dictionary를 배열로 변환하는 중간 구조체 필요
- 복잡한 변환 로직 필요

**권장: 옵션 1 (Newtonsoft.Json)**

### 3.2 새로운 응답 구조체 정의

#### 3.2.1 StateDelta 구조체
```csharp
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class StateDelta
{
    [JsonProperty("npc_stats")]
    public Dictionary<string, Dictionary<string, float>> npc_stats;
    
    [JsonProperty("flags")]
    public Dictionary<string, bool> flags;
    
    [JsonProperty("inventory_add")]
    public List<string> inventory_add;
    
    [JsonProperty("inventory_remove")]
    public List<string> inventory_remove;
    
    [JsonProperty("locks")]
    public Dictionary<string, bool> locks;
    
    [JsonProperty("vars")]
    public Dictionary<string, float> vars;
    
    [JsonProperty("turn_increment")]
    public int turn_increment;
}
```

#### 3.2.2 DebugInfo 구조체
```csharp
[Serializable]
public class DebugInfo
{
    [JsonProperty("game_id")]
    public int game_id;
    
    [JsonProperty("steps")]
    public List<DebugStep> steps;
    
    [JsonProperty("turn_after")]
    public int turn_after;
}

[Serializable]
public class DebugStep
{
    [JsonProperty("step")]
    public string step;
    
    [JsonProperty("newly_unlocked")]
    public List<string> newly_unlocked;
    
    [JsonProperty("state_delta")]
    public StateDelta state_delta;
    
    [JsonProperty("reached")]
    public bool reached;
}
```

#### 3.2.3 EndingInfo 구조체
```csharp
[Serializable]
public class EndingInfo
{
    [JsonProperty("ending_type")]
    public string ending_type;
    
    [JsonProperty("description")]
    public string description;
    
    // 추가 필드는 백엔드 응답에 따라 확장 가능
}
```

#### 3.2.4 GameResponse 구조체 (새로운 형식)
```csharp
[Serializable]
public class GameResponse
{
    [JsonProperty("narrative")]
    public string narrative;
    
    [JsonProperty("ending_info")]
    public EndingInfo ending_info;
    
    [JsonProperty("state_delta")]
    public StateDelta state_delta;
    
    [JsonProperty("debug")]
    public DebugInfo debug;
}
```

### 3.3 NPC 이름 매핑 확장

기존 NPC 이름 매핑에 백엔드 형식 추가:
```csharp
// 백엔드 NPC 이름 → Unity NPCType 매핑
private static Dictionary<string, NPCType> npcNameMapping = new Dictionary<string, NPCType>
{
    // 기존 매핑
    { "new_mother", NPCType.NewMother },
    { "new_father", NPCType.NewFather },
    { "sibling", NPCType.Sibling },
    { "dog", NPCType.Dog },
    { "grandmother", NPCType.Grandmother },
    
    // 새로운 백엔드 형식
    { "stepmother", NPCType.NewMother },
    { "father", NPCType.NewFather },
    { "brother", NPCType.Sibling },
    { "sister", NPCType.Sibling }, // 동생은 성별에 따라 다를 수 있음
    { "dog", NPCType.Dog },
    { "grandmother", NPCType.Grandmother }
};
```

### 3.4 NPC 속성 매핑

NPC별 속성 이름을 Unity의 호감도/인간성으로 변환:
```csharp
// NPC 속성 이름 → Unity 필드 매핑
private static Dictionary<string, string> npcAttributeMapping = new Dictionary<string, string>
{
    { "trust", "affection" },        // trust → 호감도
    { "suspicion", "affection" },    // suspicion → 호감도 (음수로 처리)
    { "fear", "humanity" },          // fear → 인간성 (음수로 처리)
    { "affection", "affection" },    // 직접 매핑
    { "humanity", "humanity" }       // 직접 매핑
};

// 속성 값을 Unity 형식으로 변환
private float ConvertAttributeValue(string attributeName, float value)
{
    switch (attributeName.ToLower())
    {
        case "suspicion":
            return -value; // 의심도는 호감도 감소로 처리
        case "fear":
            return -value; // 공포는 인간성 감소로 처리
        default:
            return value;
    }
}
```

### 3.5 데이터 변환 로직

#### 3.5.1 StateDelta → GameStateManager 변환
```csharp
private void ApplyStateDelta(StateDelta stateDelta)
{
    if (stateDelta == null) return;
    
    // NPC 상태 적용
    if (stateDelta.npc_stats != null)
    {
        ApplyNPCStats(stateDelta.npc_stats);
    }
    
    // 플래그 적용
    if (stateDelta.flags != null)
    {
        ApplyFlags(stateDelta.flags);
    }
    
    // 인벤토리 적용
    if (stateDelta.inventory_add != null)
    {
        foreach (string itemName in stateDelta.inventory_add)
        {
            ItemType itemType = ConvertItemNameToType(itemName);
            if (itemType != ItemType.None)
            {
                AddItem(itemType, 1);
            }
        }
    }
    
    if (stateDelta.inventory_remove != null)
    {
        foreach (string itemName in stateDelta.inventory_remove)
        {
            ItemType itemType = ConvertItemNameToType(itemName);
            if (itemType != ItemType.None)
            {
                RemoveItem(itemType, 1);
            }
        }
    }
    
    // 잠금 상태 적용
    if (stateDelta.locks != null)
    {
        ApplyLocks(stateDelta.locks);
    }
    
    // 커스텀 변수 적용
    if (stateDelta.vars != null)
    {
        ApplyVars(stateDelta.vars);
    }
    
    // 턴 증가량 적용
    if (stateDelta.turn_increment > 0)
    {
        for (int i = 0; i < stateDelta.turn_increment; i++)
        {
            ConsumeTurn();
        }
    }
}
```

#### 3.5.2 NPC Stats 적용
```csharp
private void ApplyNPCStats(Dictionary<string, Dictionary<string, float>> npcStats)
{
    foreach (var npcEntry in npcStats)
    {
        string npcName = npcEntry.Key;
        Dictionary<string, float> attributes = npcEntry.Value;
        
        // NPC 이름을 NPCType으로 변환
        NPCType npcType = ConvertNPCNameToType(npcName);
        if (npcType == NPCType.None) continue;
        
        // 각 속성 적용
        foreach (var attrEntry in attributes)
        {
            string attrName = attrEntry.Key;
            float attrValue = attrEntry.Value;
            
            // 속성 이름에 따라 호감도 또는 인간성 변경
            string fieldType = npcAttributeMapping.ContainsKey(attrName) 
                ? npcAttributeMapping[attrName] 
                : "affection"; // 기본값
            
            float convertedValue = ConvertAttributeValue(attrName, attrValue);
            
            if (fieldType == "affection")
            {
                ModifyAffection(npcType, convertedValue);
            }
            else if (fieldType == "humanity")
            {
                ModifyNPCHumanity(npcType, convertedValue);
            }
        }
    }
}
```

---

## 4. 구현 단계

### Phase 1: JSON 파서 변경 및 기본 구조체 정의
**예상 작업 시간: 2-3시간**

#### 작업 내용
1. Newtonsoft.Json 패키지 설치
2. 새로운 응답 구조체 정의
   - `StateDelta` 클래스
   - `DebugInfo` 클래스
   - `EndingInfo` 클래스
   - `GameResponse` 클래스 (새로운 형식)
3. 기존 `GameResponse` 클래스와의 호환성 유지 (임시)

#### 완료 조건
- [ ] Newtonsoft.Json 패키지 설치 완료
- [ ] 새로운 구조체 정의 완료
- [ ] 컴파일 에러 없음

### Phase 2: ApiClient 수정
**예상 작업 시간: 3-4시간**

#### 작업 내용
1. `ApiClient.cs`에서 JsonUtility → Newtonsoft.Json 변경
2. 새로운 응답 형식 파싱 로직 추가
3. 기존 응답 형식과의 호환성 처리 (하위 호환성)
4. NPC 이름 매핑 확장
5. NPC 속성 매핑 로직 추가

#### 완료 조건
- [ ] 새로운 응답 형식 파싱 성공
- [ ] 기존 응답 형식도 처리 가능 (하위 호환성)
- [ ] NPC 이름/속성 매핑 정상 작동
- [ ] 에러 처리 로직 추가

### Phase 3: StateDelta 변환 로직 구현
**예상 작업 시간: 4-5시간**

#### 작업 내용
1. `ApplyStateDelta()` 메서드 구현
2. `ApplyNPCStats()` 메서드 구현
3. `ApplyFlags()` 메서드 구현
4. `ApplyLocks()` 메서드 구현
5. `ApplyVars()` 메서드 구현
6. 인벤토리 변환 로직 수정

#### 완료 조건
- [ ] 모든 StateDelta 필드 변환 로직 구현
- [ ] GameStateManager와 정상 연동
- [ ] 이벤트 발생 정상 작동

### Phase 4: GameStateManager 통합
**예상 작업 시간: 2-3시간**

#### 작업 내용
1. `InputHandler` 또는 `ApiClient`에서 `ApplyStateDelta()` 호출
2. 기존 상태 업데이트 로직과 통합
3. 턴 증가량 처리 로직 추가
4. 디버그 정보 처리 (선택적)

#### 완료 조건
- [ ] 전체 플로우 정상 작동
- [ ] 기존 기능과 충돌 없음
- [ ] 테스트 완료

### Phase 5: 테스트 및 검증
**예상 작업 시간: 2-3시간**

#### 작업 내용
1. 새로운 응답 형식으로 API 호출 테스트
2. 모든 상태 변화 정상 적용 확인
3. 에러 케이스 테스트
4. 기존 기능 회귀 테스트

#### 완료 조건
- [ ] 모든 테스트 통과
- [ ] 에러 처리 정상 작동
- [ ] 문서화 완료

---

## 5. 하위 호환성 고려사항

### 5.1 기존 API 지원 (선택적)
- 기존 응답 형식도 처리할 수 있도록 두 가지 형식 모두 지원
- 응답 형식 자동 감지 로직 추가

### 5.2 마이그레이션 전략
1. **옵션 1**: 기존 형식과 새 형식 모두 지원 (권장)
2. **옵션 2**: 새 형식만 지원 (백엔드 변경 완료 후)

---

## 6. 추가 고려사항

### 6.1 새로운 필드 처리
- `locks`: 잠금 상태 관리 시스템 추가 필요
- `vars`: 커스텀 변수 저장소 추가 필요
- `debug`: 디버그 정보 표시 기능 (선택적)

### 6.2 NPC 속성 확장성
- NPC별로 다른 속성 (trust, suspicion, fear 등)을 유연하게 처리
- 새로운 속성이 추가되어도 확장 가능한 구조

### 6.3 아이템 이름 확장
- 새로운 아이템이 추가될 경우 아이템 이름 매핑 확장 필요
- 백엔드에서 제공하는 아이템 이름과 Unity ItemType 매핑

---

## 7. 예상 문제점 및 해결 방안

### 7.1 Dictionary 직렬화 문제
**문제**: Unity JsonUtility는 Dictionary를 지원하지 않음
**해결**: Newtonsoft.Json 사용

### 7.2 NPC 이름 불일치
**문제**: 백엔드 NPC 이름과 Unity NPCType enum 불일치
**해결**: 매핑 딕셔너리 사용

### 7.3 NPC 속성 불일치
**문제**: 백엔드 속성 (trust, suspicion)과 Unity 필드 (affection, humanity) 불일치
**해결**: 속성 매핑 및 변환 로직 사용

### 7.4 아이템 이름 불일치
**문제**: 백엔드 아이템 이름과 Unity ItemType enum 불일치
**해결**: 기존 아이템 이름 매핑 확장

---

## 8. 참고 문서

- [Phase1: 데이터 모델 정의](Phase1_데이터_모델_정의.md)
- [Phase2: NPC 시스템 구현](Phase2_NPC_시스템_구현.md)
- [Phase3: 아이템 시스템 구현](Phase3_아이템_시스템_구현.md)
- [Phase4: 엔딩 시스템 구현](Phase4_엔딩_시스템_구현.md)
- [동적 상태 관리 정책](동적_상태_관리_정책.md)

---

## 9. 작업 우선순위

1. **높음**: Phase 1, 2 (JSON 파서 변경 및 기본 구조)
2. **중간**: Phase 3 (StateDelta 변환 로직)
3. **낮음**: Phase 4, 5 (통합 및 테스트)

---

## 10. 완료 체크리스트

### Phase 1
- [ ] Newtonsoft.Json 패키지 설치
- [ ] 새로운 구조체 정의
- [ ] 컴파일 에러 없음

### Phase 2
- [ ] ApiClient 수정 완료
- [ ] 새로운 응답 형식 파싱 성공
- [ ] NPC 이름/속성 매핑 구현

### Phase 3
- [ ] StateDelta 변환 로직 구현
- [ ] 모든 필드 변환 로직 완료

### Phase 4
- [ ] GameStateManager 통합 완료
- [ ] 전체 플로우 정상 작동

### Phase 5
- [ ] 테스트 완료
- [ ] 문서화 완료

---

## 부록: 예상 코드 변경 사항

### A.1 ApiClient.cs 주요 변경
- `JsonUtility.FromJson` → `JsonConvert.DeserializeObject`
- `GameResponse` 클래스 재정의
- 새로운 응답 형식 파싱 로직 추가

### A.2 GameStateManager.cs 추가 메서드
- `ApplyStateDelta(StateDelta stateDelta)`
- `ApplyNPCStats(Dictionary<string, Dictionary<string, float>> npcStats)`
- `ApplyFlags(Dictionary<string, bool> flags)`
- `ApplyLocks(Dictionary<string, bool> locks)`
- `ApplyVars(Dictionary<string, float> vars)`

### A.3 새로운 매핑 딕셔너리
- NPC 이름 매핑 확장
- NPC 속성 매핑 추가
- 아이템 이름 매핑 확장 (필요 시)

