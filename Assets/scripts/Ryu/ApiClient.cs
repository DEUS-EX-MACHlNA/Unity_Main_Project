using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;

[Serializable]
public class StepRequest
{
    public string chat_input;
    public string npc_name;
    public string item_name;
}

public class ApiClient : MonoBehaviour
{
    [Header("서버 설정")]
    [Tooltip("백엔드 서버 Base URL (예: https://xxx.ngrok-free.app)")]
    public string baseUrl = "https://7783-115-95-186-2.ngrok-free.app";
    
    [Tooltip("게임 ID")]
    public int gameId = 1;

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

            Debug.Log($"[ApiClient] 요청 전송: {endpointUrl}");
            Debug.Log($"[ApiClient] 요청 데이터: {jsonData}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log($"[ApiClient] 응답 수신: {responseText}");

                // 응답을 그대로 전달 (실제 응답 형식에 맞게 파싱은 필요시 추가)
                onSuccess?.Invoke(responseText);
            }
            else
            {
                string errorMsg = $"통신 오류: {request.error}";
                Debug.LogError($"[ApiClient] {errorMsg}");
                onError?.Invoke(errorMsg);
            }
        }
    }
}
