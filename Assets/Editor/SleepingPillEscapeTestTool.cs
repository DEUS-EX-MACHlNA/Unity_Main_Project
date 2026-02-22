using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// 수면제 탈출 원 사이클 테스트를 위한 Unity Editor Tool입니다.
/// Play 모드에서 전체 테스트 사이클을 수동으로 실행합니다.
/// </summary>
public class SleepingPillEscapeTestTool : EditorWindow
{
    private enum TestStep
    {
        NotStarted,
        Step1_SiblingTrustBoost,
        Step2_BrotherEscapeRouteReveal,
        Step3_SleepingPillAttemptBlocked,
        Step4_BloodRequestAccept,
        Step5_AcquireSleepingPill,
        Step6_TeaWithSleepingPill,
        Step7_NightDialogue,
        Step8_SecretKeyFromPhoto,
        Step9_EscapeEnding,
        Completed,
        Failed
    }

    private const int StepCount = 9;
    private TestStep currentStep = TestStep.NotStarted;
    private bool[] stepCompleted = new bool[StepCount];
    private bool[] stepInProgress = new bool[StepCount];
    private string statusMessage = "테스트를 시작하려면 각 단계의 버튼을 클릭하세요.";
    private Vector2 scrollPosition;
    private IEnumerator currentCoroutine;

    [MenuItem("Tools/Test/Sleeping Pill Escape Cycle")]
    public static void ShowWindow()
    {
        GetWindow<SleepingPillEscapeTestTool>("수면제 탈출 테스트");
    }

    private void OnEnable()
    {
        EditorApplication.update += UpdateCoroutine;
    }

    private void OnDisable()
    {
        EditorApplication.update -= UpdateCoroutine;
    }

