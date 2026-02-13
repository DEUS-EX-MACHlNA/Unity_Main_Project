using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Newtonsoft.Json;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    public string dialogue;
}

/// <summary>
/// Night 씬에서 가족들의 대화를 엿듣는 대화 시스템을 관리합니다.
/// 백엔드 API를 통해 동적으로 대화를 가져오고 상태를 적용합니다.
/// </summary>
public class NightDialogueManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject clickHint;

    [Header("Scene Transition")]
    [SerializeField] private SceneFadeManager fadeManager;
    [SerializeField] private string nextSceneName = "PlayersRoom";
    [SerializeField] private float fadeDuration = 1f;

    [Header("Dialogue Settings")]
    [SerializeField] private float typingSpeed = 0.05f; // 타이핑 효과 속도 (초)

    [Header("API Settings")]
    [SerializeField] private string baseUrl = "https://d564-115-95-186-2.ngrok-free.app";
    [SerializeField] private float timeoutSeconds = 30f;

    private DialogueLine[] dialogues;
    private int currentDialogueIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private string accumulatedText = ""; // 누적된 대사 텍스트
    private NightDialogueApiClient apiClient;
    private bool isApiRequestInProgress = false;
    private bool isTestMode = false; // 테스트 모드 플래그
    private Coroutine apiRequestCoroutine; // API 요청 코루틴 추적용

    private void Awake()
    {
        // Inspector에서 설정되지 않은 경우 자동으로 찾기
        if (dialoguePanel == null)
        {
            dialoguePanel = GameObject.Find("DialoguePanel");
        }

        if (speakerNameText == null)
        {
            GameObject speakerObj = GameObject.Find("SpeakerNameText");
            if (speakerObj != null)
            {
                speakerNameText = speakerObj.GetComponent<TextMeshProUGUI>();
            }
        }

        if (dialogueText == null)
        {
            GameObject dialogueObj = GameObject.Find("DialogueText");
            if (dialogueObj != null)
            {
                dialogueText = dialogueObj.GetComponent<TextMeshProUGUI>();
            }
        }

        if (clickHint == null)
        {
            clickHint = GameObject.Find("ClickHint");
        }
    }

    private void Start()
    {
        SetupUILayout();
        
        // 누적 텍스트 초기화
        accumulatedText = "";
        
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        // 테스트 모드가 아닐 때만 API 호출
        if (!isTestMode)
        {
            InitializeApiClient();
            RequestNightDialogue();
        }
    }

    /// <summary>
    /// UI 초기 설정을 수행합니다. (레이아웃과 폰트는 Unity 에디터에서 설정)
    /// </summary>
    private void SetupUILayout()
    {
        // SpeakerNameText는 누적 방식에서는 사용하지 않음 (숨기기)
        if (speakerNameText != null)
        {
            speakerNameText.gameObject.SetActive(false);
        }

        // ClickHint 텍스트 설정
        if (clickHint != null)
        {
            TextMeshProUGUI hintText = clickHint.GetComponent<TextMeshProUGUI>();
            if (hintText != null)
            {
                hintText.text = "클릭하여 계속...";
            }
        }
    }

    /// <summary>
    /// API 클라이언트를 초기화합니다.
    /// </summary>
    private void InitializeApiClient()
    {
        // ApiClient에서 baseUrl과 gameId 가져오기 시도
        ApiClient apiClientComponent = FindFirstObjectByType<ApiClient>();
        if (apiClientComponent != null)
        {
            // ApiClient의 baseUrl을 가져오기 위해 리플렉션 사용 (또는 public 필드로 변경 필요)
            // 일단 Inspector에서 설정한 baseUrl 사용
        }

        // gameId를 가져오는 함수
        System.Func<int> getGameId = () =>
        {
            // ApiClient에서 gameId 가져오기 시도
            ApiClient client = FindFirstObjectByType<ApiClient>();
            if (client != null)
            {
                // ApiClient에 public getter가 있다면 사용
                // 일단 기본값 사용 (실제로는 ApiClient에서 가져와야 함)
            }
            return 24; // 기본값 (실제로는 ApiClient에서 가져와야 함)
        };

        apiClient = new NightDialogueApiClient(
            baseUrl,
            getGameId,
            timeoutSeconds,
            ApiClient.MOCK_RESPONSE
        );
    }

    /// <summary>
    /// 밤의 대화를 백엔드 API에서 요청합니다.
    /// </summary>
    private void RequestNightDialogue()
    {
        // 테스트 모드일 때는 API 요청 차단
        if (isTestMode)
        {
            Debug.Log("[NightDialogueManager] 테스트 모드입니다. API 요청을 차단합니다.");
            return;
        }

        if (isApiRequestInProgress)
        {
            Debug.LogWarning("[NightDialogueManager] API 요청이 이미 진행 중입니다.");
            return;
        }

        if (apiClient == null)
        {
            Debug.LogError("[NightDialogueManager] API 클라이언트가 초기화되지 않았습니다. 기본 대화를 사용합니다.");
            InitializeDefaultDialogues();
            ShowDialogue(0);
            return;
        }

        isApiRequestInProgress = true;
        apiRequestCoroutine = StartCoroutine(apiClient.RequestNightDialogueCoroutine(
            onSuccess: (backendDialogues, narrative, humanityChange, affectionChanges, humanityChanges, disabledStates, itemChanges, eventFlags, endingTrigger, locks) =>
            {
                isApiRequestInProgress = false;
                
                // 대화 배열 변환 (BackendDialogueLine[] → DialogueLine[])
                if (backendDialogues != null && backendDialogues.Length > 0)
                {
                    dialogues = new DialogueLine[backendDialogues.Length];
                    for (int i = 0; i < backendDialogues.Length; i++)
                    {
                        dialogues[i] = new DialogueLine
                        {
                            speakerName = backendDialogues[i].speaker_name,
                            dialogue = backendDialogues[i].dialogue
                        };
                    }
                }
                else
                {
                    // 대화가 없으면 기본 대화 사용
                    Debug.LogWarning("[NightDialogueManager] API 응답에 대화가 없습니다. 기본 대화를 사용합니다.");
                    InitializeDefaultDialogues();
                }

                // 상태 변화 적용
                ApplyStateChanges(humanityChange, affectionChanges, humanityChanges, disabledStates, itemChanges, eventFlags, endingTrigger, locks);

                // 첫 번째 대화 표시
                if (dialogues != null && dialogues.Length > 0)
                {
                    ShowDialogue(0);
                }
            },
            onError: (error) =>
            {
                isApiRequestInProgress = false;
                Debug.LogError($"[NightDialogueManager] 밤의 대화 요청 실패: {error}");
                Debug.Log("[NightDialogueManager] 기본 대화를 사용합니다.");
                
                // 에러 시 기본 대화 사용 (fallback)
                InitializeDefaultDialogues();
                ShowDialogue(0);
            }
        ));
    }

    /// <summary>
    /// 상태 변화를 적용합니다.
    /// </summary>
    private void ApplyStateChanges(
        float humanityChange,
        NPCAffectionChanges affectionChanges,
        NPCHumanityChanges humanityChanges,
        NPCDisabledStates disabledStates,
        ItemChanges itemChanges,
        EventFlags eventFlags,
        string endingTrigger,
        System.Collections.Generic.Dictionary<string, bool> locks)
    {
        GameStateManager manager = GameStateManager.Instance;
        if (manager == null)
        {
            Debug.LogWarning("[NightDialogueManager] GameStateManager.Instance를 찾을 수 없습니다.");
            return;
        }

        // 인간성 변화량 적용
        if (humanityChange != 0f)
        {
            GameStateApplier.ApplyHumanityChange(manager, humanityChange);
        }

        // NPC 호감도 변화량 적용
        if (affectionChanges != null)
        {
            NPCStateApplier.ApplyAffectionChanges(manager, affectionChanges);
        }

        // NPC 인간성 변화량 적용
        if (humanityChanges != null)
        {
            NPCStateApplier.ApplyHumanityChanges(manager, humanityChanges);
        }

        // NPC 무력화 상태 적용
        if (disabledStates != null)
        {
            NPCStateApplier.ApplyDisabledStates(manager, disabledStates);
        }

        // 아이템 변화량 적용
        if (itemChanges != null)
        {
            ItemStateApplier.ApplyItemChanges(manager, itemChanges);
        }

        // 이벤트 플래그 적용
        if (eventFlags != null)
        {
            EventFlagApplier.ApplyEventFlags(manager, eventFlags);
        }

        // 잠금 상태 적용
        if (locks != null && locks.Count > 0)
        {
            GameStateApplier.ApplyLocks(manager, locks);
        }

        // 엔딩 트리거 처리
        if (!string.IsNullOrEmpty(endingTrigger))
        {
            bool endingTriggered = GameStateApplier.ApplyEndingTrigger(manager, endingTrigger);
            if (endingTriggered)
            {
                Debug.Log("[NightDialogueManager] 엔딩이 트리거되었습니다.");
                // 엔딩 진입 시 더 이상 처리하지 않음 (씬 전환됨)
                return;
            }
        }
    }

    /// <summary>
    /// 기본 대화를 초기화합니다 (fallback용).
    /// </summary>
    private void InitializeDefaultDialogues()
    {
        dialogues = new DialogueLine[]
        {
            new DialogueLine
            {
                speakerName = "엘리노어 (새엄마)",
                dialogue = "오늘 우리 아이가 보여준 미소 보셨나요? 드디어 이 집의 향기에 적응한 것 같아 마음이 놓여요. 식탁 예절도 몰라보게 우아해졌더군요."
            },
            new DialogueLine
            {
                speakerName = "루카스 (동생)",
                dialogue = "응, 응! 오늘 누나(형)가 나랑 같이 인형 집으로 한참 동안 놀아줬어. 예전처럼 자꾸 나가려고 하지도 않고... 이제 우리 진짜 가족이 된 거지, 엄마?"
            },
            new DialogueLine
            {
                speakerName = "아더 (새아빠)",
                dialogue = "음, 확실히 소란을 피우지 않더군. 낮 동안 방 안에서 얌전히 지내는 걸 확인했소. 탈출하려는 헛된 시도는 이제 포기한 모양이지."
            },
            new DialogueLine
            {
                speakerName = "엘리노어 (새엄마)",
                dialogue = "어머, 아더도 참. 포기가 아니라 '순응'이라고 해주세요. 내일은 아이가 좋아하는 달콤한 홍차를 더 준비해야겠어요. 인간성이 조금 더 옅어지면, 그땐 정말 완벽한 우리 집 인형이 되겠네요."
            },
            new DialogueLine
            {
                speakerName = "루카스 (동생)",
                dialogue = "히히, 좋아! 내일은 누나(형)한테 내 소중한 지도가 어디 있는지 알려줄까 봐. 이제 도망 안 갈 거니까 괜찮지?"
            },
            new DialogueLine
            {
                speakerName = "아더 (새아빠)",
                dialogue = "너무 방심하진 마라, 루카스. 하지만 오늘처럼만 행동한다면 내일은 정원 산책 정도는 허락해 줄 수도 있겠군."
            }
        };
    }

    private void ShowDialogue(int index)
    {
        if (index < 0 || index >= dialogues.Length)
        {
            Debug.LogWarning($"[NightDialogueManager] 잘못된 대사 인덱스: {index}");
            return;
        }

        DialogueLine line = dialogues[index];
        currentDialogueIndex = index;

        // 새 대사를 누적 텍스트에 추가 (형식: "화자 이름: 대사 내용\n\n")
        string newDialogue = $"{line.speakerName}: {line.dialogue}\n\n";
        
        // 대사 타이핑 효과로 표시 (기존 텍스트는 유지하고 새 대사만 타이핑)
        if (dialogueText != null)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            typingCoroutine = StartCoroutine(TypeDialogueAccumulated(newDialogue));
        }

        // 클릭 힌트 숨기기 (타이핑 중)
        if (clickHint != null)
        {
            clickHint.SetActive(false);
        }

        Debug.Log($"[NightDialogueManager] 대사 {index + 1}/{dialogues.Length} 추가: {line.speakerName}");
    }

    /// <summary>
    /// 누적 방식으로 대사를 타이핑합니다. 기존 텍스트는 유지하고 새 대사만 추가합니다.
    /// </summary>
    private IEnumerator TypeDialogueAccumulated(string newDialogue)
    {
        isTyping = true;
        
        // 기존 누적 텍스트는 유지하고, 새 대사만 문자 단위로 추가
        string currentNewText = "";
        
        foreach (char c in newDialogue)
        {
            currentNewText += c;
            dialogueText.text = accumulatedText + currentNewText;
            yield return new WaitForSeconds(typingSpeed);
        }

        // 타이핑 완료 후 누적 텍스트에 추가
        accumulatedText += newDialogue;
        dialogueText.text = accumulatedText;
        isTyping = false;

        // 타이핑 완료 후 클릭 힌트 표시
        if (clickHint != null)
        {
            clickHint.SetActive(true);
        }
    }

    private void OnDialogueClick()
    {
        // dialogues가 null이거나 비어있으면 처리하지 않음
        if (dialogues == null || dialogues.Length == 0)
        {
            Debug.LogWarning("[NightDialogueManager] 대화 데이터가 아직 로드되지 않았습니다.");
            return;
        }

        if (isTyping)
        {
            // 타이핑 중이면 즉시 완성
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            if (currentDialogueIndex < dialogues.Length)
            {
                DialogueLine line = dialogues[currentDialogueIndex];
                string newDialogue = $"{line.speakerName}: {line.dialogue}\n\n";
                accumulatedText += newDialogue;
                dialogueText.text = accumulatedText;
                isTyping = false;
                if (clickHint != null)
                {
                    clickHint.SetActive(true);
                }
            }
            return;
        }

        // 다음 대사로 진행
        if (currentDialogueIndex < dialogues.Length - 1)
        {
            ShowDialogue(currentDialogueIndex + 1);
        }
        else
        {
            // 모든 대사 완료
            OnAllDialoguesComplete();
        }
    }

    private void OnAllDialoguesComplete()
    {
        Debug.Log("[NightDialogueManager] 모든 대사 표시 완료. 다음 날로 진행합니다.");

        // GameStateManager를 통해 다음 날 진행 (인간성 감소 포함)
        bool gameOverOccurred = false;
        if (GameStateManager.Instance != null)
        {
            gameOverOccurred = GameStateManager.Instance.AdvanceToNextDay();
        }
        else
        {
            Debug.LogWarning("[NightDialogueManager] GameStateManager.Instance를 찾을 수 없습니다.");
        }

        // 게임 오버가 발생했으면 씬 전환을 건너뜀 (TriggerGameOver에서 이미 처리됨)
        if (gameOverOccurred)
        {
            Debug.Log("[NightDialogueManager] 게임 오버가 발생했습니다. 씬 전환을 건너뜁니다.");
            return;
        }

        // PlayersRoom 씬으로 전환 (페이드 효과)
        if (fadeManager != null)
        {
            fadeManager.LoadSceneWithFade(nextSceneName, fadeDuration);
        }
        else
        {
            Debug.LogWarning("[NightDialogueManager] SceneFadeManager가 연결되지 않았습니다. 페이드 없이 씬을 전환합니다.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }

    /// <summary>
    /// 테스트 모드를 설정합니다. 테스트 모드일 때는 자동 API 호출을 하지 않습니다.
    /// </summary>
    public void SetTestMode(bool testMode)
    {
        isTestMode = testMode;
        Debug.Log($"[NightDialogueManager] 테스트 모드 설정: {testMode}");
        
        // 테스트 모드로 전환할 때 진행 중인 API 요청이 있다면 취소
        if (testMode && isApiRequestInProgress && apiRequestCoroutine != null)
        {
            Debug.Log("[NightDialogueManager] 테스트 모드 전환: 진행 중인 API 요청을 취소합니다.");
            StopCoroutine(apiRequestCoroutine);
            apiRequestCoroutine = null;
            isApiRequestInProgress = false;
        }
    }

    /// <summary>
    /// 테스트용 밤의 대화 데이터를 로드합니다. (JSON 응답에서)
    /// </summary>
    public void LoadTestDialogue(NightDialogueApiClient.BackendNightDialogueResponse response)
    {
        if (response == null)
        {
            Debug.LogError("[NightDialogueManager] 테스트 대화 데이터가 null입니다.");
            return;
        }

        // 대화 배열 변환 (BackendDialogueLine[] → DialogueLine[])
        if (response.dialogues != null && response.dialogues.Length > 0)
        {
            dialogues = new DialogueLine[response.dialogues.Length];
            for (int i = 0; i < response.dialogues.Length; i++)
            {
                dialogues[i] = new DialogueLine
                {
                    speakerName = response.dialogues[i].speaker_name,
                    dialogue = response.dialogues[i].dialogue
                };
            }
            Debug.Log($"[NightDialogueManager] 테스트 대화 데이터 로드 완료: {dialogues.Length}개 대화");
        }
        else
        {
            Debug.LogWarning("[NightDialogueManager] 테스트 응답에 대화가 없습니다. 기본 대화를 사용합니다.");
            InitializeDefaultDialogues();
        }

        // BackendResponseConverter를 사용하여 상태 변화 추출 및 적용
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

        // 상태 변화 적용
        ApplyStateChanges(humanityChange, affectionChanges, humanityChanges, disabledStates, itemChanges, eventFlags, endingTrigger, locks);

        Debug.Log($"[NightDialogueManager] 테스트 대화 상태 적용 완료: {narrative}");

        // 첫 번째 대화 표시 추가
        if (dialogues != null && dialogues.Length > 0)
        {
            ShowDialogue(0);
        }
    }

    private void Update()
    {
        // 마우스 클릭 또는 터치로 다음 대사 진행
        if (Input.GetMouseButtonDown(0))
        {
            OnDialogueClick();
        }
    }
}
