# Night 대화 시스템 백엔드 연동 고려사항

## 작업일: 2026-02-08

---

## 1. 개요

현재 `NightDialogueManager`의 대화 내용이 하드코딩되어 있습니다. 이를 백엔드 API에서 동적으로 받아오도록 변경할 때 고려해야 할 사항들을 정리합니다.

---

## 2. API 설계

### 2.1 엔드포인트 설계

#### 옵션 1: 날짜별 대화 조회
```
GET /api/v1/game/{gameId}/night/dialogue?day={day}
```

**장점:**
- 날짜별로 다른 대화 제공 가능
- 캐싱 용이
- RESTful 설계

**단점:**
- 날짜 정보를 클라이언트에서 전달해야 함

#### 옵션 2: 현재 게임 상태 기반 조회
```
GET /api/v1/game/{gameId}/night/dialogue
```

**장점:**
- 서버에서 게임 상태를 관리하여 적절한 대화 자동 선택
- 클라이언트 로직 단순화

**단점:**
- 서버에서 게임 상태 관리 필요

**권장:** 옵션 1 (날짜별 조회) - 클라이언트가 이미 GameStateManager로 날짜를 관리 중

### 2.2 요청 데이터 구조

```json
{
  "gameId": 1,
  "day": 1
}
```

**Query Parameters:**
- `day` (required): 현재 날짜 (1~5)

**Headers:**
- `Content-Type: application/json`
- 기존 인증 헤더 (필요시)

### 2.3 응답 데이터 구조

```json
{
  "success": true,
  "data": {
    "day": 1,
    "dialogues": [
      {
        "speaker_name": "엘리노어 (새엄마)",
        "dialogue": "오늘 우리 아이가 보여준 미소 보셨나요? 드디어 이 집의 향기에 적응한 것 같아 마음이 놓여요. 식탁 예절도 몰라보게 우아해졌더군요.",
        "order": 1
      },
      {
        "speaker_name": "루카스 (동생)",
        "dialogue": "응, 응! 오늘 누나(형)가 나랑 같이 인형 집으로 한참 동안 놀아줬어. 예전처럼 자꾸 나가려고 하지도 않고... 이제 우리 진짜 가족이 된 거지, 엄마?",
        "order": 2
      }
      // ... 더 많은 대화들
    ]
  },
  "error": null
}
```

**필드 설명:**
- `success`: 요청 성공 여부
- `data.day`: 응답한 날짜
- `data.dialogues`: 대화 배열
  - `speaker_name`: 화자 이름
  - `dialogue`: 대사 내용
  - `order`: 대화 순서 (정렬용, 선택적)

**에러 응답:**
```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "DIALOGUE_NOT_FOUND",
    "message": "해당 날짜의 대화를 찾을 수 없습니다."
  }
}
```

---

## 3. 클라이언트 구현 고려사항

### 3.1 ApiClient 확장

#### 3.1.1 새로운 메서드 추가

```csharp
// ApiClient.cs에 추가
[Serializable]
public class NightDialogueResponse
{
    public bool success;
    public NightDialogueData data;
    public ErrorInfo error;
}

[Serializable]
public class NightDialogueData
{
    public int day;
    public DialogueLine[] dialogues;
}

[Serializable]
public class ErrorInfo
{
    public string code;
    public string message;
}

/// <summary>
/// Night 씬의 대화 내용을 백엔드에서 가져옵니다.
/// </summary>
/// <param name="day">현재 날짜 (1~5)</param>
/// <param name="onSuccess">성공 콜백 (DialogueLine[])</param>
/// <param name="onError">에러 콜백</param>
public Coroutine GetNightDialogues(int day, Action<DialogueLine[]> onSuccess, Action<string> onError)
{
    return StartCoroutine(GetNightDialoguesCoroutine(day, onSuccess, onError));
}
```

#### 3.1.2 데이터 변환

백엔드 응답의 `speaker_name`을 클라이언트의 `speakerName`으로 변환 필요:

```csharp
private DialogueLine[] ConvertToDialogueLines(NightDialogueData data)
{
    return data.dialogues.Select(d => new DialogueLine
    {
        speakerName = d.speaker_name,
        dialogue = d.dialogue
    }).ToArray();
}
```

### 3.2 NightDialogueManager 수정

