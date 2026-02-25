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
    
    // 타이핑 효과 상태
    public bool IsTyping { get; private set; } = false;
    private string currentFullText = ""; // 현재 문단 전체 텍스트 (스킵용)

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
        // 타이핑 중이면 즉시 전체 텍스트 표시 (스킵)
        if (IsTyping)
        {
            SkipTyping();
            return;
        }

        GameStateManager gsm = EffectiveGameStateManager;
        if (gsm != null && gsm.HasPendingEndingTransition())
        {
            Debug.Log("[InputFieldManager] ResultText 클릭 → 엔딩 씬으로 전환");
            gsm.LoadEndingScene(fromUserClick: true);
            return;
        }

        // 다음 문단이 있으면 타이핑 효과로 보여주기
        if (paragraphs != null && currentParagraphIndex < paragraphs.Length - 1)
        {
            currentParagraphIndex++;
            TypeCurrentParagraph(); // ← 여기가 핵심 수정
            return;
        }

        // 모든 문단 다 봤으면 InputField로 전환
        Debug.Log("[InputFieldManager] ResultText 클릭 → InputField로 전환");
        ShowInputField();
    }

    // 타이핑 즉시 완료 (스킵)
    public void SkipTyping()
    {
        ApiClient.Instance.StopTyping();
        IsTyping = false;
        if (resultText != null)
            resultText.text = currentFullText;
    }

    // 기존 메서드 유지 (타이핑 없이 즉시 표시, 하위 호환)
    public void SetResultText(string text)
    {
        if (resultText != null)
        {
            paragraphs = text.Split(new string[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
            currentParagraphIndex = 0;
            Debug.Log($"[InputFieldManager] SetResultText 문단 수: {paragraphs.Length}");
            resultText.text = paragraphs[0];
        }
    }

    // 타이핑 효과로 텍스트 표시 (ApiClient에 코루틴 위임)
    // 타이핑 효과로 텍스트 표시 - 외부(ApiResponseHandler)에서 전체 응답 텍스트로 호출
    public void SetResultTextWithTyping(string text, float typingSpeed = 0.03f)
    {
        if (resultText == null) return;

        // 먼저 resultText 활성화 (비활성 상태면 텍스트 업데이트 안 보임)
        ShowResultText();

        paragraphs = text.Split(new string[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
        currentParagraphIndex = 0;
        Debug.Log($"[InputFieldManager] SetResultTextWithTyping 문단 수: {paragraphs.Length}");

        TypeCurrentParagraph(typingSpeed);
    }

// 현재 인덱스 문단을 타이핑 효과로 표시 (내부용)
    private void TypeCurrentParagraph(float typingSpeed = 0.03f)
    {
        currentFullText = paragraphs[currentParagraphIndex];
        IsTyping = true;
        Debug.Log($"[TypeCurrentParagraph] 호출됨! 문단: {currentFullText.Substring(0, Mathf.Min(20, currentFullText.Length))}");

        ApiClient.Instance.StartTypingEffect(currentFullText, typingSpeed, resultText, () =>
        {
            IsTyping = false;
        });
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