using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

/// <summary>
/// 게임 스텝 API를 처리하는 클래스입니다.
/// </summary>
public class GameStepApiClient
{
    [Serializable]
    private class StepRequest
    {
        public string chat_input;
        public string npc_name;
        public string item_name;
    }

    private string baseUrl;
    private Func<int> getGameId;
    private float timeoutSeconds;
    private string mockResponse;
    private BackendResponseConverter responseConverter;

    /// <summary>
    /// GameStepApiClient 생성자
    /// </summary>
    /// <param name="baseUrl">서버 기본 URL</param>
    /// <param name="getGameId">게임 ID를 가져오는 함수</param>
    /// <param name="timeoutSeconds">타임아웃 시간 (초)</param>
    /// <param name="mockResponse">목업 응답 텍스트</param>
    public GameStepApiClient(string baseUrl, Func<int> getGameId, float timeoutSeconds, string mockResponse)
    {
        this.baseUrl = baseUrl;
        this.getGameId = getGameId;
        this.timeoutSeconds = timeoutSeconds;
        this.mockResponse = mockResponse;
        this.responseConverter = new BackendResponseConverter(mockResponse);
    }

    /// <summary>
    /// 백엔드 서버에 메시지를 전송하는 코루틴을 반환합니다.
    /// </summary>
    /// <param name="chatInput">사용자 입력 텍스트</param>
    /// <param name="npcName">NPC 이름 (선택적)</param>
    /// <param name="itemName">아이템 이름 (선택적)</param>
    /// <param name="onSuccess">성공 콜백</param>
    /// <param name="onError">에러 콜백</param>
    public IEnumerator SendMessageCoroutine(
        string chatInput,
        string npcName,
        string itemName,
        Action<string, float, NPCAffectionChanges, NPCHumanityChanges, NPCDisabledStates, ItemChanges, EventFlags, string, Dictionary<string, bool>> onSuccess,
        Action<string> onError)
    {
        int gameId = getGameId();
        string url = $"{baseUrl}/api/v1/game/{gameId}/step";

        StepRequest requestData = new StepRequest
        {
            chat_input = chatInput ?? "",
            npc_name = npcName ?? "",
            item_name = itemName ?? ""
        };

        string jsonBody = JsonUtility.ToJson(requestData);
        Debug.Log($"[GameStepApiClient] POST {url} | Body: {jsonBody}");

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("ngrok-skip-browser-warning", "true");  // ngrok 브라우저 경고 스킵
            request.timeout = Mathf.CeilToInt(timeoutSeconds);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError
                || request.result == UnityWebRequest.Result.ProtocolError
                || request.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.LogWarning($"[GameStepApiClient] 요청 실패 또는 타임아웃: {request.error}");
                Debug.Log($"[GameStepApiClient] 목업 데이터를 반환합니다.");
                // 타임아웃 시 목업 데이터 반환 (NPC 변화량은 모두 0, 무력화 상태는 null, 아이템 변화량은 빈 인스턴스, 이벤트 플래그와 엔딩 트리거는 null, locks는 null)
                onSuccess?.Invoke(mockResponse, 0f, new NPCAffectionChanges(), new NPCHumanityChanges(), null, new ItemChanges(), null, null, null);
                yield break;
            }

            string responseText = request.downloadHandler.text;
            Debug.Log($"[GameStepApiClient] ========== 백엔드 원본 응답 ==========");
            Debug.Log($"[GameStepApiClient] {responseText}");
            Debug.Log($"[GameStepApiClient] ======================================");

            try
            {
                // Json.NET을 사용하여 백엔드 응답 파싱
                BackendGameResponse backendResponse = JsonConvert.DeserializeObject<BackendGameResponse>(responseText);

                // 백엔드 응답을 콘솔에 출력 (JSON 형식으로 포맷팅)
                string formattedResponse = JsonConvert.SerializeObject(backendResponse, Formatting.Indented);
                Debug.Log($"[GameStepApiClient] ========== 백엔드 응답 (파싱됨) ==========");
                Debug.Log($"[GameStepApiClient] {formattedResponse}");
                Debug.Log($"[GameStepApiClient] ==========================================");
                
                // 백엔드 응답 객체의 각 필드를 개별적으로 로그 출력
                Debug.Log($"[GameStepApiClient] 백엔드 응답 상세 정보:");
                Debug.Log($"  - narrative: {backendResponse?.narrative ?? "null"}");
                Debug.Log($"  - ending_info: {(backendResponse?.ending_info != null ? $"ending_type={backendResponse.ending_info.ending_type}, description={backendResponse.ending_info.description}" : "null")}");

                // state_result 확인
                BackendStateDelta stateResult = backendResponse?.state_result;
                if (stateResult != null)
                {
                    Debug.Log($"  - state_result.humanity: {(stateResult.humanity.HasValue ? stateResult.humanity.Value.ToString() : "null")}");
                    Debug.Log($"  - state_result.flags: {(stateResult.flags != null ? JsonConvert.SerializeObject(stateResult.flags) : "null")}");
                    Debug.Log($"  - state_result.npc_stats: {(stateResult.npc_stats != null ? JsonConvert.SerializeObject(stateResult.npc_stats) : "null")}");
                    Debug.Log($"  - state_result.inventory_add: {(stateResult.inventory_add != null ? string.Join(", ", stateResult.inventory_add) : "null")}");
                    Debug.Log($"  - state_result.inventory_remove: {(stateResult.inventory_remove != null ? string.Join(", ", stateResult.inventory_remove) : "null")}");
                    Debug.Log($"  - state_result.locks: {(stateResult.locks != null ? JsonConvert.SerializeObject(stateResult.locks) : "null")}");
                    Debug.Log($"  - state_result.vars: {(stateResult.vars != null ? JsonConvert.SerializeObject(stateResult.vars) : "null")}");
                }
                else
                {
                    Debug.LogWarning("  - state_result가 null입니다!");
                }

                Debug.Log($"  - debug: {(backendResponse?.debug != null ? $"game_id={backendResponse.debug.game_id}, reasoning={backendResponse.debug.reasoning}" : "null")}");

                // 백엔드 응답을 현재 구조로 변환
                responseConverter.ConvertBackendResponseToCurrentFormat(
                    backendResponse,
                    GameStateManager.Instance,
                    out string response,
                    out float humanityChange,
                    out NPCAffectionChanges affectionChanges,
                    out NPCHumanityChanges humanityChanges,
                    out NPCDisabledStates disabledStates,
                    out ItemChanges itemChanges,
                    out EventFlags eventFlags,
                    out string endingTrigger,
                    out Dictionary<string, bool> locks
                );

                onSuccess?.Invoke(response, humanityChange, affectionChanges, humanityChanges, disabledStates, itemChanges, eventFlags, endingTrigger, locks);
            }
            catch (JsonException e)
            {
                Debug.LogError($"[GameStepApiClient] JSON 파싱 에러: {e.Message}");
                Debug.LogError($"[GameStepApiClient] JSON 내용: {responseText}");
                onError?.Invoke($"응답 파싱 실패: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[GameStepApiClient] 예상치 못한 에러: {e.Message}");
                onError?.Invoke($"처리 실패: {e.Message}");
            }
        }
    }
}