#### 3.2.1 초기화 흐름 변경

**현재:**
```csharp
Start()
  ↓
InitializeDialogues() // 하드코딩된 대화 로드
  ↓
ShowDialogue(0)
```

**변경 후:**
```csharp
Start()
  ↓
LoadDialoguesFromServer() // API 호출
  ↓
[로딩 UI 표시]
  ↓
[API 응답 대기]
  ↓
InitializeDialogues(dialogues) // 서버에서 받은 대화 설정
  ↓
ShowDialogue(0)
```

#### 3.2.2 로딩 상태 관리

```csharp
[Header("Loading State")]
[SerializeField] private GameObject loadingPanel;
[SerializeField] private TextMeshProUGUI loadingText;

private bool isLoadingDialogues = false;

private void Start()
{
    // 로딩 UI 표시
    ShowLoadingUI();
    
    // API 호출
    LoadDialoguesFromServer();
}

private void LoadDialoguesFromServer()
{
    isLoadingDialogues = true;
    
    int currentDay = GameStateManager.Instance != null 
        ? GameStateManager.Instance.GetCurrentDay() 
        : 1;
    
    if (apiClient != null)
    {
        apiClient.GetNightDialogues(currentDay, OnDialoguesLoaded, OnDialoguesError);
    }
    else
    {
        Debug.LogError("[NightDialogueManager] ApiClient가 연결되지 않았습니다.");
        OnDialoguesError("ApiClient가 연결되지 않았습니다.");
    }
}

private void OnDialoguesLoaded(DialogueLine[] loadedDialogues)
{
    isLoadingDialogues = false;
    HideLoadingUI();
    
    if (loadedDialogues == null || loadedDialogues.Length == 0)
    {
        Debug.LogWarning("[NightDialogueManager] 받은 대화가 비어있습니다. 기본 대화를 사용합니다.");
        InitializeDefaultDialogues();
    }
    else
    {
        dialogues = loadedDialogues;
        SetupUILayout();
        accumulatedText = "";
        dialoguePanel?.SetActive(true);
        ShowDialogue(0);
    }
}

private void OnDialoguesError(string error)
{
    isLoadingDialogues = false;
    Debug.LogError($"[NightDialogueManager] 대화 로드 실패: {error}");
    
    // 폴백: 기본 대화 사용
    Debug.Log("[NightDialogueManager] 기본 대화를 사용합니다.");
    InitializeDefaultDialogues();
    HideLoadingUI();
    SetupUILayout();
    accumulatedText = "";
    dialoguePanel?.SetActive(true);
    ShowDialogue(0);
}
```

### 3.3 폴백 전략

#### 3.3.1 기본 대화 유지

서버 오류 시 하드코딩된 기본 대화를 사용:

```csharp
private void InitializeDefaultDialogues()
{
    // 기존 하드코딩된 대화 내용
    dialogues = new DialogueLine[]
    {
        new DialogueLine { speakerName = "엘리노어 (새엄마)", dialogue = "..." },
        // ...
    };
}
```

#### 3.3.2 로컬 캐싱 (선택사항)

PlayerPrefs나 파일 시스템에 마지막으로 받은 대화를 저장:

```csharp
private const string CACHE_KEY_PREFIX = "NightDialogue_Day_";

private void CacheDialogues(int day, DialogueLine[] dialogues)
{
    string json = JsonUtility.ToJson(new DialogueCache { dialogues = dialogues });
    PlayerPrefs.SetString($"{CACHE_KEY_PREFIX}{day}", json);
    PlayerPrefs.Save();
}

private DialogueLine[] LoadCachedDialogues(int day)
{
    string json = PlayerPrefs.GetString($"{CACHE_KEY_PREFIX}{day}", "");
    if (!string.IsNullOrEmpty(json))
    {
        DialogueCache cache = JsonUtility.FromJson<DialogueCache>(json);
        return cache.dialogues;
    }
    return null;
}
```

---

## 4. 에러 처리

### 4.1 네트워크 에러

- **타임아웃**: 기본 대화 사용
- **연결 실패**: 기본 대화 사용 + 사용자에게 알림
- **서버 에러 (500)**: 기본 대화 사용

### 4.2 데이터 검증

