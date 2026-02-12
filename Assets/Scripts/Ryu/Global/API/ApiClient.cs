using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// API 클라이언트 메인 코디네이터 클래스입니다.
/// 각 API 모듈을 조율하여 API 요청을 처리합니다.
/// </summary>
public class ApiClient : MonoBehaviour
{
    [Header("Server Settings")]
    [SerializeField] private string baseUrl = "https://d564-115-95-186-2.ngrok-free.app";
    [SerializeField] private int gameId = 17;
    // 시나리오 시작 API를 제거했으므로, userId는 gameId 생성에만 사용됩니다.

    [Header("Timeout Settings")]
    [SerializeField] private float timeoutSeconds = 3f;

    public const string MOCK_RESPONSE = "서버 응답을 기다리는 중... 기본 응답입니다.";

    // API 클라이언트 인스턴스
    private GameStepApiClient gameStepApiClient;

    private void Awake()
    {
        // API 클라이언트 초기화
        gameStepApiClient = new GameStepApiClient(baseUrl, () => gameId, timeoutSeconds, MOCK_RESPONSE);
    }

    private void SetGameId(int newGameId)
    {
        gameId = newGameId;
        Debug.Log($"[ApiClient] 게임 ID 설정: {gameId}");
    }

    // ============================================
    // 이름 매핑 헬퍼 메서드 (하위 호환성을 위해 유지)
    // ============================================

    /// <summary>
    /// 백엔드 아이템 이름을 ItemType enum으로 변환합니다.
    /// </summary>
    public static ItemType ConvertItemNameToType(string itemName)
    {
        return NameMapper.ConvertItemNameToType(itemName);
    }

    /// <summary>
    /// 백엔드 위치 이름을 GameLocation enum으로 변환합니다.
    /// </summary>
    public static GameLocation ConvertLocationNameToType(string locationName)
    {
        return NameMapper.ConvertLocationNameToType(locationName);
    }

    /// <summary>
    /// 백엔드 엔딩 이름을 EndingType enum으로 변환합니다.
    /// </summary>
    public static EndingType ConvertEndingNameToType(string endingName)
    {
        return NameMapper.ConvertEndingNameToType(endingName);
    }

    /// <summary>
    /// 백엔드 아이템 상태 이름을 ItemState enum으로 변환합니다.
    /// </summary>
    public static ItemState ConvertItemStateNameToType(string stateName)
    {
        return NameMapper.ConvertItemStateNameToType(stateName);
    }

    // ============================================
    // Scenario (로컬)
    // ============================================

    /// <summary>
    /// 시나리오를 시작합니다. (시작 API 호출 없이 로컬에서 gameId를 생성)
    /// </summary>
    /// <param name="scenarioId">시나리오 ID</param>
    /// <param name="userId">사용자 ID</param>
    /// <returns>생성된 gameId</returns>
    public int StartScenario(int scenarioId, int userId)
    {
        // 최대한 단순하게: 서버에 "시나리오 시작"을 요청하지 않고, 클라이언트가 gameId를 만든다.
        // 충돌 가능성을 줄이기 위해 userId, scenarioId, 시간 기반 값을 섞는다.
        int seed = Environment.TickCount;
        int generated = unchecked((userId * 1000003) ^ (scenarioId * 9176) ^ seed);
        generated = Mathf.Abs(generated);
        if (generated == 0) generated = 1;

        SetGameId(generated);
        Debug.Log($"[ApiClient] 시나리오 시작(로컬): scenarioId={scenarioId}, userId={userId}, gameId={gameId}");
        return gameId;
    }

    // ============================================
    // Game Step API
    // ============================================

    /// <summary>
    /// 백엔드 서버에 메시지를 전송하고 응답을 콜백으로 반환합니다.
    /// 3초 타임아웃 시 목업 데이터를 반환합니다.
    /// </summary>
    /// <param name="chatInput">사용자 입력 텍스트</param>
    /// <param name="onSuccess">성공 콜백 (response, humanityChange)</param>
    /// <param name="onError">에러 콜백</param>
    public Coroutine SendMessage(string chatInput, Action<string, float> onSuccess, Action<string> onError)
    {
        // 하위 호환성을 위해 기존 시그니처 유지
        // Action<string, float>를 9개 매개변수를 받는 Action으로 래핑
        Action<string, float, NPCAffectionChanges, NPCHumanityChanges, NPCDisabledStates, ItemChanges, EventFlags, string, Dictionary<string, bool>> wrappedOnSuccess = null;
        if (onSuccess != null)
        {
            wrappedOnSuccess = (response, humanityChange, npcAffectionChanges, npcHumanityChanges, npcDisabledStates, itemChanges, eventFlags, endingTrigger, locks) =>
            {
                onSuccess(response, humanityChange);
            };
        }
        return SendMessage(chatInput, null, null, wrappedOnSuccess, onError);
    }

    /// <summary>
    /// 백엔드 서버에 메시지를 전송하고 응답을 콜백으로 반환합니다.
    /// 3초 타임아웃 시 목업 데이터를 반환합니다.
    /// </summary>
    /// <param name="chatInput">사용자 입력 텍스트</param>
    /// <param name="onSuccess">성공 콜백 (response, humanityChange, npcAffectionChanges, npcHumanityChanges, npcDisabledStates, itemChanges, eventFlags, endingTrigger, locks)</param>
    /// <param name="onError">에러 콜백</param>
    public Coroutine SendMessage(
        string chatInput,
        Action<string, float, NPCAffectionChanges, NPCHumanityChanges, NPCDisabledStates, ItemChanges, EventFlags, string, Dictionary<string, bool>> onSuccess,
        Action<string> onError)
    {
        return SendMessage(chatInput, null, null, onSuccess, onError);
    }

    /// <summary>
    /// 백엔드 서버에 메시지를 전송하고 응답을 콜백으로 반환합니다.
    /// 3초 타임아웃 시 목업 데이터를 반환합니다.
    /// </summary>
    /// <param name="chatInput">사용자 입력 텍스트</param>
    /// <param name="npcName">NPC 이름 (선택적)</param>
    /// <param name="itemName">아이템 이름 (선택적)</param>
    /// <param name="onSuccess">성공 콜백 (response, humanityChange, npcAffectionChanges, npcHumanityChanges, npcDisabledStates, itemChanges, eventFlags, endingTrigger, locks)</param>
    /// <param name="onError">에러 콜백</param>
    public Coroutine SendMessage(
        string chatInput,
        string npcName,
        string itemName,
        Action<string, float, NPCAffectionChanges, NPCHumanityChanges, NPCDisabledStates, ItemChanges, EventFlags, string, Dictionary<string, bool>> onSuccess,
        Action<string> onError)
    {
        if (gameStepApiClient == null)
        {
            gameStepApiClient = new GameStepApiClient(baseUrl, () => gameId, timeoutSeconds, MOCK_RESPONSE);
        }
        return StartCoroutine(gameStepApiClient.SendMessageCoroutine(chatInput, npcName, itemName, onSuccess, onError));
    }
}
