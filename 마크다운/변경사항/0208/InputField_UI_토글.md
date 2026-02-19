# InputField / ResultText UI 토글 기능 추가

## 작업일: 2026-02-08

---

## 1. 개요

InputField와 ResultText가 동일한 위치에 배치되어 있어, 두 요소를 `SetActive()`로 토글하여 한 번에 하나만 표시되도록 수정했습니다. 또한 ResultText 클릭 시 다시 InputField로 전환되는 기능을 추가했습니다.

---

## 2. 변경 파일

### `Assets/Scripts/Ryu/InputHandler.cs` (수정)

**변경 전**: InputField와 ResultText가 항상 활성화 상태. 응답 수신 후 InputField에 포커스만 복원.

**변경 후**: 입력/출력 상태에 따라 두 UI를 `SetActive()`로 토글.

---

## 3. 추가된 메서드

| 메서드 | 역할 |
|--------|------|
| `ShowInputField()` | InputField 활성화 + ResultText 비활성화 |
| `ShowResultText()` | ResultText 활성화 + InputField 비활성화 |
| `AddClickListenerToResultText()` | ResultText에 EventTrigger 클릭 이벤트 등록 |
| `OnResultTextClicked()` | ResultText 클릭 시 InputField로 전환 |

---

## 4. UI 토글 동작 흐름

```
[초기 상태]
  InputField: 활성화 (입력 대기)
  ResultText: 비활성화

        ↓ 사용자 Enter 입력

[전송 중]
  InputField: 비활성화
  ResultText: 활성화 ("전송 중..." 표시)

        ↓ 서버 응답 수신 (또는 3초 타임아웃 → 목업 데이터)

[응답 표시]
  InputField: 비활성화
  ResultText: 활성화 (응답 텍스트 표시)

        ↓ ResultText 클릭

[다시 입력 대기]
  InputField: 활성화 (포커스 자동 복원)
  ResultText: 비활성화
```

---

## 5. ResultText 클릭 감지 구현

- `Start()`에서 ResultText GameObject에 `EventTrigger` 컴포넌트를 런타임으로 추가
- `PointerClick` 이벤트를 등록하여 클릭 감지
- `resultText.raycastTarget = true`로 설정하여 클릭 이벤트 수신 가능하게 처리

```csharp
private void AddClickListenerToResultText()
{
    resultText.raycastTarget = true;

    EventTrigger trigger = resultObj.GetComponent<EventTrigger>();
    if (trigger == null)
        trigger = resultObj.AddComponent<EventTrigger>();

    EventTrigger.Entry entry = new EventTrigger.Entry();
    entry.eventID = EventTriggerType.PointerClick;
    entry.callback.AddListener((data) => OnResultTextClicked());
    trigger.triggers.Add(entry);
}
```

---

## 6. 추가된 using 문

```csharp
using UnityEngine.EventSystems;  // EventTrigger 사용을 위해 추가
```

---

## 7. 디버그 로그 추가

| 태그 | 내용 |
|------|------|
| `[InputHandler]` | `ResultText 클릭 → InputField로 전환` |

