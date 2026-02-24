using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class ApiResponseHandler
{
    private GameStateManager gameStateManager;
    private TurnManager turnManager;
    private RoomTurnManager roomTurnManager;
    private TextMeshProUGUI resultText;
    private InputFieldManager inputFieldManager; // 추가!

    public ApiResponseHandler(GameStateManager gameStateManager, TurnManager turnManager, TextMeshProUGUI resultText, InputFieldManager inputFieldManager = null)
    {
        this.gameStateManager = gameStateManager;
        this.turnManager = turnManager;
        this.roomTurnManager = null;
        this.resultText = resultText;
        this.inputFieldManager = inputFieldManager;
    }

    public ApiResponseHandler(GameStateManager gameStateManager, RoomTurnManager roomTurnManager, TextMeshProUGUI resultText, InputFieldManager inputFieldManager = null)
    {
        this.gameStateManager = gameStateManager;
        this.turnManager = null;
        this.roomTurnManager = roomTurnManager;
        this.resultText = resultText;
        this.inputFieldManager = inputFieldManager;
    }

    public void OnApiSuccess(
        string response,
        float humanityChange,
        NPCAffectionChanges npcAffectionChanges,
        NPCHumanityChanges npcHumanityChanges,
        NPCDisabledStates npcDisabledStates,
        ItemChanges itemChanges,
        EventFlags eventFlags,
        string endingTrigger)
    {
        Debug.Log($"[ApiResponseHandler] 응답 수신: {response}");
        Debug.Log($"[ApiResponseHandler] 인간성 변화량: {humanityChange:F1}");

        // 응답 텍스트 표시 - InputFieldManager 있으면 문단 나누기 사용
        if (inputFieldManager != null)
        {
            inputFieldManager.SetResultText(response);
        }
        else if (resultText != null)
        {
            resultText.text = response;
        }

        if (gameStateManager == null)
        {
            Debug.LogWarning("[ApiResponseHandler] GameStateManager가 연결되지 않았습니다.");
            return;
        }

        GameStateApplier.ApplyHumanityChange(gameStateManager, humanityChange);

        if (npcAffectionChanges != null)
            NPCStateApplier.ApplyAffectionChanges(gameStateManager, npcAffectionChanges);

        if (npcHumanityChanges != null)
            NPCStateApplier.ApplyHumanityChanges(gameStateManager, npcHumanityChanges);

        if (npcDisabledStates != null)
            NPCStateApplier.ApplyDisabledStates(gameStateManager, npcDisabledStates);

        if (itemChanges != null)
            ItemStateApplier.ApplyItemChanges(gameStateManager, itemChanges);

        if (eventFlags != null)
            EventFlagApplier.ApplyEventFlags(gameStateManager, eventFlags);

        if (!string.IsNullOrEmpty(endingTrigger))
        {
            bool endingTriggered = GameStateApplier.ApplyEndingTrigger(gameStateManager, endingTrigger);
            if (endingTriggered)
            {
                if (resultText != null)
                {
                    string current = resultText.text ?? "";
                    if (!current.Contains("클릭") && !current.Contains("넘기"))
                        resultText.text = current + "\n\n[클릭하여 엔딩으로]";
                }
                return;
            }
        }

        if (turnManager != null)
            turnManager.ConsumeTurn();
        else if (roomTurnManager != null)
            roomTurnManager.ConsumeTurn();
        else
            Debug.LogWarning("[ApiResponseHandler] TurnManager가 연결되지 않았습니다.");
    }

    public void OnApiError(string error)
    {
        Debug.LogError($"[ApiResponseHandler] API 에러: {error}");
        if (resultText != null)
            resultText.text = $"에러: {error}";
        
        if (gameStateManager != null)
            CheckFallbackEndings();
    }

    private void CheckFallbackEndings()
    {
        if (gameStateManager.GetHumanity() <= 0f)
        {
            Debug.Log("[ApiResponseHandler] 폴백: UnfinishedDoll 엔딩 트리거");
            gameStateManager.TriggerEnding(EndingType.UnfinishedDoll);
            return;
        }
        
        if (gameStateManager.GetCurrentDay() >= 5)
        {
            Debug.Log("[ApiResponseHandler] 폴백: EternalDinner 엔딩 트리거");
            gameStateManager.TriggerEnding(EndingType.EternalDinner);
            return;
        }
        
        Debug.Log("[ApiResponseHandler] 폴백: 기본 엔딩 조건 미충족");
    }
}