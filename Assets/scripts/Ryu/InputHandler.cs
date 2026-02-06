using UnityEngine;
using TMPro;

public class InputHandler : MonoBehaviour
{
    public TMP_InputField myInputField; 
    public TextMeshProUGUI resultText;
    public ApiClient apiClient;

    void Start()
    {
        if (myInputField != null)
        {
            myInputField.onSubmit.AddListener(SubmitText);
        }
    }

    public void SubmitText(string input) 
    {
        // 공백을 제외한 내용이 있을 때만 실행
        if (!string.IsNullOrWhiteSpace(input))
        {
            // 전송 중 표시
            if (resultText != null)
            {
                resultText.text = "전송 중...";
            }
            
            Debug.Log("입력 성공: " + input);

            // API로 메시지 전송
            if (apiClient != null)
            {
                apiClient.SendMessage(input, OnApiSuccess, OnApiError);
            }
            else
            {
                Debug.LogWarning("ApiClient가 연결되지 않았습니다.");
                if (resultText != null)
                {
                    resultText.text = "입력된 내용: " + input;
                }
            }

            if (myInputField != null)
            {
                myInputField.text = "";
                // 엔터 후 바로 다시 입력할 수 있게 포커스를 잡아줍니다.
                myInputField.ActivateInputField(); 
            }
        }
    }

    private void OnApiSuccess(string response)
    {
        if (resultText != null)
        {
            resultText.text = response;
        }
        Debug.Log("서버 응답: " + response);
    }

    private void OnApiError(string error)
    {
        if (resultText != null)
        {
            resultText.text = "오류: " + error;
        }
        Debug.LogError("API 오류: " + error);
    }
}