```csharp
private bool ValidateDialogues(DialogueLine[] dialogues)
{
    if (dialogues == null)
    {
        Debug.LogWarning("[NightDialogueManager] 대화 배열이 null입니다.");
        return false;
    }
    
    if (dialogues.Length == 0)
    {
        Debug.LogWarning("[NightDialogueManager] 대화 배열이 비어있습니다.");
        return false;
    }
    
    // 각 대화의 필수 필드 검증
    foreach (var dialogue in dialogues)
    {
        if (string.IsNullOrEmpty(dialogue.speakerName))
        {
            Debug.LogWarning("[NightDialogueManager] 화자 이름이 비어있는 대화가 있습니다.");
            return false;
        }
        
        if (string.IsNullOrEmpty(dialogue.dialogue))
        {
            Debug.LogWarning("[NightDialogueManager] 대사 내용이 비어있는 대화가 있습니다.");
            return false;
        }
    }
    
    return true;
}
```

### 4.3 에러 UI 피드백

```csharp
[Header("Error UI")]
[SerializeField] private GameObject errorPanel;
[SerializeField] private TextMeshProUGUI errorText;
[SerializeField] private float errorDisplayDuration = 3f;

private void ShowError(string message)
{
    if (errorPanel != null)
    {
        errorPanel.SetActive(true);
        if (errorText != null)
        {
            errorText.text = $"대화를 불러오는 중 오류가 발생했습니다.\n{message}\n기본 대화를 사용합니다.";
        }
        StartCoroutine(HideErrorAfterDelay());
    }
}

private IEnumerator HideErrorAfterDelay()
{
    yield return new WaitForSeconds(errorDisplayDuration);
    if (errorPanel != null)
    {
        errorPanel.SetActive(false);
    }
}
```

---

## 5. 성능 최적화

### 5.1 사전 로딩

Tutorial 씬에서 Night 씬으로 전환하기 전에 대화를 미리 로드:

```csharp
// TurnManager.cs 또는 GameStateManager.cs에서
public void PreloadNightDialogues(int day)
{
    if (apiClient != null)
    {
        apiClient.GetNightDialogues(day, 
            dialogues => {
                // 캐시에 저장
                CacheDialogues(day, dialogues);
            },
            error => {
                Debug.LogWarning($"[Preload] 대화 사전 로드 실패: {error}");
            }
        );
    }
}
```

### 5.2 요청 중복 방지

```csharp
private bool isRequestingDialogues = false;

private void LoadDialoguesFromServer()
{
    if (isRequestingDialogues)
    {
        Debug.LogWarning("[NightDialogueManager] 이미 대화를 요청 중입니다.");
        return;
    }
    
    isRequestingDialogues = true;
    // ... API 호출
}
```

### 5.3 타임아웃 설정

```csharp
[Header("API Settings")]
[SerializeField] private float dialogueLoadTimeout = 5f; // 대화 로드 타임아웃 (초)
```

---

## 6. 게임 루프 연동

### 6.1 날짜 정보 전달

```csharp
private void LoadDialoguesFromServer()
{
    int currentDay = 1;
    
    if (GameStateManager.Instance != null)
    {
        currentDay = GameStateManager.Instance.GetCurrentDay();
    }
    else
    {
        Debug.LogWarning("[NightDialogueManager] GameStateManager를 찾을 수 없습니다. 기본값(1일차)을 사용합니다.");
    }
    
    apiClient.GetNightDialogues(currentDay, OnDialoguesLoaded, OnDialoguesError);
}
```

### 6.2 날짜 변경 감지

GameStateManager의 날짜 변경 이벤트를 구독하여 대화 재로드 (선택사항):

```csharp
private void OnEnable()
{
    if (GameStateManager.Instance != null)
    {
        GameStateManager.Instance.OnDayChanged += OnDayChanged;
    }
}

private void OnDisable()
{
    if (GameStateManager.Instance != null)
    {
        GameStateManager.Instance.OnDayChanged -= OnDayChanged;
    }
}

private void OnDayChanged(int newDay)
{
    // Night 씬이 활성화되어 있을 때만 재로드
    if (gameObject.activeInHierarchy)
    {
        LoadDialoguesFromServer();
    }
}
```

---

## 7. 구현 단계

### Phase 1: 기본 연동
1. ApiClient에 `GetNightDialogues` 메서드 추가
2. NightDialogueManager에 API 호출 로직 추가
3. 기본 폴백 전략 구현

