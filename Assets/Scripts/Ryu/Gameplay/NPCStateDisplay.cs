using UnityEngine;

/// <summary>
/// NPC GameObject에 부착하여 Inspector에서 NPC 상태를 확인할 수 있는 컴포넌트입니다.
/// </summary>
public class NPCStateDisplay : MonoBehaviour
{
    [Header("NPC 정보")]
    [Tooltip("NPC 타입 (자동 감지 또는 수동 설정)")]
    [SerializeField] private NPCType npcType;

    [Header("현재 상태 (플레이 중 편집 시 GameStateManager에 즉시 반영)")]
    [Tooltip("호감도. 플레이 중 값 변경 시 즉시 반영됩니다.")]
    [SerializeField] private float affection = 0f;
    [Tooltip("NPC 인간성. 플레이 중 값 변경 시 즉시 반영됩니다.")]
    [SerializeField] private float npcHumanity = 0f;
    [Tooltip("대화 가능 여부 (무력화 해제 시 true로 갱신됨).")]
    [SerializeField] private bool isAvailable = false;
    [Tooltip("무력화 여부. true로 바꾸면 disabledRemainingTurns/disabledReason이 적용됩니다.")]
    [SerializeField] private bool isDisabled = false;
    [Tooltip("무력화 시 남은 턴 수. isDisabled가 true일 때 적용됩니다.")]
    [SerializeField] private int disabledRemainingTurns = 0;
    [Tooltip("무력화 사유. isDisabled가 true일 때 적용됩니다.")]
    [SerializeField] private string disabledReason = "";
    [Tooltip("현재 위치. 플레이 중 값 변경 시 즉시 반영됩니다.")]
    [SerializeField] private GameLocation currentLocation = GameLocation.Hallway;

    [Header("상태 업데이트")]
    [Tooltip("자동으로 상태를 업데이트할지 여부")]
    [SerializeField] private bool autoUpdate = true;
    [Tooltip("상태 업데이트 간격 (초)")]
    [SerializeField] private float updateInterval = 0.5f;

    [Header("무력화 시 씬 표시")]
    [Tooltip("isDisabled가 true일 때 씬에서 보이지 않게 할지 여부")]
    [SerializeField] private bool hideInSceneWhenDisabled = true;

    [Header("Inspector에서 상태 적용 (테스트용)")]
    [Tooltip("'위치 적용' 시 이 위치로 설정됩니다")]
    [SerializeField] private GameLocation applyLocation = GameLocation.Hallway;

    private float lastUpdateTime = 0f;
    private Renderer[] cachedRenderers;
    private Collider[] cachedColliders;
    private bool lastAppliedVisible = true;

    /// <summary>
    /// Inspector에서 값이 변경되면 호출됩니다. 플레이 중 상태 필드를 수정하면 GameStateManager에 반영합니다.
    /// </summary>
    private void OnValidate()
    {
        if (!Application.isPlaying || GameStateManager.Instance == null || npcType == 0)
            return;

        var gsm = GameStateManager.Instance;

        // 호감도: 목표값과의 차이로 반영
        float currentAffection = gsm.GetAffection(npcType);
        float affectionDelta = affection - currentAffection;
        if (Mathf.Abs(affectionDelta) > 0.001f)
            gsm.ModifyAffection(npcType, affectionDelta);

        // NPC 인간성: 목표값과의 차이로 반영 (새엄마는 NPCManager에서 변경 불가 처리됨)
        float currentHumanity = gsm.GetNPCHumanity(npcType);
        float humanityDelta = npcHumanity - currentHumanity;
        if (Mathf.Abs(humanityDelta) > 0.001f)
            gsm.ModifyNPCHumanity(npcType, humanityDelta);

        // 무력화: isDisabled에 따라 설정/해제
        if (isDisabled)
            gsm.SetNPCDisabled(npcType, Mathf.Max(0, disabledRemainingTurns), string.IsNullOrEmpty(disabledReason) ? "Inspector" : disabledReason);
        else
            gsm.ClearNPCDisabled(npcType);

        // 위치
        gsm.SetNPCLocation(npcType, currentLocation);

        UpdateState();
    }

    private void Start()
    {
        // GameObject 이름에서 NPC 타입 자동 감지
        if (npcType == 0) // 기본값이면
        {
            npcType = ParseNPCTypeFromName(gameObject.name);
            if (npcType == 0)
            {
                Debug.LogWarning($"[NPCStateDisplay] {gameObject.name}: NPC 타입을 자동 감지할 수 없습니다. Inspector에서 수동으로 설정해주세요.");
            }
        }

        CacheRenderersAndColliders();
        UpdateState();
    }

    private void CacheRenderersAndColliders()
    {
        cachedRenderers = GetComponentsInChildren<Renderer>(true);
        cachedColliders = GetComponentsInChildren<Collider>(true);
    }

    private void Update()
    {
        if (autoUpdate && Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateState();
            lastUpdateTime = Time.time;
        }
    }

