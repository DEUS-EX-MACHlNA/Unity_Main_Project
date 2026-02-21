using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

/// <summary>
/// 밤의 대화 API를 처리하는 클래스입니다.
/// BackendGameResponse 기반 구조를 사용하며, dialogues 배열을 추가로 포함합니다.
/// </summary>
public class NightDialogueApiClient
{
    /// <summary>
    /// 밤의 대화 응답 구조 (BackendGameResponse 확장)
    /// </summary>
    [Serializable]
    public class BackendNightDialogueResponse : BackendGameResponse
    {
        [JsonProperty("dialogues")]
        public BackendDialogueLine[] dialogues;
    }

    /// <summary>
    /// 대화 라인 구조
    /// </summary>
    [Serializable]
    public class BackendDialogueLine
    {
        [JsonProperty("speaker_name")]
        public string speaker_name;
        
        [JsonProperty("dialogue")]
        public string dialogue;
    }

    private string baseUrl;
    private Func<int> getGameId;
    private float timeoutSeconds;
    private string mockResponse;
    private BackendResponseConverter responseConverter;

    /// <summary>
    /// NightDialogueApiClient 생성자
    /// </summary>
    /// <param name="baseUrl">서버 기본 URL</param>
    /// <param name="getGameId">게임 ID를 가져오는 함수</param>
    /// <param name="timeoutSeconds">타임아웃 시간 (초)</param>
    /// <param name="mockResponse">목업 응답 텍스트</param>
    public NightDialogueApiClient(
        string baseUrl, 
        Func<int> getGameId, 
        float timeoutSeconds, 
        string mockResponse)
    {
        this.baseUrl = baseUrl;
        this.getGameId = getGameId;
        this.timeoutSeconds = timeoutSeconds;
        this.mockResponse = mockResponse;
        this.responseConverter = new BackendResponseConverter(mockResponse);
    }

    /// <summary>
    /// 밤의 대화를 요청하는 코루틴을 반환합니다.
    /// </summary>
    /// <param name="onSuccess">성공 콜백 (dialogues, response, humanityChange, affectionChanges, humanityChanges, disabledStates, itemChanges, eventFlags, endingTrigger, locks)</param>
    /// <param name="onError">에러 콜백</param>
    public IEnumerator RequestNightDialogueCoroutine(
        Action<BackendDialogueLine[], string, float, NPCAffectionChanges, NPCHumanityChanges, NPCDisabledStates, ItemChanges, EventFlags, string, Dictionary<string, bool>> onSuccess,
        Action<string> onError)
    {
        int gameId = getGameId();
        string url = $"{baseUrl}/api/v1/game/{gameId}/night_dialogue";

        Debug.Log($"[NightDialogueApiClient] POST {url}");

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("ngrok-skip-browser-warning", "true");
            request.timeout = Mathf.CeilToInt(timeoutSeconds);

            yield return request.SendWebRequest();

            // 에러 처리
            if (request.result == UnityWebRequest.Result.ConnectionError
                || request.result == UnityWebRequest.Result.ProtocolError
                || request.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.LogWarning($"[NightDialogueApiClient] 요청 실패: {request.error}");
                
                // 타임아웃인 경우 특별 처리
                if (request.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogWarning("[NightDialogueApiClient] 네트워크 연결 오류. Fallback 모드로 전환합니다.");
                }
                
                onError?.Invoke($"요청 실패: {request.error}");
                yield break;
            }

            string responseText = request.downloadHandler.text;

            try
            {
                // Json.NET을 사용하여 응답 파싱 (BackendGameResponse 기반)
                BackendNightDialogueResponse response = JsonConvert.DeserializeObject<BackendNightDialogueResponse>(responseText);
                
                // 응답 로깅
                string formattedResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
                Debug.Log($"[NightDialogueApiClient] ========== 밤의 대화 응답 ==========");
                Debug.Log($"[NightDialogueApiClient] {formattedResponse}");
                Debug.Log($"[NightDialogueApiClient] ==================================");
                
                // BackendResponseConverter를 사용하여 state_result 변환
                responseConverter.ConvertBackendResponseToCurrentFormat(
                    response,  // BackendGameResponse로 캐스팅 가능
                    GameStateManager.Instance,
                    out string narrative,
                    out float humanityChange,
                    out NPCAffectionChanges affectionChanges,
                    out NPCHumanityChanges humanityChanges,
                    out NPCDisabledStates disabledStates,
                    out ItemChanges itemChanges,
                    out EventFlags eventFlags,
                    out string endingTrigger,
                    out Dictionary<string, bool> locks
                );
                
                // dialogues 배열과 변환된 데이터를 콜백에 전달
                onSuccess?.Invoke(
                    response.dialogues ?? new BackendDialogueLine[0],
                    narrative,
                    humanityChange,
                    affectionChanges,
                    humanityChanges,
                    disabledStates,
                    itemChanges,
                    eventFlags,
                    endingTrigger,
                    locks
                );
            }
            catch (JsonException e)
            {
                Debug.LogError($"[NightDialogueApiClient] JSON 파싱 에러: {e.Message}");
                Debug.LogError($"[NightDialogueApiClient] JSON 내용: {responseText}");
                onError?.Invoke($"응답 파싱 실패: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[NightDialogueApiClient] 예상치 못한 에러: {e.Message}");
                onError?.Invoke($"처리 실패: {e.Message}");
            }
        }
    }
}

