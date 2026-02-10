using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ApiClient : MonoBehaviour
{
    [Header("Server Settings")]
    [SerializeField] private string baseUrl = "https://7783-115-95-186-2.ngrok-free.app";
    [SerializeField] private int gameId = 1;

    [Header("Timeout Settings")]
    [SerializeField] private float timeoutSeconds = 3f;

    private const string MOCK_RESPONSE = "서버 응답을 기다리는 중... 기본 응답입니다.";

    // ============================================
    // 이름 매핑 헬퍼 메서드
    // ============================================

    // 아이템 이름 매핑
    private static Dictionary<string, ItemType> itemNameMapping = new Dictionary<string, ItemType>
    {
        { "sleeping_pill", ItemType.SleepingPill },
        { "earl_grey_tea", ItemType.EarlGreyTea },
        { "real_family_photo", ItemType.RealFamilyPhoto },
        { "oil_bottle", ItemType.OilBottle },
        { "silver_lighter", ItemType.SilverLighter },
        { "siblings_toy", ItemType.SiblingsToy },
        { "brass_key", ItemType.BrassKey }
    };

    // 위치 이름 매핑
    private static Dictionary<string, GameLocation> locationNameMapping = new Dictionary<string, GameLocation>
    {
        { "players_room", GameLocation.PlayersRoom },
        { "hallway", GameLocation.Hallway },
        { "living_room", GameLocation.LivingRoom },
        { "kitchen", GameLocation.Kitchen },
        { "siblings_room", GameLocation.SiblingsRoom },
        { "basement", GameLocation.Basement },
        { "backyard", GameLocation.Backyard }
    };

    // 엔딩 이름 매핑
    private static Dictionary<string, EndingType> endingNameMapping = new Dictionary<string, EndingType>
    {
        { "stealth_exit", EndingType.StealthExit },
        { "chaotic_breakout", EndingType.ChaoticBreakout },
        { "siblings_help", EndingType.SiblingsHelp },
        { "unfinished_doll", EndingType.UnfinishedDoll },
        { "eternal_dinner", EndingType.EternalDinner },
        { "none", EndingType.None }
    };

    // 아이템 상태 이름 매핑
    private static Dictionary<string, ItemState> itemStateNameMapping = new Dictionary<string, ItemState>
    {
        { "in_world", ItemState.InWorld },
        { "in_inventory", ItemState.InInventory },
        { "used", ItemState.Used }
    };

    /// <summary>
    /// 백엔드 아이템 이름을 ItemType enum으로 변환합니다.
    /// </summary>
    private static ItemType ConvertItemNameToType(string itemName)
    {
        if (string.IsNullOrEmpty(itemName))
            return ItemType.None;
        
        if (itemNameMapping.TryGetValue(itemName.ToLower(), out ItemType itemType))
            return itemType;
        
        Debug.LogWarning($"[ApiClient] 알 수 없는 아이템 이름: {itemName}");
        return ItemType.None;
    }

    /// <summary>
    /// 백엔드 위치 이름을 GameLocation enum으로 변환합니다.
    /// </summary>
    private static GameLocation ConvertLocationNameToType(string locationName)
    {
        if (string.IsNullOrEmpty(locationName))
            return GameLocation.Hallway; // 기본값
        
        if (locationNameMapping.TryGetValue(locationName.ToLower(), out GameLocation location))
            return location;
        
        Debug.LogWarning($"[ApiClient] 알 수 없는 위치 이름: {locationName}");
        return GameLocation.Hallway; // 기본값
    }

    /// <summary>
    /// 백엔드 엔딩 이름을 EndingType enum으로 변환합니다.
    /// </summary>
    private static EndingType ConvertEndingNameToType(string endingName)
    {
        if (string.IsNullOrEmpty(endingName))
            return EndingType.None;
        
        if (endingNameMapping.TryGetValue(endingName.ToLower(), out EndingType endingType))
            return endingType;
        
        Debug.LogWarning($"[ApiClient] 알 수 없는 엔딩 이름: {endingName}");
        return EndingType.None;
    }

    /// <summary>
    /// 백엔드 아이템 상태 이름을 ItemState enum으로 변환합니다.
    /// </summary>
    private static ItemState ConvertItemStateNameToType(string stateName)
    {
        if (string.IsNullOrEmpty(stateName))
            return ItemState.InWorld; // 기본값
        
        if (itemStateNameMapping.TryGetValue(stateName.ToLower(), out ItemState state))
            return state;
        
        Debug.LogWarning($"[ApiClient] 알 수 없는 아이템 상태 이름: {stateName}");
        return ItemState.InWorld; // 기본값
    }

    [Serializable]
    private class StepRequest
    {
        public string chat_input;
        public string npc_name;
        public string item_name;
    }

    // ============================================
    // 백엔드 API 응답 구조체는 GameDataTypes.cs로 이동됨
    // ============================================

    [Serializable]
    private class GameResponse
    {
        public string response;                           // 응답 텍스트
        public float humanity_change;                      // 플레이어 인간성 변화량
        public NPCAffectionChanges npc_affection_changes;    // NPC 호감도 변화량 (선택적)
        public NPCHumanityChanges npc_humanity_changes;     // NPC 인간성 변화량 (선택적)
        public NPCDisabledStates npc_disabled_states;        // NPC 무력화 상태 (선택적)
        public ItemChanges item_changes;                   // 아이템 변화량 (선택적)
        public EventFlags event_flags;                      // 이벤트 플래그 (선택적)
        public NPCLocations npc_locations;                  // NPC 위치 (선택적)
        public string ending_trigger;                       // 엔딩 트리거 (선택적, null 가능)
    }

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
        return SendMessage(chatInput,
            (response, humanity, aff, hum, disabled, locations, items, flags, ending) => onSuccess(response, humanity),
            onError);
    }

    /// <summary>
    /// 백엔드 서버에 메시지를 전송하고 응답을 콜백으로 반환합니다.
    /// 3초 타임아웃 시 목업 데이터를 반환합니다.
    /// </summary>
    /// <param name="chatInput">사용자 입력 텍스트</param>
    /// <param name="onSuccess">성공 콜백 (response, humanityChange, npcAffectionChanges, npcHumanityChanges, npcDisabledStates, npcLocations, itemChanges, eventFlags, endingTrigger)</param>
    /// <param name="onError">에러 콜백</param>
    public Coroutine SendMessage(
        string chatInput, 
        Action<string, float, NPCAffectionChanges, NPCHumanityChanges, NPCDisabledStates, NPCLocations, ItemChanges, EventFlags, string> onSuccess, 
        Action<string> onError)
    {
        return StartCoroutine(SendMessageCoroutine(chatInput, onSuccess, onError));
    }

    private IEnumerator SendMessageCoroutine(
        string chatInput, 
        Action<string, float, NPCAffectionChanges, NPCHumanityChanges, NPCDisabledStates, NPCLocations, ItemChanges, EventFlags, string> onSuccess, 
        Action<string> onError)
    {
        string url = $"{baseUrl}/api/v1/game/{gameId}/step";

        StepRequest requestData = new StepRequest
        {
            chat_input = chatInput,
            npc_name = null,
            item_name = null
        };

        string jsonBody = JsonUtility.ToJson(requestData);
        Debug.Log($"[ApiClient] POST {url} | Body: {jsonBody}");

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = Mathf.CeilToInt(timeoutSeconds);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError
                || request.result == UnityWebRequest.Result.ProtocolError
                || request.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.LogWarning($"[ApiClient] 요청 실패 또는 타임아웃: {request.error}");
                Debug.Log($"[ApiClient] 목업 데이터를 반환합니다.");
                // 타임아웃 시 목업 데이터 반환 (NPC 변화량은 모두 0, 무력화 상태와 위치는 null, 아이템 변화량은 빈 인스턴스, 이벤트 플래그와 엔딩 트리거는 null)
                onSuccess?.Invoke(MOCK_RESPONSE, 0f, new NPCAffectionChanges(), new NPCHumanityChanges(), null, null, new ItemChanges(), null, null);
                yield break;
            }

            string responseText = request.downloadHandler.text;
            Debug.Log($"[ApiClient] 응답 수신: {responseText}");

            try
            {
                GameResponse gameResponse = JsonUtility.FromJson<GameResponse>(responseText);
                
                string response = !string.IsNullOrEmpty(gameResponse.response) 
                    ? gameResponse.response 
                    : MOCK_RESPONSE;
                
                // humanity_change는 선택적 필드 (없으면 0으로 처리)
                float humanityChange = gameResponse.humanity_change;
                
                // NPC 변화량 추출 (null 체크 포함, 없으면 새 인스턴스 생성)
                NPCAffectionChanges affectionChanges = gameResponse.npc_affection_changes ?? new NPCAffectionChanges();
                NPCHumanityChanges humanityChanges = gameResponse.npc_humanity_changes ?? new NPCHumanityChanges();
                
                // NPC 무력화 상태 추출 (null 체크 포함, 없으면 null)
                NPCDisabledStates disabledStates = gameResponse.npc_disabled_states;
                
                // NPC 위치 추출 (null 체크 포함, 없으면 null)
                NPCLocations npcLocations = gameResponse.npc_locations;
                
                // 아이템 변화량 추출 (null 체크 포함, 없으면 새 인스턴스 생성)
                ItemChanges itemChanges = gameResponse.item_changes ?? new ItemChanges();
                
                // 이벤트 플래그 추출 (null 체크 포함, 없으면 null)
                EventFlags eventFlags = gameResponse.event_flags;
                
                // 엔딩 트리거 추출 (null 체크 포함, 없으면 null)
                string endingTrigger = gameResponse.ending_trigger;
                
                onSuccess?.Invoke(response, humanityChange, affectionChanges, humanityChanges, disabledStates, npcLocations, itemChanges, eventFlags, endingTrigger);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ApiClient] JSON 파싱 에러: {e.Message}");
                onError?.Invoke($"응답 파싱 실패: {e.Message}");
            }
        }
    }
}