    /// <summary>
    /// GameObject 이름에서 NPC 타입을 파싱합니다.
    /// </summary>
    private NPCType ParseNPCTypeFromName(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
            return 0;

        // "NPC_" 접두사 제거
        if (objectName.StartsWith("NPC_"))
        {
            string typeName = objectName.Substring(4); // "NPC_" 제거
            
            // Enum.TryParse를 사용하여 NPCType으로 변환
            if (System.Enum.TryParse<NPCType>(typeName, true, out NPCType result))
            {
                return result;
            }
        }

        return 0;
    }

    /// <summary>
    /// GameStateManager에서 현재 NPC 상태를 조회하여 Inspector에 표시합니다.
    /// </summary>
    public void UpdateState()
    {
        if (GameStateManager.Instance == null)
        {
            Debug.LogWarning("[NPCStateDisplay] GameStateManager.Instance가 없습니다.");
            return;
        }

        if (npcType == 0)
        {
            return; // NPC 타입이 설정되지 않음
        }

        // NPC 상태 조회
        NPCStatus status = GameStateManager.Instance.GetNPCStatus(npcType);
        if (status != null)
        {
            affection = status.affection;
            npcHumanity = status.humanity;
            isAvailable = status.isAvailable;
            isDisabled = status.isDisabled;
            disabledRemainingTurns = status.disabledRemainingTurns;
            disabledReason = status.disabledReason ?? "";
        }

        // NPC 위치 조회
        currentLocation = GameStateManager.Instance.GetNPCLocation(npcType);

        // 무력화 시 씬에서 보이지 않게 처리
        if (hideInSceneWhenDisabled)
        {
            bool shouldBeVisible = !isDisabled;
            if (lastAppliedVisible != shouldBeVisible)
            {
                ApplyVisibility(shouldBeVisible);
                lastAppliedVisible = shouldBeVisible;
            }
        }
    }

    /// <summary>
    /// NPC의 씬 표시 여부를 적용합니다. isDisabled일 때 숨기기 위해 사용합니다.
    /// </summary>
    private void ApplyVisibility(bool visible)
    {
        if (cachedRenderers != null)
        {
            for (int i = 0; i < cachedRenderers.Length; i++)
            {
                if (cachedRenderers[i] != null)
                    cachedRenderers[i].enabled = visible;
            }
        }
        if (cachedColliders != null)
        {
            for (int i = 0; i < cachedColliders.Length; i++)
            {
                if (cachedColliders[i] != null)
                    cachedColliders[i].enabled = visible;
            }
        }
    }

    /// <summary>
    /// 수동으로 상태를 업데이트합니다 (Inspector에서 버튼으로 호출 가능).
    /// </summary>
    [ContextMenu("상태 업데이트")]
    public void ManualUpdate()
    {
        UpdateState();
    }

    // ========== Inspector에서 상태 적용 (컴포넌트 우클릭 Context Menu) ==========

    [ContextMenu("상태 적용: 무력화 (3턴)")]
    private void ApplyDisabled3Turns()
    {
        if (!TryGetGameStateManager(out var gsm)) return;
        gsm.SetNPCDisabled(npcType, 3, "Inspector 테스트");
        UpdateState();
    }

    [ContextMenu("상태 적용: 무력화 해제")]
    private void ApplyClearDisabled()
    {
        if (!TryGetGameStateManager(out var gsm)) return;
        gsm.ClearNPCDisabled(npcType);
        UpdateState();
    }

    [ContextMenu("상태 적용: 호감도 +10")]
    private void ApplyAffectionPlus10()
    {
        if (!TryGetGameStateManager(out var gsm)) return;
        gsm.ModifyAffection(npcType, 10f);
        UpdateState();
    }

    [ContextMenu("상태 적용: 호감도 -10")]
    private void ApplyAffectionMinus10()
    {
        if (!TryGetGameStateManager(out var gsm)) return;
        gsm.ModifyAffection(npcType, -10f);
        UpdateState();
    }

    [ContextMenu("상태 적용: 인간성 +10")]
    private void ApplyHumanityPlus10()
    {
        if (!TryGetGameStateManager(out var gsm)) return;
        gsm.ModifyNPCHumanity(npcType, 10f);
        UpdateState();
    }

    [ContextMenu("상태 적용: 인간성 -10")]
    private void ApplyHumanityMinus10()
    {
        if (!TryGetGameStateManager(out var gsm)) return;
        gsm.ModifyNPCHumanity(npcType, -10f);
        UpdateState();
    }

    [ContextMenu("상태 적용: 위치 설정")]
    private void ApplyLocation()
    {
        if (!TryGetGameStateManager(out var gsm)) return;
        gsm.SetNPCLocation(npcType, applyLocation);
        UpdateState();
    }

    private bool TryGetGameStateManager(out GameStateManager gsm)
    {
        gsm = GameStateManager.Instance;
        if (gsm == null)
        {
            Debug.LogWarning("[NPCStateDisplay] GameStateManager.Instance가 없습니다. 플레이 모드에서 실행해주세요.");
            return false;
        }
        if (npcType == 0)
        {
            Debug.LogWarning("[NPCStateDisplay] NPC 타입을 설정해주세요.");
            return false;
        }
        return true;
    }
}

