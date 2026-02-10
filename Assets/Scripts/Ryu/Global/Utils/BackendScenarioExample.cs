using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// 백엔드 응답을 하드코딩으로 시뮬레이션하는 예시 시나리오 클래스입니다.
/// 실제 게임 플레이 흐름을 이해하기 위한 데모 코드입니다.
/// </summary>
public class BackendScenarioExample : MonoBehaviour
{
    [Header("시나리오 설정")]
    [SerializeField] private bool autoRunScenario = false;
    [SerializeField] private bool autoCreateGameStateManager = true;
    
    private void Start()
    {
        // GameStateManager가 없으면 자동으로 찾거나 생성
        if (GameStateManager.Instance == null && autoCreateGameStateManager)
        {
            EnsureGameStateManagerExists();
        }
        
        if (autoRunScenario)
        {
            // 시나리오 자동 실행
            RunCompleteScenario();
        }
    }
    
    /// <summary>
    /// GameStateManager가 존재하는지 확인하고, 없으면 찾거나 생성합니다.
    /// </summary>
    private void EnsureGameStateManagerExists()
    {
        // 1. 씬에서 GameStateManager 찾기
        GameStateManager existingManager = FindFirstObjectByType<GameStateManager>();
        if (existingManager != null)
        {
            Debug.Log("[BackendScenarioExample] 씬에서 GameStateManager를 찾았습니다.");
            return;
        }
        
        // 2. 없으면 새로 생성
        Debug.LogWarning("[BackendScenarioExample] GameStateManager가 없어서 자동으로 생성합니다.");
        GameObject gameStateManagerObj = new GameObject("GameStateManager");
        gameStateManagerObj.AddComponent<GameStateManager>();
        Debug.Log("[BackendScenarioExample] GameStateManager 생성 완료!");
    }
    
    /// <summary>
    /// 전체 시나리오를 실행합니다.
    /// 시나리오: 플레이어가 홍차에 수면제를 넣어서 가족을 재우고 탈출하는 경우
    /// </summary>
    public void RunCompleteScenario()
    {
        Debug.Log("==========================================");
        Debug.Log("[시나리오 시작] 홍차에 수면제 넣기");
        Debug.Log("==========================================");
        
        // 1단계: 플레이어가 홍차에 수면제를 넣는 액션 수행
        SimulateBackendResponse_AddSleepingPillToTea();
        
        // 2단계: 가족이 홍차를 마시고 잠들음
        SimulateBackendResponse_FamilyFallsAsleep();
        
        // 3단계: 플레이어가 뒷마당으로 이동하여 탈출 시도
        SimulateBackendResponse_AttemptEscape();
    }
    
    // ============================================
    // 시나리오 1: 홍차에 수면제 넣기
    // ============================================
    
    /// <summary>
    /// 백엔드 응답 시뮬레이션: 홍차에 수면제를 넣는 액션
    /// </summary>
    public void SimulateBackendResponse_AddSleepingPillToTea()
    {
        Debug.Log("\n[1단계] 홍차에 수면제 넣기 - 백엔드 응답 시뮬레이션");
        Debug.Log("------------------------------------------");
        
        // ============================================
        // [API/] 백엔드에서 받은 하드코딩된 응답
        // ============================================
        string hardcodedBackendResponse = @"{
            ""narrative"": ""당신은 조용히 주방으로 향했습니다. 홍차 포트 옆에 있는 보라색 라벨의 약병을 손에 쥐었습니다. 약병을 열고, 홍차에 조용히 수면제를 넣었습니다. 아무도 눈치채지 못했습니다."",
            ""ending_info"": null,
            ""state_delta"": {
                ""npc_stats"": {},
                ""flags"": {
                    ""tea_with_sleeping_pill"": true
                },
                ""inventory_add"": [],
                ""inventory_remove"": [""sleeping_pill""],
                ""locks"": {},
                ""humanity_change"": -5.0,
                ""vars"": {}
            }
        }";
        
        Debug.Log($"[API/] 백엔드 응답 수신:\n{hardcodedBackendResponse}");
        
        // ============================================
        // [API/BackendResponseConverter] 백엔드 응답을 Unity 형식으로 변환
        // ============================================
        BackendGameResponse backendResponse = JsonConvert.DeserializeObject<BackendGameResponse>(hardcodedBackendResponse);
        BackendResponseConverter converter = new BackendResponseConverter("기본 응답");
        
