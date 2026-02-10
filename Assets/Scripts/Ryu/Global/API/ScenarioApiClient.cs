using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

/// <summary>
/// 시나리오 시작 API를 처리하는 클래스입니다.
/// </summary>
public class ScenarioApiClient
{
    private string baseUrl;
    private float timeoutSeconds;
    private Action<int> onGameIdUpdated;

    /// <summary>
    /// ScenarioApiClient 생성자
    /// </summary>
    /// <param name="baseUrl">서버 기본 URL</param>
    /// <param name="timeoutSeconds">타임아웃 시간 (초)</param>
    /// <param name="onGameIdUpdated">게임 ID 업데이트 콜백</param>
    public ScenarioApiClient(string baseUrl, float timeoutSeconds, Action<int> onGameIdUpdated)
    {
        this.baseUrl = baseUrl;
        this.timeoutSeconds = timeoutSeconds;
        this.onGameIdUpdated = onGameIdUpdated;
    }

    /// <summary>
    /// 시나리오를 시작하는 코루틴을 반환합니다.
    /// </summary>
    /// <param name="scenarioId">시나리오 ID</param>
    /// <param name="userId">사용자 ID</param>
    /// <param name="onSuccess">성공 콜백 (gameId)</param>
    /// <param name="onError">에러 콜백</param>
    public IEnumerator StartScenarioCoroutine(int scenarioId, int userId, Action<int> onSuccess, Action<string> onError)
    {
        string url = $"{baseUrl}/api/v1/scenario/start/{scenarioId}?user_id={userId}";
        Debug.Log($"[ScenarioApiClient] GET {url}");

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("ngrok-skip-browser-warning", "true");  // ngrok 브라우저 경고 스킵
            request.timeout = Mathf.CeilToInt(timeoutSeconds);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError
                || request.result == UnityWebRequest.Result.ProtocolError
                || request.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.LogWarning($"[ScenarioApiClient] 시나리오 시작 실패: {request.error}");
                onError?.Invoke($"시나리오 시작 실패: {request.error}");
                yield break;
            }

            string responseText = request.downloadHandler.text;
            Debug.Log($"[ScenarioApiClient] ========== 시나리오 시작 원본 응답 ==========");
            Debug.Log($"[ScenarioApiClient] {responseText}");
            Debug.Log($"[ScenarioApiClient] ============================================");

            try
            {
                // Json.NET을 사용하여 응답 파싱
                ScenarioStartResponse response = JsonConvert.DeserializeObject<ScenarioStartResponse>(responseText);

                // 파싱된 응답을 콘솔에 출력 (JSON 형식으로 포맷팅)
                string formattedResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
                Debug.Log($"[ScenarioApiClient] ========== 시나리오 시작 응답 (파싱됨) ==========");
                Debug.Log($"[ScenarioApiClient] {formattedResponse}");
                Debug.Log($"[ScenarioApiClient] ================================================");

                if (response != null && response.game_id > 0)
                {
                    // 게임 ID 업데이트 콜백 호출
                    onGameIdUpdated?.Invoke(response.game_id);
                    onSuccess?.Invoke(response.game_id);
                }
                else
                {
                    Debug.LogError($"[ScenarioApiClient] 잘못된 응답 형식: {responseText}");
                    onError?.Invoke("잘못된 응답 형식");
                }
            }
            catch (JsonException e)
            {
                Debug.LogError($"[ScenarioApiClient] JSON 파싱 에러: {e.Message}");
                Debug.LogError($"[ScenarioApiClient] JSON 내용: {responseText}");
                onError?.Invoke($"응답 파싱 실패: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ScenarioApiClient] 예상치 못한 에러: {e.Message}");
                onError?.Invoke($"처리 실패: {e.Message}");
            }
        }
    }
}

