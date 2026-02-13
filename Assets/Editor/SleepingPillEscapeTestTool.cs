using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using System.IO;

/// <summary>
/// 수면제 탈출 원 사이클 테스트를 위한 Unity Editor Tool입니다.
/// Play 모드에서 전체 테스트 사이클을 수동으로 실행합니다.
/// </summary>
public class SleepingPillEscapeTestTool : EditorWindow
{
    private enum TestStep
    {
        NotStarted,
        Step1_AcquireSleepingPill,
        Step2_MotherAffection,
        Step3_TeaWithSleepingPill,
        Step4_NightDialogue,
        Step5_StealKey,
        Step6_EscapeEnding,
        Completed,
        Failed
    }

    private TestStep currentStep = TestStep.NotStarted;
    private bool[] stepCompleted = new bool[6];
    private bool[] stepInProgress = new bool[6];
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
        
        // 각 단계별 버튼과 상태 표시
        DrawStepButton("1. 수면제 획득", 0, TestStep.Step1_AcquireSleepingPill,
            "주방 찬장에서 수면제를 찾는다", "", "", "sleeping_pill_acquisition.json");
        
        DrawStepButton("2. 새엄마 호감도 상승", 1, TestStep.Step2_MotherAffection,
            "엄마, 오늘도 정말 맛있는 식사를 준비해주셔서 고마워요. 이 집이 정말 따뜻하고 좋아요.",
            "new_mother", "", "mother_affection_boost.json");
        
        DrawStepButton("3. 홍차에 수면제 투여", 2, TestStep.Step3_TeaWithSleepingPill,
            "홍차에 수면제를 탄다", "", "sleeping_pill", "tea_with_sleeping_pill.json");
        
        DrawStepButton("4. 밤의 대화 (무력화 확인)", 3, TestStep.Step4_NightDialogue,
            null, null, null, "night_dialogue_family_asleep.json", true); // 스킵 가능한 단계
        
        DrawStepButton("5. 열쇠 획득", 4, TestStep.Step5_StealKey,
            "잠든 엄마의 목걸이에서 열쇠를 훔친다", "new_mother", "", "master_key_stolen.json");
        
        DrawStepButton("6. 탈출 엔딩", 5, TestStep.Step6_EscapeEnding,
            "개구멍의 문을 열고 탈출한다", "", "master_key", "ending_stealth_exit.json");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("상태 메시지:", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(statusMessage, MessageType.None);

        EditorGUILayout.EndScrollView();
    }

    private void DrawStepButton(string label, int stepIndex, TestStep stepType,
        string chatInput, string npcName, string itemName, string jsonFileName, bool isSkipStep = false)
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
        
        if (isSkipStep)
        {
            // 스킵 가능한 단계 (밤의 대화)
            if (GUILayout.Button("스킵", GUILayout.Width(60)))
            {
                stepCompleted[stepIndex] = true;
                statusMessage = $"{label} 단계를 스킵했습니다. (NightDialogueManager에서 자동 처리됨)";
            }
        }
        else
        {
            // 실행 버튼
            string buttonText = stepCompleted[stepIndex] ? "다시 실행" : "실행";
            if (GUILayout.Button(buttonText, GUILayout.Width(80)))
            {
                ExecuteSingleStep(stepIndex, stepType, chatInput, npcName, itemName, jsonFileName);
            }
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
        for (int i = 0; i < stepCompleted.Length; i++)
        {
            stepCompleted[i] = false;
            stepInProgress[i] = false;
        }
        statusMessage = "테스트가 리셋되었습니다. 각 단계의 버튼을 클릭하여 실행하세요.";
        currentCoroutine = null;
    }

    private void ExecuteSingleStep(int stepIndex, TestStep stepType, string chatInput, 
        string npcName, string itemName, string jsonFileName)
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
            
            // 단계별 검증
            if (stepIndex == 2) // 홍차에 수면제 투여
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
            onError
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
        System.Action onError)
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

        // JSON 파싱
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
            out string endingTrigger,
            out Dictionary<string, bool> locks
        );

        // ResultText 찾기 및 JSON 응답 표시
        TMPro.TextMeshProUGUI resultText = null;
        
        // InputHandler를 통해 resultText 찾기
        InputHandler inputHandler = Object.FindFirstObjectByType<InputHandler>();
        if (inputHandler != null)
        {
            // InputHandler의 resultText는 private이므로 리플렉션 사용 또는 다른 방법 필요
            // 일단 GameObject.Find로 시도
            GameObject resultTextObj = GameObject.Find("ResultText");
            if (resultTextObj != null)
            {
                resultText = resultTextObj.GetComponent<TMPro.TextMeshProUGUI>();
            }
        }
        
        // InputHandler가 없거나 찾지 못한 경우 직접 찾기
        if (resultText == null)
        {
            // Canvas 하위에서 찾기
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                TMPro.TextMeshProUGUI[] texts = canvas.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
                foreach (var text in texts)
                {
                    if (text.gameObject.name.Contains("Result") || text.gameObject.name == "ResultText")
                    {
                        resultText = text;
                        break;
                    }
                }
            }
        }
        
        if (resultText != null)
        {
            // JSON 응답을 포맷팅하여 표시
            string formattedJson = JsonConvert.SerializeObject(response, Formatting.Indented);
            resultText.text = $"{narrative}\n\n[JSON Response]\n{formattedJson}";
            
            // ResultText를 활성화 (InputFieldManager와 유사한 로직)
            resultText.gameObject.SetActive(true);
            
            Debug.Log($"[SleepingPillEscapeTestTool] ResultText에 응답 표시 완료");
        }
        else
        {
            Debug.LogWarning("[SleepingPillEscapeTestTool] ResultText를 찾을 수 없습니다. Scene에 ResultText가 있는지 확인하세요.");
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
            
            if (locks != null && locks.Count > 0)
                GameStateApplier.ApplyLocks(manager, locks);
            
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


