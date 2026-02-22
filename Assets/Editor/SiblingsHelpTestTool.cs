using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using System.Reflection;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// 조력자의 희생 엔딩 원 사이클 테스트를 위한 Unity Editor Tool입니다.
/// 조력자의 희생.md 스펙 기준. Play 모드에서 전체 테스트 사이클을 수동으로 실행합니다.
/// </summary>
public class SiblingsHelpTestTool : EditorWindow
{
    private enum TestStep
    {
        NotStarted,
        Step1_BaronTrustBoost,
        Step2_BaronGivesRealFamilyPhoto,
        Step3_SiblingShowRealFamilyPhoto,
        Step4_NightDialogueSiblingsHelp,
        Step5_SecretKeyFromLivingRoom,
        Step6_EscapeEnding,
        Completed,
        Failed
    }

    private const int StepCount = 6;
    private TestStep currentStep = TestStep.NotStarted;
    private bool[] stepCompleted = new bool[StepCount];
    private bool[] stepInProgress = new bool[StepCount];
    private string statusMessage = "테스트를 시작하려면 각 단계의 버튼을 클릭하세요.";
    private Vector2 scrollPosition;
    private IEnumerator currentCoroutine;

    [MenuItem("Tools/Test/Siblings Help (조력자의 희생) Cycle")]
    public static void ShowWindow()
    {
        GetWindow<SiblingsHelpTestTool>("조력자의 희생 테스트");
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
        GUILayout.Label("조력자의 희생 엔딩 테스트", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("이 테스트는 Play 모드에서만 실행할 수 있습니다. 먼저 Play 모드를 시작해주세요.", MessageType.Warning);
            if (GUILayout.Button("Play 모드 시작"))
            {
                EditorApplication.isPlaying = true;
            }
            return;
        }

        if (GUILayout.Button("테스트 리셋", GUILayout.Height(25)))
        {
            ResetTest();
        }

        EditorGUILayout.Space();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.LabelField("테스트 단계 (조력자의 희생.md):", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 조건 1 — 바론 호감도·액자
        DrawStepButton("1. 바론 돌봐 주기", 0, TestStep.Step1_BaronTrustBoost,
            "바론에게 다가가 살짝 쓰다듬어 준다.", "baron", "", "baron_trust_boost.json");

        DrawStepButton("2. 바론 호감도 최대 시 액자 건네줌", 1, TestStep.Step2_BaronGivesRealFamilyPhoto,
            "마당에서 바론과 놀아 준다.", "baron", "", "baron_gives_real_family_photo.json");

        // 조건 2 — 동생의 희생 & 탈출
        DrawStepButton("3. 동생에게 액자 보여 주기", 2, TestStep.Step3_SiblingShowRealFamilyPhoto,
            "동생에게 액자를 보여 준다.", "sibling", "real_family_photo", "sibling_show_real_family_photo.json");

        DrawStepButton("4. 밤의 대화 (동생 거짓말·내일 찾자)", 3, TestStep.Step4_NightDialogueSiblingsHelp,
            null, null, null, "night_dialogue_siblings_help.json");

        DrawStepButton("5. 거실에서 비밀 열쇠 획득", 4, TestStep.Step5_SecretKeyFromLivingRoom,
            "거실에서 액자를 탐색해 비밀 열쇠를 찾는다.", "", "livingroom_photo", "secret_key_siblings_help.json");

        DrawStepButton("6. 개구멍 탈출 엔딩", 5, TestStep.Step6_EscapeEnding,
            "개구멍의 자물쇠에 열쇠를 꽂아 연다.", "", "hole", "ending_siblings_help.json", "secret_key");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("상태 메시지:", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(statusMessage, MessageType.None);

        EditorGUILayout.EndScrollView();
    }

    private void DrawStepButton(string label, int stepIndex, TestStep stepType,
        string chatInput, string npcName, string itemName, string jsonFileName, string itemName2 = null)
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.Toggle(stepCompleted[stepIndex], GUILayout.Width(20));
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.LabelField(label, GUILayout.Width(220));

        EditorGUI.BeginDisabledGroup(stepInProgress[stepIndex]);
        string buttonText = stepCompleted[stepIndex] ? "다시 실행" : "실행";
        if (GUILayout.Button(buttonText, GUILayout.Width(80)))
        {
            ExecuteSingleStep(stepIndex, stepType, chatInput, npcName, itemName, jsonFileName, itemName2);
        }
        EditorGUI.EndDisabledGroup();

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

        if (Object.FindFirstObjectByType<ApiClient>() == null)
        {
            statusMessage = "오류: ApiClient를 찾을 수 없습니다.";
            return;
        }

        if (GameStateManager.Instance == null)
        {
            statusMessage = "오류: GameStateManager.Instance를 찾을 수 없습니다.";
            return;
        }

        GameStateManager gameStateManager = GameStateManager.Instance;
        stepInProgress[stepIndex] = true;
        currentStep = stepType;
        statusMessage = $"단계 {stepIndex + 1} 실행 중...";

        System.Action onSuccess = () =>
        {
            stepCompleted[stepIndex] = true;
            stepInProgress[stepIndex] = false;

            // 3. 동생에게 액자 보여줌 → sibling_escape_plan_agreed 플래그 검증
            if (stepIndex == 2)
            {
                bool flagSet = gameStateManager.GetCustomEvent("sibling_escape_plan_agreed");
                if (!flagSet)
                {
                    statusMessage = "경고: sibling_escape_plan_agreed 플래그가 설정되지 않았습니다.";
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

        currentCoroutine = ExecuteStep(chatInput, npcName, itemName, jsonFileName, stepIndex, onSuccess, onError, itemName2);
    }

    private IEnumerator ExecuteStep(
        string chatInput,
        string npcName,
        string itemName,
        string jsonFileName,
        int stepIndex,
        System.Action onSuccess,
        System.Action onError,
        string itemName2 = null)
    {
        string jsonPath = $"Assets/TestData/Scenarios/{jsonFileName}";
        TextAsset jsonAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(jsonPath);

        if (jsonAsset == null)
        {
            statusMessage = $"오류: JSON 파일을 찾을 수 없습니다: {jsonPath}";
            onError?.Invoke();
            yield break;
        }

        // 밤의 대화 단계 (stepIndex == 3)
        if (stepIndex == 3)
        {
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

            NightDialogueManager nightDialogueManager = Object.FindFirstObjectByType<NightDialogueManager>();

            if (nightDialogueManager == null)
            {
                string nightSceneName = "Night";
                string currentSceneName = SceneManager.GetActiveScene().name;

                if (currentSceneName != nightSceneName)
                {
                    statusMessage = "Night 씬으로 전환 중...";
                    SceneManager.LoadScene(nightSceneName);
                    yield return new WaitForSeconds(1.0f);
                    nightDialogueManager = Object.FindFirstObjectByType<NightDialogueManager>();

                    if (nightDialogueManager == null)
                    {
                        statusMessage = "오류: Night 씬 전환 후에도 NightDialogueManager를 찾을 수 없습니다.";
                        onError?.Invoke();
                        yield break;
                    }
                    nightDialogueManager.SetTestMode(true);
                }
                else
                {
                    statusMessage = "오류: Night 씬에 있지만 NightDialogueManager를 찾을 수 없습니다.";
                    onError?.Invoke();
                    yield break;
                }
            }

            if (nightDialogueManager != null)
            {
                nightDialogueManager.SetTestMode(true);
                nightDialogueManager.LoadTestDialogue(nightResponse);
            }

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

            InputHandler nightInputHandler = Object.FindFirstObjectByType<InputHandler>();
            InputFieldManager nightInputFieldManager = null;
            if (nightInputHandler != null)
            {
                FieldInfo fieldInfo = typeof(InputHandler).GetField("inputFieldManager", BindingFlags.NonPublic | BindingFlags.Instance);
                if (fieldInfo != null)
                    nightInputFieldManager = fieldInfo.GetValue(nightInputHandler) as InputFieldManager;
            }

            if (nightInputFieldManager != null)
            {
                nightInputFieldManager.ShowResultText();
                nightInputFieldManager.SetResultText(nightNarrative ?? "");
            }

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
                    if (GameStateApplier.ApplyEndingTrigger(nightManager, nightEndingTrigger))
                    {
                        onSuccess?.Invoke();
                        yield break;
                    }
                }
            }

            RoomTurnManager nightRoomTurnManager = Object.FindFirstObjectByType<RoomTurnManager>();
            int turnAfter = nightResponse.debug?.turn_after ?? 0;
            if (nightRoomTurnManager != null && turnAfter > 0)
            {
                for (int i = 0; i < turnAfter; i++)
                {
                    nightRoomTurnManager.ConsumeTurn();
                    yield return new WaitForSeconds(0.1f);
                }
            }
            else if (nightManager != null && turnAfter > 0)
            {
                nightManager.ConsumeTurn(turnAfter);
            }

            yield return new WaitForSeconds(0.5f);
            onSuccess?.Invoke();
            yield break;
        }

        // 일반 단계
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

        InputHandler inputHandler = Object.FindFirstObjectByType<InputHandler>();
        InputFieldManager inputFieldManager = null;
        if (inputHandler != null)
        {
            FieldInfo fieldInfo = typeof(InputHandler).GetField("inputFieldManager", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo != null)
                inputFieldManager = fieldInfo.GetValue(inputHandler) as InputFieldManager;
        }

        if (inputFieldManager != null)
        {
            inputFieldManager.ShowResultText();
            inputFieldManager.SetResultText(narrative ?? "");
        }
        else
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                TMPro.TextMeshProUGUI[] texts = canvas.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
                foreach (var text in texts)
                {
                    if (text.gameObject.name.Contains("Result") || text.gameObject.name == "ResultText")
                    {
                        text.text = narrative ?? "";
                        text.gameObject.SetActive(true);
                        TMP_InputField inputField = Object.FindFirstObjectByType<TMP_InputField>();
                        if (inputField != null)
                            inputField.gameObject.SetActive(false);
                        break;
                    }
                }
            }
        }

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
                if (GameStateApplier.ApplyEndingTrigger(manager, endingTrigger))
                {
                    onSuccess?.Invoke();
                    yield break;
                }
            }
        }

        RoomTurnManager roomTurnManager = Object.FindFirstObjectByType<RoomTurnManager>();
        int turnAfterNormal = response.debug?.turn_after ?? 1;
        if (roomTurnManager != null && turnAfterNormal > 0)
        {
            for (int i = 0; i < turnAfterNormal; i++)
            {
                roomTurnManager.ConsumeTurn();
                yield return new WaitForSeconds(0.1f);
            }
        }
        else if (manager != null && turnAfterNormal > 0)
        {
            manager.ConsumeTurn(turnAfterNormal);
        }

        yield return new WaitForSeconds(0.5f);
        onSuccess?.Invoke();
    }
}
