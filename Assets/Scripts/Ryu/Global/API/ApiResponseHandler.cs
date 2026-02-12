using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

/// <summary>
/// API 응답 처리를 담당하는 클래스입니다.
/// API 성공/에러 응답을 처리하고 각 StateApplier로 위임합니다.
/// </summary>
public class ApiResponseHandler
{
    private GameStateManager gameStateManager;
    private TurnManager turnManager;
    private RoomTurnManager roomTurnManager;
    private TextMeshProUGUI resultText;

    /// <summary>
    /// ApiResponseHandler 생성자 (전역 TurnManager 사용)
    /// </summary>
    /// <param name="gameStateManager">GameStateManager 인스턴스</param>
    /// <param name="turnManager">TurnManager 인스턴스</param>
    /// <param name="resultText">결과 텍스트 UI</param>
    public ApiResponseHandler(GameStateManager gameStateManager, TurnManager turnManager, TextMeshProUGUI resultText)
    {
        this.gameStateManager = gameStateManager;
        this.turnManager = turnManager;
        this.roomTurnManager = null;
        this.resultText = resultText;
    }

    /// <summary>
    /// ApiResponseHandler 생성자 (RoomTurnManager 사용)
    /// </summary>
    /// <param name="gameStateManager">GameStateManager 인스턴스</param>
    /// <param name="roomTurnManager">RoomTurnManager 인스턴스</param>
    /// <param name="resultText">결과 텍스트 UI</param>
    public ApiResponseHandler(GameStateManager gameStateManager, RoomTurnManager roomTurnManager, TextMeshProUGUI resultText)
    {
        this.gameStateManager = gameStateManager;
        this.turnManager = null;
        this.roomTurnManager = roomTurnManager;
        this.resultText = resultText;
    }

    /// <summary>
    /// API 성공 응답을 처리합니다.
    /// </summary>
    /// <param name="response">응답 텍스트</param>
    /// <param name="humanityChange">인간성 변화량</param>
    /// <param name="npcAffectionChanges">NPC 호감도 변화량</param>
    /// <param name="npcHumanityChanges">NPC 인간성 변화량</param>
    /// <param name="npcDisabledStates">NPC 무력화 상태</param>
    /// <param name="itemChanges">아이템 변화량</param>
    /// <param name="eventFlags">이벤트 플래그</param>
    /// <param name="endingTrigger">엔딩 트리거</param>
    /// <param name="locks">잠금 상태</param>
    public void OnApiSuccess(
        string response,
        float humanityChange,
        NPCAffectionChanges npcAffectionChanges,
        NPCHumanityChanges npcHumanityChanges,
        NPCDisabledStates npcDisabledStates,
        ItemChanges itemChanges,
        EventFlags eventFlags,
        string endingTrigger,
        Dictionary<string, bool> locks)
    {
        Debug.Log($"[ApiResponseHandler] 응답 수신: {response}");
        Debug.Log($"[ApiResponseHandler] 인간성 변화량: {humanityChange:F1}");

        // 응답 텍스트 표시
        if (resultText != null)
        {
            resultText.text = response;
        }

        // GameStateManager가 없으면 경고 후 종료
        if (gameStateManager == null)
        {
            Debug.LogWarning("[ApiResponseHandler] GameStateManager가 연결되지 않았습니다.");
            return;
        }

        // 플레이어 인간성 변화량 적용
        GameStateApplier.ApplyHumanityChange(gameStateManager, humanityChange);

        // NPC 호감도 변화량 적용
        if (npcAffectionChanges != null)
        {
            NPCStateApplier.ApplyAffectionChanges(gameStateManager, npcAffectionChanges);
        }

        // NPC 인간성 변화량 적용
        if (npcHumanityChanges != null)
        {
            NPCStateApplier.ApplyHumanityChanges(gameStateManager, npcHumanityChanges);
        }

        // NPC 무력화 상태 적용 (백엔드에서 제공 시만)
        if (npcDisabledStates != null)
        {
            NPCStateApplier.ApplyDisabledStates(gameStateManager, npcDisabledStates);
        }

        // 아이템 변화량 적용 (백엔드에서 제공 시만)
        if (itemChanges != null)
        {
            ItemStateApplier.ApplyItemChanges(gameStateManager, itemChanges);
        }

        // 이벤트 플래그 적용 (백엔드에서 제공 시만)
        if (eventFlags != null)
        {
            EventFlagApplier.ApplyEventFlags(gameStateManager, eventFlags);
        }

        // 잠금 상태 적용 (백엔드에서 제공 시만)
        if (locks != null && locks.Count > 0)
        {
            GameStateApplier.ApplyLocks(gameStateManager, locks);
        }

        // 엔딩 트리거 처리
        if (!string.IsNullOrEmpty(endingTrigger))
        {
            bool endingTriggered = GameStateApplier.ApplyEndingTrigger(gameStateManager, endingTrigger);
            if (endingTriggered)
            {
                // 엔딩 진입 시 더 이상 처리하지 않음
                return;
            }
        }

        // 턴 소모
        if (turnManager != null)
        {
            turnManager.ConsumeTurn();
        }
        else if (roomTurnManager != null)
        {
            roomTurnManager.ConsumeTurn();
        }
        else
        {
            Debug.LogWarning("[ApiResponseHandler] TurnManager가 연결되지 않았습니다.");
        }
    }

    /// <summary>
    /// API 에러 응답을 처리합니다.
    /// </summary>
    /// <param name="error">에러 메시지</param>
    public void OnApiError(string error)
    {
        Debug.LogError($"[ApiResponseHandler] API 에러: {error}");
        if (resultText != null)
        {
            resultText.text = $"에러: {error}";
        }
        
        // 백엔드 응답 실패 시 기본 엔딩만 폴백으로 체크
        if (gameStateManager != null)
        {
            CheckFallbackEndings();
        }
    }

    /// <summary>
    /// 백엔드 응답이 없을 때 기본 엔딩만 폴백으로 체크합니다.
    /// UnfinishedDoll과 EternalDinner만 체크합니다.
    /// </summary>
    private void CheckFallbackEndings()
    {
        // UnfinishedDoll 체크 (인간성 0%)
        if (gameStateManager.GetHumanity() <= 0f)
        {
            Debug.Log("[ApiResponseHandler] 폴백: UnfinishedDoll 엔딩 트리거");
            gameStateManager.TriggerEnding(EndingType.UnfinishedDoll);
            return;
        }
        
        // EternalDinner 체크 (5일차 종료 시)
        if (gameStateManager.GetCurrentDay() >= 5)
        {
            Debug.Log("[ApiResponseHandler] 폴백: EternalDinner 엔딩 트리거");
            gameStateManager.TriggerEnding(EndingType.EternalDinner);
            return;
        }
        
        // 그 외 엔딩은 백엔드에서만 처리
        Debug.Log("[ApiResponseHandler] 폴백: 기본 엔딩 조건 미충족");
    }
}