        string response;
        float humanityChange;
        NPCAffectionChanges affectionChanges;
        NPCHumanityChanges humanityChanges;
        NPCDisabledStates disabledStates;
        NPCLocations npcLocations;
        ItemChanges itemChanges;
        EventFlags eventFlags;
        string endingTrigger;
        Dictionary<string, bool> locks;
        
        converter.ConvertBackendResponseToCurrentFormat(
            backendResponse,
            out response,
            out humanityChange,
            out affectionChanges,
            out humanityChanges,
            out disabledStates,
            out npcLocations,
            out itemChanges,
            out eventFlags,
            out endingTrigger,
            out locks
        );
        
        Debug.Log($"[API/BackendResponseConverter] 변환 완료:");
        Debug.Log($"  - narrative: {response}");
        Debug.Log($"  - humanity_change: {humanityChange}");
        Debug.Log($"  - event_flags.teaWithSleepingPill: {eventFlags?.teaWithSleepingPill}");
        Debug.Log($"  - item_changes.consumed_items: {itemChanges?.consumed_items?.Length}개");
        
        // ============================================
        // [Utils/NameMapper] 백엔드 이름을 Unity enum으로 변환
        // ============================================
        if (itemChanges?.consumed_items != null)
        {
            foreach (var consumed in itemChanges.consumed_items)
            {
                ItemType itemType = NameMapper.ConvertItemNameToType(consumed.item_name);
                Debug.Log($"[Utils/NameMapper] '{consumed.item_name}' → {itemType}");
            }
        }
        
        // ============================================
        // [State/] 각 Applier가 상태를 게임에 적용
        // ============================================
        if (GameStateManager.Instance == null)
        {
            Debug.LogWarning("[BackendScenarioExample] GameStateManager.Instance가 null입니다!");
            Debug.LogWarning("해결 방법:");
            Debug.LogWarning("1. Hierarchy에 'GameStateManager' GameObject가 있는지 확인");
            Debug.LogWarning("2. Play 모드에서 실행 중인지 확인 (에디터 모드에서는 Awake가 실행되지 않음)");
            Debug.LogWarning("3. GameStateManager 컴포넌트가 GameObject에 추가되어 있는지 확인");
            return;
        }
        
        // 1. [State/GameStateApplier] 인간성 변화 적용
        GameStateApplier.ApplyHumanityChange(GameStateManager.Instance, humanityChange);
        
        // 2. [State/ItemStateApplier] 아이템 소모 적용
        if (itemChanges != null)
        {
            ItemStateApplier.ApplyItemChanges(GameStateManager.Instance, itemChanges);
        }
        
        // 3. [State/EventFlagApplier] 이벤트 플래그 적용
        if (eventFlags != null)
        {
            EventFlagApplier.ApplyEventFlags(GameStateManager.Instance, eventFlags);
        }
        
        // ============================================
        // [Managers/] 각 매니저가 상태를 관리
        // ============================================
        Debug.Log("\n[Managers/] 상태 확인:");
        Debug.Log($"  - EventFlagManager: teaWithSleepingPill = {GameStateManager.Instance.GetEventFlag("teaWithSleepingPill")}");
        Debug.Log($"  - GameStateManager: 현재 인간성 = {GameStateManager.Instance.GetHumanity()}");
        Debug.Log($"  - InventoryManager: 수면제 보유 여부 = {GameStateManager.Instance.HasItem(ItemType.SleepingPill)}");
        
