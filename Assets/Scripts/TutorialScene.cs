using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

// ==================== InputHandler ====================
public class InputHandler : MonoBehaviour, IPointerClickHandler
{
    public TMP_InputField myInputField; 
    public TextMeshProUGUI resultText;
    public ApiClient apiClient;
    public DayTurnUI dayTurnUI;
    public NpcStateManager npcStateManager;

    private bool isNightConversationInProgress = false;

    void Start()
    {
        if (dayTurnUI == null)
        {
            dayTurnUI = GetComponent<DayTurnUI>();
        }

        if (npcStateManager == null)
        {
            npcStateManager = NpcStateManager.Instance;
        }

        if (myInputField != null)
        {
            // InputField가 활성화되어 있는지 확인
            if (!myInputField.gameObject.activeSelf)
            {
                myInputField.gameObject.SetActive(true);
            }

            myInputField.onSubmit.AddListener(SubmitText);
            myInputField.onSelect.AddListener(OnInputFieldSelected);
            myInputField.onDeselect.AddListener(OnInputFieldDeselected);
            
            // InputField 텍스트 색상을 하얀색으로 설정
            if (myInputField.textComponent != null)
            {
                myInputField.textComponent.color = Color.white;
            }

            // InputField가 상호작용 가능한지 확인
            if (!myInputField.interactable)
            {
                myInputField.interactable = true;
            }

            Debug.Log("[InputHandler] InputField 초기화 완료");
        }
        else
        {
            Debug.LogError("[InputHandler] myInputField가 null입니다!");
        }

        // resultText에 클릭 이벤트를 위해 EventTrigger 추가
        if (resultText != null)
        {
            GameObject resultTextObj = resultText.gameObject;
            
            EventTrigger trigger = resultTextObj.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = resultTextObj.AddComponent<EventTrigger>();
            }

            // 기존 이벤트 제거 후 새로 추가
            trigger.triggers.Clear();
            
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((data) => { OnResultTextClicked(); });
            trigger.triggers.Add(entry);

            // 초기에는 resultText 비활성화 (InputField가 활성화되어야 함)
            resultText.gameObject.SetActive(false);
        }
    }

    void OnInputFieldSelected(string text)
    {
        // InputField가 활성화되면 resultText 비활성화
        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }
    }

    void OnInputFieldDeselected(string text)
    {
        // InputField 포커스 해제 시 특별한 처리 없음
        // SubmitText와 OnResultTextClicked에서 UI 토글 처리
    }

    void OnResultTextClicked()
    {
        // resultText 클릭 시 출력 종료하고 InputField 활성화
        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }

        if (myInputField != null)
        {
            myInputField.gameObject.SetActive(true);
            myInputField.ActivateInputField();
        }
    }

    // IPointerClickHandler 구현 (필요시 사용)
    public void OnPointerClick(PointerEventData eventData)
    {
        // 이 메서드는 필요시 사용할 수 있지만, EventTrigger로 처리 중
    }

    public void SubmitText(string input) 
    {
        // 밤의 대화 진행 중이면 입력 차단
        if (isNightConversationInProgress)
        {
            Debug.Log("[InputHandler] 밤의 대화 진행 중이므로 입력이 차단되었습니다.");
            return;
        }

        // 공백을 제외한 내용이 있을 때만 실행
        if (!string.IsNullOrWhiteSpace(input))
        {
            // InputField 비활성화하고 resultText 활성화
            if (myInputField != null)
            {
                myInputField.text = "";
                myInputField.DeactivateInputField();
                myInputField.gameObject.SetActive(false); // GameObject 자체를 비활성화
            }

            // 전송 중 표시
            if (resultText != null)
            {
                resultText.gameObject.SetActive(true);
                resultText.text = "전송 중...";
            }
            
            Debug.Log("입력 성공: " + input);

            // API로 메시지 전송
            if (apiClient != null)
            {
                apiClient.SendMessage(input, OnApiSuccess, OnApiError);
            }
            else
            {
                Debug.LogWarning("ApiClient가 연결되지 않았습니다.");
                if (resultText != null)
                {
                    resultText.text = "입력된 내용: " + input;
                }
            }
        }
    }

    private void OnApiSuccess(string response)
    {
        // 밤의 대화 진행 중이면 일반 응답 처리 안 함
        if (isNightConversationInProgress)
        {
            return;
        }

        if (resultText != null)
        {
            resultText.gameObject.SetActive(true);
            resultText.text = response;
        }

        // 대화(응답 수신) 1회마다 턴 1 소모
        if (dayTurnUI != null)
        {
            dayTurnUI.SetRemainingTurns(dayTurnUI.remainingTurns - 1);
            
            // 턴이 0이 되면 밤의 대화 트리거
            if (dayTurnUI.remainingTurns == 0)
            {
                CheckAndTriggerNight();
                return; // 밤의 대화 트리거 후 여기서 종료
            }
        }
        Debug.Log("서버 응답: " + response);
    }

    private void OnApiError(string error)
    {
        if (resultText != null)
        {
            resultText.gameObject.SetActive(true);
            resultText.text = "오류: " + error;
        }
        Debug.LogError("API 오류: " + error);
    }

    /// <summary>
    /// 턴이 0이 되었을 때 밤의 대화를 트리거합니다.
    /// </summary>
    private void CheckAndTriggerNight()
    {
        if (isNightConversationInProgress)
        {
            return;
        }

        if (apiClient == null || npcStateManager == null || dayTurnUI == null)
        {
            Debug.LogError("[InputHandler] 밤의 대화를 실행하기 위한 필수 컴포넌트가 없습니다.");
            return;
        }

        isNightConversationInProgress = true;

        // InputField 비활성화
        if (myInputField != null)
        {
            myInputField.gameObject.SetActive(false);
        }

        // 밤의 대화 요청 데이터 생성
        int initialTurns = 10; // 하루 시작 턴 수
        int turnSpentToday = initialTurns - dayTurnUI.remainingTurns;
        NightRequest requestData = npcStateManager.GetNightRequestData(dayTurnUI.day, turnSpentToday);

        // resultText에 로딩 메시지 표시
        if (resultText != null)
        {
            resultText.gameObject.SetActive(true);
            resultText.text = "밤의 대화를 진행 중...";
        }

        Debug.Log($"[InputHandler] 밤의 대화 트리거: Day {dayTurnUI.day}, 소모된 턴: {turnSpentToday}");

        // API 호출
        apiClient.RequestNightConversation(requestData, OnNightConversationSuccess, OnNightConversationError);
    }

    /// <summary>
    /// 밤의 대화 성공 시 호출됩니다.
    /// </summary>
    private void OnNightConversationSuccess(NightResponse response)
    {
        Debug.Log($"[InputHandler] 밤의 대화 성공: {response.nightId}");

        // resultText에 밤의 대화 로그 표시
        if (resultText != null)
        {
            resultText.gameObject.SetActive(true);
            
            // ui.resultText가 있으면 우선 사용, 없으면 exposedLog.lines 조합
            if (response.ui != null && !string.IsNullOrEmpty(response.ui.resultText))
            {
                resultText.text = response.ui.resultText;
            }
            else if (response.exposedLog != null && response.exposedLog.lines != null && response.exposedLog.lines.Length > 0)
            {
                string title = !string.IsNullOrEmpty(response.exposedLog.title) ? response.exposedLog.title + "\n\n" : "";
                resultText.text = title + string.Join("\n", response.exposedLog.lines);
            }
            else
            {
                resultText.text = "밤의 대화가 끝났습니다.";
            }
        }

        // NPC 상태 업데이트
        if (response.effects != null && response.effects.npcDeltas != null)
        {
            foreach (var npcDelta in response.effects.npcDeltas)
            {
                npcStateManager.UpdateNpcState(npcDelta.id, npcDelta.affectionDelta, npcDelta.humanityDelta);
                Debug.Log($"[InputHandler] NPC 상태 업데이트: {npcDelta.id}, 호감도: {npcDelta.affectionDelta}, 인간성: {npcDelta.humanityDelta}");
            }
        }

        // 플레이어 상태 업데이트
        if (response.effects != null && response.effects.player != null)
        {
            if (response.effects.player.humanityDelta != 0)
            {
                npcStateManager.UpdatePlayerHumanity(response.effects.player.humanityDelta);
                Debug.Log($"[InputHandler] 플레이어 인간성 변경: {response.effects.player.humanityDelta}");
            }

            if (response.effects.player.statusTagsAdded != null)
            {
                foreach (var tag in response.effects.player.statusTagsAdded)
                {
                    npcStateManager.AddPlayerFlag(tag);
                    Debug.Log($"[InputHandler] 플레이어 플래그 추가: {tag}");
                }
            }
        }

        // 다음 날로 넘어가기
        if (dayTurnUI != null)
        {
            // 턴 페널티 적용
            if (response.effects != null && response.effects.player != null)
            {
                int penalty = response.effects.player.turnPenaltyNextDay;
                if (penalty > 0)
                {
                    Debug.Log($"[InputHandler] 다음 날 턴 페널티: {penalty}");
                }
            }

            dayTurnUI.NextDay();
        }

        isNightConversationInProgress = false;

        // 다음 날 시작을 위해 resultText 비활성화하고 InputField 활성화
        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }

        if (myInputField != null)
        {
            myInputField.gameObject.SetActive(true);
            // 약간의 지연 후 활성화 (UI 업데이트 완료 대기)
            StartCoroutine(ActivateInputFieldDelayed());
        }
        else
        {
            Debug.LogWarning("[InputHandler] myInputField가 null입니다. InputField를 다시 활성화할 수 없습니다.");
        }
    }

    /// <summary>
    /// InputField를 약간의 지연 후 활성화합니다.
    /// </summary>
    private System.Collections.IEnumerator ActivateInputFieldDelayed()
    {
        yield return new WaitForSeconds(0.1f);
        if (myInputField != null)
        {
            myInputField.ActivateInputField();
        }
    }

    /// <summary>
    /// 밤의 대화 실패 시 호출됩니다.
    /// </summary>
    private void OnNightConversationError(string error)
    {
        Debug.LogError($"[InputHandler] 밤의 대화 오류: {error}");

        if (resultText != null)
        {
            resultText.gameObject.SetActive(true);
            resultText.text = $"밤의 대화 오류: {error}\n(클릭하여 계속)";
        }

        isNightConversationInProgress = false;

        // 에러 발생 시 InputField는 숨겨두고, resultText 클릭 시 활성화되도록 함
        if (myInputField != null)
        {
            myInputField.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// InputField에 블록을 추가합니다. GameObject 클릭 시 호출됩니다.
    /// </summary>
    /// <param name="blockName">추가할 블록 이름 (예: "StepMother")</param>
    public void AddBlockToInput(string blockName)
    {
        if (myInputField != null)
        {
            string blockText = $"@{blockName} ";
            string currentText = myInputField.text;
            
            // 커서 위치에 삽입하거나 텍스트 끝에 추가
            int caretPosition = myInputField.caretPosition;
            string newText = currentText.Insert(caretPosition, blockText);
            
            myInputField.text = newText;
            myInputField.caretPosition = caretPosition + blockText.Length;
            myInputField.ActivateInputField();
            
            Debug.Log($"블록 추가됨: {blockName}");
        }
    }
}

// ==================== DayTurnUI ====================
public class DayTurnUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI turnsText;

    [Header("Values")]
    [Min(1)] public int day = 1;
    [Min(0)] public int remainingTurns = 10;

    void Start()
    {
        Refresh();
    }

    public void SetDay(int newDay)
    {
        day = Mathf.Max(1, newDay);
        Refresh();
    }

    public void SetRemainingTurns(int turns)
    {
        remainingTurns = Mathf.Max(0, turns);
        Refresh();
    }

    public void Refresh()
    {
        if (dayText != null)
            dayText.text = $"Day {day:00}";

        if (turnsText != null)
            turnsText.text = $"남은 턴수 : {remainingTurns}";
    }

    /// <summary>
    /// 다음 날로 넘어가고 턴을 리셋합니다.
    /// </summary>
    public void NextDay()
    {
        day++;
        remainingTurns = 10;
        Refresh();
        Debug.Log($"[DayTurnUI] 다음 날로 넘어감: Day {day:00}, 턴 리셋: {remainingTurns}");
    }
}

