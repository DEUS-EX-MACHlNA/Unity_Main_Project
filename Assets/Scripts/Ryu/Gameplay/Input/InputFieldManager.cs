using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class InputFieldManager
{
    private TMP_InputField inputField;
    private TextMeshProUGUI resultText;
    private GameStateManager gameStateManager;

    private string[] paragraphs;
    private int currentParagraphIndex = 0;

    public InputFieldManager(TMP_InputField inputField, TextMeshProUGUI resultText, GameStateManager gameStateManager = null)
    {
        this.inputField = inputField;
        this.resultText = resultText;
        this.gameStateManager = gameStateManager;
    }

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

    public void AddClickListenerToResultText()
    {
        if (resultText == null)
            return;

        GameObject resultObj = resultText.gameObject;
        resultText.raycastTarget = true;

        EventTrigger trigger = resultObj.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = resultObj.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => OnResultTextClicked());
        trigger.triggers.Add(entry);
    }

    private GameStateManager EffectiveGameStateManager =>
        GameStateManager.Instance != null ? GameStateManager.Instance : gameStateManager;

    private void OnResultTextClicked()
    {
        GameStateManager gsm = EffectiveGameStateManager;
        if (gsm != null && gsm.HasPendingEndingTransition())
        {
            Debug.Log("[InputFieldManager] ResultText 클릭 → 엔딩 씬으로 전환");
            gsm.LoadEndingScene(fromUserClick: true);
            return;
        }

        // 다음 문단이 있으면 보여주기
        if (paragraphs != null && currentParagraphIndex < paragraphs.Length - 1)
        {
            currentParagraphIndex++;
            resultText.text = paragraphs[currentParagraphIndex];
            return;
        }

        // 모든 문단 다 봤으면 InputField로 전환
        Debug.Log("[InputFieldManager] ResultText 클릭 → InputField로 전환");
        ShowInputField();
    }

    public void SetResultText(string text)
    {
        if (resultText != null)
        {
            paragraphs = text.Split(new string[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
            currentParagraphIndex = 0;
            Debug.Log($"[InputFieldManager] 문단 수: {paragraphs.Length}"); // 이거 추가!
            resultText.text = paragraphs[0];
        }
    }

    public void ClearInputField()
    {
        if (inputField != null)
        {
            inputField.text = "";
        }
    }

    public bool IsInputFieldActive()
    {
        return inputField != null && inputField.gameObject.activeSelf;
    }
}