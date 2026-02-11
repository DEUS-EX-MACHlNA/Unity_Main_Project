# 백엔드 응답 형식 구체화 문서

## 개요
자연어 처리 기반 world state 판단을 백엔드에 맡기는 경우의 구체적인 응답 형식입니다.

---

## 1. 전체 응답 구조

```json
{
  "narrative": "string",           // 필수: 게임 내 표시할 대화/서술 텍스트
  "ending_info": {                 // 선택적: 엔딩 트리거 정보
    "ending_type": "string",        // null이면 엔딩 없음
    "description": "string"          // 선택적: 엔딩 설명
  },
  "state_result": {                // 필수: 상태 결과값 (Result 형식)
    "npc_stats": {...},            // 선택적: NPC 통계 현재 값
    "flags": {...},                 // 선택적: 이벤트 플래그 현재 상태
    "inventory_add": [...],         // 선택적: 아이템 획득
    "inventory_remove": [...],     // 선택적: 아이템 소모
    "npc_disabled_states": {...},  // 선택적: NPC 무력화 상태
    "locks": {...},                 // 선택적: 잠금 상태
    "humanity": 0.0                 // 선택적: 플레이어 인간성 현재 값
  },
  "debug": {                       // 선택적: 디버그 정보
    "game_id": 0,
    "reasoning": "string",          // 백엔드 판단 근거
    "steps": [...],
    "turn_after": 0
  }
}
```

---

## 2. 각 필드 상세 설명

### 2.1 narrative (필수)
- **타입**: `string`
- **설명**: 플레이어에게 표시할 대화/서술 텍스트
- **예시**: `"할머니가 홍차를 마시고 곧 잠에 빠졌습니다. 이제 안전하게 지하실을 탐색할 수 있습니다."`

### 2.2 ending_info (선택적)
- **타입**: `object` 또는 `null`
- **설명**: 엔딩 조건 충족 시 트리거 정보

```json
{
  "ending_type": "stealth_exit",  // null이면 엔딩 없음
  "description": "완벽한 기만 엔딩"  // 선택적
}
```

**엔딩 타입 값:**
- `"stealth_exit"` → `EndingType.StealthExit`
- `"chaotic_breakout"` → `EndingType.ChaoticBreakout`
- `"siblings_help"` → `EndingType.SiblingsHelp`
- `"unfinished_doll"` → `EndingType.UnfinishedDoll`
- `"eternal_dinner"` → `EndingType.EternalDinner`
- `null` 또는 `"none"` → 엔딩 없음

### 2.3 state_result (필수)
상태 결과값을 Result 형식으로 전달합니다. 백엔드에서 델타 값을 처리한 최종 상태를 반환합니다. **변경되지 않은 필드는 생략 가능**합니다.

#### 2.3.1 npc_stats (선택적)
NPC 통계 현재 값 (호감도, 의심도, 공포도)

```json
{
  "npc_stats": {
    "new_mother": {
      "trust": 50.0,        // 호감도 현재 값 (0~100)
      "suspicion": 10.0,    // 의심도 현재 값 (현재 미사용)
      "fear": 0.0           // 공포도 현재 값 (현재 미사용)
    },
    "grandmother": {
      "trust": 60.0,
      "suspicion": 0.0,
      "fear": 0.0
    }
  }
}
```

**NPC 이름 매핑:**
- `"new_mother"`, `"stepmother"` → `NPCType.NewMother`
- `"new_father"`, `"father"` → `NPCType.NewFather`
- `"sibling"`, `"brother"` → `NPCType.Sibling`
- `"dog"`, `"baron"` → `NPCType.Dog`
- `"grandmother"` → `NPCType.Grandmother`

**주의사항:**
- 변경되지 않은 NPC는 생략 가능
- 새엄마는 `humanity` 변경 불가 (최종보스)

#### 2.3.2 flags (선택적)
이벤트 플래그 (게임 이벤트 발생 여부)

