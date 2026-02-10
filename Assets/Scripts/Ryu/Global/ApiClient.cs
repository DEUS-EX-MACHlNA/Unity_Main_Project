using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class ApiClient : MonoBehaviour
{
    [Header("Server Settings")]
    [SerializeField] private string baseUrl = "https://d564-115-95-186-2.ngrok-free.app";
    [SerializeField] private int gameId = 1;
    [SerializeField] private int userId = 1;  // 사용자 ID

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

    // ============================================
    // Scenario API
    // ============================================

    /// <summary>
    /// 시나리오를 시작합니다.
    /// </summary>
    /// <param name="scenarioId">시나리오 ID</param>
    /// <param name="userId">사용자 ID</param>
    /// <param name="onSuccess">성공 콜백 (gameId)</param>
    /// <param name="onError">에러 콜백</param>
    public Coroutine StartScenario(int scenarioId, int userId, Action<int> onSuccess, Action<string> onError)
    {
        return StartCoroutine(StartScenarioCoroutine(scenarioId, userId, onSuccess, onError));
    }

    private IEnumerator StartScenarioCoroutine(int scenarioId, int userId, Action<int> onSuccess, Action<string> onError)
    {
        string url = $"{baseUrl}/api/v1/scenario/start/{scenarioId}?user_id={userId}";
        Debug.Log($"[ApiClient] GET {url}");

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("ngrok-skip-browser-warning", "true");  // ngrok 브라우저 경고 스킵
            request.timeout = Mathf.CeilToInt(timeoutSeconds);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError
                || request.result == UnityWebRequest.Result.ProtocolError
                || request.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.LogWarning($"[ApiClient] 시나리오 시작 실패: {request.error}");
                onError?.Invoke($"시나리오 시작 실패: {request.error}");
                yield break;
            }

            string responseText = request.downloadHandler.text;
            Debug.Log($"[ApiClient] ========== 시나리오 시작 원본 응답 ==========");
            Debug.Log($"[ApiClient] {responseText}");
            Debug.Log($"[ApiClient] ============================================");

            try
            {
                // Json.NET을 사용하여 응답 파싱
                ScenarioStartResponse response = JsonConvert.DeserializeObject<ScenarioStartResponse>(responseText);
                
                // 파싱된 응답을 콘솔에 출력 (JSON 형식으로 포맷팅)
                string formattedResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
                Debug.Log($"[ApiClient] ========== 시나리오 시작 응답 (파싱됨) ==========");
                Debug.Log($"[ApiClient] {formattedResponse}");
                Debug.Log($"[ApiClient] ================================================");
                
                if (response != null && response.game_id > 0)
                {
                    gameId = response.game_id;  // 시작된 게임 ID로 업데이트
                    onSuccess?.Invoke(response.game_id);
                }
                else
                {
                    Debug.LogError($"[ApiClient] 잘못된 응답 형식: {responseText}");
                    onError?.Invoke("잘못된 응답 형식");
                }
            }
            catch (JsonException e)
            {
                Debug.LogError($"[ApiClient] JSON 파싱 에러: {e.Message}");
                Debug.LogError($"[ApiClient] JSON 내용: {responseText}");
                onError?.Invoke($"응답 파싱 실패: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ApiClient] 예상치 못한 에러: {e.Message}");
                onError?.Invoke($"처리 실패: {e.Message}");
            }
        }
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
        Action<string, float, NPCAffectionChanges, NPCHumanityChanges, NPCDisabledStates, NPCLocations, ItemChanges, EventFlags, string> wrappedOnSuccess = null;
        if (onSuccess != null)
        {
            wrappedOnSuccess = (response, humanityChange, npcAffectionChanges, npcHumanityChanges, npcDisabledStates, npcLocations, itemChanges, eventFlags, endingTrigger) =>
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
    /// <param name="onSuccess">성공 콜백 (response, humanityChange, npcAffectionChanges, npcHumanityChanges, npcDisabledStates, npcLocations, itemChanges, eventFlags, endingTrigger)</param>
    /// <param name="onError">에러 콜백</param>
    public Coroutine SendMessage(
        string chatInput, 
        Action<string, float, NPCAffectionChanges, NPCHumanityChanges, NPCDisabledStates, NPCLocations, ItemChanges, EventFlags, string> onSuccess, 
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
    /// <param name="onSuccess">성공 콜백 (response, humanityChange, npcAffectionChanges, npcHumanityChanges, npcDisabledStates, npcLocations, itemChanges, eventFlags, endingTrigger)</param>
    /// <param name="onError">에러 콜백</param>
    public Coroutine SendMessage(
        string chatInput,
        string npcName,
        string itemName,
        Action<string, float, NPCAffectionChanges, NPCHumanityChanges, NPCDisabledStates, NPCLocations, ItemChanges, EventFlags, string> onSuccess, 
        Action<string> onError)
    {
        return StartCoroutine(SendMessageCoroutine(chatInput, npcName, itemName, onSuccess, onError));
    }

    private IEnumerator SendMessageCoroutine(
        string chatInput,
        string npcName,
        string itemName,
        Action<string, float, NPCAffectionChanges, NPCHumanityChanges, NPCDisabledStates, NPCLocations, ItemChanges, EventFlags, string> onSuccess, 
        Action<string> onError)
    {
        string url = $"{baseUrl}/api/v1/game/{gameId}/step";

        StepRequest requestData = new StepRequest
        {
            chat_input = chatInput ?? "",
            npc_name = npcName ?? "",
            item_name = itemName ?? ""
        };

        string jsonBody = JsonUtility.ToJson(requestData);
        Debug.Log($"[ApiClient] POST {url} | Body: {jsonBody}");

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("ngrok-skip-browser-warning", "true");  // ngrok 브라우저 경고 스킵
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
            Debug.Log($"[ApiClient] ========== 백엔드 원본 응답 ==========");
            Debug.Log($"[ApiClient] {responseText}");
            Debug.Log($"[ApiClient] ======================================");

            try
            {
                // Json.NET을 사용하여 백엔드 응답 파싱
                BackendGameResponse backendResponse = JsonConvert.DeserializeObject<BackendGameResponse>(responseText);
                
                // 백엔드 응답을 콘솔에 출력 (JSON 형식으로 포맷팅)
                string formattedResponse = JsonConvert.SerializeObject(backendResponse, Formatting.Indented);
                Debug.Log($"[ApiClient] ========== 백엔드 응답 (파싱됨) ==========");
                Debug.Log($"[ApiClient] {formattedResponse}");
                Debug.Log($"[ApiClient] ==========================================");
                
                // 백엔드 응답을 현재 구조로 변환
                ConvertBackendResponseToCurrentFormat(
                    backendResponse,
                    out string response,
                    out float humanityChange,
                    out NPCAffectionChanges affectionChanges,
                    out NPCHumanityChanges humanityChanges,
                    out NPCDisabledStates disabledStates,
                    out NPCLocations npcLocations,
                    out ItemChanges itemChanges,
                    out EventFlags eventFlags,
                    out string endingTrigger
                );
                
                onSuccess?.Invoke(response, humanityChange, affectionChanges, humanityChanges, disabledStates, npcLocations, itemChanges, eventFlags, endingTrigger);
            }
            catch (JsonException e)
            {
                Debug.LogError($"[ApiClient] JSON 파싱 에러: {e.Message}");
                Debug.LogError($"[ApiClient] JSON 내용: {responseText}");
                onError?.Invoke($"응답 파싱 실패: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ApiClient] 예상치 못한 에러: {e.Message}");
                onError?.Invoke($"처리 실패: {e.Message}");
            }
        }
    }

    /// <summary>
    /// 백엔드 응답을 현재 구조로 변환합니다.
    /// </summary>
    private void ConvertBackendResponseToCurrentFormat(
        BackendGameResponse backendResponse,
        out string response,
        out float humanityChange,
        out NPCAffectionChanges affectionChanges,
        out NPCHumanityChanges humanityChanges,
        out NPCDisabledStates disabledStates,
        out NPCLocations npcLocations,
        out ItemChanges itemChanges,
        out EventFlags eventFlags,
        out string endingTrigger)
    {
        // 1. narrative → response
        response = !string.IsNullOrEmpty(backendResponse.narrative) 
            ? backendResponse.narrative 
            : MOCK_RESPONSE;

        // 2. ending_info → ending_trigger
        endingTrigger = backendResponse.ending_info?.ending_type;

        // 3. humanity_change는 백엔드 응답에 없으므로 0으로 설정
        // (또는 state_delta.vars에서 가져올 수 있다면 그렇게 처리)
        humanityChange = 0f;
        if (backendResponse.state_delta?.vars != null 
            && backendResponse.state_delta.vars.ContainsKey("humanity_change"))
        {
            humanityChange = backendResponse.state_delta.vars["humanity_change"];
        }

        // 4. state_delta.npc_stats → npc_affection_changes, npc_humanity_changes
        affectionChanges = new NPCAffectionChanges();
        humanityChanges = new NPCHumanityChanges();

        if (backendResponse.state_delta?.npc_stats != null)
        {
            foreach (var kvp in backendResponse.state_delta.npc_stats)
            {
                string npcName = kvp.Key;
                BackendNPCStats stats = kvp.Value;

                // NPC 이름 매핑 (백엔드 이름 → Unity enum)
                switch (npcName.ToLower())
                {
                    case "stepmother":
                    case "new_mother":
                        // trust를 affection으로 매핑 (값이 0이 아닐 때만 적용)
                        if (stats.trust != 0f)
                            affectionChanges.new_mother = stats.trust;
                        // suspicion은 현재 구조에 없으므로 무시하거나 로깅
                        if (stats.suspicion != 0f)
                            Debug.Log($"[ApiClient] stepmother suspicion: {stats.suspicion} (현재 구조에 없어 무시됨)");
                        break;

                    case "new_father":
                    case "father":
                        if (stats.trust != 0f)
                            affectionChanges.new_father = stats.trust;
                        if (stats.fear != 0f)
                            humanityChanges.new_father = stats.fear;  // fear를 humanity로 매핑 (임시)
                        break;

                    case "sibling":
                    case "brother":
                        // 백엔드 예시: brother는 fear만 있음
                        if (stats.trust != 0f)
                            affectionChanges.sibling = stats.trust;
                        if (stats.fear != 0f)
                            humanityChanges.sibling = stats.fear;
                        break;

                    case "dog":
                    case "baron":
                        if (stats.trust != 0f)
                            affectionChanges.dog = stats.trust;
                        if (stats.fear != 0f)
                            humanityChanges.dog = stats.fear;
                        break;

                    case "grandmother":
                        if (stats.trust != 0f)
                            affectionChanges.grandmother = stats.trust;
                        if (stats.fear != 0f)
                            humanityChanges.grandmother = stats.fear;
                        break;

                    default:
                        Debug.LogWarning($"[ApiClient] 알 수 없는 NPC 이름: {npcName}");
                        break;
                }
            }
        }

        // 5. state_delta.flags → eventFlags
        eventFlags = null;
        if (backendResponse.state_delta?.flags != null && backendResponse.state_delta.flags.Count > 0)
        {
            eventFlags = new EventFlags();
            
            // Dictionary의 각 플래그를 EventFlags의 고정 필드에 매핑
            foreach (var kvp in backendResponse.state_delta.flags)
            {
                string flagName = kvp.Key;
                bool flagValue = kvp.Value;

                switch (flagName.ToLower())
                {
                    case "met_mother":
                    case "metmother":
                        // 새로운 플래그 - customEvents에 저장
                        if (eventFlags.customEvents == null)
                            eventFlags.customEvents = new Dictionary<string, bool>();
                        eventFlags.customEvents["met_mother"] = flagValue;
                        Debug.Log($"[ApiClient] 플래그 설정: met_mother = {flagValue}");
                        break;

                    case "heard_rumor":
                    case "heardrumor":
                        // 새로운 플래그 - customEvents에 저장
                        if (eventFlags.customEvents == null)
                            eventFlags.customEvents = new Dictionary<string, bool>();
                        eventFlags.customEvents["heard_rumor"] = flagValue;
                        Debug.Log($"[ApiClient] 플래그 설정: heard_rumor = {flagValue}");
                        break;

                    case "grandmother_cooperation":
                    case "grandmothercooperation":
                        eventFlags.grandmotherCooperation = flagValue;
                        break;

                    case "hole_unlocked":
                    case "holeunlocked":
                        eventFlags.holeUnlocked = flagValue;
                        break;

                    case "fire_started":
                    case "firestarted":
                        eventFlags.fireStarted = flagValue;
                        break;

                    case "family_asleep":
                    case "familyasleep":
                        eventFlags.familyAsleep = flagValue;
                        break;

                    case "tea_with_sleeping_pill":
                    case "teawithsleepingpill":
                        eventFlags.teaWithSleepingPill = flagValue;
                        break;

                    case "key_stolen":
                    case "keystolen":
                        eventFlags.keyStolen = flagValue;
                        break;

                    case "caught_by_father":
                    case "caughtbyfather":
                        eventFlags.caughtByFather = flagValue;
                        break;

                    case "caught_by_mother":
                    case "caughtbymother":
                        eventFlags.caughtByMother = flagValue;
                        break;

                    default:
                        // 커스텀 플래그는 customEvents Dictionary에 저장
                        if (eventFlags.customEvents == null)
                            eventFlags.customEvents = new Dictionary<string, bool>();
                        eventFlags.customEvents[flagName] = flagValue;
                        Debug.Log($"[ApiClient] 커스텀 플래그 설정: {flagName} = {flagValue}");
                        break;
                }
            }
        }

        // 6. state_delta.inventory_add/remove → itemChanges
        itemChanges = new ItemChanges();
        
        if (backendResponse.state_delta?.inventory_add != null && backendResponse.state_delta.inventory_add.Count > 0)
        {
            List<ItemAcquisition> acquisitions = new List<ItemAcquisition>();
            foreach (string itemName in backendResponse.state_delta.inventory_add)
            {
                acquisitions.Add(new ItemAcquisition
                {
                    item_name = itemName,
                    count = 1  // 기본값 1 (백엔드에서 개수 제공 시 수정)
                });
            }
            itemChanges.acquired_items = acquisitions.ToArray();
        }

        if (backendResponse.state_delta?.inventory_remove != null && backendResponse.state_delta.inventory_remove.Count > 0)
        {
            List<ItemConsumption> consumptions = new List<ItemConsumption>();
            foreach (string itemName in backendResponse.state_delta.inventory_remove)
            {
                consumptions.Add(new ItemConsumption
                {
                    item_name = itemName,
                    count = 1  // 기본값 1
                });
            }
            itemChanges.consumed_items = consumptions.ToArray();
        }

        // 7. npc_locations, disabled_states는 백엔드 응답에 없으므로 null
        // (필요 시 state_delta에 추가 요청)
        npcLocations = null;
        disabledStates = null;

        // 8. turn_increment 처리 (필요 시 TurnManager에 전달)
        if (backendResponse.state_delta?.turn_increment > 0)
        {
            Debug.Log($"[ApiClient] 턴 증가량: {backendResponse.state_delta.turn_increment}");
            // TurnManager에 전달하는 로직 추가 필요
        }

        // 9. locks, vars 처리 (새로운 필드 - GameStateManager에 추가 필요)
        if (backendResponse.state_delta?.locks != null)
        {
            Debug.Log($"[ApiClient] 잠금 상태 변경: {JsonConvert.SerializeObject(backendResponse.state_delta.locks)}");
            // GameStateManager에 locks 필드 추가 후 처리
        }

        if (backendResponse.state_delta?.vars != null)
        {
            Debug.Log($"[ApiClient] 변수 변경: {JsonConvert.SerializeObject(backendResponse.state_delta.vars)}");
            // GameStateManager에 vars 필드 추가 후 처리
        }
    }
}
