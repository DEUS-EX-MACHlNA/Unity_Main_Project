# 밤의 대화 엔드포인트 초안

## 개요
밤의 대화를 위한 API 엔드포인트 설계 초안입니다. 낮 대화(`POST /api/v1/game/{game_id}/step`)와 유사한 형태로 설계되었으며, 플레이어가 가족들의 대화를 엿듣는 시나리오에 맞게 최적화되었습니다.

## 작업일: 2026-02-13

---

## 1. 밤의 대화 API

### 1.1 밤의 대화 요청

**엔드포인트:** `POST /api/v1/game/{game_id}/night_dialogue`

**설명:** 밤 씬에서 가족들의 대화를 요청합니다. 낮 동안의 플레이어 행동과 게임 상태에 따라 대화 내용이 동적으로 생성됩니다.

**요청:**
- HTTP Method: `POST`
- Path: `/api/v1/game/{game_id}/night_dialogue`
- Path Parameters:
  - `game_id` (integer, required): 게임 ID
    - 예시: `24`
- Request Body (application/json, required):
  ```json
  {
    "day": 1
  }
  ```
  - `day` (integer, required): 현재 날짜 (1~5)
    - 예시: `1`

**요청 예시:**
```json
POST /api/v1/game/24/night_dialogue
Content-Type: application/json

{
  "day": 1
}
```

**응답:**
```json
{
  "dialogues": [
    {
      "speaker_name": "엘리노어 (새엄마)",
      "dialogue": "오늘 우리 아이가 보여준 미소 보셨나요? 드디어 이 집의 향기에 적응한 것 같아 마음이 놓여요."
    },
    {
      "speaker_name": "루카스 (동생)",
      "dialogue": "응, 응! 오늘 누나(형)가 나랑 같이 인형 집으로 한참 동안 놀아줬어."
    },
    {
      "speaker_name": "아더 (새아빠)",
      "dialogue": "음, 확실히 소란을 피우지 않더군. 낮 동안 방 안에서 얌전히 지내는 걸 확인했소."
    }
  ],
  "humanity_state_result": 90.0,
  "npc_affection_state_results": {
    "엘리노어": 50.0,
    "루카스": 30.0,
    "아더": 20.0
  },
  "npc_humanity_state_results": {
    "엘리노어": -100,
    "루카스": -60.0,
    "아더": -20.0
  },
  "event_flags": {
  },
  "ending_trigger": null
}
```

**응답 필드 설명:**

| 필드명 | 타입 | 설명 |
|------|------|------|
| `dialogues` | array | 대화 목록 (순차적으로 표시) |
| `dialogues[].speaker_name` | string | 화자 이름 |
| `dialogues[].dialogue` | string | 대화 내용 |
| `humanity_state_result` | float | 인간성 결과값 (변화량 반영 후 최종 값) |
| `npc_affection_state_results` | object | NPC 호감도 결과값 (key: NPC 이름, value: 변화량 반영 후 최종 값) |
| `npc_humanity_state_results` | object | NPC 인간성 결과값 (key: NPC 이름, value: 변화량 반영 후 최종 값) |
| `event_flags` | object | 이벤트 플래그 (key: 플래그 이름, value: boolean) |
| `ending_trigger` | string \| null | 엔딩 트리거 (해당 시 엔딩 ID, 없으면 null) |

---

## 2. 낮 대화와의 차이점

### 2.1 요청 차이점

| 항목 | 낮 대화 (`/step`) | 밤의 대화 (`/night_dialogue`) |
|------|------------------|------------------------------|
| **플레이어 입력** | ✅ `chat_input` 필수 | ❌ 없음 (엿듣기) |
| **NPC 이름** | ✅ `npc_name` 필수 | ❌ 없음 (여러 화자) |
| **아이템 이름** | ✅ `item_name` 필수 | ❌ 없음 |
| **날짜 정보** | ❌ 없음 | ✅ `day` 필수 |

### 2.2 응답 차이점

