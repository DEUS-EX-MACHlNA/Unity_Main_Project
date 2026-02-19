# 백엔드 전달사항 (클라이언트 연동 계약 / 변경 요약)

## 0. 목적
Unity 클라이언트는 **상태 판단(특히 엔딩 판정)을 프론트에서 하지 않고**, 백엔드 응답의 **trigger 결과**를 적용합니다.  
또한 현재 프로젝트에서는 **NPC 위치(`npc_locations`)를 백엔드 응답에서 제거**했고, **시나리오 시작 API도 제거**하여 클라이언트가 `game_id`를 로컬에서 생성합니다.

본 문서는 백엔드 구현/연동 시 필요한 **필수 계약**을 정리합니다.

---

## 1. 응답 스펙 기준 문서
- 기준: `BackendResponseSpec.md`
- 핵심 필드:
  - `narrative` (필수)
  - `ending_info` (선택)
  - `state_result` (필수, 단 변경이 없으면 내부 필드는 생략 가능)

---

## 2. 엔딩 처리 정책 (중요)
### 2.1 프론트는 엔딩 조건을 자체 판정하지 않음
- 클라이언트는 엔딩 조건(아이템/플래그/시간/장소/호감도 등)을 **직접 계산하지 않습니다.**
- 엔딩은 백엔드가 판단하여 아래 필드로 내려줘야 합니다.

### 2.2 엔딩 트리거 전달 방식
- 백엔드가 엔딩을 트리거하려면, 응답에 아래를 포함합니다.

```json
{
  "ending_info": {
    "ending_type": "stealth_exit",
    "description": "선택"
  }
}
```

- `ending_type` 허용값 매핑은 `BackendResponseSpec.md`의 엔딩 타입 표를 따릅니다.
- 클라이언트는 `ending_info.ending_type`을 받으면 즉시 엔딩 씬 전환을 수행합니다.

### 2.3 인간성 0% (UnfinishedDoll) 예외
- 클라이언트는 **인간성 0% 도달 시 즉시 GameOver(=UnfinishedDoll) 엔딩 처리**를 합니다. (프론트 폴백)
- 따라서 백엔드가 별도로 `unfinished_doll`을 내려줘도 되지만, 클라이언트에서 이미 처리될 수 있습니다.

---

## 3. NPC 위치(`npc_locations`) 정책 변경: 백엔드에서 제거
### 3.1 결론
- 백엔드 응답에서 **`state_result.npc_locations` 필드는 사용하지 않습니다.**
- Unity 클라이언트 코드에서도 `npc_locations` 파싱/적용 경로를 제거했습니다.

### 3.2 백엔드 요청/응답에서 기대사항
- 백엔드는 `npc_locations`를 **보내지 않아야** 합니다. (있어도 무시/미사용이 아니라, 구조 자체에서 제외하는 방향)
- NPC 위치는 현재 프론트 내부 상태로만 운영(또는 추후 별도 방식으로 재정의)합니다.

---

## 4. 시나리오 시작 API 제거 및 game_id 생성 방식 변경 (중요)
### 4.1 결론
- 기존 `GET /api/v1/scenario/start/{scenario_id}?user_id={user_id}` 호출을 **클라이언트에서 제거**했습니다.
- 클라이언트는 게임 시작 시 `scenarioId`, `userId`를 기반으로 **로컬에서 `game_id`를 생성**합니다.

### 4.2 백엔드에 필요한 동작
- 백엔드는 `POST /api/v1/game/{game_id}/step` 요청을 받았을 때:
  - 해당 `game_id` 세션이 아직 없다면 **첫 요청 시 자동으로 세션을 생성**해야 합니다.
  - 세션 생성에 `user_id`, `scenario_id`가 필요하다면, 아래 중 하나를 택해 설계를 맞춰야 합니다.

#### 옵션 A (권장): Step 요청 바디에 user/scenario 포함
- `step` 요청 바디에 `user_id`, `scenario_id`를 추가해서 백엔드가 세션 생성에 사용

#### 옵션 B: game_id만으로 내부에서 추적
- `game_id`만으로 세션을 구성하고, user/scenario 매핑을 서버가 자체적으로 관리

> 현재 클라이언트 `step` 요청 바디는 `chat_input`, `npc_name`, `item_name`만 포함합니다.  
> 백엔드가 세션 생성에 추가 값이 필요하면, **요청 바디 확장 요구사항을 백엔드에서 명시**해주세요.

---

## 5. `state_result` 처리 원칙
- `state_result`는 “델타”가 아니라 “백엔드가 판단한 최종 결과” 형태로 보내는 것을 전제로 합니다. (문서 기준)
- 변경 없는 필드는 생략 가능합니다.

### 5.1 클라이언트 적용 우선순위
- 클라이언트는 **백엔드가 내려준 필드만 적용**합니다.
- (예) `flags`가 없으면 기존 플래그 유지, `inventory_add/remove`가 없으면 인벤토리 변화 없음.

---

## 6. 에러 응답 계약
`BackendResponseSpec.md`의 에러 형태를 따릅니다.

```json
{
  "error": {
    "code": "INVALID_ACTION",
    "message": "수면제가 인벤토리에 없습니다.",
    "details": {}
  }
}
```

- 클라이언트는 에러 응답이면 `narrative` 대신 에러 메시지를 표시하고, `state_result`는 적용하지 않습니다.

---

## 7. 체크리스트 (백엔드)
- [ ] `npc_locations` 필드 미사용/미전달
- [ ] 엔딩은 `ending_info.ending_type`로만 트리거 전달
- [ ] `scenario/start` 호출 없이도 `game/{game_id}/step` 첫 요청에서 세션 생성 가능
- [ ] `state_result`는 변경 필드만 포함 가능 (생략 허용)
- [ ] 에러 응답은 `error` 객체 형태로 반환


