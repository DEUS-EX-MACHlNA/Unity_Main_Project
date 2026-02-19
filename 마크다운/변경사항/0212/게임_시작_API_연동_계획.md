# 게임 시작 API 연동 계획 (2026-02-12)

## 개요
게임 시작 시 백엔드 API를 호출하여 `game_id`와 `user_id`를 받아 게임을 시작하도록 변경합니다.

## 현재 상태

### 현재 구현
- `ApiClient.StartScenario()`: 로컬에서 `gameId`를 생성하는 방식
- `TitleManager`: 게임 시작 버튼 클릭 시 바로 PlayersRoom 씬으로 전환
- 백엔드 API 호출 없이 클라이언트가 자체적으로 `gameId` 생성

### 목표
- 게임 시작 시 백엔드 API `GET /api/v1/scenario/start/{scenario_id}?user_id={user_id}` 호출
- 응답으로 받은 `game_id`를 `ApiClient`에 설정
- `game_id` 설정 후 PlayersRoom 씬으로 전환

## 백엔드 API 스펙

### 엔드포인트
```
GET /api/v1/scenario/start/{scenario_id}?user_id={user_id}
```

### 요청
- **Path Parameters:**
  - `scenario_id` (integer, required): 시나리오 ID
- **Query Parameters:**
  - `user_id` (integer, required): 사용자 ID

### 응답
```json
{
  "game_id": 40,
  "user_id": 1
}
```

### 예시
```bash
curl -X 'GET' \
  'https://d564-115-95-186-2.ngrok-free.app/api/v1/scenario/start/4?user_id=1' \
  -H 'accept: application/json'
```

## 수정이 필요한 파일

### 1. GameDataTypes.cs
**위치**: `Assets/Scripts/Ryu/Global/API/GameDataTypes.cs`

**추가할 내용:**
- `ScenarioStartResponse` 클래스 정의

```csharp
/// <summary>
/// 시나리오 시작 API 응답 구조
/// </summary>
[Serializable]
public class ScenarioStartResponse
{
    [JsonProperty("game_id")]
    public int game_id;
    
    [JsonProperty("user_id")]
    public int user_id;
}
```

### 2. ScenarioStartApiClient.cs (새 파일)
**위치**: `Assets/Scripts/Ryu/Global/API/ScenarioStartApiClient.cs`

**기능:**
- 시나리오 시작 API를 호출하는 클라이언트
- `GameStepApiClient`와 유사한 패턴으로 구현
- UnityWebRequest를 사용한 GET 요청
- Json.NET을 사용한 응답 파싱

**주요 메서드:**
```csharp
public IEnumerator StartScenarioCoroutine(
    int scenarioId,
    int userId,
    Action<ScenarioStartResponse> onSuccess,
    Action<string> onError)
```

### 3. ApiClient.cs
**위치**: `Assets/Scripts/Ryu/Global/API/ApiClient.cs`

**수정 내용:**
1. `ScenarioStartApiClient` 인스턴스 추가
2. `StartScenario()` 메서드 수정:
   - 현재: 로컬에서 `gameId` 생성
   - 변경: 백엔드 API 호출 후 응답으로 받은 `game_id` 사용
3. `Awake()`에서 `ScenarioStartApiClient` 초기화

**변경 전:**
```csharp
public int StartScenario(int scenarioId, int userId)
{
    // 로컬에서 gameId 생성
    int seed = Environment.TickCount;
    int generated = unchecked((userId * 1000003) ^ (scenarioId * 9176) ^ seed);
    generated = Mathf.Abs(generated);
    if (generated == 0) generated = 1;
    
    SetGameId(generated);
    return gameId;
}
```

**변경 후:**
```csharp
public Coroutine StartScenario(int scenarioId, int userId, Action<int> onSuccess, Action<string> onError)
{
    return StartCoroutine(scenarioStartApiClient.StartScenarioCoroutine(
        scenarioId,
        userId,
        (response) => {
            SetGameId(response.game_id);
            onSuccess?.Invoke(response.game_id);
        },
        onError
    ));
}
```

### 4. TitleManager.cs
**위치**: `Assets/Scripts/Ryu/Title/TitleManager.cs`

**수정 내용:**
1. `ApiClient` 참조 추가
2. `scenario_id`와 `user_id` 설정 방법 결정:
   - 옵션 A: 하드코딩 (임시)
   - 옵션 B: UI 입력 필드 추가 (추후 구현)
3. `OnStartButtonClicked()` 수정:
   - 백엔드 API 호출
   - 성공 시 `game_id` 설정 후 씬 전환
   - 실패 시 에러 메시지 표시

