using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;
using System.Text.RegularExpressions;

[Serializable]
public class StepRequest
{
    public string chat_input;
    public string npc_name;
    public string item_name;
}

[Serializable]
public class GameResponse
{
    public string[] event_description;
}

[Serializable]
public class PlayerState
{
    public int humanity;
    public string[] flags;
}

[Serializable]
public class NpcState
{
    public string id;
    public int affection;
    public int humanity;
}

[Serializable]
public class GameContext
{
    public string location;
    public string[] recentPlayerActions;
}

[Serializable]
public class NightRequest
{
    public int day;
    public int turnSpentToday;
    public PlayerState player;
    public NpcState[] npcs;
    public GameContext context;
}

[Serializable]
public class ExposedLog
{
    public string title;
    public string[] lines;
}

[Serializable]
public class PlayerEffects
{
    public int humanityDelta;
    public int turnPenaltyNextDay;
    public string[] statusTagsAdded;
}

[Serializable]
public class NpcDelta
{
    public string id;
    public int affectionDelta;
    public int humanityDelta;
}

[Serializable]
public class NightEffects
{
    public PlayerEffects player;
    public NpcDelta[] npcDeltas;
}

[Serializable]
public class NightUI
{
    public string resultText;
}

[Serializable]
public class NightResponse
{
    public string nightId;
    public int gameId;
    public int day;
    public ExposedLog exposedLog;
    public NightEffects effects;
    public NightUI ui;
}

public class ApiClient : MonoBehaviour
{
    [Header("서버 설정")]
    [Tooltip("백엔드 서버 Base URL (예: https://xxx.ngrok-free.app)")]
    public string baseUrl = "https://7783-115-95-186-2.ngrok-free.app";
    
    [Tooltip("게임 ID")]
    public int gameId = 1;
    
    [Header("타임아웃 설정")]
    [Tooltip("API 요청 타임아웃 (초)")]
    public float requestTimeout = 3f;
    
    [Tooltip("타임아웃 시 목업 응답 사용")]
    public bool useMockOnTimeout = true;

    /// <summary>
    /// 서버로 메시지를 전송하고 응답을 콜백으로 반환합니다.
    /// </summary>
    /// <param name="message">전송할 메시지</param>
    /// <param name="onSuccess">성공 시 호출되는 콜백 (응답 JSON 문자열)</param>
    /// <param name="onError">실패 시 호출되는 콜백 (에러 메시지)</param>
    public void SendMessage(string message, Action<string> onSuccess, Action<string> onError = null)
    {
        StartCoroutine(PostRequest(message, onSuccess, onError));
    }

    private IEnumerator PostRequest(string message, Action<string> onSuccess, Action<string> onError)
    {
        // API 스펙에 맞는 요청 데이터 생성
        StepRequest requestData = new StepRequest 
        { 
            chat_input = message,
            npc_name = null,
            item_name = null
        };
        
        string jsonData = JsonUtility.ToJson(requestData);
        
        // 엔드포인트 URL 생성
        string endpointUrl = $"{baseUrl}/api/v1/game/{gameId}/step";

        using (UnityWebRequest request = new UnityWebRequest(endpointUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = Mathf.CeilToInt(requestTimeout);

            Debug.Log($"[ApiClient] 요청 전송: {endpointUrl} (타임아웃: {requestTimeout}초)");
            Debug.Log($"[ApiClient] 요청 데이터: {jsonData}");

            float startTime = Time.time;
            yield return request.SendWebRequest();
            float elapsedTime = Time.time - startTime;

            // 타임아웃 체크
            if (request.result == UnityWebRequest.Result.ConnectionError && 
                elapsedTime >= requestTimeout && useMockOnTimeout)
            {
                Debug.LogWarning($"[ApiClient] 타임아웃 발생 ({elapsedTime:F2}초) - 목업 응답 반환");
                string mockResponse = GenerateMockResponse(message);
                onSuccess?.Invoke(mockResponse);
                yield break;
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log($"[ApiClient] 응답 수신 ({elapsedTime:F2}초): {responseText}");

                try
                {
                    // event_description 추출
                    string eventDescription = ExtractEventDescription(responseText);
                    onSuccess?.Invoke(eventDescription);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ApiClient] 응답 파싱 오류: {e.Message}");
                    
                    // 파싱 실패 시에도 목업 응답 반환
                    if (useMockOnTimeout)
                    {
                        Debug.LogWarning("[ApiClient] 파싱 실패 - 목업 응답 반환");
                        string mockResponse = GenerateMockResponse(message);
                        onSuccess?.Invoke(mockResponse);
                    }
                    else
                    {
                        onError?.Invoke("응답 파싱 오류");
                    }
                }
            }
            else
            {
                string errorMsg = $"통신 오류: {request.error}";
                Debug.LogError($"[ApiClient] {errorMsg} ({elapsedTime:F2}초)");
                
                // 타임아웃이나 연결 오류 시 목업 응답 반환
                if (useMockOnTimeout && (request.result == UnityWebRequest.Result.ConnectionError || 
                    request.result == UnityWebRequest.Result.ProtocolError))
                {
                    Debug.LogWarning("[ApiClient] 통신 오류 - 목업 응답 반환");
                    string mockResponse = GenerateMockResponse(message);
                    onSuccess?.Invoke(mockResponse);
                }
                else
                {
                    onError?.Invoke(errorMsg);
                }
            }
        }
    }

