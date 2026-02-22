using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// InputField와 ResultText UI 관리를 담당하는 클래스입니다.
/// UI 표시/숨김 및 클릭 이벤트를 처리합니다.
/// </summary>
public class InputFieldManager
{
    private TMP_InputField inputField;
    private TextMeshProUGUI resultText;
    private GameStateManager gameStateManager;

    /// <summary>
    /// InputFieldManager 생성자
    /// </summary>
    /// <param name="inputField">InputField 컴포넌트</param>
    /// <param name="resultText">ResultText 컴포넌트</param>
    /// <param name="gameStateManager">엔딩 대기 시 클릭으로 씬 전환을 위해 사용 (선택)</param>
    public InputFieldManager(TMP_InputField inputField, TextMeshProUGUI resultText, GameStateManager gameStateManager = null)
    {
        this.inputField = inputField;
        this.resultText = resultText;
        this.gameStateManager = gameStateManager;
    }

    /// <summary>
    /// InputField를 활성화하고 ResultText를 비활성화합니다.
    /// </summary>
    public void ShowInputField()
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
    public void ShowResultText()
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

    /// <summary>
    /// ResultText에 클릭 이벤트 리스너를 추가합니다.
    /// </summary>
    public void AddClickListenerToResultText()
    {
        if (resultText == null)
            return;

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
    /// 엔딩/씬 전환에 사용할 GameStateManager. 씬 전환 후에도 동작하도록 Instance를 우선 사용합니다.
    /// </summary>
    private GameStateManager EffectiveGameStateManager =>
        GameStateManager.Instance != null ? GameStateManager.Instance : gameStateManager;

    /// <summary>
    /// ResultText 클릭 시: 엔딩 대기 중이면 엔딩 씬으로 전환, 아니면 InputField로 전환합니다.
    /// </summary>
    private void OnResultTextClicked()
    {
        GameStateManager gsm = EffectiveGameStateManager;
        if (gsm != null && gsm.HasPendingEndingTransition())
        {
            Debug.Log("[InputFieldManager] ResultText 클릭 → 엔딩 씬으로 전환");
            gsm.LoadEndingScene(fromUserClick: true);
            return;
        }
        Debug.Log("[InputFieldManager] ResultText 클릭 → InputField로 전환");
        ShowInputField();
    }

    /// <summary>
    /// ResultText의 텍스트를 설정합니다.
    /// </summary>
    /// <param name="text">설정할 텍스트</param>
    public void SetResultText(string text)
    {
        if (resultText != null)
        {
            resultText.text = text;
        }
    }

    /// <summary>
    /// InputField의 텍스트를 비웁니다.
    /// </summary>
    public void ClearInputField()
    {
        if (inputField != null)
        {
            inputField.text = "";
        }
    }

    /// <summary>
    /// InputField가 활성화되어 있는지 확인합니다.
    /// </summary>
    /// <returns>활성화되어 있으면 true</returns>
    public bool IsInputFieldActive()
    {
        return inputField != null && inputField.gameObject.activeSelf;
    }
}