**변경 전:**
```csharp
private void OnStartButtonClicked()
{
    Debug.Log("[TitleManager] 게임 시작");
    // 바로 씬 전환
    fadeManager.LoadSceneWithFade(PLAYERS_ROOM_SCENE_NAME, FADE_DURATION);
}
```

**변경 후:**
```csharp
private void OnStartButtonClicked()
{
    Debug.Log("[TitleManager] 게임 시작");
    
    // ApiClient 가져오기
    ApiClient apiClient = FindFirstObjectByType<ApiClient>();
    if (apiClient == null)
    {
        Debug.LogError("[TitleManager] ApiClient를 찾을 수 없습니다.");
        return;
    }
    
    // 시나리오 시작 API 호출
    int scenarioId = 1; // 임시: 하드코딩 (추후 UI 입력으로 변경)
    int userId = 1;     // 임시: 하드코딩 (추후 UI 입력으로 변경)
    
    apiClient.StartScenario(scenarioId, userId,
        (gameId) => {
            Debug.Log($"[TitleManager] 게임 시작 성공: gameId={gameId}");
            // 씬 전환
            if (fadeManager != null)
            {
                fadeManager.LoadSceneWithFade(PLAYERS_ROOM_SCENE_NAME, FADE_DURATION);
            }
            else
            {
                SceneManager.LoadScene(PLAYERS_ROOM_SCENE_NAME);
            }
        },
        (error) => {
            Debug.LogError($"[TitleManager] 게임 시작 실패: {error}");
            // 에러 메시지 표시 (선택적)
        }
    );
}
```

## 구현 단계

### Step 1: 데이터 타입 정의
1. `GameDataTypes.cs`에 `ScenarioStartResponse` 클래스 추가

### Step 2: API 클라이언트 생성
1. `ScenarioStartApiClient.cs` 파일 생성
2. `GameStepApiClient` 패턴 참고하여 구현
3. GET 요청 처리
4. Json.NET을 사용한 응답 파싱

### Step 3: ApiClient 수정
1. `ScenarioStartApiClient` 인스턴스 추가
2. `StartScenario()` 메서드 수정
3. `Awake()`에서 초기화

### Step 4: TitleManager 수정
1. `ApiClient` 참조 추가
2. `OnStartButtonClicked()` 수정
3. API 호출 후 씬 전환 로직 구현

### Step 5: 테스트
1. 게임 시작 버튼 클릭
2. 백엔드 API 호출 확인
3. `game_id` 설정 확인
4. PlayersRoom 씬 전환 확인

## 참고사항

### 에러 처리
- API 호출 실패 시 사용자에게 알림
- 네트워크 오류 처리
- 타임아웃 처리 (기존 `timeoutSeconds` 사용)

### scenario_id와 user_id 입력 방법
- **1단계 (현재)**: 하드코딩으로 임시 구현
- **2단계 (추후)**: Title 씬에 입력 필드 추가
  - InputField 2개 (scenario_id, user_id)
  - 입력값 검증
  - 버튼 클릭 시 입력값 사용

### 기존 코드와의 호환성
- `ApiClient.SetGameId()` 메서드는 그대로 사용
- `GameStepApiClient`는 변경 없음 (이미 `getGameId` 함수 사용)
- 다른 시스템에 영향 없음

## 체크리스트

- [ ] `GameDataTypes.cs`에 `ScenarioStartResponse` 추가
- [ ] `ScenarioStartApiClient.cs` 생성 및 구현
- [ ] `ApiClient.cs`에 `ScenarioStartApiClient` 추가
- [ ] `ApiClient.StartScenario()` 메서드 수정
- [ ] `TitleManager.cs`에 `ApiClient` 참조 추가
- [ ] `TitleManager.OnStartButtonClicked()` 수정
- [ ] 게임 시작 버튼 클릭 테스트
- [ ] 백엔드 API 호출 확인
- [ ] `game_id` 설정 확인
- [ ] 씬 전환 확인
- [ ] 에러 처리 테스트

## 추가 개선 사항 (선택)

### UI 입력 필드 추가
- Title 씬에 `scenario_id`와 `user_id` 입력 필드 추가
- 입력값 검증
- 기본값 설정 가능

### 로딩 UI
- API 호출 중 로딩 표시
- 버튼 비활성화 (중복 클릭 방지)

### 에러 메시지 표시
- API 호출 실패 시 사용자에게 메시지 표시
- 재시도 버튼 제공