| 항목 | 낮 대화 | 밤의 대화 |
|------|---------|-----------|
| **대화 형식** | 단일 응답 문자열 | 대화 배열 (여러 화자) |
| **대화 순서** | 순차적이지 않음 | 순차적 (배열 순서대로) |
| **인간성 결과** | 동적 (행동에 따라) | 변화량 반영 후 최종 값 |

---

## 3. 구현 참고사항

### 3.1 Unity C# 구현 예시

#### Night Dialogue API 호출
```csharp
[Serializable]
public class NightDialogueRequest
{
    public int day;
}

[Serializable]
public class NightDialogueResponse
{
    public DialogueLine[] dialogues;
    public float humanity_state_result;
    public Dictionary<string, float> npc_affection_state_results;
    public Dictionary<string, float> npc_humanity_state_results;
    public Dictionary<string, bool> event_flags;
    public string ending_trigger;
}

[Serializable]
public class DialogueLine
{
    public string speaker_name;
    public string dialogue;
}

public IEnumerator RequestNightDialogueCoroutine(
    int day,
    Action<NightDialogueResponse> onSuccess,
    Action<string> onError)
{
    int gameId = getGameId();
    string url = $"{baseUrl}/api/v1/game/{gameId}/night_dialogue";

    NightDialogueRequest requestData = new NightDialogueRequest
    {
        day = day
    };

    string jsonBody = JsonConvert.SerializeObject(requestData);
    Debug.Log($"[NightDialogueApiClient] POST {url} | Body: {jsonBody}");

    using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
    {
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("ngrok-skip-browser-warning", "true");
        request.timeout = Mathf.CeilToInt(timeoutSeconds);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError
            || request.result == UnityWebRequest.Result.ProtocolError
            || request.result == UnityWebRequest.Result.DataProcessingError)
        {
            Debug.LogError($"[NightDialogueApiClient] 요청 실패: {request.error}");
            onError?.Invoke($"요청 실패: {request.error}");
            yield break;
        }

        string responseText = request.downloadHandler.text;

        try
        {
            NightDialogueResponse response = JsonConvert.DeserializeObject<NightDialogueResponse>(responseText);
            onSuccess?.Invoke(response);
        }
        catch (JsonException e)
        {
            Debug.LogError($"[NightDialogueApiClient] JSON 파싱 에러: {e.Message}");
            onError?.Invoke($"응답 파싱 실패: {e.Message}");
        }
    }
}
```

### 3.2 NightDialogueManager 통합 예시

```csharp
public class NightDialogueManager : MonoBehaviour
{
    private NightDialogueApiClient apiClient;
    
    private void Start()
    {
        // API 클라이언트 초기화
        apiClient = new NightDialogueApiClient(
            baseUrl: GameStateManager.Instance.GetBaseUrl(),
            getGameId: () => GameStateManager.Instance.GetGameId(),
            timeoutSeconds: 30f
        );
        
        // 밤의 대화 요청
        RequestNightDialogue();
    }
    
    private void RequestNightDialogue()
    {
        int currentDay = GameStateManager.Instance.GetCurrentDay();
        
        StartCoroutine(apiClient.RequestNightDialogueCoroutine(
            day: currentDay,
            onSuccess: (response) => {
                // 대화 목록 설정
                dialogues = response.dialogues;
                currentDialogueIndex = 0;
                
                // 첫 번째 대화 표시
                ShowDialogue(0);
                
                // 상태 변화 적용
                ApplyStateChanges(response);
            },
            onError: (error) => {
                Debug.LogError($"[NightDialogueManager] 밤의 대화 요청 실패: {error}");
                // 에러 시 기본 대화 사용 (fallback)
                InitializeDefaultDialogues();
            }
        ));
    }
    
    private void ApplyStateChanges(NightDialogueResponse response)
    {
        // 인간성 결과값 적용
        GameStateManager.Instance.SetHumanity(response.humanity_state_result);
        
        // NPC 호감도 결과값 적용
        if (response.npc_affection_state_results != null)
        {
            foreach (var result in response.npc_affection_state_results)
            {
                NPCType npcType = GetNPCTypeByName(result.Key);
                GameStateManager.Instance.SetAffection(npcType, result.Value);
            }
        }
        
        // NPC 인간성 결과값 적용
        if (response.npc_humanity_state_results != null)
        {
            foreach (var result in response.npc_humanity_state_results)
            {
                NPCType npcType = GetNPCTypeByName(result.Key);
                GameStateManager.Instance.SetNPCHumanity(npcType, result.Value);
            }
        }
        
        // 이벤트 플래그 적용
        if (response.event_flags != null)
        {
            foreach (var flag in response.event_flags)
            {
                GameStateManager.Instance.SetEventFlag(flag.Key, flag.Value);
            }
        }
        
        // 엔딩 트리거 확인
        if (!string.IsNullOrEmpty(response.ending_trigger))
        {
            GameStateManager.Instance.TriggerEnding(response.ending_trigger);
        }
    }
}
```

