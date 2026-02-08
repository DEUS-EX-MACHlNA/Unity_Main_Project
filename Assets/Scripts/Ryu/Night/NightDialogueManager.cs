using System.Collections;
using UnityEngine;
using TMPro;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    public string dialogue;
}

/// <summary>
/// Night 씬에서 가족들의 대화를 엿듣는 대화 시스템을 관리합니다.
/// </summary>
public class NightDialogueManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject clickHint;

    [Header("Dialogue Settings")]
    [SerializeField] private float typingSpeed = 0.05f; // 타이핑 효과 속도 (초)

    private DialogueLine[] dialogues;
    private int currentDialogueIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private string accumulatedText = ""; // 누적된 대사 텍스트

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
        InitializeDialogues();
        SetupUILayout();
        
        // 누적 텍스트 초기화
        accumulatedText = "";
        
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        ShowDialogue(0);
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

    private void InitializeDialogues()
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
        Debug.Log("[NightDialogueManager] 모든 대사 표시 완료");
        // TODO: 다음 동작 (씬 전환 또는 대기)
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
