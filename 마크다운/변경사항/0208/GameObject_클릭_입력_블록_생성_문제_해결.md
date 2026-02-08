# GameObject 클릭 시 InputField에 블록 생성 기능

**작성일:** 2026-02-06  
**문제:** Unity 씬의 GameObject(예: StepMother)를 클릭하면 InputField에 자동으로 블록이 생성되도록 구현

---

## 📋 문제 상황

### 현재 상태
- Unity 씬에 `StepMother` 오브젝트가 존재함
- `InputHandler`가 `TMP_InputField`를 관리하고 있음
- **문제:** GameObject 클릭과 InputField 간의 연결이 없음

### 요구사항
1. **StepMother 오브젝트를 클릭**하면
2. **InputField에 `StepMother` 블록이 자동으로 생성**되어야 함
3. 블록은 코드 참조 형식처럼 보이도록 표시 (예: `@StepMother` 또는 `[StepMother]`)

### 기술적 제약사항
- Unity의 GameObject 클릭 이벤트 감지 필요
- InputField에 텍스트를 동적으로 삽입하는 기능 필요
- 기존 입력 텍스트와의 통합 (기존 텍스트 뒤에 추가)

---

## 🔧 해결 방안

### 접근 방법

#### 1. **GameObject 클릭 감지**
- `IPointerClickHandler` 인터페이스 구현
- 또는 `EventTrigger` 컴포넌트 사용
- 또는 `Collider` + `OnMouseDown()` 사용 (3D 오브젝트인 경우)

#### 2. **InputHandler에 블록 추가 메서드 구현**
- `AddBlockToInput(string blockName)` 메서드 추가
- InputField의 현재 텍스트에 블록을 추가
- 커서 위치에 삽입 또는 텍스트 끝에 추가

#### 3. **GameObject와 InputHandler 연결**
- GameObject에 클릭 이벤트 스크립트 추가
- InputHandler 참조를 통해 블록 추가 메서드 호출

---

## 💻 구현 계획

### Step 1: InputHandler에 블록 추가 메서드 구현

```csharp
// InputHandler.cs에 추가할 메서드
public void AddBlockToInput(string blockName)
{
    if (myInputField != null)
    {
        string blockText = $"@{blockName} "; // 또는 [blockName] 형식
        string currentText = myInputField.text;
        
        // 커서 위치에 삽입하거나 텍스트 끝에 추가
        int caretPosition = myInputField.caretPosition;
        string newText = currentText.Insert(caretPosition, blockText);
        
        myInputField.text = newText;
        myInputField.caretPosition = caretPosition + blockText.Length;
        myInputField.ActivateInputField();
    }
}
```

### Step 2: GameObject 클릭 감지 스크립트 생성

**새 파일:** `Assets/scripts/Ryu/ClickableObject.cs`

```csharp
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableObject : MonoBehaviour, IPointerClickHandler
{
    [Header("Input 연결")]
    public InputHandler inputHandler;
    
    [Header("블록 설정")]
    public string blockName; // Inspector에서 설정하거나 GameObject 이름 사용
    
    void Start()
    {
        // blockName이 비어있으면 GameObject 이름 사용
        if (string.IsNullOrEmpty(blockName))
        {
            blockName = gameObject.name;
        }
        
        // InputHandler를 자동으로 찾기 (선택사항)
        if (inputHandler == null)
        {
            inputHandler = FindObjectOfType<InputHandler>();
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (inputHandler != null)
        {
            inputHandler.AddBlockToInput(blockName);
            Debug.Log($"블록 추가: {blockName}");
        }
        else
        {
            Debug.LogWarning("InputHandler를 찾을 수 없습니다.");
        }
    }
}
```

### Step 3: StepMother 오브젝트에 스크립트 추가

1. Unity Editor에서 `StepMother` GameObject 선택
2. `ClickableObject` 컴포넌트 추가
3. Inspector에서 설정:
   - `Input Handler`: GameManager의 InputHandler 드래그 앤 드롭
   - `Block Name`: "StepMother" (또는 비워두면 GameObject 이름 사용)

### Step 4: UI 이벤트 시스템 확인

- 씬에 `EventSystem`이 있는지 확인
- `GraphicRaycaster`가 Canvas에 있는지 확인 (UI 오브젝트인 경우)
- 3D 오브젝트인 경우 `Collider` 컴포넌트 필요

---

## 🎯 구현 세부사항

### 블록 형식 옵션

1. **코드 참조 형식:** `@StepMother`
2. **대괄호 형식:** `[StepMother]`
3. **태그 형식:** `<StepMother>`
4. **커스텀 형식:** 사용자 정의

### InputField 텍스트 삽입 위치

- **옵션 1:** 커서 위치에 삽입 (권장)
- **옵션 2:** 텍스트 끝에 추가
- **옵션 3:** 텍스트 앞에 추가

### 추가 고려사항

1. **중복 방지:** 같은 블록이 이미 있으면 추가하지 않음
2. **공백 처리:** 블록 앞뒤에 자동으로 공백 추가
3. **포커스 관리:** 블록 추가 후 InputField 자동 활성화

---

## 📝 사용 예시

### 시나리오
1. 사용자가 InputField에 "안녕하세요 " 입력
2. StepMother 오브젝트 클릭
3. InputField에 자동으로 `@StepMother ` 추가
4. 최종 텍스트: "안녕하세요 @StepMother "
5. 엔터 입력 시 전체 텍스트가 서버로 전송

---

## ⚠️ 주의사항

1. **EventSystem 필수:** UI 클릭 이벤트를 사용하려면 씬에 EventSystem이 있어야 함
2. **Raycast 설정:** 3D 오브젝트인 경우 Camera 설정 확인
3. **InputHandler 참조:** GameObject와 InputHandler 간의 연결 필요
4. **컴포넌트 추가:** 각 클릭 가능한 GameObject에 `ClickableObject` 컴포넌트 추가 필요

---

## 🔄 확장 가능성

### 향후 개선 사항
1. **다중 블록 선택:** 여러 GameObject를 선택하여 한 번에 블록 추가
2. **블록 삭제:** 특정 블록을 클릭하여 제거
3. **블록 자동완성:** 입력 중 자동완성 제안
4. **블록 하이라이트:** InputField 내 블록을 다른 색상으로 표시

---

## 📚 참고 자료

- Unity IPointerClickHandler: https://docs.unity3d.com/ScriptReference/EventSystems.IPointerClickHandler.html
- TMP_InputField API: https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/api/TMPro.TMP_InputField.html
- Unity EventSystem: https://docs.unity3d.com/Manual/EventSystem.html

