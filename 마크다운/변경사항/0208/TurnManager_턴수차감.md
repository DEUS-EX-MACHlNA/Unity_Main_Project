# API 응답 시 턴수 차감 기능 추가

## 작업일: 2026-02-08

---

## 1. 개요

API 응답을 성공적으로 받았을 때마다(목업 데이터 포함) 남은 턴수가 자동으로 1씩 차감되도록 구현했습니다. 턴수는 HUD_TopLeft의 TurnsText에 표시됩니다.

---

## 2. 생성된 파일

### `Assets/Scripts/Ryu/Tutorial/TurnManager.cs` (신규)

**역할**: 남은 턴수 관리 및 HUD_TopLeft의 TurnsText UI 업데이트

**주요 기능**:
- 초기 턴수 설정 (기본값: 10)
- 턴수 차감 (`ConsumeTurn()`)
- 턴수 조회/설정/리셋
- TurnsText 자동 업데이트

**Inspector 설정 항목**:
| 항목 | 타입 | 기본값 | 설명 |
|------|------|--------|------|
| turnsText | TextMeshProUGUI | (비어있음) | HUD_TopLeft의 TurnsText 참조 |
| initialTurns | int | 10 | 초기 턴수 |

**주요 메서드**:
```csharp
public void ConsumeTurn()              // 턴수 1 차감
public int GetRemainingTurns()         // 남은 턴수 반환
public void SetRemainingTurns(int)     // 턴수 설정
public void ResetTurns()              // 턴수 초기값으로 리셋
private void UpdateTurnsDisplay()     // TurnsText UI 업데이트
```

**UI 표시 형식**: `"남은 턴수 : {remainingTurns}"`

---

## 3. 수정된 파일

### `Assets/Scripts/Ryu/Tutorial/InputHandler.cs` (수정)

#### 3.1 추가된 필드

```csharp
[Header("Turn Management")]
[SerializeField] private TurnManager turnManager;
```

#### 3.2 수정된 메서드

**`OnApiSuccess(string response)`**
- API 응답 수신 시 `turnManager.ConsumeTurn()` 호출
- 목업 데이터를 받은 경우에도 턴수 차감 (목업 데이터는 `onSuccess` 콜백으로 전달되므로)
- TurnManager가 연결되지 않은 경우 경고 로그 출력

```csharp
private void OnApiSuccess(string response)
{
    Debug.Log($"[InputHandler] 응답 수신: {response}");
    resultText.text = response;

    // 턴수 차감 (목업 데이터 포함)
    if (turnManager != null)
    {
        turnManager.ConsumeTurn();
    }
    else
    {
        Debug.LogWarning("[InputHandler] TurnManager가 연결되지 않았습니다.");
    }
}
```

---

## 4. 씬 변경사항 (Tutorial.unity)

### 컴포넌트 추가

| GameObject | instanceID | 추가된 컴포넌트 |
|------------|-----------|----------------|
| GameManager | -4660 | TurnManager |

### Inspector 참조 연결 필요

**GameManager의 TurnManager 컴포넌트**:
- `Turns Text` → `Canvas/HUD_TopLeft/TurnsText` (instanceID: 66606)

**GameManager의 InputHandler 컴포넌트**:
- `Turn Manager` → GameManager의 TurnManager 컴포넌트

---

## 5. 동작 흐름

```
[사용자가 InputField에 입력 후 Enter]
  ↓
InputHandler.OnSubmit() 호출
  ↓
ApiClient.SendMessage() 호출
  ↓
[서버 응답 수신 또는 3초 타임아웃 → 목업 데이터]
  ↓
InputHandler.OnApiSuccess() 호출
  ↓
TurnManager.ConsumeTurn() 호출
  ↓
remainingTurns-- (1 차감)
  ↓
UpdateTurnsDisplay() 호출
  ↓
[HUD_TopLeft/TurnsText에 "남은 턴수 : {remainingTurns}" 표시]
```

---

## 6. 턴수 차감 조건

- ✅ **서버 응답 성공**: 턴수 차감
- ✅ **목업 데이터 반환** (3초 타임아웃 또는 에러): 턴수 차감
- ❌ **API 에러** (`OnApiError` 호출): 턴수 차감 안 함

> **참고**: 목업 데이터도 `onSuccess` 콜백으로 전달되므로 턴수가 차감됩니다. 이는 의도된 동작입니다.

---

## 7. 디버그 로그

| 태그 | 내용 |
|------|------|
| `[TurnManager]` | `턴 소모. 남은 턴수: {remainingTurns}` |
| `[TurnManager]` | `남은 턴수가 0입니다.` (경고) |
| `[TurnManager]` | `턴수 설정: {remainingTurns}` |
| `[TurnManager]` | `턴수 리셋: {remainingTurns}` |
| `[InputHandler]` | `TurnManager가 연결되지 않았습니다.` (경고) |

---

## 8. 파일 구조

```
Assets/
└── Scripts/
    └── Ryu/
        ├── Global/
        │   └── ApiClient.cs
        └── Tutorial/
            ├── InputHandler.cs          ← TurnManager 참조 추가, OnApiSuccess() 수정
            ├── TurnManager.cs             ← 턴수 관리 (신규)
            ├── ClickableObject.cs
            └── HoverGlow.cs
```