    private void UpdateCoroutine()
    {
        if (currentCoroutine != null)
        {
            if (!currentCoroutine.MoveNext())
            {
                currentCoroutine = null;
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("수면제 탈출 원 사이클 테스트", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Play 모드 체크
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("이 테스트는 Play 모드에서만 실행할 수 있습니다. 먼저 Play 모드를 시작해주세요.", MessageType.Warning);
            if (GUILayout.Button("Play 모드 시작"))
            {
                EditorApplication.isPlaying = true;
            }
            return;
        }

        // 리셋 버튼
        if (GUILayout.Button("테스트 리셋", GUILayout.Height(25)))
        {
            ResetTest();
        }

        EditorGUILayout.Space();

        // 진행 상태 표시
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        EditorGUILayout.LabelField("테스트 단계:", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // 각 단계별 버튼과 상태 표시 (완벽한기만.md · Assets/TestData/Scenarios 흐름)
        // 조건 1 — 동생에게서 탈출 정보 습득
        DrawStepButton("1. 동생 호감도 상승", 0, TestStep.Step1_SiblingTrustBoost,
            "동생과 함께 장난감으로 놀아준다", "brother", "", "sibling_trust_boost.json");
        
        DrawStepButton("2. 탈출 경로 정보 해금 (동생 대화)", 1, TestStep.Step2_BrotherEscapeRouteReveal,
            "루카스야, 같이 놀자.", "brother", "", "brother_escape_route_reveal.json");
        
        // 조건 2 — 수면제 ~ 탈출
        DrawStepButton("3. 수면제 훔치기 시도 (실패·제지)", 2, TestStep.Step3_SleepingPillAttemptBlocked,
            "주방 찬장에서 수면제를 훔치려 한다.", "stepmother", "industrial_sedative", "sleeping_pill_attempt_blocked.json");
        
        DrawStepButton("4. 피 요청 수락·새엄마 자리 비움", 3, TestStep.Step4_BloodRequestAccept,
            "네, 괜찮아요. (피를 홍차에 넣어도 된다고 수락한다)", "stepmother", "", "blood_request_accept_stepmother_leaves.json");
        
        DrawStepButton("5. 수면제 획득", 4, TestStep.Step5_AcquireSleepingPill,
            "수면제를 꺼낸다.", "", "industrial_sedative", "sleeping_pill_acquisition.json");
        
        DrawStepButton("6. 홍차에 수면제 투여", 5, TestStep.Step6_TeaWithSleepingPill,
            "홍차에 수면제를 몰래 탄다", "", "earl_grey_tea", "tea_with_sleeping_pill.json", "industrial_sedative");
        
        DrawStepButton("7. 밤의 대화 (가족 수면)", 6, TestStep.Step7_NightDialogue,
            null, null, null, "night_dialogue_family_asleep.json");
        
        DrawStepButton("8. 비밀 열쇠 획득 (거실 액자)", 7, TestStep.Step8_SecretKeyFromPhoto,
            "거실 액자를 살펴보며 비밀 열쇠를 찾는다", "", "livingroom_photo", "secret_key_living_room.json");
        
        DrawStepButton("9. 탈출 엔딩", 8, TestStep.Step9_EscapeEnding,
            "개구멍의 자물쇠에 열쇠를 꽂아 연다", "", "hole", "ending_stealth_exit.json", "secret_key");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("상태 메시지:", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(statusMessage, MessageType.None);

        EditorGUILayout.EndScrollView();
    }

    private void DrawStepButton(string label, int stepIndex, TestStep stepType,
        string chatInput, string npcName, string itemName, string jsonFileName, string itemName2 = null)
    {
        EditorGUILayout.BeginHorizontal();
        
        // 체크박스 (완료 상태 표시)
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.Toggle(stepCompleted[stepIndex], GUILayout.Width(20));
        EditorGUI.EndDisabledGroup();
        
        // 라벨
        EditorGUILayout.LabelField(label, GUILayout.Width(200));
        
        // 버튼
        EditorGUI.BeginDisabledGroup(stepInProgress[stepIndex]);
        
        // 실행 버튼
        string buttonText = stepCompleted[stepIndex] ? "다시 실행" : "실행";
        if (GUILayout.Button(buttonText, GUILayout.Width(80)))
        {
            ExecuteSingleStep(stepIndex, stepType, chatInput, npcName, itemName, jsonFileName, itemName2);
        }
        
        EditorGUI.EndDisabledGroup();
        
        // 진행 중 표시
        if (stepInProgress[stepIndex])
        {
            EditorGUILayout.LabelField("진행 중...", EditorStyles.miniLabel);
        }
        
        EditorGUILayout.EndHorizontal();
    }

    private void ResetTest()
    {
        currentStep = TestStep.NotStarted;
        for (int i = 0; i < StepCount; i++)
        {
            stepCompleted[i] = false;
            stepInProgress[i] = false;
        }
        statusMessage = "테스트가 리셋되었습니다. 각 단계의 버튼을 클릭하여 실행하세요.";
        currentCoroutine = null;
    }

    private void ExecuteSingleStep(int stepIndex, TestStep stepType, string chatInput,
        string npcName, string itemName, string jsonFileName, string itemName2 = null)
    {
        if (stepInProgress[stepIndex])
            return;

        ApiClient apiClient = Object.FindFirstObjectByType<ApiClient>();
        if (apiClient == null)
        {
            statusMessage = "오류: ApiClient를 찾을 수 없습니다.";
            return;
        }

        GameStateManager gameStateManager = GameStateManager.Instance;
        if (gameStateManager == null)
        {
            statusMessage = "오류: GameStateManager.Instance를 찾을 수 없습니다.";
            return;
        }

        stepInProgress[stepIndex] = true;
        currentStep = stepType;
        statusMessage = $"단계 {stepIndex + 1} 실행 중...";

        // 단계별 검증 콜백
        System.Action onSuccess = () =>
        {
            stepCompleted[stepIndex] = true;
            stepInProgress[stepIndex] = false;
            
            // 단계별 검증 (6. 홍차에 수면제 투여)
            if (stepIndex == 5)
            {
                bool flagSet = gameStateManager.GetEventFlag("teaWithSleepingPill");
                if (!flagSet)
                {
                    statusMessage = "경고: tea_with_sleeping_pill 플래그가 설정되지 않았습니다.";
                }
                else
                {
                    statusMessage = $"단계 {stepIndex + 1} 완료!";
                }
            }
            else
            {
                statusMessage = $"단계 {stepIndex + 1} 완료!";
            }
        };

        System.Action onError = () =>
        {
            stepInProgress[stepIndex] = false;
            statusMessage = $"단계 {stepIndex + 1} 실행 실패!";
        };

        // 코루틴 실행
        currentCoroutine = ExecuteStep(
            apiClient,
            chatInput,
            npcName,
            itemName,
            jsonFileName,
            stepIndex,
            onSuccess,
            onError,
            itemName2
        );
    }

    private IEnumerator ExecuteStep(
        ApiClient apiClient,
        string chatInput,
        string npcName,
        string itemName,
        string jsonFileName,
        int stepIndex,
        System.Action onSuccess,
        System.Action onError,
        string itemName2 = null)
    {
        // JSON 파일 로드
        string jsonPath = $"Assets/TestData/Scenarios/{jsonFileName}";
        TextAsset jsonAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(jsonPath);
        
        if (jsonAsset == null)
        {
            statusMessage = $"오류: JSON 파일을 찾을 수 없습니다: {jsonPath}";
            onError?.Invoke();
            yield break;
        }

        // 밤의 대화 단계인 경우 (stepIndex == 6) 특별 처리
        if (stepIndex == 6)
        {
            // NightDialogueApiClient.BackendNightDialogueResponse로 파싱
            NightDialogueApiClient.BackendNightDialogueResponse nightResponse;
            try
            {
                nightResponse = JsonConvert.DeserializeObject<NightDialogueApiClient.BackendNightDialogueResponse>(jsonAsset.text);
            }
            catch (System.Exception e)
            {
                statusMessage = $"오류: JSON 파싱 실패: {e.Message}";
                onError?.Invoke();
                yield break;
            }

            // NightDialogueManager 찾기 및 테스트 데이터 로드
            NightDialogueManager nightDialogueManager = Object.FindFirstObjectByType<NightDialogueManager>();
            
            // NightDialogueManager가 없으면 Night 씬으로 전환
            if (nightDialogueManager == null)
            {
                string nightSceneName = "Night";
                string currentSceneName = SceneManager.GetActiveScene().name;
                
                if (currentSceneName != nightSceneName)
                {
                    Debug.Log($"[SleepingPillEscapeTestTool] NightDialogueManager를 찾을 수 없습니다. Night 씬으로 전환합니다. (현재 씬: {currentSceneName})");
                    statusMessage = $"Night 씬으로 전환 중...";
                    
                    // 씬 전환
                    SceneManager.LoadScene(nightSceneName);
                    
                    // 씬 전환 후 대기 (씬이 완전히 로드될 때까지)
                    yield return new WaitForSeconds(1.0f);
                    
                    // 다시 NightDialogueManager 찾기
                    nightDialogueManager = Object.FindFirstObjectByType<NightDialogueManager>();
                    
                    if (nightDialogueManager == null)
                    {
                        statusMessage = "오류: Night 씬 전환 후에도 NightDialogueManager를 찾을 수 없습니다.";
                        Debug.LogError("[SleepingPillEscapeTestTool] Night 씬 전환 후에도 NightDialogueManager를 찾을 수 없습니다.");
                        onError?.Invoke();
                        yield break;
                    }
                    
                    // 즉시 테스트 모드 설정 (API 요청 차단)
                    nightDialogueManager.SetTestMode(true);
                    Debug.Log("[SleepingPillEscapeTestTool] Night 씬 전환 완료. NightDialogueManager를 찾았고 테스트 모드를 설정했습니다.");
                }
                else
                {
                    statusMessage = "오류: Night 씬에 있지만 NightDialogueManager를 찾을 수 없습니다.";
                    Debug.LogError("[SleepingPillEscapeTestTool] Night 씬에 있지만 NightDialogueManager를 찾을 수 없습니다.");
                    onError?.Invoke();
                    yield break;
                }
            }
            
            // NightDialogueManager에 테스트 데이터 로드
            if (nightDialogueManager != null)
            {
                nightDialogueManager.SetTestMode(true);
                nightDialogueManager.LoadTestDialogue(nightResponse);
                Debug.Log("[SleepingPillEscapeTestTool] 밤의 대화 테스트 데이터 로드 완료");
            }

            // BackendResponseConverter를 사용하여 응답 변환 (상태 적용용)
            BackendResponseConverter nightConverter = new BackendResponseConverter(ApiClient.MOCK_RESPONSE);
            nightConverter.ConvertBackendResponseToCurrentFormat(
                nightResponse,
                GameStateManager.Instance,
                out string nightNarrative,
                out float nightHumanityChange,
                out NPCAffectionChanges nightAffectionChanges,
                out NPCHumanityChanges nightHumanityChanges,
                out NPCDisabledStates nightDisabledStates,
                out ItemChanges nightItemChanges,
                out EventFlags nightEventFlags,
                out string nightEndingTrigger
            );

            // InputHandler를 통해 InputFieldManager에 접근하여 ResultText 표시
            InputHandler nightInputHandler = Object.FindFirstObjectByType<InputHandler>();
            InputFieldManager nightInputFieldManager = null;
            
            if (nightInputHandler != null)
            {
                FieldInfo fieldInfo = typeof(InputHandler).GetField("inputFieldManager", BindingFlags.NonPublic | BindingFlags.Instance);
                if (fieldInfo != null)
                {
                    nightInputFieldManager = fieldInfo.GetValue(nightInputHandler) as InputFieldManager;
                }
            }
            
            if (nightInputFieldManager != null)
            {
                // 테스트 모드에서는 narrative만 표시 (JSON Response 제거)
                string displayText = nightNarrative;
                
                nightInputFieldManager.ShowResultText();
                nightInputFieldManager.SetResultText(displayText);
                
                Debug.Log($"[SleepingPillEscapeTestTool] ResultText에 밤의 대화 응답 표시 완료");
            }

            // 상태 적용
            GameStateManager nightManager = GameStateManager.Instance;
            if (nightManager != null)
            {
                GameStateApplier.ApplyHumanityChange(nightManager, nightHumanityChange);
                
                if (nightAffectionChanges != null)
                    NPCStateApplier.ApplyAffectionChanges(nightManager, nightAffectionChanges);
                
                if (nightHumanityChanges != null)
                    NPCStateApplier.ApplyHumanityChanges(nightManager, nightHumanityChanges);
                
                if (nightDisabledStates != null)
                    NPCStateApplier.ApplyDisabledStates(nightManager, nightDisabledStates);
                
                if (nightItemChanges != null)
                    ItemStateApplier.ApplyItemChanges(nightManager, nightItemChanges);
                
                if (nightEventFlags != null)
                    EventFlagApplier.ApplyEventFlags(nightManager, nightEventFlags);
                
                if (!string.IsNullOrEmpty(nightEndingTrigger))
                {
                    bool endingTriggered = GameStateApplier.ApplyEndingTrigger(nightManager, nightEndingTrigger);
                    if (endingTriggered)
                    {
                        Debug.Log($"[SleepingPillEscapeTestTool] 엔딩 트리거됨: {nightEndingTrigger}");
                        onSuccess?.Invoke();
                        yield break;
                    }
                }
            }

            // 턴 소모
            RoomTurnManager nightRoomTurnManager = Object.FindFirstObjectByType<RoomTurnManager>();
            if (nightRoomTurnManager != null)
            {
                int turnAfter = nightResponse.debug?.turn_after ?? 0;
                if (turnAfter > 0)
                {
                    for (int i = 0; i < turnAfter; i++)
                    {
                        nightRoomTurnManager.ConsumeTurn();
                        yield return new WaitForSeconds(0.1f);
                    }
                    Debug.Log($"[SleepingPillEscapeTestTool] 턴 {turnAfter}개 소모 완료. 남은 턴수: {nightRoomTurnManager.GetRemainingTurns()}");
                }
            }
            else if (nightManager != null)
            {
                int turnAfter = nightResponse.debug?.turn_after ?? 0;
                if (turnAfter > 0)
                {
                    nightManager.ConsumeTurn(turnAfter);
                    Debug.Log($"[SleepingPillEscapeTestTool] 턴 {turnAfter}개 소모 완료. 남은 턴수: {nightManager.GetRemainingTurns()}");
                }
            }

            Debug.Log($"[SleepingPillEscapeTestTool] 밤의 대화 단계 완료: {nightNarrative}");
            yield return new WaitForSeconds(0.5f);
            onSuccess?.Invoke();
            yield break;
        }

        // 일반 단계 처리 (기존 로직)
        BackendGameResponse response;
        try
        {
            response = JsonConvert.DeserializeObject<BackendGameResponse>(jsonAsset.text);
        }
        catch (System.Exception e)
        {
            statusMessage = $"오류: JSON 파싱 실패: {e.Message}";
            onError?.Invoke();
            yield break;
        }

        // BackendResponseConverter를 사용하여 응답 변환
        BackendResponseConverter converter = new BackendResponseConverter(ApiClient.MOCK_RESPONSE);
        converter.ConvertBackendResponseToCurrentFormat(
            response,
            GameStateManager.Instance,
            out string narrative,
            out float humanityChange,
            out NPCAffectionChanges affectionChanges,
            out NPCHumanityChanges humanityChanges,
            out NPCDisabledStates disabledStates,
            out ItemChanges itemChanges,
            out EventFlags eventFlags,
            out string endingTrigger
        );

        // InputHandler를 통해 InputFieldManager에 접근하여 ResultText 표시
        InputHandler inputHandler = Object.FindFirstObjectByType<InputHandler>();
        InputFieldManager inputFieldManager = null;
        
        if (inputHandler != null)
        {
            // 리플렉션을 사용하여 InputHandler의 private inputFieldManager 필드에 접근
            FieldInfo fieldInfo = typeof(InputHandler).GetField("inputFieldManager", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo != null)
            {
                inputFieldManager = fieldInfo.GetValue(inputHandler) as InputFieldManager;
            }
        }
        
        // InputFieldManager를 통해 ResultText 표시
        if (inputFieldManager != null)
        {
            // 테스트 모드에서는 narrative만 표시 (JSON Response 제거)
            string displayText = narrative;
            
            // InputFieldManager를 통해 ResultText 표시 (InputField 비활성화, ResultText 활성화)
            inputFieldManager.ShowResultText();
            inputFieldManager.SetResultText(displayText);
            
            Debug.Log($"[SleepingPillEscapeTestTool] ResultText에 응답 표시 완료 (InputFieldManager 사용)");
        }
        else
        {
            // InputFieldManager를 찾지 못한 경우 직접 찾기 시도
            TMPro.TextMeshProUGUI resultText = null;
            
            // Canvas 하위에서 찾기
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                TMPro.TextMeshProUGUI[] texts = canvas.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true); // 비활성화된 것도 포함
                foreach (var text in texts)
                {
                    if (text.gameObject.name.Contains("Result") || text.gameObject.name == "ResultText")
                    {
                        resultText = text;
                        break;
                    }
                }
            }
            
            if (resultText != null)
            {
                // 테스트 모드에서는 narrative만 표시 (JSON Response 제거)
                resultText.text = narrative;
                
                // ResultText를 활성화하고 InputField 비활성화
                resultText.gameObject.SetActive(true);
                
                // InputField 찾아서 비활성화
                TMP_InputField inputField = Object.FindFirstObjectByType<TMP_InputField>();
                if (inputField != null)
                {
                    inputField.gameObject.SetActive(false);
                }
                
                Debug.Log($"[SleepingPillEscapeTestTool] ResultText에 응답 표시 완료 (직접 접근)");
            }
            else
            {
                Debug.LogWarning("[SleepingPillEscapeTestTool] InputFieldManager와 ResultText를 찾을 수 없습니다. Scene에 InputHandler가 있는지 확인하세요.");
            }
        }

        // 상태 적용 (ApiResponseHandler와 동일한 로직)
        GameStateManager manager = GameStateManager.Instance;
        if (manager != null)
        {
            GameStateApplier.ApplyHumanityChange(manager, humanityChange);
            
            if (affectionChanges != null)
                NPCStateApplier.ApplyAffectionChanges(manager, affectionChanges);
                
            if (humanityChanges != null)
                NPCStateApplier.ApplyHumanityChanges(manager, humanityChanges);
            
            if (disabledStates != null)
                NPCStateApplier.ApplyDisabledStates(manager, disabledStates);
            
            if (itemChanges != null)
                ItemStateApplier.ApplyItemChanges(manager, itemChanges);
            
            if (eventFlags != null)
                EventFlagApplier.ApplyEventFlags(manager, eventFlags);
            
            if (!string.IsNullOrEmpty(endingTrigger))
            {
                bool endingTriggered = GameStateApplier.ApplyEndingTrigger(manager, endingTrigger);
                if (endingTriggered)
                {
                    Debug.Log($"[SleepingPillEscapeTestTool] 엔딩 트리거됨: {endingTrigger}");
                    // 엔딩 진입 시 더 이상 처리하지 않음
                    onSuccess?.Invoke();
                    yield break;
                }
            }
        }

        // 턴 소모 (RoomTurnManager 또는 TurnManager 찾기)
        RoomTurnManager roomTurnManager = Object.FindFirstObjectByType<RoomTurnManager>();
        if (roomTurnManager != null)
        {
            // debug.turn_after 값을 사용하여 턴 차감
            int turnAfter = response.debug?.turn_after ?? 1;
            if (turnAfter > 0)
            {
                for (int i = 0; i < turnAfter; i++)
                {
                    roomTurnManager.ConsumeTurn();
                    yield return new WaitForSeconds(0.1f); // 턴 차감 간격
                }
                Debug.Log($"[SleepingPillEscapeTestTool] 턴 {turnAfter}개 소모 완료. 남은 턴수: {roomTurnManager.GetRemainingTurns()}");
            }
        }
        else
        {
            // RoomTurnManager가 없으면 GameStateManager의 TurnManager 사용
            if (manager != null)
            {
                int turnAfter = response.debug?.turn_after ?? 1;
                if (turnAfter > 0)
                {
                    manager.ConsumeTurn(turnAfter);
                    Debug.Log($"[SleepingPillEscapeTestTool] 턴 {turnAfter}개 소모 완료. 남은 턴수: {manager.GetRemainingTurns()}");
                }
            }
        }

        Debug.Log($"[SleepingPillEscapeTestTool] 단계 {stepIndex + 1} 완료: {narrative}");
        
        // 약간의 대기 시간 (상태 적용이 완료되도록)
        yield return new WaitForSeconds(0.5f);
        
        onSuccess?.Invoke();
    }
}


