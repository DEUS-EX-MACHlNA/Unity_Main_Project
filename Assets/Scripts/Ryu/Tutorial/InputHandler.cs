using UnityEngine;
using TMPro;

/// <summary>
/// 입력 처리를 조율하는 메인 코디네이터 클래스입니다.
/// 각 모듈을 연결하여 입력 처리 흐름을 관리합니다.
/// </summary>
public class InputHandler : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI resultText;

    [Header("API")]
    [SerializeField] private ApiClient apiClient;

    [Header("Turn Management")]
    [SerializeField] private RoomTurnManager turnManager;

    [Header("Game State")]
    [SerializeField] private GameStateManager gameStateManager;

    // 모듈 인스턴스
    private InputFieldManager inputFieldManager;
    private BlockInserter blockInserter;
    private ApiResponseHandler apiResponseHandler;

    private void Start()
    {
        // 모듈 초기화
        if (inputField != null && resultText != null)
        {
            inputFieldManager = new InputFieldManager(inputField, resultText);
            blockInserter = new BlockInserter(inputField);
        }
        else
        {
            Debug.LogError("[InputHandler] InputField 또는 ResultText가 연결되지 않았습니다.");
        }

        // ApiResponseHandler 초기화
        if (gameStateManager != null && turnManager != null && resultText != null)
        {
            apiResponseHandler = new ApiResponseHandler(gameStateManager, turnManager, resultText);
        }

        // InputField 이벤트 리스너 등록
        if (inputField != null)
        {
            inputField.onSubmit.AddListener(OnSubmit);
        }
        else
        {
            Debug.LogError("[InputHandler] InputField이 연결되지 않았습니다.");
        }

        // ResultText에 클릭 이벤트 추가
        if (resultText != null)
        {
            resultText.text = "";
            if (inputFieldManager != null)
            {
                inputFieldManager.AddClickListenerToResultText();
            }
        }

        // 초기 상태: InputField 활성화, ResultText 비활성화
        if (inputFieldManager != null)
        {
            inputFieldManager.ShowInputField();
        }
    }


    private void OnSubmit(string text)
    {
        // 입력 검증
        if (!InputValidator.ValidateInput(text))
            return;

        Debug.Log($"[InputHandler] 입력 전송: {text}");

        // InputField 숨기고 ResultText 표시
        if (inputFieldManager != null)
        {
            inputFieldManager.ShowResultText();
            inputFieldManager.SetResultText("전송 중...");
            inputFieldManager.ClearInputField();
        }

        // API 호출
        if (apiClient != null && apiResponseHandler != null)
        {
            apiClient.SendMessage(text, apiResponseHandler.OnApiSuccess, apiResponseHandler.OnApiError);
        }
        else
        {
            Debug.LogError("[InputHandler] ApiClient 또는 ApiResponseHandler가 연결되지 않았습니다.");
            if (inputFieldManager != null)
            {
                inputFieldManager.SetResultText("ApiClient가 연결되지 않았습니다.");
            }
        }
    }


    /// <summary>
    /// 외부에서 호출하여 InputField에 @블록이름을 삽입합니다.
    /// ResultText가 표시 중이면 InputField로 전환합니다.
    /// </summary>
    public void AddBlockToInput(string blockName)
    {
        if (blockInserter != null && inputFieldManager != null)
        {
            blockInserter.AddBlockToInput(blockName, inputFieldManager.ShowInputField);
        }
        else
        {
            Debug.LogWarning("[InputHandler] BlockInserter 또는 InputFieldManager가 초기화되지 않았습니다.");
        }
    }

    private void LateUpdate()
    {
        if (blockInserter != null)
        {
            blockInserter.UpdateCaretPosition();
        }
    }

    private void OnDestroy()
    {
        if (inputField != null)
        {
            inputField.onSubmit.RemoveListener(OnSubmit);
        }
    }
}