```json
{
  "flags": {
    "grandmother_cooperation": true,
    "tea_with_sleeping_pill": true,
    "family_asleep": false,
    "fire_started": false,
    "key_stolen": false,
    "hole_unlocked": false,
    "custom_event_1": true  // 커스텀 이벤트
  }
}
```

**표준 플래그 목록:**
- `"grandmother_cooperation"` → `EventFlags.grandmotherCooperation`
- `"hole_unlocked"` → `EventFlags.holeUnlocked`
- `"fire_started"` → `EventFlags.fireStarted`
- `"family_asleep"` → `EventFlags.familyAsleep`
- `"tea_with_sleeping_pill"` → `EventFlags.teaWithSleepingPill`
- `"key_stolen"` → `EventFlags.keyStolen`
- 기타 → `EventFlags.customEvents`에 저장

**주의사항:**
- 플래그가 `false`이면 생략 가능 (변경 없음)

#### 2.3.3 inventory_add (선택적)
획득한 아이템 목록

```json
{
  "inventory_add": [
    "sleeping_pill",      // 아이템 이름 (개수는 기본값 1)
    "earl_grey_tea"
  ]
}
```

**아이템 이름 매핑:**
- `"sleeping_pill"` → `ItemType.SleepingPill`
- `"earl_grey_tea"` → `ItemType.EarlGreyTea`
- `"real_family_photo"` → `ItemType.RealFamilyPhoto`
- `"oil_bottle"` → `ItemType.OilBottle`
- `"silver_lighter"` → `ItemType.SilverLighter`
- `"siblings_toy"` → `ItemType.SiblingsToy`
- `"brass_key"` → `ItemType.BrassKey`

**주의사항:**
- 빈 배열이면 생략 가능
- 개수는 기본값 1 (필요 시 확장 가능)

#### 2.3.4 inventory_remove (선택적)
소모/사용한 아이템 목록

```json
{
  "inventory_remove": [
    "sleeping_pill",
    "earl_grey_tea"
  ]
}
```

**주의사항:**
- 빈 배열이면 생략 가능
- 백엔드가 판단: "수면제를 탄 홍차를 드렸다" → 두 아이템 모두 제거

#### 2.3.5 npc_disabled_states (선택적)
NPC 무력화 상태 (수면제 등으로 무력화)

```json
{
  "npc_disabled_states": {
    "grandmother": {
      "is_disabled": true,
      "remaining_turns": 3,
      "reason": "수면제 복용"
    },
    "new_father": {
      "is_disabled": true,
      "remaining_turns": 2,
      "reason": "수면제 복용"
    },
    "new_mother": {
      "is_disabled": true,
      "remaining_turns": 1,
      "reason": "수면제 복용"
    }
  }
}
```

**주의사항:**
- `is_disabled: false`이면 생략 가능 (무력화 해제)
- `remaining_turns: 0`이면 무력화 해제

#### 2.3.6 locks (선택적)
잠금 상태 (문, 상자 등)

```json
{
  "locks": {
    "basement_door": false,      // 잠금 해제
  }
}
```

**주의사항:**
- 잠금 상태 변경이 없으면 생략 가능
- `true` = 잠금, `false` = 해제

#### 2.3.7 humanity (선택적)
플레이어 인간성 현재 값

```json
{
  "humanity": 75.0  // 플레이어 인간성 현재 값 (0~100)
}
```

**주의사항:**
- 변경되지 않았으면 생략 가능
- 0~100 범위의 값

### 2.4 debug (선택적)
디버그 정보

```json
{
  "debug": {
    "game_id": 12,
    "reasoning": "플레이어가 할머니에게 수면제를 탄 홍차를 제공했습니다. 이는 tea_with_sleeping_pill 플래그를 true로 설정하고, 할머니를 3턴간 무력화시킵니다.",
    "steps": [],
    "turn_after": 6
  }
}
```

