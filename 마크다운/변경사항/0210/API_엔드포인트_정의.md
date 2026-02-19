# API 엔드포인트 정의

## 개요
백엔드 API의 주요 엔드포인트 정의 및 사용 방법을 정리합니다.

## 1. Scenario 관련 API

### 1.1 사용 가능한 시나리오 목록 조회

**엔드포인트:** `GET /api/v1/scenario/`

**설명:** 사용 가능한 시나리오 목록을 조회합니다.

**요청:**
- HTTP Method: `GET`
- Path: `/api/v1/scenario/`
- 파라미터: 없음

**응답:**
- (상세 응답 구조는 백엔드 문서 참조)

---

### 1.2 시나리오 시작

**엔드포인트:** `GET /api/v1/scenario/start/{scenario_id}`

**설명:** 특정 시나리오를 시작합니다.

**요청:**
- HTTP Method: `GET`
- Path: `/api/v1/scenario/start/{scenario_id}`
- Path Parameters:
  - `scenario_id` (integer, required): 시작할 시나리오 ID
    - 예시: `1`
- Query Parameters:
  - `user_id` (integer, required): 사용자 ID
    - 예시: `1`

**요청 예시:**
```
GET /api/v1/scenario/start/1?user_id=1
```

**응답:**
- (상세 응답 구조는 백엔드 문서 참조)

---

## 2. Game 관련 API

### 2.1 게임 대화 요청

**엔드포인트:** `POST /api/v1/game/{game_id}/step`

**설명:** 게임 내 대화를 요청합니다. 플레이어의 입력을 받아 다음 스텝으로 진행합니다.

**요청:**
- HTTP Method: `POST`
- Path: `/api/v1/game/{game_id}/step`
- Path Parameters:
  - `game_id` (integer, required): 게임 ID
    - 예시: `24`
- Request Body (application/json, required):
  ```json
  {
    "chat_input": "string",
    "npc_name": "string",
    "item_name": "string"
  }
  ```
  - `chat_input` (string): 플레이어의 대화 입력
  - `npc_name` (string): NPC 이름
  - `item_name` (string): 아이템 이름

**요청 예시:**
```json
POST /api/v1/game/24/step
Content-Type: application/json

{
  "chat_input": "안녕하세요",
  "npc_name": "계모",
  "item_name": "열쇠"
}
```

**응답:**
- (상세 응답 구조는 백엔드 문서 참조)

---

## 구현 참고사항

### Unity C# 구현 예시

#### Scenario 시작 API 호출
```csharp
public async Task StartScenario(int scenarioId, int userId)
{
    string url = $"{baseUrl}/api/v1/scenario/start/{scenarioId}?user_id={userId}";
    
    using (UnityWebRequest request = UnityWebRequest.Get(url))
    {
        await request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseJson = request.downloadHandler.text;
            // Json.NET을 사용하여 응답 파싱
            var response = JsonConvert.DeserializeObject<ScenarioStartResponse>(responseJson);
            return response;
        }
        else
        {
            Debug.LogError($"시나리오 시작 실패: {request.error}");
            return null;
        }
    }
}
```

#### Game Step API 호출
```csharp
public async Task<GameStepResponse> SendGameStep(int gameId, GameStepRequest requestData)
{
    string url = $"{baseUrl}/api/v1/game/{gameId}/step";
    
    string jsonBody = JsonConvert.SerializeObject(requestData);
    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
    
    using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
    {
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        
        await request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseJson = request.downloadHandler.text;
            var response = JsonConvert.DeserializeObject<GameStepResponse>(responseJson);
            return response;
        }
        else
        {
            Debug.LogError($"게임 스텝 요청 실패: {request.error}");
            return null;
        }
    }
}

[Serializable]
public class GameStepRequest
{
    public string chat_input;
    public string npc_name;
    public string item_name;
}
```

---

## 참고
- 모든 API는 RESTful 규칙을 따릅니다.
- JSON 형식의 요청/응답을 사용합니다.
- 에러 처리 및 응답 구조는 백엔드 API 문서를 참조하세요.