        Debug.Log("\n[1단계 완료] 홍차에 수면제를 넣었습니다.\n");
    }
    
    // ============================================
    // 시나리오 2: 가족이 잠들음
    // ============================================
    
    /// <summary>
    /// 백엔드 응답 시뮬레이션: 가족이 홍차를 마시고 잠들음
    /// </summary>
    public void SimulateBackendResponse_FamilyFallsAsleep()
    {
        Debug.Log("\n[2단계] 가족이 홍차를 마시고 잠들음 - 백엔드 응답 시뮬레이션");
        Debug.Log("------------------------------------------");
        
        // ============================================
        // [API/] 백엔드에서 받은 하드코딩된 응답
        // ============================================
        string hardcodedBackendResponse = @"{
            ""narrative"": ""저녁 식사 시간이 되었습니다. 가족들이 모두 거실에 모여 홍차를 마셨습니다. 몇 분 후, 새아빠가 먼저 고개를 끄덕이기 시작했습니다. 곧이어 동생과 강아지도 잠에 빠졌습니다. 새엄마만이 아직 깨어있지만, 그녀도 곧 졸음에 빠질 것 같습니다."",
            ""ending_info"": null,
            ""state_delta"": {
                ""npc_stats"": {
                    ""new_father"": {
                        ""trust"": 0,
                        ""suspicion"": 0,
                        ""fear"": 0
                    },
                    ""sibling"": {
                        ""trust"": 0,
                        ""suspicion"": 0,
                        ""fear"": 0
                    },
                    ""dog"": {
                        ""trust"": 0,
                        ""suspicion"": 0,
                        ""fear"": 0
                    }
                },
                ""flags"": {
                    ""family_asleep"": true,
                    ""tea_with_sleeping_pill"": true
                },
                ""inventory_add"": [],
                ""inventory_remove"": [],
                ""locks"": {},
                ""vars"": {}
            }
        }";
        
        Debug.Log($"[API/] 백엔드 응답 수신 (가족 잠듦)");
        
        // ============================================
        // [API/BackendResponseConverter] 변환
        // ============================================
        BackendGameResponse backendResponse = JsonConvert.DeserializeObject<BackendGameResponse>(hardcodedBackendResponse);
        BackendResponseConverter converter = new BackendResponseConverter("기본 응답");
        
        string response;
        float humanityChange;
        NPCAffectionChanges affectionChanges;
        NPCHumanityChanges humanityChanges;
        NPCDisabledStates disabledStates;
        NPCLocations npcLocations;
        ItemChanges itemChanges;
        EventFlags eventFlags;
        string endingTrigger;
        Dictionary<string, bool> locks;
        
        converter.ConvertBackendResponseToCurrentFormat(
            backendResponse,
            out response,
            out humanityChange,
            out affectionChanges,
            out humanityChanges,
            out disabledStates,
            out npcLocations,
            out itemChanges,
            out eventFlags,
            out endingTrigger,
            out locks
        );
        
        Debug.Log($"[API/BackendResponseConverter] 변환 완료:");
        Debug.Log($"  - event_flags.familyAsleep: {eventFlags?.familyAsleep}");
        
        // ============================================
        // [State/] 상태 적용
        // ============================================
        if (GameStateManager.Instance == null)
        {
            Debug.LogWarning("[BackendScenarioExample] GameStateManager.Instance가 null입니다!");
            return;
        }
        
        // [State/EventFlagApplier] 이벤트 플래그 적용
        if (eventFlags != null)
        {
            EventFlagApplier.ApplyEventFlags(GameStateManager.Instance, eventFlags);
        }
        
        // ============================================
        // [Managers/] 상태 확인
        // ============================================
        Debug.Log("\n[Managers/] 상태 확인:");
        Debug.Log($"  - EventFlagManager: familyAsleep = {GameStateManager.Instance.GetEventFlag("familyAsleep")}");
        Debug.Log($"  - EventFlagManager: teaWithSleepingPill = {GameStateManager.Instance.GetEventFlag("teaWithSleepingPill")}");
        
        Debug.Log("\n[2단계 완료] 가족이 잠들었습니다.\n");
    }
    
    // ============================================
    // 시나리오 3: 탈출 시도
    // ============================================
    
    /// <summary>
    /// 백엔드 응답 시뮬레이션: 뒷마당으로 이동하여 탈출 시도
    /// </summary>
    public void SimulateBackendResponse_AttemptEscape()
    {
        Debug.Log("\n[3단계] 뒷마당으로 이동하여 탈출 시도 - 백엔드 응답 시뮬레이션");
        Debug.Log("------------------------------------------");
        
        // ============================================
        // [API/] 백엔드에서 받은 하드코딩된 응답
        // ============================================
        string hardcodedBackendResponse = @"{
            ""narrative"": ""당신은 조용히 뒷마당으로 향했습니다. 잠든 가족들을 뒤로하고, 문을 열었습니다. 밤하늘이 보입니다. 당신은 자유를 향해 달려갔습니다."",
            ""ending_info"": {
                ""ending_type"": ""stealth_exit"",
                ""description"": ""완벽한 기만 - 당신은 아무도 눈치채지 못하게 탈출했습니다.""
            },
            ""state_delta"": {
                ""npc_stats"": {},
                ""flags"": {
                    ""family_asleep"": true,
                    ""tea_with_sleeping_pill"": true,
                    ""key_stolen"": true
                },
                ""inventory_add"": [],
                ""inventory_remove"": [],
                ""locks"": {},
                ""vars"": {}
            }
        }";
        
        Debug.Log($"[API/] 백엔드 응답 수신 (탈출 성공)");
        
        // ============================================
        // [API/BackendResponseConverter] 변환
        // ============================================
        BackendGameResponse backendResponse = JsonConvert.DeserializeObject<BackendGameResponse>(hardcodedBackendResponse);
        BackendResponseConverter converter = new BackendResponseConverter("기본 응답");
        
        string response;
        float humanityChange;
        NPCAffectionChanges affectionChanges;
        NPCHumanityChanges humanityChanges;
        NPCDisabledStates disabledStates;
        NPCLocations npcLocations;
        ItemChanges itemChanges;
        EventFlags eventFlags;
        string endingTrigger;
        Dictionary<string, bool> locks;
        
        converter.ConvertBackendResponseToCurrentFormat(
            backendResponse,
            out response,
            out humanityChange,
            out affectionChanges,
            out humanityChanges,
            out disabledStates,
            out npcLocations,
            out itemChanges,
            out eventFlags,
            out endingTrigger,
            out locks
        );
        
        Debug.Log($"[API/BackendResponseConverter] 변환 완료:");
        Debug.Log($"  - ending_trigger: {endingTrigger}");
        
        // ============================================
        // [Utils/NameMapper] 엔딩 이름 변환
        // ============================================
        EndingType endingType = NameMapper.ConvertEndingNameToType(endingTrigger);
        Debug.Log($"[Utils/NameMapper] '{endingTrigger}' → {endingType}");
        
        // ============================================
        // [State/] 상태 적용
        // ============================================
        if (GameStateManager.Instance == null)
        {
            Debug.LogWarning("[BackendScenarioExample] GameStateManager.Instance가 null입니다!");
            return;
        }
        
        // [State/EventFlagApplier] 이벤트 플래그 적용
        if (eventFlags != null)
        {
            EventFlagApplier.ApplyEventFlags(GameStateManager.Instance, eventFlags);
        }
        
        // [State/GameStateApplier] 엔딩 트리거 적용
        if (!string.IsNullOrEmpty(endingTrigger))
        {
            bool endingTriggered = GameStateApplier.ApplyEndingTrigger(GameStateManager.Instance, endingTrigger);
            Debug.Log($"[State/GameStateApplier] 엔딩 트리거 결과: {endingTriggered}");
        }
        
        // ============================================
        // [Managers/EndingManager] 엔딩 조건 체크 및 처리
        // ============================================
        Debug.Log("\n[Managers/EndingManager] 엔딩 조건 체크:");
        Debug.Log($"  - teaWithSleepingPill: {GameStateManager.Instance.GetEventFlag("teaWithSleepingPill")}");
        Debug.Log($"  - familyAsleep: {GameStateManager.Instance.GetEventFlag("familyAsleep")}");
        Debug.Log($"  - keyStolen: {GameStateManager.Instance.GetEventFlag("keyStolen")}");
        Debug.Log($"  - 현재 위치: Backyard (가정)");
        Debug.Log($"  → StealthExit 엔딩 조건 달성!");
        
        Debug.Log("\n[3단계 완료] 탈출 성공! 엔딩 트리거됨.\n");
        
        Debug.Log("==========================================");
        Debug.Log("[시나리오 완료] 전체 플로우 실행 완료");
        Debug.Log("==========================================");
    }
    
    // ============================================
    // 유틸리티: 개별 시나리오 테스트
    // ============================================
    
    [ContextMenu("1단계: 홍차에 수면제 넣기")]
    private void TestStep1()
    {
        SimulateBackendResponse_AddSleepingPillToTea();
    }
    
    [ContextMenu("2단계: 가족이 잠들음")]
    private void TestStep2()
    {
        SimulateBackendResponse_FamilyFallsAsleep();
    }
    
    [ContextMenu("3단계: 탈출 시도")]
    private void TestStep3()
    {
        SimulateBackendResponse_AttemptEscape();
    }
    
    [ContextMenu("전체 시나리오 실행")]
    private void TestFullScenario()
    {
        RunCompleteScenario();
    }
}

