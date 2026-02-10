using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class InputHandler : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI resultText;

    [Header("API")]
    [SerializeField] private ApiClient apiClient;

    [Header("Turn Management")]
    [SerializeField] private TurnManager turnManager;

    [Header("Game State")]
    [SerializeField] private GameStateManager gameStateManager;

    private bool needsDeselect = false;
    private int pendingCaretPos;

    private void Start()
    {
        if (inputField != null)
        {
            inputField.onSubmit.AddListener(OnSubmit);
        }
        else
        {
            Debug.LogError("[InputHandler] InputField이 연결되지 않았습니다.");
        }

        // ResultText에 클릭 이벤트 추가
        if (resultText != null)
        {
            resultText.text = "";
            AddClickListenerToResultText();
        }

        // 초기 상태: InputField 활성화, ResultText 비활성화
        ShowInputField();
    }

    /// <summary>
    /// ResultText GameObject에 EventTrigger를 추가하여 클릭 이벤트를 감지합니다.
    /// </summary>
    private void AddClickListenerToResultText()
    {
        GameObject resultObj = resultText.gameObject;

        // Raycast Target이 활성화되어 있어야 클릭 감지 가능
        resultText.raycastTarget = true;

        EventTrigger trigger = resultObj.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = resultObj.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => OnResultTextClicked());
        trigger.triggers.Add(entry);
    }

    /// <summary>
    /// ResultText 클릭 시 InputField로 전환합니다.
    /// </summary>
    private void OnResultTextClicked()
    {
        Debug.Log("[InputHandler] ResultText 클릭 → InputField로 전환");
        ShowInputField();
    }

    /// <summary>
    /// InputField를 활성화하고 ResultText를 비활성화합니다.
    /// </summary>
    private void ShowInputField()
    {
        if (inputField != null)
        {
            inputField.gameObject.SetActive(true);
            inputField.text = "";
            inputField.ActivateInputField();
        }

        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// ResultText를 활성화하고 InputField를 비활성화합니다.
    /// </summary>
    private void ShowResultText()
    {
        if (inputField != null)
        {
            inputField.gameObject.SetActive(false);
        }

        if (resultText != null)
        {
            resultText.gameObject.SetActive(true);
        }
    }

    private void OnSubmit(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        Debug.Log($"[InputHandler] 입력 전송: {text}");

        // InputField 숨기고 ResultText 표시
        ShowResultText();
        resultText.text = "전송 중...";

        // InputField 비우기
        inputField.text = "";

        // API 호출
        if (apiClient != null)
        {
            apiClient.SendMessage(text, OnApiSuccess, OnApiError);
        }
        else
        {
            Debug.LogError("[InputHandler] ApiClient가 연결되지 않았습니다.");
            resultText.text = "ApiClient가 연결되지 않았습니다.";
        }
    }

    private void OnApiSuccess(
        string response, 
        float humanityChange, 
        NPCAffectionChanges npcAffectionChanges, 
        NPCHumanityChanges npcHumanityChanges,
        NPCDisabledStates npcDisabledStates,
        NPCLocations npcLocations,
        ItemChanges itemChanges,
        EventFlags eventFlags,
        string endingTrigger)
    {
        Debug.Log($"[InputHandler] 응답 수신: {response}");
        Debug.Log($"[InputHandler] 인간성 변화량: {humanityChange:F1}");

        // 응답 텍스트 표시
        resultText.text = response;

        // 플레이어 인간성 변화량 적용
        if (gameStateManager != null)
        {
            gameStateManager.ModifyHumanity(humanityChange);
            
            // NPC 호감도 변화량 적용
            if (npcAffectionChanges != null)
            {
                if (npcAffectionChanges.new_mother != 0f)
                    gameStateManager.ModifyAffection(NPCType.NewMother, npcAffectionChanges.new_mother);
                if (npcAffectionChanges.new_father != 0f)
                    gameStateManager.ModifyAffection(NPCType.NewFather, npcAffectionChanges.new_father);
                if (npcAffectionChanges.sibling != 0f)
                    gameStateManager.ModifyAffection(NPCType.Sibling, npcAffectionChanges.sibling);
                if (npcAffectionChanges.dog != 0f)
                    gameStateManager.ModifyAffection(NPCType.Dog, npcAffectionChanges.dog);
                if (npcAffectionChanges.grandmother != 0f)
                    gameStateManager.ModifyAffection(NPCType.Grandmother, npcAffectionChanges.grandmother);
            }
            
            // NPC 인간성 변화량 적용
            if (npcHumanityChanges != null)
            {
                if (npcHumanityChanges.new_father != 0f)
                    gameStateManager.ModifyNPCHumanity(NPCType.NewFather, npcHumanityChanges.new_father);
                if (npcHumanityChanges.sibling != 0f)
                    gameStateManager.ModifyNPCHumanity(NPCType.Sibling, npcHumanityChanges.sibling);
                if (npcHumanityChanges.dog != 0f)
                    gameStateManager.ModifyNPCHumanity(NPCType.Dog, npcHumanityChanges.dog);
                if (npcHumanityChanges.grandmother != 0f)
                    gameStateManager.ModifyNPCHumanity(NPCType.Grandmother, npcHumanityChanges.grandmother);
            }
            
            // NPC 무력화 상태 적용 (백엔드에서 제공 시만)
            if (npcDisabledStates != null)
            {
                ApplyNPCDisabledStates(npcDisabledStates);
            }
            
            // NPC 위치 업데이트 (백엔드에서 제공 시만, 백엔드 응답이 항상 우선)
            if (npcLocations != null)
            {
                ApplyNPCLocations(npcLocations);
            }
            
            // 아이템 변화량 적용 (백엔드에서 제공 시만)
            if (itemChanges != null)
            {
                ApplyItemChanges(itemChanges);
            }
            
            // 이벤트 플래그 적용 (백엔드에서 제공 시만)
            if (eventFlags != null)
            {
                ApplyEventFlags(eventFlags);
            }
            
            // 엔딩 트리거 처리
            if (!string.IsNullOrEmpty(endingTrigger))
            {
                EndingType endingType = ApiClient.ConvertEndingNameToType(endingTrigger);
                if (endingType != EndingType.None)
                {
                    // 백엔드에서 엔딩 트리거를 받았으므로 즉시 엔딩 진입
                    gameStateManager.TriggerEnding(endingType);
                    Debug.Log($"[InputHandler] 백엔드에서 엔딩 트리거 수신: {endingType}");
                    return; // 엔딩 진입 시 더 이상 처리하지 않음
                }
            }
        }
        else
        {
            Debug.LogWarning("[InputHandler] GameStateManager가 연결되지 않았습니다.");
        }

        // 턴 소모
        if (turnManager != null)
        {
            turnManager.ConsumeTurn();
        }
        else
        {
            Debug.LogWarning("[InputHandler] TurnManager가 연결되지 않았습니다.");
        }

        // ResultText 표시 상태 유지 → 클릭하면 다시 InputField로 전환
    }

    // NPC 무력화 상태 적용 헬퍼 메서드
    private void ApplyNPCDisabledStates(NPCDisabledStates disabledStates)
    {
        if (gameStateManager == null)
            return;

        if (disabledStates.new_father != null && disabledStates.new_father.is_disabled)
        {
            gameStateManager.SetNPCDisabled(
                NPCType.NewFather, 
                disabledStates.new_father.remaining_turns, 
                disabledStates.new_father.reason
            );
        }
        
        if (disabledStates.sibling != null && disabledStates.sibling.is_disabled)
        {
            gameStateManager.SetNPCDisabled(
                NPCType.Sibling, 
                disabledStates.sibling.remaining_turns, 
                disabledStates.sibling.reason
            );
        }
        
        if (disabledStates.dog != null && disabledStates.dog.is_disabled)
        {
            gameStateManager.SetNPCDisabled(
                NPCType.Dog, 
                disabledStates.dog.remaining_turns, 
                disabledStates.dog.reason
            );
        }
        
        if (disabledStates.grandmother != null && disabledStates.grandmother.is_disabled)
        {
            gameStateManager.SetNPCDisabled(
                NPCType.Grandmother, 
                disabledStates.grandmother.remaining_turns, 
                disabledStates.grandmother.reason
            );
        }
        
        // 새엄마는 무력화 불가 (최종보스)
    }

    // NPC 위치 적용 헬퍼 메서드 (위치 이름 매핑 사용)
    private void ApplyNPCLocations(NPCLocations npcLocations)
    {
        if (gameStateManager == null)
            return;

        // 위치 이름 매핑 헬퍼 사용
        if (!string.IsNullOrEmpty(npcLocations.new_mother))
        {
            GameLocation location = ApiClient.ConvertLocationNameToType(npcLocations.new_mother);
            gameStateManager.SetNPCLocation(NPCType.NewMother, location);
        }
        
        if (!string.IsNullOrEmpty(npcLocations.new_father))
        {
            GameLocation location = ApiClient.ConvertLocationNameToType(npcLocations.new_father);
            gameStateManager.SetNPCLocation(NPCType.NewFather, location);
        }
        
        if (!string.IsNullOrEmpty(npcLocations.sibling))
        {
            GameLocation location = ApiClient.ConvertLocationNameToType(npcLocations.sibling);
            gameStateManager.SetNPCLocation(NPCType.Sibling, location);
        }
        
        if (!string.IsNullOrEmpty(npcLocations.dog))
        {
            GameLocation location = ApiClient.ConvertLocationNameToType(npcLocations.dog);
            gameStateManager.SetNPCLocation(NPCType.Dog, location);
        }
        
        if (!string.IsNullOrEmpty(npcLocations.grandmother))
        {
            GameLocation location = ApiClient.ConvertLocationNameToType(npcLocations.grandmother);
            gameStateManager.SetNPCLocation(NPCType.Grandmother, location);
        }
    }

    // 아이템 변화량 적용 헬퍼 메서드
    private void ApplyItemChanges(ItemChanges itemChanges)
    {
        if (gameStateManager == null)
            return;

        // 아이템 획득 처리
        if (itemChanges.acquired_items != null)
        {
            foreach (var acquisition in itemChanges.acquired_items)
            {
                ItemType itemType = ApiClient.ConvertItemNameToType(acquisition.item_name);
                if (itemType != ItemType.None) // None이 아닌 경우만 처리
                {
                    gameStateManager.AddItem(itemType, acquisition.count);
                    Debug.Log($"[InputHandler] 아이템 획득: {itemType} x{acquisition.count}");
                }
            }
        }
        
        // 아이템 사용/소모 처리
        if (itemChanges.consumed_items != null)
        {
            foreach (var consumption in itemChanges.consumed_items)
            {
                ItemType itemType = ApiClient.ConvertItemNameToType(consumption.item_name);
                if (itemType != ItemType.None)
                {
                    gameStateManager.RemoveItem(itemType, consumption.count);
                    Debug.Log($"[InputHandler] 아이템 사용/소모: {itemType} x{consumption.count}");
                }
            }
        }
        
        // 아이템 상태 변경 처리
        if (itemChanges.state_changes != null)
        {
            foreach (var stateChange in itemChanges.state_changes)
            {
                ItemType itemType = ApiClient.ConvertItemNameToType(stateChange.item_name);
                ItemState newState = ApiClient.ConvertItemStateNameToType(stateChange.new_state);
                if (itemType != ItemType.None)
                {
                    gameStateManager.SetItemState(itemType, newState);
                    Debug.Log($"[InputHandler] 아이템 상태 변경: {itemType} → {newState}");
                }
            }
        }
    }

    // 이벤트 플래그 적용 헬퍼 메서드
    private void ApplyEventFlags(EventFlags eventFlags)
    {
        if (gameStateManager == null)
            return;

        // 표준 이벤트 플래그 적용 (EventFlags 구조체는 bool 필드를 직접 가지고 있으므로 직접 사용)
        gameStateManager.SetEventFlag("grandmotherCooperation", eventFlags.grandmotherCooperation);
        gameStateManager.SetEventFlag("holeUnlocked", eventFlags.holeUnlocked);
        gameStateManager.SetEventFlag("fireStarted", eventFlags.fireStarted);
        gameStateManager.SetEventFlag("familyAsleep", eventFlags.familyAsleep);
        gameStateManager.SetEventFlag("teaWithSleepingPill", eventFlags.teaWithSleepingPill);
        gameStateManager.SetEventFlag("keyStolen", eventFlags.keyStolen);
        gameStateManager.SetEventFlag("caughtByFather", eventFlags.caughtByFather);
        gameStateManager.SetEventFlag("caughtByMother", eventFlags.caughtByMother);
        
        // 커스텀 이벤트 적용
        if (eventFlags.customEvents != null)
        {
            foreach (var customEvent in eventFlags.customEvents)
            {
                gameStateManager.SetCustomEvent(customEvent.Key, customEvent.Value);
            }
        }
        
        Debug.Log($"[InputHandler] 이벤트 플래그 적용 완료");
    }

    private void OnApiError(string error)
    {
        Debug.LogError($"[InputHandler] API 에러: {error}");
        resultText.text = $"에러: {error}";
        // ResultText 표시 상태 유지 → 클릭하면 다시 InputField로 전환
    }

    /// <summary>
    /// 외부에서 호출하여 InputField에 @블록이름을 삽입합니다.
    /// ResultText가 표시 중이면 InputField로 전환합니다.
    /// </summary>
    public void AddBlockToInput(string blockName)
    {
        // ResultText 표시 중이면 InputField로 전환
        if (!inputField.gameObject.activeSelf)
        {
            ShowInputField();
        }

        string blockText = $"@{blockName} ";
        int caretPos = Mathf.Clamp(inputField.caretPosition, 0, inputField.text.Length);
        inputField.text = inputField.text.Insert(caretPos, blockText);
        int newCaretPos = caretPos + blockText.Length;
        
        inputField.ActivateInputField();
        needsDeselect = true;
        pendingCaretPos = newCaretPos;

        Debug.Log($"[InputHandler] 블록 추가: {blockText.Trim()}");
    }

    private void LateUpdate()
    {
        if (needsDeselect)
        {
            needsDeselect = false;
            inputField.caretPosition = pendingCaretPos;
            inputField.selectionAnchorPosition = pendingCaretPos;
            inputField.selectionFocusPosition = pendingCaretPos;
        }
    }

    private void OnDestroy()
    {
        if (inputField != null)
        {
            inputField.onSubmit.RemoveListener(OnSubmit);
        }
    }
}
