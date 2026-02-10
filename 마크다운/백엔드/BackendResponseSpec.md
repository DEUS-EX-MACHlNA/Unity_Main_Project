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
  "state_delta": {                 // 필수: 상태 변화량 (Delta 형식)
    "npc_stats": {...},            // 선택적: NPC 통계 변화
    "flags": {...},                 // 선택적: 이벤트 플래그
    "inventory_add": [...],         // 선택적: 아이템 획득
    "inventory_remove": [...],     // 선택적: 아이템 소모
    "item_state_changes": [...],    // 선택적: 아이템 상태 변경
    "npc_disabled_states": {...},  // 선택적: NPC 무력화 상태
    "npc_locations": {...},         // 선택적: NPC 위치 변경
    "locks": {...},                 // 선택적: 잠금 상태
    "humanity_change": 0.0          // 선택적: 플레이어 인간성 변화량
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

### 2.3 state_delta (필수)
상태 변화량을 Delta 형식으로 전달합니다. **변화가 없는 필드는 생략 가능**합니다.

#### 2.3.1 npc_stats (선택적)
NPC 통계 변화량 (호감도, 의심도, 공포도)

```json
{
  "npc_stats": {
    "new_mother": {
      "trust": 5.0,        // 호감도 변화량 (양수/음수)
      "suspicion": -2.0,   // 의심도 변화량 (현재 미사용)
      "fear": 0.0          // 공포도 변화량 (현재 미사용)
    },
    "grandmother": {
      "trust": 10.0,
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
- `trust` 값이 0이면 생략 가능 (변화 없음)
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
- 플래그가 `false`이면 생략 가능 (변화 없음)

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

#### 2.3.5 item_state_changes (선택적)
아이템 상태 변경 (월드 → 인벤토리, 인벤토리 → 사용 등)

```json
{
  "item_state_changes": [
    {
      "item_name": "earl_grey_tea",
      "new_state": "used"  // "in_world", "in_inventory", "used"
    },
    {
      "item_name": "brass_key",
      "new_state": "in_inventory"
    }
  ]
}
```

**상태 값:**
- `"in_world"` → `ItemState.InWorld` (월드에 존재)
- `"in_inventory"` → `ItemState.InInventory` (인벤토리에 있음)
- `"used"` → `ItemState.Used` (사용됨)

#### 2.3.6 npc_disabled_states (선택적)
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

#### 2.3.7 npc_locations (선택적)
NPC 위치 변경

```json
{
  "npc_locations": {
    "grandmother": "basement",
    "new_father": "kitchen"
  }
}
```

**위치 이름 매핑:**
- `"players_room"` → `GameLocation.PlayersRoom`
- `"hallway"` → `GameLocation.Hallway`
- `"living_room"` → `GameLocation.LivingRoom`
- `"kitchen"` → `GameLocation.Kitchen`
- `"siblings_room"` → `GameLocation.SiblingsRoom`
- `"basement"` → `GameLocation.Basement`
- `"backyard"` → `GameLocation.Backyard`

**주의사항:**
- 위치 변경이 없으면 생략 가능

#### 2.3.8 locks (선택적)
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

#### 2.3.9 humanity_change (선택적)
플레이어 인간성 변화량

```json
{
  "humanity_change": -5.0  // 플레이어 인간성 변화량 (양수/음수)
}
```

**주의사항:**
- 0이면 생략 가능 (변화 없음)
- 양수면 인간성 증가, 음수면 감소

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

### 예시 1: 수면제를 탄 홍차를 할머니에게 제공

**요청:**
```json
{
  "chat_input": "할머니에게 수면제를 탄 홍차를 드리겠습니다",
  "npc_name": "grandmother",
  "item_name": "earl_grey_tea"
}
```

**응답:**
```json
{
  "narrative": "할머니가 홍차를 마시고 곧 잠에 빠졌습니다. 이제 안전하게 지하실을 탐색할 수 있습니다.",
  "ending_info": null,
  "state_delta": {
    "flags": {
      "tea_with_sleeping_pill": true,
      "grandmother_cooperation": true
    },
    "inventory_remove": [
      "earl_grey_tea",
      "sleeping_pill"
    ],
    "item_state_changes": [
      {
        "item_name": "earl_grey_tea",
        "new_state": "used"
      }
    ],
    "npc_disabled_states": {
      "grandmother": {
        "is_disabled": true,
        "remaining_turns": 3,
        "reason": "수면제 복용"
      }
    },
    "humanity_change": -5.0
  },
  "debug": {
    "game_id": 12,
    "reasoning": "플레이어가 할머니에게 수면제를 탄 홍차를 제공했습니다. 이는 tea_with_sleeping_pill 플래그를 true로 설정하고, 할머니를 3턴간 무력화시킵니다. 인간성은 -5 감소합니다.",
    "turn_after": 6
  }
}
```

### 예시 2: 라이터로 불 지르기

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
  "state_delta": {
    "flags": {
      "fire_started": true
    },
    "inventory_remove": [
      "silver_lighter"
    ],
    "item_state_changes": [
      {
        "item_name": "silver_lighter",
        "new_state": "used"
      }
    ],
    "humanity_change": -15.0
  },
  "debug": {
    "game_id": 12,
    "reasoning": "플레이어가 라이터로 불을 질렀습니다. fire_started 플래그를 true로 설정하고, 인간성은 -15 감소합니다.",
    "turn_after": 7
  }
}
```

### 예시 3: 엔딩 트리거 (StealthExit)

**요청:**
```json
{
  "chat_input": "저녁 식사 전 홍차에 수면제를 타서 가족들에게 대접한다",
  "npc_name": "",
  "item_name": "sleeping_pill"
}
```

**응답:**
```json
{
  "narrative": "홍차의 진한 향이 수면제의 냄새를 가려줍니다. 평소처럼 순종적인 태도로 가족들에게 차를 대접하자, 모두가 기꺼이 마셔줍니다. 곧 수면제가 효과를 발휘해 온 가족이 식탁에서 잠들기 시작합니다. 조용히 새엄마에게 다가가 목걸이에서 열쇠를 훔쳐냅니다. 아무도 눈치채지 못한 채, 당신은 인형 속 세계에서 달아납니다.",
  "ending_info": {
    "ending_type": "stealth_exit",
    "description": "완벽한 기만 엔딩"
  },
  "state_delta": {
    "flags": {
      "tea_with_sleeping_pill": true,
      "key_stolen": true,
      "family_asleep": true
    },
    "inventory_remove": [
      "sleeping_pill",
      "earl_grey_tea"
    ],
    "npc_disabled_states": {
      "new_mother": {
        "is_disabled": true,
        "remaining_turns": 10,
        "reason": "수면제 복용"
      },
      "new_father": {
        "is_disabled": true,
        "remaining_turns": 10,
        "reason": "수면제 복용"
      },
      "sibling": {
        "is_disabled": true,
        "remaining_turns": 10,
        "reason": "수면제 복용"
      }
    }
  },
  "debug": {
    "game_id": 12,
    "reasoning": "StealthExit 엔딩 조건 충족: 수면제 보유 + 홍차에 수면제 투입 + 가족들에게 대접 + 열쇠 탈취 성공",
    "turn_after": 8
  }
}
```

### 예시 4: 변화 없음 (단순 대화)

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
  "state_delta": {
    "npc_stats": {
      "new_father": {
        "trust": 1.0
      }
    }
  }
}
```

---

## 4. 필드 생략 규칙

**다음 경우 필드를 생략할 수 있습니다:**

1. **빈 배열/객체**: `[]`, `{}`
2. **변화 없음**: 값이 0이거나 기본값인 경우
3. **선택적 필드**: `ending_info`, `debug` 등

**예시:**
```json
{
  "narrative": "대화 텍스트",
  "state_delta": {
    // 모든 필드는 생략 가능 (변화 없음)
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

게임은 에러 응답을 받으면 `narrative`에 에러 메시지를 표시하고, `state_delta`는 적용하지 않습니다.

---

## 6. 구현 참고사항

### 6.1 Unity C# 구조체
현재 `GameDataTypes.cs`의 `BackendStateDelta` 구조체를 확장하여 위 형식을 지원합니다.

### 6.2 변환 로직
`BackendResponseConverter.cs`에서 백엔드 응답을 게임 내부 형식으로 변환합니다.

### 6.3 필드 매핑
백엔드의 snake_case 이름을 Unity의 PascalCase로 변환하는 `NameMapper` 클래스를 사용합니다.

