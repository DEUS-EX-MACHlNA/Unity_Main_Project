# StepMother / Cake 클릭 시 InputField에 블록 삽입 기능

## 작업일: 2026-02-08

---

## 1. 개요

StepMother와 Cake 오브젝트를 클릭하면 InputField에 `@StepMother` 또는 `@Cake` 형식의 블록 텍스트가 자동으로 삽입되도록 구현했습니다. 또한 InputField의 기존 텍스트가 선택되지 않도록 수정했습니다.

---

## 2. 생성된 파일

### `Assets/Scripts/Ryu/Tutorial/ClickableObject.cs` (신규)

**역할**: 클릭 가능한 오브젝트에 부착하여 클릭 시 InputField에 블록 텍스트 삽입

**주요 기능**:
- `OnMouseDown()`으로 클릭 감지
- `InputHandler.AddBlockToInput()` 호출하여 블록 삽입
- `FindFirstObjectByType<InputHandler>()`로 InputHandler 자동 탐색
- 0.1초 쿨다운으로 중복 클릭 방지

**필수 컴포넌트** (`RequireComponent`):
- `BoxCollider2D` — 마우스 클릭 이벤트 감지용

**Inspector 설정 항목**:
| 항목 | 타입 | 기본값 | 설명 |
|------|------|--------|------|
| blockName | string | (비어있음) | InputField에 삽입될 블록 이름. 비어있으면 GameObject 이름 사용 |
| clickCooldown | float | 0.1 | 중복 클릭 방지 쿨다운 (초) |

**주요 메서드**:
```csharp
private void OnMouseDown()  // 클릭 감지 → InputHandler.AddBlockToInput() 호출
```

---

## 3. 수정된 파일

### `Assets/Scripts/Ryu/Tutorial/InputHandler.cs` (수정)

#### 3.1 추가된 메서드

**`AddBlockToInput(string blockName)`** (공개 메서드)
- 외부에서 호출하여 InputField에 `@블록이름 ` 텍스트를 커서 위치에 삽입
- ResultText가 표시 중이면 자동으로 InputField로 전환
- 커서 위치는 삽입된 텍스트 뒤로 이동

**`LateUpdate()`** (비공개 메서드)
- `ActivateInputField()` 호출 후 발생하는 전체 텍스트 선택을 해제
- `needsDeselect` 플래그와 `pendingCaretPos`를 사용하여 선택 해제 처리

#### 3.2 추가된 필드

```csharp
private bool needsDeselect = false;  // 선택 해제 필요 플래그
private int pendingCaretPos;        // 삽입 후 커서 위치
```

#### 3.3 동작 원리

1. `AddBlockToInput()`에서 텍스트 삽입 후 `ActivateInputField()` 호출
2. `TMP_InputField.ActivateInputField()`는 내부적으로 `LateUpdate`에서 전체 텍스트를 선택함
3. `LateUpdate()`에서 `needsDeselect`가 true이면:
   - `caretPosition`, `selectionAnchorPosition`, `selectionFocusPosition`을 모두 동일한 위치로 설정
   - 결과적으로 텍스트 선택이 해제되고 커서만 표시됨

---

## 4. 씬 변경사항 (Tutorial.unity)

### 컴포넌트 추가

| GameObject | instanceID | 추가된 컴포넌트 |
|------------|-----------|----------------|
| StepMother | 66614 | ClickableObject |
| Cake | 66622 | ClickableObject |

두 오브젝트 모두 기존에 `BoxCollider2D`를 보유하고 있어, `ClickableObject`만 추가하면 즉시 동작합니다.

---

## 5. 동작 흐름

```
[사용자가 StepMother 클릭]
  ↓
ClickableObject.OnMouseDown() 호출
  ↓
InputHandler.AddBlockToInput("StepMother") 호출
  ↓
[InputField에 "@StepMother " 삽입]
  ↓
ActivateInputField() 호출
  ↓
LateUpdate()에서 전체 선택 해제
  ↓
[결과: "@StepMother "만 삽입되고 기존 텍스트는 선택되지 않음]
```

---

## 6. 버그 수정: InputField 전체 텍스트 선택 문제

**문제**: 오브젝트 클릭 시 블록 이름은 정상 삽입되지만, InputField의 모든 텍스트가 선택됨

**원인**: `TMP_InputField.ActivateInputField()`는 내부적으로 `LateUpdate`에서 전체 텍스트를 선택하는 동작을 수행함. 따라서 텍스트 삽입 직후 `ActivateInputField()`를 호출하면, 다음 프레임의 `LateUpdate`에서 전체 선택이 발생함.

**해결**: `LateUpdate()`를 추가하여 `ActivateInputField()`의 전체 선택 동작 이후에 선택을 해제하도록 처리.

```csharp
// AddBlockToInput() 내부
inputField.ActivateInputField();
needsDeselect = true;
pendingCaretPos = newCaretPos;

// LateUpdate()에서 처리
if (needsDeselect)
{
    needsDeselect = false;
    inputField.caretPosition = pendingCaretPos;
    inputField.selectionAnchorPosition = pendingCaretPos;
    inputField.selectionFocusPosition = pendingCaretPos;
}
```

---

## 7. 디버그 로그

| 태그 | 내용 |
|------|------|
| `[ClickableObject]` | `{오브젝트명} 클릭 → @{blockName} 삽입` |
| `[InputHandler]` | `블록 추가: @{blockName}` |

---

## 8. 파일 구조

```
Assets/
└── Scripts/
    └── Ryu/
        ├── Global/
        │   └── ApiClient.cs
        └── Tutorial/
            ├── InputHandler.cs          ← AddBlockToInput() 추가, LateUpdate() 추가
            ├── ClickableObject.cs        ← 클릭 시 블록 삽입 (신규)
            └── HoverGlow.cs
```