---

## 4. 백엔드 구현 고려사항

### 4.1 대화 생성 로직

- **낮 동안의 행동 반영**: 플레이어가 낮 동안 어떤 행동을 했는지에 따라 대화 내용이 달라져야 합니다.
  - 예: 특정 NPC와 많이 대화했다면, 그 NPC가 밤 대화에서 더 자주 언급됨
  - 예: 특정 아이템을 획득했다면, 그 아이템에 대한 언급이 포함됨
  - 예: 인간성이 낮아졌다면, 가족들이 더 만족스러워하는 대화


### 4.2 상태 결과값 계산

- **인간성 결과값**: 변화량을 반영한 최종 인간성 값
  - 일반적으로 기존 값에서 -10.0 (시간 경과 페널티)를 반영한 결과
  - 특정 조건에 따라 변화량이 달라질 수 있음

- **NPC 호감도/인간성 결과값**: 낮 동안의 행동에 따라 변화량을 반영한 최종 값
  - 플레이어가 특정 NPC와 많이 상호작용했다면 해당 NPC의 변화량이 더 클 수 있음

### 4.3 엔딩 트리거

- 특정 조건이 만족되면 `ending_trigger` 필드에 엔딩 ID를 반환
- 예: 5일차 종료 시, 특정 인간성 수치 달성 시 등

---

## 5. 향후 개선 사항

### 5.1 대화 분기

- 낮 동안의 행동에 따라 완전히 다른 대화 시퀀스 제공
- 예: 탈출 시도 → 경고성 대화, 순응 행동 → 칭찬 대화

### 5.3 대화 저장/재생

- 밤의 대화를 게임 내 기록으로 저장
- 플레이어가 이전 밤의 대화를 다시 볼 수 있는 기능

---

## 6. 참고 문서

- [낮 대화 API 엔드포인트 정의](./0210/API_엔드포인트_정의.md)
- [게임 상태 관리 정책](./0210/동적_상태_관리_정책.md)
- [NightDialogueManager 구현](./0208/night_scene_transition_implementation.md)

---

## 7. 체크리스트

### 백엔드
- [ ] 엔드포인트 구현 (`POST /api/v1/game/{game_id}/night_dialogue`)
- [ ] 요청 바디 검증 (`day` 필드 필수)
- [ ] 대화 생성 로직 구현 (낮 행동 반영)
- [ ] 상태 변화 계산 로직 구현
- [ ] 응답 형식 정의 및 문서화

### 프론트엔드 (Unity)
- [ ] `NightDialogueApiClient` 클래스 생성
- [ ] `NightDialogueManager`에 API 통합
- [ ] 응답 파싱 및 상태 변화 적용 로직 구현
- [ ] 에러 처리 및 fallback 로직 구현
- [ ] 기존 하드코딩된 대화 제거

### 테스트
- [ ] API 엔드포인트 테스트
- [ ] 다양한 날짜/상태에서 대화 생성 테스트
- [ ] 상태 변화 적용 테스트
- [ ] 에러 케이스 테스트 (네트워크 오류, 잘못된 응답 등)

---

## 변경 이력

- 2026-02-13: 초안 작성

