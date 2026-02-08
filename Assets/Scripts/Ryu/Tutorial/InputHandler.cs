using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class InputHandler : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI resultText;

    [Header("API")]
    [SerializeField] private ApiClient apiClient;

    [Header("Turn Management")]
    [SerializeField] private TurnManager turnManager;

    [Header("Game State")]
    [SerializeField] private GameStateManager gameStateManager;

    private bool needsDeselect = false;
    private int pendingCaretPos;

    private void Start()
    {
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
            AddClickListenerToResultText();
        }

        // 초기 상태: InputField 활성화, ResultText 비활성화
        ShowInputField();
    }

    /// <summary>
    /// ResultText GameObject에 EventTrigger를 추가하여 클릭 이벤트를 감지합니다.
    /// </summary>
    private void AddClickListenerToResultText()
    {
        GameObject resultObj = resultText.gameObject;

        // Raycast Target이 활성화되어 있어야 클릭 감지 가능
        resultText.raycastTarget = true;

        EventTrigger trigger = resultObj.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = resultObj.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => OnResultTextClicked());
        trigger.triggers.Add(entry);
    }

    /// <summary>
    /// ResultText 클릭 시 InputField로 전환합니다.
    /// </summary>
    private void OnResultTextClicked()
    {
        Debug.Log("[InputHandler] ResultText 클릭 → InputField로 전환");
        ShowInputField();
    }

    /// <summary>
    /// InputField를 활성화하고 ResultText를 비활성화합니다.
    /// </summary>
    private void ShowInputField()
    {
        if (inputField != null)
        {
            inputField.gameObject.SetActive(true);
            inputField.text = "";
            inputField.ActivateInputField();
        }

        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// ResultText를 활성화하고 InputField를 비활성화합니다.
    /// </summary>
    private void ShowResultText()
    {
        if (inputField != null)
        {
            inputField.gameObject.SetActive(false);
        }

        if (resultText != null)
        {
            resultText.gameObject.SetActive(true);
        }
    }

    private void OnSubmit(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        Debug.Log($"[InputHandler] 입력 전송: {text}");

        // InputField 숨기고 ResultText 표시
        ShowResultText();
        resultText.text = "전송 중...";

        // InputField 비우기
        inputField.text = "";

        // API 호출
        if (apiClient != null)
        {
            apiClient.SendMessage(text, OnApiSuccess, OnApiError);
        }
        else
        {
            Debug.LogError("[InputHandler] ApiClient가 연결되지 않았습니다.");
            resultText.text = "ApiClient가 연결되지 않았습니다.";
        }
    }

    private void OnApiSuccess(string response, float humanityChange)
    {
        Debug.Log($"[InputHandler] 응답 수신: {response}");
        Debug.Log($"[InputHandler] 인간성 변화량: {humanityChange:F1}");

        // 응답 텍스트 표시
        resultText.text = response;

        // 인간성 변화량 적용
        if (gameStateManager != null)
        {
            gameStateManager.ModifyHumanity(humanityChange);
        }
        else
        {
            Debug.LogWarning("[InputHandler] GameStateManager가 연결되지 않았습니다.");
        }

        // 턴 소모
        if (turnManager != null)
        {
            turnManager.ConsumeTurn();
        }
        else
        {
            Debug.LogWarning("[InputHandler] TurnManager가 연결되지 않았습니다.");
        }

        // ResultText 표시 상태 유지 → 클릭하면 다시 InputField로 전환
    }

    private void OnApiError(string error)
    {
        Debug.LogError($"[InputHandler] API 에러: {error}");
        resultText.text = $"에러: {error}";
        // ResultText 표시 상태 유지 → 클릭하면 다시 InputField로 전환
    }

    /// <summary>
    /// 외부에서 호출하여 InputField에 @블록이름을 삽입합니다.
    /// ResultText가 표시 중이면 InputField로 전환합니다.
    /// </summary>
    public void AddBlockToInput(string blockName)
    {
        // ResultText 표시 중이면 InputField로 전환
        if (!inputField.gameObject.activeSelf)
        {
            ShowInputField();
        }

        string blockText = $"@{blockName} ";
        int caretPos = Mathf.Clamp(inputField.caretPosition, 0, inputField.text.Length);
        inputField.text = inputField.text.Insert(caretPos, blockText);
        int newCaretPos = caretPos + blockText.Length;
        
        inputField.ActivateInputField();
        needsDeselect = true;
        pendingCaretPos = newCaretPos;

        Debug.Log($"[InputHandler] 블록 추가: {blockText.Trim()}");
    }

    private void LateUpdate()
    {
        if (needsDeselect)
        {
            needsDeselect = false;
            inputField.caretPosition = pendingCaretPos;
            inputField.selectionAnchorPosition = pendingCaretPos;
            inputField.selectionFocusPosition = pendingCaretPos;
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