    /// <summary>
    /// 타임아웃 또는 오류 발생 시 사용할 목업 응답을 생성합니다.
    /// </summary>
    private string GenerateMockResponse(string userInput)
    {
        // 입력에 따라 다양한 응답 생성
        string[] mockResponses;
        
        // 입력 키워드 기반 응답 선택
        if (userInput.Contains("엘리노어") || userInput.Contains("StepMother") || userInput.Contains("새엄마"))
        {
            mockResponses = new string[]
            {
                "엘리노어가 당신을 빤히 쳐다본다. 그녀의 눈빛은 차갑지만, 무언가를 숨기고 있는 것 같다.",
                "새엄마는 당신의 말을 듣고 있지만, 아무 대답도 하지 않는다.",
                "엘리노어가 미소를 짓는다. 그 미소는 따뜻하지 않다.",
                "새엄마가 조용히 고개를 끄덕인다. '그래, 알겠어.'"
            };
        }
        else if (userInput.Contains("케이크") || userInput.Contains("Cake") || userInput.Contains("먹"))
        {
            mockResponses = new string[]
            {
                "케이크는 맛있어 보인다. 하지만 왠지 손이 가지 않는다.",
                "달콤한 냄새가 코를 찌른다. 누가 만든 걸까?",
                "케이크 위에 작은 메모가 붙어있다. '먹어도 좋아'",
                "케이크를 바라보니 배가 고프다. 하지만 지금 먹어도 될까?"
            };
        }
        else if (userInput.Contains("방") || userInput.Contains("집") || userInput.Contains("둘러"))
        {
            mockResponses = new string[]
            {
                "방 안을 둘러본다. 모든 것이 평범해 보이지만, 무언가 이상하다.",
                "집은 조용하다. 너무 조용하다.",
                "창문 밖으로 보이는 풍경이 낯설다.",
                "가구들이 제자리에 있지만, 위치가 살짝 바뀐 것 같다."
            };
        }
        else if (userInput.Contains("누구") || userInput.Contains("이름") || userInput.Contains("나"))
        {
            mockResponses = new string[]
            {
                "당신은 누구인가? 그 질문에 대답하기가 어렵다.",
                "거울을 보면 낯선 얼굴이 보인다. 이게 정말 나일까?",
                "기억이 흐릿하다. 어제 무엇을 했는지 기억나지 않는다.",
                "당신의 이름은... 생각이 나지 않는다."
            };
        }
        else
        {
            // 일반적인 응답
            mockResponses = new string[]
            {
                "당신의 행동에는 아무 일도 일어나지 않는다.",
                "시간이 천천히 흘러간다.",
                "조용한 침묵이 방을 가득 채운다.",
                "당신은 잠시 생각에 잠긴다.",
                "무언가 중요한 것을 놓치고 있는 것 같다.",
                "멀리서 시계 소리가 들린다. 똑딱, 똑딱.",
                "[서버 응답 대기 중... 임시 응답입니다]"
            };
        }
        
        // 랜덤하게 응답 선택
        int randomIndex = UnityEngine.Random.Range(0, mockResponses.Length);
        string selectedResponse = mockResponses[randomIndex];
        
        Debug.Log($"[ApiClient] 목업 응답 생성: {selectedResponse}");
        return selectedResponse;
    }

