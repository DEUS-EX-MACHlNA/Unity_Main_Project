using UnityEngine;
using TMPro;

public class InputHandler : MonoBehaviour
{
    // 사용자가 글자를 입력하는 곳
    public TMP_InputField myInputField; 
    
    // 입력한 결과가 화면에 나타날 곳
    public TextMeshProUGUI resultText;  

    public void OnSubmitInput(string text)
    {
        // 입력창이 비어있지 않을 때만 실행
        if (!string.IsNullOrEmpty(text))
        {
            // 화면에 있는 텍스트 오브젝트의 내용을 변경
            resultText.text = "입력된 내용: " + text;
            
            // 콘솔창 확인용 (제대로 작동하는지 체크)
            Debug.Log("데이터 전송 완료: " + text);

            // 입력이 끝난 후 인풋창을 깨끗하게 비움
            myInputField.text = "";
        }
    }
}