### Phase 2: 에러 처리 강화
1. 상세한 에러 처리 추가
2. 에러 UI 피드백 구현
3. 데이터 검증 로직 추가

### Phase 3: 최적화
1. 사전 로딩 구현
2. 캐싱 전략 추가 (선택사항)
3. 성능 모니터링

---

## 8. 테스트 시나리오

### 8.1 정상 케이스
- ✅ 서버에서 대화 정상 수신
- ✅ 날짜별로 다른 대화 표시
- ✅ 대화 순서 정확히 표시

### 8.2 에러 케이스
- ✅ 네트워크 연결 실패 → 기본 대화 사용
- ✅ 타임아웃 발생 → 기본 대화 사용
- ✅ 서버 에러 (500) → 기본 대화 사용
- ✅ 빈 대화 배열 수신 → 기본 대화 사용
- ✅ 잘못된 JSON 형식 → 기본 대화 사용

### 8.3 엣지 케이스
- ✅ GameStateManager가 없는 경우 → 기본값(1일차) 사용
- ✅ 날짜가 범위를 벗어난 경우 (0, 음수, 6 이상) → 기본 대화 사용
- ✅ 대화 중 네트워크 재연결 → 현재 대화 계속 진행

---

## 9. 주의사항

### 9.1 데이터 일관성
- 서버에서 받은 대화의 순서가 보장되어야 함
- `order` 필드가 있다면 정렬 후 사용

### 9.2 메모리 관리
- 대화 데이터가 클 경우 메모리 사용량 고려
- 필요시 이전 날짜 대화 캐시 삭제

### 9.3 보안
- API 키나 인증 토큰이 필요하다면 안전하게 저장
- HTTPS 사용 권장

### 9.4 버전 관리
- API 버전이 변경될 경우 호환성 고려
- 클라이언트 버전과 서버 버전 불일치 시 처리

---

## 10. 참고사항

### 10.1 기존 코드 구조
- `ApiClient.cs`: UnityWebRequest 기반 API 클라이언트
- `GameStateManager.cs`: 싱글톤으로 게임 상태 관리 (날짜 포함)
- `NightDialogueManager.cs`: 현재 하드코딩된 대화 사용

### 10.2 관련 파일
- `Assets/Scripts/Ryu/Global/ApiClient.cs`
- `Assets/Scripts/Ryu/Global/GameStateManager.cs`
- `Assets/Scripts/Ryu/Night/NightDialogueManager.cs`

---

## 11. 추가 개선 사항 (선택사항)

### 11.1 대화 애니메이션
- 서버에서 대화별 애니메이션 타입 지정 가능
- 예: 타이핑 속도, 페이드 효과 등

### 11.2 다국어 지원
- 서버에서 언어 코드를 받아 해당 언어 대화 반환
- 예: `?day=1&lang=ko` 또는 `?day=1&lang=en`

### 11.3 A/B 테스트
- 서버에서 사용자별로 다른 대화 제공
- 게임 밸런스 테스트에 활용

---

## 12. 체크리스트

### 백엔드
- [ ] API 엔드포인트 구현
- [ ] 날짜별 대화 데이터 준비
- [ ] 에러 응답 형식 정의
- [ ] API 문서 작성

### 프론트엔드
- [ ] ApiClient 확장
- [ ] NightDialogueManager 수정
- [ ] 로딩 UI 구현
- [ ] 에러 처리 구현
- [ ] 폴백 전략 구현
- [ ] 데이터 검증 로직 추가
- [ ] 테스트 시나리오 검증

---

## 13. 예상 작업 시간

- **Phase 1 (기본 연동)**: 2-3시간
- **Phase 2 (에러 처리)**: 1-2시간
- **Phase 3 (최적화)**: 1-2시간

**총 예상 시간**: 4-7시간

---

## 14. 결론

백엔드 연동 시 가장 중요한 것은 **안정적인 폴백 전략**입니다. 네트워크 오류나 서버 문제가 발생해도 게임이 중단되지 않도록 기본 대화를 항상 준비해야 합니다.

또한 사용자 경험을 위해 로딩 상태를 명확히 표시하고, 에러 발생 시 적절한 피드백을 제공해야 합니다.

