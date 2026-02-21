using UnityEngine;

/// <summary>
/// NPC GameObject에 부착하여 Inspector에서 NPC 상태를 확인할 수 있는 컴포넌트입니다.
/// </summary>
public class NPCStateDisplay : MonoBehaviour
{
    [Header("NPC 정보")]
    [Tooltip("NPC 타입 (자동 감지 또는 수동 설정)")]
    [SerializeField] private NPCType npcType;

    [Header("현재 상태 (읽기 전용)")]
    [SerializeField] private float affection = 0f;
    [SerializeField] private float npcHumanity = 0f;
    [SerializeField] private bool isAvailable = false;
    [SerializeField] private bool isDisabled = false;
    [SerializeField] private int disabledRemainingTurns = 0;
    [SerializeField] private string disabledReason = "";
    [SerializeField] private GameLocation currentLocation = GameLocation.Hallway;

    [Header("상태 업데이트")]
    [Tooltip("자동으로 상태를 업데이트할지 여부")]
    [SerializeField] private bool autoUpdate = true;
    [Tooltip("상태 업데이트 간격 (초)")]
    [SerializeField] private float updateInterval = 0.5f;

    private float lastUpdateTime = 0f;

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

        UpdateState();
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
    }

    /// <summary>
    /// 수동으로 상태를 업데이트합니다 (Inspector에서 버튼으로 호출 가능).
    /// </summary>
    [ContextMenu("상태 업데이트")]
    public void ManualUpdate()
    {
        UpdateState();
    }
}

