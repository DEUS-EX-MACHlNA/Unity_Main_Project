using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 위치 이동 API를 처리하는 클래스입니다.
/// </summary>
public class MoveApiClient
{
    private string baseUrl;
    private Func<int> getGameId;
    private float timeoutSeconds;

    /// <summary>
    /// MoveApiClient 생성자
    /// </summary>
    /// <param name="baseUrl">서버 기본 URL</param>
    /// <param name="getGameId">게임 ID를 가져오는 함수</param>
    /// <param name="timeoutSeconds">타임아웃 시간 (초)</param>
    public MoveApiClient(string baseUrl, Func<int> getGameId, float timeoutSeconds)
    {
        this.baseUrl = baseUrl;
        this.getGameId = getGameId;
        this.timeoutSeconds = timeoutSeconds;
    }

    /// <summary>
    /// 백엔드 서버에 위치 이동 정보를 전송하는 코루틴을 반환합니다.
    /// </summary>
    /// <param name="location">이동할 위치 (GameLocation enum)</param>
    /// <param name="onSuccess">성공 콜백</param>
    /// <param name="onError">에러 콜백</param>
    public IEnumerator MoveCoroutine(
        GameLocation location,
        Action onSuccess,
        Action<string> onError)
    {
        int gameId = getGameId();
        if (gameId <= 0)
        {
            onError?.Invoke("게임 ID가 설정되지 않았습니다.");
            yield break;
        }

        // GameLocation을 백엔드 위치 이름으로 변환
        string locationName = NameMapper.ConvertLocationTypeToName(location);
        string url = $"{baseUrl}/api/v1/game/{gameId}/move?location={locationName}";

        // curl 형식 로그 출력
        Debug.Log($"[MoveApiClient] ========== 위치 이동 API 요청 ==========");
        Debug.Log($"[MoveApiClient] curl -X 'POST' \\");
        Debug.Log($"[MoveApiClient]   '{url}' \\");
        Debug.Log($"[MoveApiClient]   -H 'accept: application/json' \\");
        Debug.Log($"[MoveApiClient]   -H 'Content-Type: application/json' \\");
        Debug.Log($"[MoveApiClient]   -H 'ngrok-skip-browser-warning: true' \\");
        Debug.Log($"[MoveApiClient]   -d ''");
        Debug.Log($"[MoveApiClient] ========================================");

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("ngrok-skip-browser-warning", "true");  // ngrok 브라우저 경고 스킵
            request.timeout = Mathf.CeilToInt(timeoutSeconds);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError
                || request.result == UnityWebRequest.Result.DataProcessingError
                || request.result == UnityWebRequest.Result.ProtocolError)
            {
                string errorMessage = $"위치 이동 API 호출 실패: {request.error}";
                if (request.responseCode > 0)
                {
                    errorMessage += $" (HTTP {request.responseCode})";
                }
                Debug.LogError($"[MoveApiClient] ========== 위치 이동 API 실패 ==========");
                Debug.LogError($"[MoveApiClient] 위치: {locationName} (GameLocation: {location})");
                Debug.LogError($"[MoveApiClient] 게임 ID: {gameId}");
                Debug.LogError($"[MoveApiClient] 에러: {errorMessage}");
                if (request.responseCode > 0)
                {
                    Debug.LogError($"[MoveApiClient] HTTP 상태 코드: {request.responseCode}");
                }
                Debug.LogError($"[MoveApiClient] ========================================");
                onError?.Invoke(errorMessage);
            }
            else if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"[MoveApiClient] ========== 위치 이동 API 성공 ==========");
                Debug.Log($"[MoveApiClient] 위치: {locationName} (GameLocation: {location})");
                Debug.Log($"[MoveApiClient] 게임 ID: {gameId}");
                Debug.Log($"[MoveApiClient] HTTP 상태 코드: {request.responseCode}");
                Debug.Log($"[MoveApiClient] ========================================");
                onSuccess?.Invoke();
            }
        }
    }
}