// ==================== ClickableObject ====================
public class ClickableObject : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Input 연결")]
    public InputHandler inputHandler;
    
    [Header("블록 설정")]
    [Tooltip("블록 이름 (비워두면 GameObject 이름 사용)")]
    public string blockName;
    
    [Header("Hover 효과 설정")]
    [Tooltip("Hover 시 테두리 색상")]
    public Color hoverBorderColor = Color.white;
    
    [Tooltip("Hover 시 테두리 두께 (X, Y)")]
    public Vector2 hoverBorderThickness = new Vector2(5f, 5f);
    
    // 중복 호출 방지 (시간 기반)
    private float _lastClickTime = -1f;
    private const float CLICK_COOLDOWN = 0.1f; // 0.1초 내 중복 클릭 무시
    
    // Hover 효과 관련 컴포넌트
    private Outline _outlineComponent;
    private Image _imageComponent;
    private SpriteRenderer _spriteRenderer;
    private bool _isHovering = false;
    
    void Start()
    {
        // blockName이 비어있으면 GameObject 이름 사용
        if (string.IsNullOrEmpty(blockName))
        {
            blockName = gameObject.name;
        }
        
        // InputHandler를 자동으로 찾기 (선택사항)
        if (inputHandler == null)
        {
            inputHandler = FindFirstObjectByType<InputHandler>();
        }
        
        // Outline 컴포넌트 설정
        SetupOutlineComponent();
    }
    
    /// <summary>
    /// Outline 컴포넌트를 설정합니다. UI Image 또는 SpriteRenderer에 따라 다르게 처리합니다.
    /// </summary>
    private void SetupOutlineComponent()
    {
        // UI Image 컴포넌트 확인
        _imageComponent = GetComponent<Image>();
        if (_imageComponent != null)
        {
            // UI Image의 경우 Outline 컴포넌트 사용
            _outlineComponent = GetComponent<Outline>();
            if (_outlineComponent == null)
            {
                _outlineComponent = gameObject.AddComponent<Outline>();
            }
            
            // 초기 설정: 비활성화 상태
            _outlineComponent.enabled = false;
            _outlineComponent.effectColor = hoverBorderColor;
            _outlineComponent.effectDistance = hoverBorderThickness;
            _outlineComponent.useGraphicAlpha = true;
            
            Debug.Log($"[ClickableObject] UI Image에 Outline 컴포넌트 추가됨: {blockName}");
            return;
        }
        
        // SpriteRenderer 컴포넌트 확인
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
        {
            // SpriteRenderer의 경우 별도 처리 필요 (추후 구현 가능)
            Debug.Log($"[ClickableObject] SpriteRenderer 감지됨: {blockName} (Outline은 UI Image에서만 지원)");
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isHovering) return;
        _isHovering = true;
        
        // Outline 활성화
        if (_outlineComponent != null)
        {
            _outlineComponent.enabled = true;
            Debug.Log($"[ClickableObject] Hover 시작: {blockName}");
        }
        else if (_spriteRenderer != null)
        {
            // SpriteRenderer의 경우 색상 변경으로 강조 (훨씬 밝게)
            _spriteRenderer.color = new Color(2f, 2f, 2f, 1f); // 2배 밝게 (더 눈에 띄게)
            Debug.Log($"[ClickableObject] SpriteRenderer Hover 시작: {blockName}");
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_isHovering) return;
        _isHovering = false;
        
        // Outline 비활성화
        if (_outlineComponent != null)
        {
            _outlineComponent.enabled = false;
            Debug.Log($"[ClickableObject] Hover 종료: {blockName}");
        }
        else if (_spriteRenderer != null)
        {
            // SpriteRenderer 색상 복원
            _spriteRenderer.color = Color.white;
            Debug.Log($"[ClickableObject] SpriteRenderer Hover 종료: {blockName}");
        }
    }
    
    // OnMouse 이벤트 추가 (2D 오브젝트에서 더 안정적)
    void OnMouseEnter()
    {
        if (_isHovering) return;
        _isHovering = true;
        
        if (_outlineComponent != null)
        {
            _outlineComponent.enabled = true;
            Debug.Log($"[ClickableObject] OnMouse Hover 시작: {blockName}");
        }
        else if (_spriteRenderer != null)
        {
            _spriteRenderer.color = new Color(2f, 2f, 2f, 1f);
            Debug.Log($"[ClickableObject] OnMouse SpriteRenderer Hover 시작: {blockName}");
        }
    }
    
    void OnMouseExit()
    {
        if (!_isHovering) return;
        _isHovering = false;
        
        if (_outlineComponent != null)
        {
            _outlineComponent.enabled = false;
            Debug.Log($"[ClickableObject] OnMouse Hover 종료: {blockName}");
        }
        else if (_spriteRenderer != null)
        {
            _spriteRenderer.color = Color.white;
            Debug.Log($"[ClickableObject] OnMouse SpriteRenderer Hover 종료: {blockName}");
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // 중복 클릭 방지 (0.1초 내 재클릭 무시)
        float currentTime = Time.time;
        if (currentTime - _lastClickTime < CLICK_COOLDOWN)
        {
            Debug.Log($"중복 클릭 무시: {blockName} ({(currentTime - _lastClickTime) * 1000f:F1}ms 전)");
            return;
        }
        _lastClickTime = currentTime;

        if (inputHandler != null)
        {
            inputHandler.AddBlockToInput(blockName);
            Debug.Log($"블록 추가: {blockName}");
        }
        else
        {
            Debug.LogWarning("InputHandler를 찾을 수 없습니다.");
        }
    }
    
    void OnDestroy()
    {
        // Hover 상태 초기화
        if (_isHovering)
        {
            OnPointerExit(null);
        }
    }
}

