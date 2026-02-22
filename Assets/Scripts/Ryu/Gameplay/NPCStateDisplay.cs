using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NPC GameObject에 부착하여 Inspector에서 NPC 상태를 확인할 수 있는 컴포넌트입니다.
/// </summary>
public class NPCStateDisplay : MonoBehaviour
{
    /// <summary>
    /// 무력화 해제 시 비활성화된 NPC 오브젝트를 다시 켜기 위한 등록 테이블.
    /// (SetActive(false) 후에는 본 컴포넌트가 동작하지 않으므로, GameStateManager에서 이벤트로 재활성화)
    /// </summary>
    private static readonly Dictionary<NPCType, GameObject> NpcGameObjectsByType = new Dictionary<NPCType, GameObject>();

    /// <summary>
    /// 무력화 해제 시 해당 NPC GameObject를 다시 활성화합니다. (이벤트 구독처에서 호출)
    /// </summary>
    public static void TryReactivateNPC(NPCType npcType)
    {
        if (npcType == NPCType.None) return;
        if (NpcGameObjectsByType.TryGetValue(npcType, out GameObject go) && go != null && !go.activeSelf)
            go.SetActive(true);
    }
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
    private bool subscribedToStatusChanged;

    /// <summary>
    /// Inspector에서 값이 변경되면 호출됩니다. 플레이 중 상태 필드를 수정하면 GameStateManager에 반영합니다.
    /// </summary>
    private void OnValidate()
    {
        if (!Application.isPlaying || GameStateManager.Instance == null || npcType == NPCType.None)
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
        // 단, false로 바뀐 경우 GameStateManager도 아직 무력화가 아님일 때만 Clear 호출.
        // (UpdateState()가 이전 프레임 값으로 필드를 갱신한 뒤 OnValidate가 돌면, 백엔드에서 적용한 무력화를 덮어쓰지 않도록)
        if (isDisabled)
            gsm.SetNPCDisabled(npcType, Mathf.Max(0, disabledRemainingTurns), string.IsNullOrEmpty(disabledReason) ? "Inspector" : disabledReason);
        else
        {
            NPCStatus current = gsm.GetNPCStatus(npcType);
            if (current != null && !current.isDisabled)
                gsm.ClearNPCDisabled(npcType);
        }

        // 위치
        gsm.SetNPCLocation(npcType, currentLocation);

        UpdateState();
    }

    private void Start()
    {
        // GameObject 이름에서 NPC 타입 자동 감지
        if (npcType == NPCType.None) // 기본값이면
        {
            npcType = ParseNPCTypeFromName(gameObject.name);
            if (npcType == NPCType.None)
            {
                Debug.LogWarning($"[NPCStateDisplay] {gameObject.name}: NPC 타입을 자동 감지할 수 없습니다. Inspector에서 수동으로 설정해주세요.");
            }
        }

        if (npcType != NPCType.None)
        {
            NpcGameObjectsByType[npcType] = gameObject;
            SubscribeToStatusChangedIfNeeded();
        }

        CacheRenderersAndColliders();
        UpdateState();
    }

    private void OnEnable()
    {
        if (npcType != NPCType.None)
        {
            NpcGameObjectsByType[npcType] = gameObject;
            SubscribeToStatusChangedIfNeeded();
            // 씬 로드/활성화 시 싱글톤 기준으로 즉시 동기화 (다른 씬에서 넘어온 경우 최신 상태 반영)
            if (GameStateManager.Instance != null)
                UpdateState();
        }
    }

    private void OnDisable()
    {
        UnsubscribeFromStatusChanged();
    }

    private void OnDestroy()
    {
        if (npcType != NPCType.None)
            NpcGameObjectsByType.Remove(npcType);
        UnsubscribeFromStatusChanged();
    }

    private void SubscribeToStatusChangedIfNeeded()
    {
        if (subscribedToStatusChanged || GameStateManager.Instance == null || npcType == NPCType.None)
            return;
        GameStateManager.Instance.OnNPCStatusChanged += OnNPCStatusChanged;
        subscribedToStatusChanged = true;
    }

    private void UnsubscribeFromStatusChanged()
    {
        if (!subscribedToStatusChanged || GameStateManager.Instance == null)
            return;
        GameStateManager.Instance.OnNPCStatusChanged -= OnNPCStatusChanged;
        subscribedToStatusChanged = false;
    }

    private void OnNPCStatusChanged(NPCType npc, NPCStatus status)
    {
        if (npc == npcType)
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
            string enumName = MapNPCNameToEnumName(typeName);

            if (System.Enum.TryParse<NPCType>(enumName, true, out NPCType result))
            {
                return result;
            }
        }

        return 0;
    }

    /// <summary>
    /// GameObject/백엔드 NPC 이름을 NPCType enum 이름으로 매핑합니다.
    /// (씬 이름이 소문자나 스네이크 케이스일 때 매칭 보장)
    /// </summary>
    private static string MapNPCNameToEnumName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        switch (name.ToLower())
        {
            case "newmother":
            case "new_mother":
            case "stepmother":
                return "NewMother";
            case "newfather":
            case "new_father":
            case "father":
                return "NewFather";
            case "sibling":
            case "brother":
                return "Sibling";
            case "dog":
            case "baron":
                return "Dog";
            case "grandmother":
            case "grandma":
                return "Grandmother";
            default:
                return name;
        }
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

        if (npcType == NPCType.None)
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
    /// GameObject 자체를 비활성화하여 확실히 숨기고, 무력화 해제 시 OnNPCStatusChanged에서 TryReactivateNPC로 다시 켭니다.
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
        // 무력화 시 전체 오브젝트 비활성화로 확실히 숨김 (재활성화는 GameStateManager 쪽 이벤트로 처리)
        if (gameObject.activeSelf != visible)
            gameObject.SetActive(visible);
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
        if (npcType == NPCType.None)
        {
            Debug.LogWarning("[NPCStateDisplay] NPC 타입을 설정해주세요.");
            return false;
        }
        return true;
    }
}

