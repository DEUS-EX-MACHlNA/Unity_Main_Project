using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ApiClient : MonoBehaviour
{
    [Header("Server Settings")]
    [SerializeField] private string baseUrl = "https://7783-115-95-186-2.ngrok-free.app";
    [SerializeField] private int gameId = 1;

    [Header("Timeout Settings")]
    [SerializeField] private float timeoutSeconds = 3f;

    private const string MOCK_RESPONSE = "서버 응답을 기다리는 중... 기본 응답입니다.";

    [Serializable]
    private class StepRequest
    {
        public string chat_input;
        public string npc_name;
        public string item_name;
    }

    [Serializable]
    private class GameResponse
    {
        public string response;
        public float humanity_change; // 백엔드에서 제공하는 인간성 변화량
    }

    /// <summary>
    /// 백엔드 서버에 메시지를 전송하고 응답을 콜백으로 반환합니다.
    /// 3초 타임아웃 시 목업 데이터를 반환합니다.
    /// </summary>
    /// <param name="chatInput">사용자 입력 텍스트</param>
    /// <param name="onSuccess">성공 콜백 (response, humanityChange)</param>
    /// <param name="onError">에러 콜백</param>
    public Coroutine SendMessage(string chatInput, Action<string, float> onSuccess, Action<string> onError)
    {
        return StartCoroutine(SendMessageCoroutine(chatInput, onSuccess, onError));
    }

    private IEnumerator SendMessageCoroutine(string chatInput, Action<string, float> onSuccess, Action<string> onError)
    {
        string url = $"{baseUrl}/api/v1/game/{gameId}/step";

        StepRequest requestData = new StepRequest
        {
            chat_input = chatInput,
            npc_name = null,
            item_name = null
        };

        string jsonBody = JsonUtility.ToJson(requestData);
        Debug.Log($"[ApiClient] POST {url} | Body: {jsonBody}");

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = Mathf.CeilToInt(timeoutSeconds);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError
                || request.result == UnityWebRequest.Result.ProtocolError
                || request.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.LogWarning($"[ApiClient] 요청 실패 또는 타임아웃: {request.error}");
                Debug.Log($"[ApiClient] 목업 데이터를 반환합니다.");
                onSuccess?.Invoke(MOCK_RESPONSE, 0f);
                yield break;
            }

            string responseText = request.downloadHandler.text;
            Debug.Log($"[ApiClient] 응답 수신: {responseText}");

            try
            {
                GameResponse gameResponse = JsonUtility.FromJson<GameResponse>(responseText);
                
                string response = !string.IsNullOrEmpty(gameResponse.response) 
                    ? gameResponse.response 
                    : MOCK_RESPONSE;
                
                // humanity_change는 선택적 필드 (없으면 0으로 처리)
                float humanityChange = gameResponse.humanity_change;
                
                onSuccess?.Invoke(response, humanityChange);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ApiClient] JSON 파싱 에러: {e.Message}");
                onError?.Invoke($"응답 파싱 실패: {e.Message}");
            }
        }
    }
}
