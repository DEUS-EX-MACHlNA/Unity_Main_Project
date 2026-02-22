using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using System.Reflection;
using TMPro;

/// <summary>
/// 혼돈의 밤 엔딩 원 사이클 테스트를 위한 Unity Editor Tool입니다.
/// 혼돈의 밤.md 스펙 기준. Play 모드에서 전체 테스트 사이클을 수동으로 실행합니다.
/// </summary>
public class ChaosNightTestTool : EditorWindow
{
    private enum TestStep
    {
        NotStarted,
        Step1_LighterAcquisition,
        Step2_OilBottleAcquisition,
        Step3_GrandmotherTalk,
        Step4_GrandmotherShareVitality,
        Step5_ChaoticBreakoutEnding,
        Completed,
        Failed
    }

    private const int StepCount = 5;
    private TestStep currentStep = TestStep.NotStarted;
    private bool[] stepCompleted = new bool[StepCount];
    private bool[] stepInProgress = new bool[StepCount];
    private string statusMessage = "테스트를 시작하려면 각 단계의 버튼을 클릭하세요.";
    private Vector2 scrollPosition;
    private IEnumerator currentCoroutine;

    [MenuItem("Tools/Test/Chaos Night (혼돈의 밤) Cycle")]
    public static void ShowWindow()
    {
        GetWindow<ChaosNightTestTool>("혼돈의 밤 테스트");
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
        GUILayout.Label("혼돈의 밤 엔딩 테스트", EditorStyles.boldLabel);
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

        EditorGUILayout.LabelField("테스트 단계 (혼돈의 밤.md):", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 조건 1 — 아이템 습득
        DrawStepButton("1. 안방에서 라이터 습득", 0, TestStep.Step1_LighterAcquisition,
            "안방에서 라이터를 집어 든다.", "", "", "lighter_acquisition.json");

        DrawStepButton("2. 지하실에서 기름병 습득", 1, TestStep.Step2_OilBottleAcquisition,
            "지하실에서 기름병을 찾아 든다.", "", "", "oil_bottle_acquisition.json");

        // 조건 2 — 할머니에게서 탈출 정보 후 엔딩
        DrawStepButton("3. 할머니에게 말 걸기", 2, TestStep.Step3_GrandmotherTalk,
            "할머니에게 다가가 말을 건다.", "grandmother", "", "grandmother_talk.json");

        DrawStepButton("4. 생기를 나누어 주기", 3, TestStep.Step4_GrandmotherShareVitality,
            "할머니에게 생기를 나누어 준다.", "grandmother", "", "grandmother_share_vitality.json");

        DrawStepButton("5. 새엄마에게 기름+라이터 → 엔딩", 4, TestStep.Step5_ChaoticBreakoutEnding,
            "새엄마에게 기름병을 던지고 라이터로 불을 붙인다.", "stepmother", "oil_bottle", "ending_chaotic_breakout.json", "silver_lighter");

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

        EditorGUILayout.LabelField(label, GUILayout.Width(240));

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

        stepInProgress[stepIndex] = true;
        currentStep = stepType;
        statusMessage = $"단계 {stepIndex + 1} 실행 중...";

        System.Action onSuccess = () =>
        {
            stepCompleted[stepIndex] = true;
            stepInProgress[stepIndex] = false;
            statusMessage = $"단계 {stepIndex + 1} 완료!";
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
        int turnAfter = response.debug?.turn_after ?? 1;
        if (roomTurnManager != null && turnAfter > 0)
        {
            for (int i = 0; i < turnAfter; i++)
            {
                roomTurnManager.ConsumeTurn();
                yield return new WaitForSeconds(0.1f);
            }
        }
        else if (manager != null && turnAfter > 0)
        {
            manager.ConsumeTurn(turnAfter);
        }

        yield return new WaitForSeconds(0.5f);
        onSuccess?.Invoke();
    }
}
