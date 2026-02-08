# InputField → Backend API → ResultText 연동 변경사항

## 작업일: 2026-02-08

---

## 1. 개요

Tutorial 씬의 InputField(TMP)에 사용자가 텍스트를 입력하고 Enter를 누르면, 백엔드 서버에 POST 요청을 보내고 응답을 ResultText에 표시하는 기능을 구현했습니다. 서버 응답이 3초 이상 지연되면 목업 데이터를 자동으로 반환합니다.

---

## 2. 생성된 파일

### 2.1 `Assets/Scripts/Ryu/Global/ApiClient.cs` (신규)

**역할**: 백엔드 서버와의 HTTP 통신 전담

**주요 기능**:
- `UnityWebRequest`를 사용한 POST 요청
- 엔드포인트: `/api/v1/game/{gameId}/step`
- 요청 형식:
  ```json
  {
    "chat_input": "사용자 입력 텍스트",
    "npc_name": null,
    "item_name": null
  }
  ```
- 응답 형식: `{ "response": "응답 텍스트" }`
- 3초 타임아웃 또는 에러 시 목업 데이터 반환: `"서버 응답을 기다리는 중... 기본 응답입니다."`

**Inspector 설정 가능 항목**:
| 항목 | 기본값 | 설명 |
|------|--------|------|
| baseUrl | `https://7783-115-95-186-2.ngrok-free.app` | 백엔드 서버 URL |
| gameId | 1 | 게임 ID |
| timeoutSeconds | 3 | 타임아웃 시간 (초) |

**주요 클래스**:
- `StepRequest`: 요청 데이터 (chat_input, npc_name, item_name)
- `GameResponse`: 응답 데이터 (response)

**공개 메서드**:
```csharp
public Coroutine SendMessage(string chatInput, Action<string> onSuccess, Action<string> onError)
```

---

### 2.2 `Assets/Scripts/Ryu/InputHandler.cs` (신규)

**역할**: InputField 입력 이벤트 처리 및 결과 표시

**주요 기능**:
- `TMP_InputField`의 `onSubmit` 이벤트 (Enter 키) 수신
- 입력 텍스트를 ApiClient로 전달하여 서버 요청
- 서버 응답(또는 목업 데이터)을 `ResultText`에 표시
- 응답 수신 후 InputField에 자동 포커스 복원
- 전송 중 "전송 중..." 메시지 표시

**Inspector 설정 항목**:
| 항목 | 타입 | 설명 |
|------|------|------|
| inputField | TMP_InputField | 사용자 입력 필드 참조 |
| resultText | TextMeshProUGUI | 결과 표시 텍스트 참조 |
| apiClient | ApiClient | API 통신 클래스 참조 |

**주요 메서드**:
```csharp
private void OnSubmit(string text)      // Enter 입력 시 호출
private void OnApiSuccess(string response) // 서버 응답 성공
private void OnApiError(string error)      // 서버 응답 에러
```

---

## 3. 씬 변경사항 (Tutorial.unity)

### 3.1 새로 추가된 GameObject

| GameObject | 위치 | 컴포넌트 |
|------------|------|----------|
| GameManager | (0, 0, 0) | Transform, ApiClient, InputHandler |

### 3.2 컴포넌트 참조 연결

`InputHandler` 컴포넌트에 다음 참조가 연결됨:
- **inputField** → `Canvas/InputField (TMP)` (instanceID: 66596)
- **resultText** → `Canvas/ResultText` (instanceID: 66588)
- **apiClient** → `GameManager`의 ApiClient 컴포넌트

---

## 4. 데이터 흐름

```
1. 사용자가 InputField에 텍스트 입력
2. Enter 키 입력 → InputHandler.OnSubmit() 호출
3. ResultText에 "전송 중..." 표시, InputField 비우기
4. ApiClient.SendMessage()로 백엔드 서버에 POST 요청
5-a. 3초 이내 응답 → 서버 응답 텍스트를 ResultText에 표시
5-b. 3초 타임아웃 또는 에러 → 목업 데이터를 ResultText에 표시
6. InputField에 포커스 복원
```

---

## 5. 디버그 로그

| 태그 | 내용 |
|------|------|
| `[ApiClient]` | POST 요청 URL 및 Body, 응답 수신, 타임아웃/에러 |
| `[InputHandler]` | 입력 전송, 응답 수신, 에러 |

---

## 6. 파일 구조

```
Assets/
└── Scripts/
    └── Ryu/
        ├── Global/
        │   └── ApiClient.cs          ← HTTP 통신 (신규)
        └── InputHandler.cs            ← 입력 처리 (신규)
```