    /// <summary>
    /// JSON 응답에서 event_description 배열을 추출하여 문자열로 반환합니다.
    /// </summary>
    private string ExtractEventDescription(string jsonResponse)
    {
        try
        {
            // JsonUtility로 파싱 시도
            GameResponse response = JsonUtility.FromJson<GameResponse>(jsonResponse);
            
            if (response.event_description != null && response.event_description.Length > 0)
            {
                // 배열을 줄바꿈으로 연결
                return string.Join("\n", response.event_description);
            }
        }
        catch
        {
            // JsonUtility 파싱 실패 시 정규식으로 추출
            Debug.LogWarning("[ApiClient] JsonUtility 파싱 실패, 정규식으로 시도");
        }

        // 정규식으로 event_description 배열 추출
        // "event_description":["문자열1","문자열2"] 형식 매칭
        Match match = Regex.Match(jsonResponse, @"""event_description"":\s*\[(.*?)\]");
        
        if (match.Success)
        {
            string arrayContent = match.Groups[1].Value;
            
            // 배열 내 문자열 추출 (따옴표로 감싸진 내용)
            MatchCollection stringMatches = Regex.Matches(arrayContent, @"""([^""]*)""");
            
            if (stringMatches.Count > 0)
            {
                System.Collections.Generic.List<string> descriptions = new System.Collections.Generic.List<string>();
                foreach (Match m in stringMatches)
                {
                    descriptions.Add(m.Groups[1].Value);
                }
                return string.Join("\n", descriptions);
            }
        }

        // 추출 실패 시 원본 반환
        Debug.LogWarning("[ApiClient] event_description 추출 실패, 원본 반환");
        return jsonResponse;
    }

    /// <summary>
    /// 밤의 대화를 요청하고 응답을 콜백으로 반환합니다.
    /// </summary>
    /// <param name="requestData">밤의 대화 요청 데이터</param>
    /// <param name="onSuccess">성공 시 호출되는 콜백 (NightResponse 객체)</param>
    /// <param name="onError">실패 시 호출되는 콜백 (에러 메시지)</param>
    public void RequestNightConversation(NightRequest requestData, Action<NightResponse> onSuccess, Action<string> onError = null)
    {
        StartCoroutine(NightConversationRequest(requestData, onSuccess, onError));
    }

    private IEnumerator NightConversationRequest(NightRequest requestData, Action<NightResponse> onSuccess, Action<string> onError)
    {
        string jsonData = JsonUtility.ToJson(requestData);
        
        // 엔드포인트 URL 생성
        string endpointUrl = $"{baseUrl}/api/v1/games/{gameId}/nights";

        using (UnityWebRequest request = new UnityWebRequest(endpointUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = Mathf.CeilToInt(requestTimeout);

            Debug.Log($"[ApiClient] 밤의 대화 요청 전송: {endpointUrl} (타임아웃: {requestTimeout}초)");
            Debug.Log($"[ApiClient] 요청 데이터: {jsonData}");

            float startTime = Time.time;
            yield return request.SendWebRequest();
            float elapsedTime = Time.time - startTime;

            // 타임아웃 체크
            if (request.result == UnityWebRequest.Result.ConnectionError && 
                elapsedTime >= requestTimeout && useMockOnTimeout)
            {
                Debug.LogWarning($"[ApiClient] 밤의 대화 타임아웃 발생 ({elapsedTime:F2}초) - 목업 응답 반환");
                NightResponse mockResponse = GenerateMockNightResponse(requestData.day);
                onSuccess?.Invoke(mockResponse);
                yield break;
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log($"[ApiClient] 밤의 대화 응답 수신 ({elapsedTime:F2}초): {responseText}");

                try
                {
                    NightResponse response = JsonUtility.FromJson<NightResponse>(responseText);
                    onSuccess?.Invoke(response);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ApiClient] 밤의 대화 응답 파싱 오류: {e.Message}");
                    
                    // 파싱 실패 시에도 목업 응답 반환
                    if (useMockOnTimeout)
                    {
                        Debug.LogWarning("[ApiClient] 밤의 대화 파싱 실패 - 목업 응답 반환");
                        NightResponse mockResponse = GenerateMockNightResponse(requestData.day);
                        onSuccess?.Invoke(mockResponse);
                    }
                    else
                    {
                        onError?.Invoke("응답 파싱 오류");
                    }
                }
            }
            else
            {
                string errorMsg = $"통신 오류: {request.error}";
                Debug.LogError($"[ApiClient] 밤의 대화 {errorMsg} ({elapsedTime:F2}초)");
                
                // 타임아웃이나 연결 오류 시 목업 응답 반환
                if (useMockOnTimeout && (request.result == UnityWebRequest.Result.ConnectionError || 
                    request.result == UnityWebRequest.Result.ProtocolError))
                {
                    Debug.LogWarning("[ApiClient] 밤의 대화 통신 오류 - 목업 응답 반환");
                    NightResponse mockResponse = GenerateMockNightResponse(requestData.day);
                    onSuccess?.Invoke(mockResponse);
                }
                else
                {
                    onError?.Invoke(errorMsg);
                }
            }
        }
    }

    /// <summary>
    /// 밤의 대화 목업 응답을 생성합니다.
    /// </summary>
    private NightResponse GenerateMockNightResponse(int day)
    {
        string[] nightLogs = new string[]
        {
            "밤이 깊어간다.",
            "창밖에서 이상한 소리가 들린다.",
            "누군가가 복도를 걷는 소리가 들린다.",
            "시계가 자정을 알린다.",
            "어둠 속에서 무언가가 움직인다.",
            "[서버 응답 대기 중... 임시 밤의 대화입니다]"
        };

        string selectedLog = nightLogs[UnityEngine.Random.Range(0, nightLogs.Length)];

        NightResponse mockResponse = new NightResponse
        {
            nightId = $"mock_night_{day}_{System.DateTime.Now.Ticks}",
            gameId = gameId,
            day = day,
            exposedLog = new ExposedLog
            {
                title = $"Day {day}의 밤",
                lines = new string[] { selectedLog }
            },
            effects = new NightEffects
            {
                player = new PlayerEffects
                {
                    humanityDelta = 0,
                    turnPenaltyNextDay = 0,
                    statusTagsAdded = new string[0]
                },
                npcDeltas = new NpcDelta[0]
            },
            ui = new NightUI
            {
                resultText = $"Day {day}의 밤\n\n{selectedLog}\n\n(목업 응답)"
            }
        };

        Debug.Log($"[ApiClient] 밤의 대화 목업 응답 생성: {selectedLog}");
        return mockResponse;
    }
}

