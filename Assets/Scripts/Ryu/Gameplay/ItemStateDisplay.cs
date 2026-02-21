using UnityEngine;

/// <summary>
/// 아이템 GameObject에 부착하여 Inspector에서 아이템 상태를 확인할 수 있는 컴포넌트입니다.
/// </summary>
public class ItemStateDisplay : MonoBehaviour
{
    [Header("아이템 정보")]
    [Tooltip("아이템 타입 (자동 감지 또는 수동 설정)")]
    [SerializeField] private ItemType itemType = ItemType.None;

    [Header("현재 상태 (읽기 전용)")]
    [SerializeField] private int inventoryCount = 0;
    [SerializeField] private ItemState itemState = ItemState.InWorld;
    [SerializeField] private GameLocation itemLocation = GameLocation.Hallway;
    [SerializeField] private string locationDetails = "";

    [Header("상태 업데이트")]
    [Tooltip("자동으로 상태를 업데이트할지 여부")]
    [SerializeField] private bool autoUpdate = true;
    [Tooltip("상태 업데이트 간격 (초)")]
    [SerializeField] private float updateInterval = 0.5f;

    private float lastUpdateTime = 0f;

    private void Start()
    {
        // GameObject 이름에서 아이템 타입 자동 감지
        if (itemType == ItemType.None)
        {
            itemType = ParseItemTypeFromName(gameObject.name);
            if (itemType == ItemType.None)
            {
                Debug.LogWarning($"[ItemStateDisplay] {gameObject.name}: 아이템 타입을 자동 감지할 수 없습니다. Inspector에서 수동으로 설정해주세요.");
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
    /// GameObject 이름에서 ItemType을 파싱합니다.
    /// </summary>
    private ItemType ParseItemTypeFromName(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
            return ItemType.None;

        // "Item_" 접두사 제거
        if (objectName.StartsWith("Item_"))
        {
            string typeName = objectName.Substring(5); // "Item_" 제거
            
            // GameObject 이름과 ItemType enum 이름 매핑
            // 예: "Item_SleepingPill" -> "SleepingPills"
            typeName = MapItemNameToEnumName(typeName);
            
            // Enum.TryParse를 사용하여 ItemType으로 변환
            if (System.Enum.TryParse<ItemType>(typeName, true, out ItemType result))
            {
                return result;
            }
        }

        return ItemType.None;
    }

    /// <summary>
    /// GameObject 이름을 ItemType enum 이름으로 매핑합니다.
    /// </summary>
    private string MapItemNameToEnumName(string itemName)
    {
        // GameObject 이름과 enum 이름이 다른 경우 매핑
        switch (itemName.ToLower())
        {
            case "sleepingpill":
            case "sleeping_pill":
                return "SleepingPills";
            case "earlgreytea":
            case "earl_grey_tea":
            case "blacktea":
            case "black_tea":
                return "EarlGreyTea";
            case "realfamilyphoto":
            case "real_family_photo":
            case "familyphoto":
            case "family_photo":
                return "RealFamilyPhoto";
            case "whaleoilcan":
            case "whale_oil_can":
            case "oilbottle":
            case "oil_bottle":
                return "WhaleOilCan";
            case "silverlighter":
            case "silver_lighter":
            case "lighter":
                return "SilverLighter";
            case "brasskey":
            case "brass_key":
                return "BrassKey";
            default:
                // 직접 매칭 시도
                return itemName;
        }
    }

    /// <summary>
    /// GameStateManager에서 현재 아이템 상태를 조회하여 Inspector에 표시합니다.
    /// </summary>
    public void UpdateState()
    {
        if (GameStateManager.Instance == null)
        {
            Debug.LogWarning("[ItemStateDisplay] GameStateManager.Instance가 없습니다.");
            return;
        }

        if (itemType == ItemType.None)
        {
            return; // 아이템 타입이 설정되지 않음
        }

        // 인벤토리 개수 조회
        inventoryCount = GameStateManager.Instance.GetItemCount(itemType);

        // 아이템 상태 조회
        itemState = GameStateManager.Instance.GetItemState(itemType);

        // 아이템 위치 정보는 ItemStateManager에서 직접 가져올 수 없으므로
        // 상태에 따라 표시
        if (itemState == ItemState.InWorld)
        {
            itemLocation = GameLocation.Hallway; // 기본값 (실제 위치는 씬에서 확인)
            locationDetails = "월드에 존재";
        }
        else if (itemState == ItemState.InInventory)
        {
            itemLocation = GameLocation.Hallway; // 인벤토리는 위치 개념 없음
            locationDetails = "인벤토리에 있음";
        }
        else if (itemState == ItemState.Used)
        {
            itemLocation = GameLocation.Hallway;
            locationDetails = "사용됨";
        }
        else if (itemState == ItemState.Hidden)
        {
            itemLocation = GameLocation.Hallway;
            locationDetails = "숨김";
        }

        // Hidden: 씬에 비표시. InWorld일 때만 월드 오브젝트 표시 (획득/사용 후엔 월드에 없음)
        bool visibleInWorld = (itemState == ItemState.InWorld);
        if (gameObject.activeSelf != visibleInWorld)
            gameObject.SetActive(visibleInWorld);
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