**필드 설명:**
- `reasoning`: 백엔드가 판단한 근거 (디버깅용)
- `steps`: 시나리오 스텝 정보 (선택적)
- `turn_after`: 처리 후 턴 수 (선택적)

---

## 3. 실제 사용 예시

### 예시 1: 라이터로 불 지르기

**요청:**
```json
{
  "chat_input": "라이터를 이용해 불을 지른다",
  "npc_name": "",
  "item_name": "silver_lighter"
}
```

**응답:**
```json
{
  "narrative": "라이터의 불꽃이 기름에 닿자 순식간에 화재가 발생했습니다. 집안이 연기로 가득 차기 시작합니다.",
  "ending_info": null,
  "state_result": {
    "flags": {
      "fire_started": true
    },
    "inventory_remove": [
      "silver_lighter"
    ],
    "humanity": 60.0
  },
  "debug": {
    "game_id": 12,
    "reasoning": "플레이어가 라이터로 불을 질렀습니다. fire_started 플래그를 true로 설정하고, 인간성은 75에서 60으로 감소합니다.",
    "turn_after": 7
  }
}
```

### 예시 2: 동생에게 장난감 주기

**요청:**
```json
{
  "chat_input": "동생에게 장난감을 준다",
  "npc_name": "sibling",
  "item_name": "siblings_toy"
}
```

**응답:**
```json
{
  "narrative": "동생은 낡은 태엽 로봇을 받아들고 잠시 멍하니 바라봅니다. 그 순간, 그의 눈에 뭔가 반짝이는 것이 보입니다. \"이거... 나한테 있었던 거 맞지?\" 작은 목소리로 물어보는 동생의 목소리에는 잊혀진 기억의 파편이 섞여 있습니다.",
  "ending_info": null,
  "state_result": {
    "npc_stats": {
      "sibling": {
        "trust": 85.0,
        "suspicion": 0.0,
        "fear": 0.0
      }
    },
    "inventory_remove": [
      "siblings_toy"
    ]
  },
  "debug": {
    "game_id": 12,
    "reasoning": "플레이어가 동생에게 장난감을 제공했습니다. 동생의 호감도가 크게 상승하여 85.0이 되었습니다.",
    "turn_after": 5
  }
}
```

### 예시 3: 변화 없음 (단순 대화)

**요청:**
```json
{
  "chat_input": "안녕하세요",
  "npc_name": "new_father",
  "item_name": ""
}
```

**응답:**
```json
{
  "narrative": "새아빠가 무표정하게 당신을 바라봅니다.",
  "ending_info": null,
  "state_result": {
    "npc_stats": {
      "new_father": {
        "trust": 51.0
      }
    }
  }
}
```

---

## 4. 필드 생략 규칙

**다음 경우 필드를 생략할 수 있습니다:**

1. **빈 배열/객체**: `[]`, `{}`
2. **변경 없음**: 값이 변경되지 않은 경우
3. **선택적 필드**: `ending_info`, `debug` 등

**예시:**
```json
{
  "narrative": "대화 텍스트",
  "state_result": {
    // 모든 필드는 생략 가능 (변경 없음)
  }
}
```

---

## 5. 에러 처리

백엔드에서 에러가 발생한 경우:

```json
{
  "error": {
    "code": "INVALID_ACTION",
    "message": "수면제가 인벤토리에 없습니다.",
    "details": {}
  }
}
```

게임은 에러 응답을 받으면 `narrative`에 에러 메시지를 표시하고, `state_result`는 적용하지 않습니다.

---

## 6. 구현 참고사항

### 6.1 Unity C# 구조체
현재 `GameDataTypes.cs`의 `BackendStateResult` 구조체를 확장하여 위 형식을 지원합니다.

### 6.2 변환 로직
`BackendResponseConverter.cs`에서 백엔드 응답을 게임 내부 형식으로 변환합니다.

### 6.3 필드 매핑
백엔드의 snake_case 이름을 Unity의 PascalCase로 변환하는 `NameMapper` 클래스를 사용합니다.

