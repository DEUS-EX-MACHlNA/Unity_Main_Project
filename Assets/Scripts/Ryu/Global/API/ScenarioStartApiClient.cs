using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

/// <summary>
/// 시나리오 시작 API를 처리하는 클래스입니다.
/// </summary>
public class ScenarioStartApiClient
{
    private string baseUrl;
    private float timeoutSeconds;

    /// <summary>
    /// ScenarioStartApiClient 생성자
    /// </summary>
    /// <param name="baseUrl">서버 기본 URL</param>
    /// <param name="timeoutSeconds">타임아웃 시간 (초)</param>
    public ScenarioStartApiClient(string baseUrl, float timeoutSeconds)
    {
        this.baseUrl = baseUrl;
        this.timeoutSeconds = timeoutSeconds;
    }

    /// <summary>
    /// 백엔드 서버에 시나리오 시작 요청을 전송하는 코루틴을 반환합니다.
    /// </summary>
    /// <param name="scenarioId">시나리오 ID</param>
    /// <param name="userId">사용자 ID</param>
    /// <param name="onSuccess">성공 콜백</param>
    /// <param name="onError">에러 콜백</param>
    public IEnumerator StartScenarioCoroutine(
        int scenarioId,
        int userId,
        Action<ScenarioStartResponse> onSuccess,
        Action<string> onError)
    {
        string url = $"{baseUrl}/api/v1/scenario/start/{scenarioId}?user_id={userId}";
        Debug.Log($"[ScenarioStartApiClient] GET {url}");

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("accept", "application/json");
            request.SetRequestHeader("ngrok-skip-browser-warning", "true");  // ngrok 브라우저 경고 스킵
            request.timeout = Mathf.CeilToInt(timeoutSeconds);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError
                || request.result == UnityWebRequest.Result.ProtocolError
                || request.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.LogError($"[ScenarioStartApiClient] 요청 실패: {request.error}");
                onError?.Invoke($"시나리오 시작 실패: {request.error}");
                yield break;
            }

            string responseText = request.downloadHandler.text;

            try
            {
                // Json.NET을 사용하여 백엔드 응답 파싱
                ScenarioStartResponse response = JsonConvert.DeserializeObject<ScenarioStartResponse>(responseText);

                Debug.Log($"[ScenarioStartApiClient] ========== 시나리오 시작 응답 ==========");
                Debug.Log($"[ScenarioStartApiClient] game_id: {response.game_id}, user_id: {response.user_id}");
                Debug.Log($"[ScenarioStartApiClient] ======================================");

                onSuccess?.Invoke(response);
            }
            catch (JsonException e)
            {
                Debug.LogError($"[ScenarioStartApiClient] JSON 파싱 에러: {e.Message}");
                Debug.LogError($"[ScenarioStartApiClient] JSON 내용: {responseText}");
                onError?.Invoke($"응답 파싱 실패: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ScenarioStartApiClient] 예상치 못한 에러: {e.Message}");
                onError?.Invoke($"처리 실패: {e.Message}");
            }
        }
    }
}

