# state_result.inventory_add 응답 시 인벤토리 추가 구현 계획

**작성일:** 2026-02-22  
**참조:** [완벽한기만.md](../../백엔드/완벽한기만.md), [BackendResponseSpec.md](../../백엔드/BackendResponseSpec.md)

---

## 1. 목표

백엔드가 `state_result.inventory_add` 배열을 포함한 응답을 보내면, 해당 아이템들을 플레이어 인벤토리에 추가하는 로직을 명확히 하고, 필요 시 보완·검증한다.

---

## 2. 스펙 요약

- **응답 필드:** `state_result.inventory_add` (문자열 배열, 선택적)
- **예시:** `["industrial_sedative"]`, `["secret_key"]`
- **의미:** 각 ID는 1개씩 획득(문서 기준 개수는 기본 1). 백엔드가 여러 ID를 넣으면 각각 1개씩 추가.
- **아이템 ID 매핑:** BackendResponseSpec / NameMapper 참조 (예: `industrial_sedative` → 수면제, `secret_key` → 비밀 열쇠).

---

## 3. 현재 구현 상태 (기존 흐름)

대부분의 흐름은 이미 구현되어 있음. 아래는 데이터가 지나가는 경로 정리.

| 단계 | 담당 | 역할 |
|------|------|------|
| 1 | `BackendStateDelta` (GameDataTypes.cs) | `state_result.inventory_add` JSON 역직렬화 |
| 2 | `BackendResponseConverter.ConvertBackendResponseToCurrentFormat` | `inventory_add` → `ItemChanges.acquired_items` (item_name, count=1) |
| 3 | `GameStepApiClient` / `NightDialogueApiClient` | API 응답 수신 후 Converter 호출, 콜백에 `itemChanges` 전달 |
| 4 | `ApiResponseHandler.OnApiSuccess` / `NightDialogueManager` (밤 대화) | `itemChanges != null` 일 때 `ItemStateApplier.ApplyItemChanges(manager, itemChanges)` 호출 |
| 5 | `ItemStateApplier.ApplyItemChanges` | `acquired_items` 순회: `NameMapper.ConvertItemNameToType(item_name)` → `manager.AddItem(itemType, count)` |
| 6 | `GameStateManager.AddItem` | `InventoryManager.AddItem(item, count)` + `ItemStateManager.OnItemAddedToInventory(item)` |
| 7 | `InventoryManager.AddItem` | 딕셔너리 개수 증가, `OnInventoryChanged` 이벤트 발생 |

**정리:**  
`state_result.inventory_add`가 오면 → Converter에서 `ItemChanges.acquired_items`로 변환 → 게임 스텝/밤 대화 성공 콜백에서 `ItemStateApplier.ApplyItemChanges` 호출 → `GameStateManager.AddItem` → 인벤토리 추가 + 월드 아이템 상태 InInventory 반영.

---

## 4. 구현 계획 (점검·보완 항목)

### 4.1 확인할 사항 (이미 되어 있을 가능성 높음)

- [ ] **게임 스텝 응답 경로**  
  채팅/행동 한 번에 대한 API 응답을 처리하는 곳에서 `ConvertBackendResponseToCurrentFormat` 결과의 `itemChanges`를 콜백으로 넘기고, `ApiResponseHandler`에서 `ItemStateApplier.ApplyItemChanges`를 호출하는지 확인.
- [ ] **밤 대화 응답 경로**  
  `POST .../night_dialogue` 응답에도 `state_result.inventory_add`가 올 수 있으면, 밤 대화용 Converter 호출 후 `itemChanges`를 `ApplyItemChanges`에 넘기는지 확인 (NightDialogueManager 등).
- [ ] **NameMapper**  
  완벽한기만 시나리오에서 쓰는 ID(`industrial_sedative`, `secret_key` 등)가 `NameMapper.ConvertItemNameToType`에서 `ItemType.None`이 아닌 타입으로 매핑되는지 확인.

### 4.2 보완 시 고려할 부분

- **알 수 없는 아이템명**  
  `ConvertItemNameToType`이 `ItemType.None`을 반환하면 현재 `ItemStateApplier`는 해당 항목을 스킵함. 로그는 `NameMapper`에서 경고로 남음. 필요 시 `ItemStateApplier`에서도 "unknown item skipped" 로그 추가 가능.
- **인벤토리 UI 갱신**  
  `InventoryManager.OnInventoryChanged` → `GameStateManager.OnInventoryChanged` 구독처(예: 인벤토리 패널, 아이템 개수 표시)가 실제로 연결되어 있는지 확인. 연결되어 있으면 별도 작업 불필요.
- **월드 표시**  
  `GameStateManager.AddItem`에서 이미 `ItemStateManager.OnItemAddedToInventory(item)`를 호출하므로, 해당 아이템이 월드에 있던 경우 `InInventory`로 상태가 바뀌어 씬에서 사라지거나 비활성화되는지 ItemStateDisplay 등과 연동만 점검하면 됨.

### 4.3 수정이 필요한 경우 (예시)

- 게임 스텝/밤 대화 중 한 경로에서 `itemChanges`를 쓰지 않고 있다면, 해당 경로에 `ItemStateApplier.ApplyItemChanges(gameStateManager, itemChanges)` 호출 추가.
- 백엔드에서 새 아이템 ID를 쓰기로 했다면 `NameMapper`의 `itemNameMapping`에 항목 추가.

---

## 5. 검증 방법

### 5.1 시나리오 JSON으로 확인

- **수면제 획득:** `Assets/TestData/Scenarios/sleeping_pill_acquisition.json`  
  - 응답에 `state_result.inventory_add: ["industrial_sedative"]` (또는 `sleeping_pill`) 포함되도록 mock/실서버 설정 후, 스텝 실행 → 인벤토리에 수면제 1개 추가되는지 확인.
- **비밀 열쇠 획득:** `Assets/TestData/Scenarios/secret_key_living_room.json`  
  - `inventory_add: ["secret_key"]` 응답 후 인벤토리에 비밀 열쇠(또는 BrassKey) 1개 추가되는지 확인.

### 5.2 체크리스트

- [ ] 게임 스텝 API 응답에 `inventory_add` 포함 시 인벤토리 개수 증가
- [ ] 밤 대화 API 응답에 `inventory_add` 포함 시(해당 스펙일 때) 인벤토리 개수 증가
- [ ] 인벤토리 UI(있는 경우)가 즉시 갱신되는지
- [ ] 수면제/비밀 열쇠 등 해당 아이템의 월드 상태가 InInventory로 바뀌는지(표시 반영 여부)

---

## 6. 참고 파일

| 구분 | 경로 |
|------|------|
| 백엔드 응답 타입 | `Assets/Scripts/Ryu/Global/API/GameDataTypes.cs` (BackendStateDelta, ItemChanges) |
| 변환 | `Assets/Scripts/Ryu/Global/API/BackendResponseConverter.cs` |
| 적용 | `Assets/Scripts/Ryu/Global/State/ItemStateApplier.cs` |
| 인벤토리 | `Assets/Scripts/Ryu/Global/Managers/InventoryManager.cs`, `GameStateManager.cs` |
| 이름 매핑 | `Assets/Scripts/Ryu/Global/Utils/NameMapper.cs` |
| 응답 처리 | `Assets/Scripts/Ryu/Global/API/ApiResponseHandler.cs`, `GameStepApiClient.cs` |
| 시나리오 데이터 | `Assets/TestData/Scenarios/sleeping_pill_acquisition.json`, `secret_key_living_room.json` |

---

## 7. 요약

- **목표:** `state_result.inventory_add` 수신 시 해당 아이템을 인벤토리에 추가.
- **현재:** Converter → ItemChanges → ApplyItemChanges → AddItem → InventoryManager + ItemStateManager 연동까지 구현된 상태.
- **다음 단계:** 위 4.1·4.2 항목 점검 후, 부족한 경로만 보완하고 5.1·5.2로 검증.
