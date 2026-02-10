using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

// ============================================
// Enum 정의
// ============================================

/// <summary>
/// NPC 타입 열거형
/// </summary>
public enum NPCType
{
    NewMother,      // 새엄마 (엘리노어) - 최종보스, 인간성 불가
    NewFather,      // 새아빠 (아더)
    Sibling,        // 동생 (루카스)
    Dog,            // 강아지 (바론)
    Grandmother     // 할머니 (마가렛)
}

/// <summary>
/// 아이템 타입 열거형
/// </summary>
public enum ItemType
{
    None = 0,           // 없음 (기본값)
    SleepingPill,       // 보라색 라벨의 약병 (수면제)
    EarlGreyTea,       // 홍차
    RealFamilyPhoto,   // 훼손된 가족 사진 (진짜 가족 사진)
    OilBottle,         // 고래기름 통 (기름병)
    SilverLighter,     // 은색 지포 라이터
    SiblingsToy,       // 낡은 태엽 로봇 (동생의 장난감)
    BrassKey           // 황동 열쇠 (마스터 키)
}

/// <summary>
/// 아이템 상태 열거형
/// </summary>
public enum ItemState
{
    InWorld,        // 월드에 존재 (획득 가능)
    InInventory,    // 인벤토리에 있음
    Used            // 사용됨 (소모품)
}

/// <summary>
/// 시간대 열거형
/// </summary>
public enum TimeOfDay
{
    Day,            // 낮 (탐색 및 대화 가능)
    Night           // 밤 (NPC 회의 진행)
}

/// <summary>
/// 게임 위치 열거형
/// </summary>
public enum GameLocation
{
    PlayersRoom,        // 주인공의 방 (The Birdcage)
    Hallway,            // 복도와 계단 (The Veins)
    LivingRoom,         // 거실 (The Showroom)
    Kitchen,            // 주방 및 식당 (The Modification Lab)
    SiblingsRoom,       // 동생의 놀이방 (The Glitch Room)
    Basement,           // 지하실 (The Archive / 할머니의 방)
    Backyard            // 뒷마당과 정원 (The Gateway)
}

/// <summary>
/// 엔딩 타입 열거형
/// </summary>
public enum EndingType
{
    None,                   // 엔딩 없음
    StealthExit,            // 완벽한 기만 (루트 1)
    ChaoticBreakout,        // 혼돈의 밤 (루트 2)
    SiblingsHelp,           // 조력자의 희생 (루트 4)
    UnfinishedDoll,         // 불완전한 박제 (배드 루트 1)
    EternalDinner           // 영원한 식사 시간 (배드 루트 2)
}

// ============================================
// 직렬화 가능한 데이터 구조체
// ============================================

/// <summary>
/// NPC 상태 정보
/// </summary>
[Serializable]
public class NPCStatus
{
    public NPCType npcType;
    public float affection;         // 호감도 (0~100)
    public float humanity;          // NPC 인간성 (-100~100, 새엄마는 -100 고정)
    public bool isAvailable;       // 대화 가능 여부
    public bool isDisabled;        // 무력화 상태 (수면제 등)
    public int disabledRemainingTurns; // 무력화 남은 턴 수
    public string disabledReason;  // 무력화 이유
}

/// <summary>
/// 아이템 위치 정보
/// </summary>
[Serializable]
public class ItemLocation
{
    public GameLocation location;   // 게임 구역
    public string sceneName;       // 씬 이름
    public string locationId;      // 구체적 위치 식별자 (예: "Hallway_PhotoFrame", "Garden_DogHouse")
}

/// <summary>
/// 월드 아이템 상태
/// </summary>
[Serializable]
public class WorldItemState
{
    public ItemType itemType;
    public ItemState state;
    public ItemLocation location;   // null이면 여러 위치 가능 또는 위치 불명
    public bool isRespawnable;     // 매일 리스폰 여부 (홍차 등)
}

/// <summary>
/// 엔딩 조건
/// </summary>
[Serializable]
public class EndingCondition
{
    public EndingType endingType;
    public bool hasRequiredItems;      // 필수 아이템 보유 여부
    public bool hasRequiredNPCStatus; // 필수 NPC 상태 달성 여부
    public bool hasRequiredLocation;   // 필수 장소 도달 여부
    public bool hasRequiredTime;       // 필수 시간대 여부
    // Unity JsonUtility는 Dictionary를 지원하지 않으므로, 직렬화가 필요한 경우 별도 처리 필요
    public Dictionary<string, bool> customFlags; // 커스텀 플래그 (예: "GrandmotherCooperation")
}

/// <summary>
/// 이벤트 플래그
/// </summary>
[Serializable]
public class EventFlags
{
    public bool grandmotherCooperation;    // 할머니 협력 상태
    public bool holeUnlocked;             // 개구멍 개방 상태
    public bool fireStarted;              // 화재 발생 상태
    public bool familyAsleep;             // 가족 수면 상태
    public bool teaWithSleepingPill;      // 홍차에 수면제를 탔는지 여부 (StealthExit 엔딩 조건)
    public bool keyStolen;                // 열쇠 탈취 상태
    public bool caughtByFather;           // 새아빠에게 발각됨
    public bool caughtByMother;           // 새엄마에게 발각됨
    public int imprisonmentDay;           // 감금된 날짜 (-1이면 감금 아님)
    // Unity JsonUtility는 Dictionary를 지원하지 않으므로, 직렬화가 필요한 경우 별도 처리 필요
    public Dictionary<string, bool> customEvents; // 기타 커스텀 이벤트
}

/// <summary>
/// 게임 전반의 상태를 중앙에서 관리하는 싱글톤 매니저입니다.
/// 인간성 수치를 관리하고 게임 상태를 추적합니다.
/// </summary>
public class GameStateManager : MonoBehaviour
{
    // Singleton 인스턴스
    public static GameStateManager Instance { get; private set; }

    // 상태 변수
    private float humanity = 100f; // 초기값 100%
    private const float MIN_HUMANITY = 0f;
    private const float MAX_HUMANITY = 100f;

    // 날짜 변수
    private int currentDay = 1; // 1일차부터 시작 (기본값 1)
    private const int MAX_DAY = 5; // 최대 5일차

    // ============================================
    // NPC 상태 관리 필드
    // ============================================
    private Dictionary<NPCType, NPCStatus> npcStatuses;

    // ============================================
    // 아이템 시스템 필드
    // ============================================
    private Dictionary<ItemType, int> inventory;  // 인벤토리 (아이템별 개수)
    private Dictionary<ItemType, WorldItemState> worldItemStates;  // 월드 아이템 상태

    // ============================================
    // 시간대 및 턴 시스템 필드
    // ============================================
    private TimeOfDay currentTimeOfDay = TimeOfDay.Day;
    private int currentTurn = 0;
    private const int MAX_TURNS_PER_DAY = 10;

    // ============================================
    // 위치 추적 필드
    // ============================================
    private GameLocation currentLocation;
    private Dictionary<NPCType, GameLocation> npcLocations;  // NPC 현재 위치

    // ============================================
    // 엔딩 시스템 필드
    // ============================================
    private Dictionary<EndingType, EndingCondition> endingConditions;
    private EndingType currentEnding = EndingType.None;

    // ============================================
    // 이벤트 플래그 필드
    // ============================================
    private EventFlags eventFlags;

    /// <summary>
    /// 현재 날짜를 반환합니다 (1~5일차).
    /// </summary>
    public int CurrentDay 
    { 
        get { return currentDay; } 
        private set { currentDay = Mathf.Clamp(value, 1, MAX_DAY); }
    }

    // 이벤트 시스템
    /// <summary>
    /// 인간성 변경 시 호출되는 이벤트입니다.
    /// </summary>
    public event Action<float> OnHumanityChanged;

    /// <summary>
    /// 인간성 0% 도달 시 호출되는 이벤트입니다.
    /// </summary>
    public event Action OnHumanityReachedZero;

    /// <summary>
    /// 날짜가 변경될 때 호출되는 이벤트입니다. (새로운 날짜)
    /// </summary>
    public event Action<int> OnDayChanged;

    /// <summary>
    /// NPC 상태 변경 시 호출되는 이벤트입니다.
    /// </summary>
    public event Action<NPCType, NPCStatus> OnNPCStatusChanged;

    /// <summary>
    /// 인벤토리 변경 시 호출되는 이벤트입니다.
    /// </summary>
    public event Action<ItemType, int> OnInventoryChanged;

    /// <summary>
    /// 아이템 상태 변경 시 호출되는 이벤트입니다.
    /// </summary>
    public event Action<ItemType, WorldItemState> OnItemStateChanged;

    /// <summary>
    /// 시간대 변경 시 호출되는 이벤트입니다.
    /// </summary>
    public event Action<TimeOfDay> OnTimeOfDayChanged;

    /// <summary>
    /// 턴 수 변경 시 호출되는 이벤트입니다.
    /// </summary>
    public event Action<int> OnTurnChanged;

    /// <summary>
    /// 턴이 모두 소진되었을 때 호출되는 이벤트입니다.
    /// </summary>
    public event Action OnTurnsExhausted;

    /// <summary>
    /// 위치 변경 시 호출되는 이벤트입니다.
    /// </summary>
    public event Action<GameLocation> OnLocationChanged;

    /// <summary>
    /// 엔딩 트리거 시 호출되는 이벤트입니다.
    /// </summary>
    public event Action<EndingType> OnEndingTriggered;

    /// <summary>
    /// 싱글톤 패턴 초기화 및 게임 상태 초기화를 수행합니다.
    /// </summary>
    private void Awake()
    {
        // Singleton 패턴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 초기값 설정
        humanity = 10f; // 테스트용: 10%로 설정
        currentDay = 1; // 1일차부터 시작

        // 딕셔너리 초기화 (구체적인 초기값 설정은 Phase2, Phase3, Phase4에서 수행)
        npcStatuses = new Dictionary<NPCType, NPCStatus>();
        inventory = new Dictionary<ItemType, int>();
        worldItemStates = new Dictionary<ItemType, WorldItemState>();
        endingConditions = new Dictionary<EndingType, EndingCondition>();
        npcLocations = new Dictionary<NPCType, GameLocation>();
        eventFlags = new EventFlags();

        // NPC 시스템 초기화
        InitializeNPCStatuses();
        InitializeNPCLocations();
        
        // 아이템 시스템 초기화
        InitializeInventory();
        
        // 엔딩 시스템 초기화
        InitializeEndingConditions();
        InitializeEventFlags();
    }

    // ============================================
    // NPC 상태 초기화
    // ============================================

    /// <summary>
    /// NPC 상태를 초기화합니다.
    /// </summary>
    private void InitializeNPCStatuses()
    {
        npcStatuses = new Dictionary<NPCType, NPCStatus>();
        
        // 새엄마 (엘리노어) - 최종보스, 인간성 불가
        npcStatuses[NPCType.NewMother] = new NPCStatus
        {
            npcType = NPCType.NewMother,
            affection = 50f,        // 기본 호감도
            humanity = -999f,       // 매우 큰 음수 - 절대 인간화 불가 (최종보스)
            isAvailable = true,
            isDisabled = false,
            disabledRemainingTurns = 0,
            disabledReason = ""
        };
        
        // 새아빠 (아더)
        npcStatuses[NPCType.NewFather] = new NPCStatus
        {
            npcType = NPCType.NewFather,
            affection = 50f,
            humanity = -50f,      // 인간의 기억이 거의 사라진 상태 (0이 되면 기억 회복)
            isAvailable = true,
            isDisabled = false,
            disabledRemainingTurns = 0,
            disabledReason = ""
        };
        
        // 동생 (루카스)
        npcStatuses[NPCType.Sibling] = new NPCStatus
        {
            npcType = NPCType.Sibling,
            affection = 50f,
            humanity = -20f,      // 최근 개조되어 일부 기억이 남아있음 (0이 되면 기억 회복)
            isAvailable = true,
            isDisabled = false,
            disabledRemainingTurns = 0,
            disabledReason = ""
        };
        
        // 강아지 (바론)
        npcStatuses[NPCType.Dog] = new NPCStatus
        {
            npcType = NPCType.Dog,
            affection = 80f,      // 인간 상태의 주인공에게 기본 호감
            humanity = 0f,
            isAvailable = true,
            isDisabled = false,
            disabledRemainingTurns = 0,
            disabledReason = ""
        };
        
        // 할머니 (마가렛)
        npcStatuses[NPCType.Grandmother] = new NPCStatus
        {
            npcType = NPCType.Grandmother,
            affection = 0f,
            humanity = -999f,       // 생명력이 거의 다 빠진 상태
            isAvailable = false,   // 특수 조건 필요
            isDisabled = false,
            disabledRemainingTurns = 0,
            disabledReason = ""
        };
    }

    /// <summary>
    /// NPC 위치를 초기화합니다.
    /// </summary>
    private void InitializeNPCLocations()
    {
        npcLocations = new Dictionary<NPCType, GameLocation>();
        
        // 기본 위치 설정 (씬에 따라 변경 가능)
        npcLocations[NPCType.NewMother] = GameLocation.Kitchen;
        npcLocations[NPCType.NewFather] = GameLocation.LivingRoom;
        npcLocations[NPCType.Sibling] = GameLocation.SiblingsRoom;
        npcLocations[NPCType.Dog] = GameLocation.Backyard;
        npcLocations[NPCType.Grandmother] = GameLocation.Basement;
    }

    // ============================================
    // 아이템 시스템 초기화
    // ============================================

    /// <summary>
    /// 인벤토리 및 월드 아이템 상태를 초기화합니다.
    /// </summary>
    private void InitializeInventory()
    {
        inventory = new Dictionary<ItemType, int>();
        worldItemStates = new Dictionary<ItemType, WorldItemState>();
        
        // 월드 아이템 상태 초기화
        InitializeWorldItemStates();
    }

    /// <summary>
    /// 월드 아이템 상태를 초기화합니다.
    /// </summary>
    private void InitializeWorldItemStates()
    {
        // 수면제 - 주방 찬장
        worldItemStates[ItemType.SleepingPill] = new WorldItemState
        {
            itemType = ItemType.SleepingPill,
            state = ItemState.InWorld,
            location = new ItemLocation
            {
                location = GameLocation.Kitchen,
                sceneName = "Tutorial",
                locationId = "Kitchen_Cabinet"
            },
            isRespawnable = false
        };
        
        // 홍차 - 주방 식탁 (매일 리스폰)
        worldItemStates[ItemType.EarlGreyTea] = new WorldItemState
        {
            itemType = ItemType.EarlGreyTea,
            state = ItemState.InWorld,
            location = new ItemLocation
            {
                location = GameLocation.Kitchen,
                sceneName = "Tutorial",
                locationId = "Kitchen_Table"
            },
            isRespawnable = true
        };
        
        // 진짜 가족 사진 - 뒷마당 개집 근처
        worldItemStates[ItemType.RealFamilyPhoto] = new WorldItemState
        {
            itemType = ItemType.RealFamilyPhoto,
            state = ItemState.InWorld,
            location = new ItemLocation
            {
                location = GameLocation.Backyard,
                sceneName = "Tutorial",
                locationId = "Backyard_DogHouse"
            },
            isRespawnable = false
        };
        
        // 고래기름 통 - 지하실 수술대 아래
        worldItemStates[ItemType.OilBottle] = new WorldItemState
        {
            itemType = ItemType.OilBottle,
            state = ItemState.InWorld,
            location = new ItemLocation
            {
                location = GameLocation.Basement,
                sceneName = "Tutorial",
                locationId = "Basement_SurgeryTable"
            },
            isRespawnable = false
        };
        
        // 은색 라이터 - 거실 소파 틈새
        worldItemStates[ItemType.SilverLighter] = new WorldItemState
        {
            itemType = ItemType.SilverLighter,
            state = ItemState.InWorld,
            location = new ItemLocation
            {
                location = GameLocation.LivingRoom,
                sceneName = "Tutorial",
                locationId = "LivingRoom_Sofa"
            },
            isRespawnable = false
        };
        
        // 동생의 장난감 - 동생의 방 인형의 집 모형
        worldItemStates[ItemType.SiblingsToy] = new WorldItemState
        {
            itemType = ItemType.SiblingsToy,
            state = ItemState.InWorld,
            location = new ItemLocation
            {
                location = GameLocation.SiblingsRoom,
                sceneName = "Tutorial",
                locationId = "SiblingsRoom_DollHouse"
            },
            isRespawnable = false
        };
        
        // 황동 열쇠 - 새엄마 목걸이 (항상 소지)
        worldItemStates[ItemType.BrassKey] = new WorldItemState
        {
            itemType = ItemType.BrassKey,
            state = ItemState.InWorld,
            location = new ItemLocation
            {
                location = GameLocation.Kitchen,
                sceneName = "Tutorial",
                locationId = "NewMother_Necklace"
            },
            isRespawnable = false
        };
    }

    // ============================================
    // 인벤토리 관리 메서드
    // ============================================

    /// <summary>
    /// 인벤토리에 아이템을 추가합니다.
    /// 백엔드에서 받은 아이템 획득 정보를 적용합니다.
    /// </summary>
    /// <param name="item">아이템 타입</param>
    /// <param name="count">추가할 개수 (기본값: 1) - 백엔드에서 제공</param>
    public void AddItem(ItemType item, int count = 1)
    {
        if (count <= 0)
        {
            Debug.LogWarning($"[GameStateManager] 잘못된 아이템 개수: {count}");
            return;
        }
        
        if (!inventory.ContainsKey(item))
        {
            inventory[item] = 0;
        }
        
        int oldCount = inventory[item];
        inventory[item] += count;
        
        // 월드 아이템 상태 업데이트
        if (worldItemStates.ContainsKey(item))
        {
            WorldItemState itemState = worldItemStates[item];
            if (itemState.state == ItemState.InWorld)
            {
                itemState.state = ItemState.InInventory;
                worldItemStates[item] = itemState;
                OnItemStateChanged?.Invoke(item, itemState);
            }
        }
        
        OnInventoryChanged?.Invoke(item, inventory[item]);
        Debug.Log($"[GameStateManager] 아이템 추가: {item} x{count} (총 {inventory[item]}개)");
    }

    /// <summary>
    /// 인벤토리에서 아이템을 제거합니다.
    /// 백엔드에서 받은 아이템 사용/소모 정보를 적용합니다.
    /// </summary>
    /// <param name="item">아이템 타입</param>
    /// <param name="count">제거할 개수 (기본값: 1) - 백엔드에서 제공</param>
    /// <returns>제거 성공 여부</returns>
    public bool RemoveItem(ItemType item, int count = 1)
    {
        if (count <= 0)
        {
            Debug.LogWarning($"[GameStateManager] 잘못된 아이템 개수: {count}");
            return false;
        }
        
        if (!inventory.ContainsKey(item) || inventory[item] < count)
        {
            Debug.LogWarning($"[GameStateManager] 아이템이 부족합니다: {item} (보유: {inventory.GetValueOrDefault(item, 0)}, 요구: {count})");
            return false;
        }
        
        int oldCount = inventory[item];
        inventory[item] -= count;
        
        if (inventory[item] <= 0)
        {
            inventory.Remove(item);
        }
        
        OnInventoryChanged?.Invoke(item, inventory.GetValueOrDefault(item, 0));
        Debug.Log($"[GameStateManager] 아이템 제거: {item} x{count} (남은 개수: {inventory.GetValueOrDefault(item, 0)})");
        
        return true;
    }

    /// <summary>
    /// 인벤토리에 아이템이 있는지 확인합니다.
    /// </summary>
    /// <param name="item">아이템 타입</param>
    /// <returns>보유 여부</returns>
    public bool HasItem(ItemType item)
    {
        return inventory.ContainsKey(item) && inventory[item] > 0;
    }

    /// <summary>
    /// 인벤토리에 있는 아이템의 개수를 반환합니다.
    /// </summary>
    /// <param name="item">아이템 타입</param>
    /// <returns>아이템 개수</returns>
    public int GetItemCount(ItemType item)
    {
        return inventory.GetValueOrDefault(item, 0);
    }

    // ============================================
    // 월드 아이템 상태 관리 메서드
    // ============================================

    /// <summary>
    /// 아이템의 상태를 설정합니다.
    /// 백엔드에서 받은 아이템 상태 변경 정보를 적용합니다.
    /// </summary>
    /// <param name="item">아이템 타입</param>
    /// <param name="state">새로운 상태 (백엔드에서 제공)</param>
    /// <param name="location">위치 정보 (선택적)</param>
    public void SetItemState(ItemType item, ItemState state, ItemLocation location = null)
    {
        if (!worldItemStates.ContainsKey(item))
        {
            Debug.LogWarning($"[GameStateManager] 월드 아이템 상태를 찾을 수 없습니다: {item}");
            return;
        }
        
        WorldItemState itemState = worldItemStates[item];
        ItemState oldState = itemState.state;
        itemState.state = state;
        
        if (location != null)
        {
            itemState.location = location;
        }
        
        worldItemStates[item] = itemState;
        
        OnItemStateChanged?.Invoke(item, itemState);
        Debug.Log($"[GameStateManager] 아이템 상태 변경: {item} {oldState} → {state}");
    }

    /// <summary>
    /// 아이템의 현재 상태를 반환합니다.
    /// </summary>
    /// <param name="item">아이템 타입</param>
    /// <returns>아이템 상태</returns>
    public ItemState GetItemState(ItemType item)
    {
        if (!worldItemStates.ContainsKey(item))
        {
            Debug.LogWarning($"[GameStateManager] 월드 아이템 상태를 찾을 수 없습니다: {item}");
            return ItemState.InWorld;
        }
        
        return worldItemStates[item].state;
    }

    /// <summary>
    /// 매일 리스폰되는 아이템을 처리합니다 (날짜 변경 시 호출).
    /// </summary>
    /// <param name="currentDay">현재 날짜</param>
    public void RespawnDailyItems(int currentDay)
    {
        foreach (var kvp in worldItemStates)
        {
            WorldItemState itemState = kvp.Value;
            
            // 리스폰 가능하고 인벤토리에 없거나 사용된 경우 리스폰
            if (itemState.isRespawnable && 
                (itemState.state == ItemState.Used || 
                 (!HasItem(kvp.Key) && itemState.state != ItemState.InWorld)))
            {
                itemState.state = ItemState.InWorld;
                worldItemStates[kvp.Key] = itemState;
                
                OnItemStateChanged?.Invoke(kvp.Key, itemState);
                Debug.Log($"[GameStateManager] 아이템 리스폰: {kvp.Key}");
            }
        }
    }

    // ============================================
    // 엔딩 시스템 초기화
    // ============================================

    /// <summary>
    /// 엔딩 조건을 초기화합니다.
    /// </summary>
    private void InitializeEndingConditions()
    {
        endingConditions = new Dictionary<EndingType, EndingCondition>();
        
        // 완벽한 기만 (StealthExit)
        endingConditions[EndingType.StealthExit] = new EndingCondition
        {
            endingType = EndingType.StealthExit,
            hasRequiredItems = false,
            hasRequiredNPCStatus = false,
            hasRequiredLocation = false,
            hasRequiredTime = false,
            customFlags = new Dictionary<string, bool>()
        };
        
        // 혼돈의 밤 (ChaoticBreakout)
        endingConditions[EndingType.ChaoticBreakout] = new EndingCondition
        {
            endingType = EndingType.ChaoticBreakout,
            hasRequiredItems = false,
            hasRequiredNPCStatus = false,
            hasRequiredLocation = false,
            hasRequiredTime = false,
            customFlags = new Dictionary<string, bool>()
        };
        
        // 조력자의 희생 (SiblingsHelp)
        endingConditions[EndingType.SiblingsHelp] = new EndingCondition
        {
            endingType = EndingType.SiblingsHelp,
            hasRequiredItems = false,
            hasRequiredNPCStatus = false,
            hasRequiredLocation = false,
            hasRequiredTime = false,
            customFlags = new Dictionary<string, bool>()
        };
        
        // 불완전한 박제 (UnfinishedDoll) - 이미 구현됨
        endingConditions[EndingType.UnfinishedDoll] = new EndingCondition
        {
            endingType = EndingType.UnfinishedDoll,
            hasRequiredItems = false,
            hasRequiredNPCStatus = false,
            hasRequiredLocation = false,
            hasRequiredTime = false,
            customFlags = new Dictionary<string, bool>()
        };
        
        // 영원한 식사 시간 (EternalDinner)
        endingConditions[EndingType.EternalDinner] = new EndingCondition
        {
            endingType = EndingType.EternalDinner,
            hasRequiredItems = false,
            hasRequiredNPCStatus = false,
            hasRequiredLocation = false,
            hasRequiredTime = false,
            customFlags = new Dictionary<string, bool>()
        };
    }

    /// <summary>
    /// 이벤트 플래그를 초기화합니다.
    /// </summary>
    private void InitializeEventFlags()
    {
        eventFlags = new EventFlags
        {
            grandmotherCooperation = false,
            holeUnlocked = false,
            fireStarted = false,
            familyAsleep = false,
            teaWithSleepingPill = false,
            keyStolen = false,
            caughtByFather = false,
            caughtByMother = false,
            imprisonmentDay = -1,
            customEvents = new Dictionary<string, bool>()
        };
    }

    // ============================================
    // 시간대 및 턴 시스템
    // ============================================

    /// <summary>
    /// 현재 시간대를 설정합니다.
    /// </summary>
    /// <param name="time">시간대</param>
    public void SetTimeOfDay(TimeOfDay time)
    {
        if (currentTimeOfDay == time)
            return;
        
        TimeOfDay oldTime = currentTimeOfDay;
        currentTimeOfDay = time;
        
        OnTimeOfDayChanged?.Invoke(time);
        Debug.Log($"[GameStateManager] 시간대 변경: {oldTime} → {time}");
    }

    /// <summary>
    /// 현재 시간대를 반환합니다.
    /// </summary>
    /// <returns>현재 시간대</returns>
    public TimeOfDay GetCurrentTimeOfDay()
    {
        return currentTimeOfDay;
    }

    /// <summary>
    /// 턴을 소모합니다.
    /// </summary>
    /// <param name="amount">소모할 턴 수 (기본값: 1)</param>
    /// <returns>턴 소모 성공 여부</returns>
    public bool ConsumeTurn(int amount = 1)
    {
        if (currentTurn + amount > MAX_TURNS_PER_DAY)
        {
            Debug.LogWarning($"[GameStateManager] 턴 수가 부족합니다. (현재: {currentTurn}, 최대: {MAX_TURNS_PER_DAY})");
            return false;
        }
        
        currentTurn += amount;
        OnTurnChanged?.Invoke(GetRemainingTurns());
        
        // NPC 무력화 상태 업데이트
        UpdateNPCDisabledStates();
        
        Debug.Log($"[GameStateManager] 턴 소모: {amount} (사용된 턴: {currentTurn}/{MAX_TURNS_PER_DAY})");
        
        // 턴 소진 체크
        if (currentTurn >= MAX_TURNS_PER_DAY)
        {
            OnTurnsExhausted?.Invoke();
            
            // 밤의 대화 전에 엔딩 조건 체크 (수면제 엔딩 등)
            // 백엔드에서 엔딩 트리거를 받아오는 경우 이 체크는 폴백(fallback) 용도
            if (eventFlags.teaWithSleepingPill && HasItem(ItemType.SleepingPill))
            {
                // 수면제 엔딩 조건 충족 시 밤의 대화 전에 엔딩 진입
                EndingType preNightEnding = CheckEndingConditions();
                if (preNightEnding != EndingType.None && 
                    preNightEnding != EndingType.EternalDinner &&
                    preNightEnding != EndingType.UnfinishedDoll)
                {
                    // 엔딩 트리거 (밤의 대화 전에 엔딩으로 진입)
                    TriggerEnding(preNightEnding);
                    return true; // 턴 소모 성공, 엔딩으로 진입
                }
            }
            
            SetTimeOfDay(TimeOfDay.Night);
        }
        
        return true;
    }

    /// <summary>
    /// 남은 턴 수를 반환합니다.
    /// </summary>
    /// <returns>남은 턴 수</returns>
    public int GetRemainingTurns()
    {
        return Mathf.Max(0, MAX_TURNS_PER_DAY - currentTurn);
    }

    /// <summary>
    /// 남은 턴이 있는지 확인합니다.
    /// </summary>
    /// <returns>남은 턴 존재 여부</returns>
    public bool HasRemainingTurns()
    {
        return GetRemainingTurns() > 0;
    }

    // ============================================
    // 위치 관리 메서드
    // ============================================

    /// <summary>
    /// 플레이어의 현재 위치를 설정합니다.
    /// </summary>
    /// <param name="location">위치</param>
    public void SetCurrentLocation(GameLocation location)
    {
        if (currentLocation == location)
            return;
        
        GameLocation oldLocation = currentLocation;
        currentLocation = location;
        
        OnLocationChanged?.Invoke(location);
        Debug.Log($"[GameStateManager] 위치 변경: {oldLocation} → {location}");
    }

    /// <summary>
    /// 플레이어의 현재 위치를 반환합니다.
    /// </summary>
    /// <returns>현재 위치</returns>
    public GameLocation GetCurrentLocation()
    {
        return currentLocation;
    }

    // ============================================
    // 엔딩 조건 체크 메서드
    // ============================================

    /// <summary>
    /// 엔딩의 아이템 조건을 체크합니다.
    /// </summary>
    /// <param name="ending">엔딩 타입</param>
    /// <returns>조건 달성 여부</returns>
    private bool CheckEndingItemCondition(EndingType ending)
    {
        switch (ending)
        {
            case EndingType.StealthExit:
                // 수면제 보유 + 홍차에 수면제를 탔다는 조건 필요
                return HasItem(ItemType.SleepingPill) && eventFlags.teaWithSleepingPill;
                
            case EndingType.ChaoticBreakout:
                // 기름병 + 라이터 보유 필요
                return HasItem(ItemType.OilBottle) && HasItem(ItemType.SilverLighter);
                
            case EndingType.SiblingsHelp:
                // 가족 사진 + 동생의 장난감 보유 필요
                return HasItem(ItemType.RealFamilyPhoto) && HasItem(ItemType.SiblingsToy);
                
            case EndingType.UnfinishedDoll:
                // 인간성 0% (이미 TriggerGameOver에서 처리)
                return humanity <= MIN_HUMANITY;
                
            case EndingType.EternalDinner:
                // 5일차 저녁 식사 전 탈출 엔딩 미달성 (AdvanceToNextDay에서 처리)
                return false;
                
            default:
                return false;
        }
    }

    /// <summary>
    /// 엔딩의 NPC 조건을 체크합니다.
    /// </summary>
    /// <param name="ending">엔딩 타입</param>
    /// <returns>조건 달성 여부</returns>
    private bool CheckEndingNPCCondition(EndingType ending)
    {
        switch (ending)
        {
            case EndingType.StealthExit:
                // 새엄마 호감도 중간 이상 (50 이상)
                return GetAffection(NPCType.NewMother) >= 50f;
                
            case EndingType.ChaoticBreakout:
                // 할머니 협력 상태 (조건부)
                return eventFlags.grandmotherCooperation;
                
            case EndingType.SiblingsHelp:
                // 동생 호감도 최대 (90 이상)
                return GetAffection(NPCType.Sibling) >= 90f;
                
            default:
                return true;
        }
    }

    /// <summary>
    /// 엔딩의 위치 조건을 체크합니다.
    /// </summary>
    /// <param name="ending">엔딩 타입</param>
    /// <returns>조건 달성 여부</returns>
    private bool CheckEndingLocationCondition(EndingType ending)
    {
        switch (ending)
        {
            case EndingType.StealthExit:
                // 위치 무관 (홍차에 수면제를 탔다는 조건으로 대체)
                return true;
                
            case EndingType.ChaoticBreakout:
                // 거실 또는 지하실
                return currentLocation == GameLocation.LivingRoom || 
                       currentLocation == GameLocation.Basement;
                
            case EndingType.SiblingsHelp:
                // 동생의 방 (단둘이 있을 때)
                return currentLocation == GameLocation.SiblingsRoom &&
                       GetNPCLocation(NPCType.Sibling) == GameLocation.SiblingsRoom;
                
            default:
                return true;
        }
    }

    /// <summary>
    /// 엔딩의 시간 조건을 체크합니다.
    /// </summary>
    /// <param name="ending">엔딩 타입</param>
    /// <returns>조건 달성 여부</returns>
    private bool CheckEndingTimeCondition(EndingType ending)
    {
        switch (ending)
        {
            case EndingType.StealthExit:
                // 시간대 무관 (홍차에 수면제를 탔다는 조건으로 대체)
                return true;
                
            case EndingType.ChaoticBreakout:
                // 시간대 무관
                return true;
                
            case EndingType.SiblingsHelp:
                // 낮 시간
                return currentTimeOfDay == TimeOfDay.Day;
                
            default:
                return true;
        }
    }

    /// <summary>
    /// 특정 엔딩을 달성할 수 있는지 확인합니다.
    /// </summary>
    /// <param name="ending">엔딩 타입</param>
    /// <returns>달성 가능 여부</returns>
    public bool CanAchieveEnding(EndingType ending)
    {
        // 1일차는 엔딩 진입 불가
        if (CurrentDay == 1)
            return false;
        
        // 불완전한 박제는 이미 처리됨
        if (ending == EndingType.UnfinishedDoll)
            return humanity <= MIN_HUMANITY;
        
        // 영원한 식사 시간은 5일차 종료 시 처리
        if (ending == EndingType.EternalDinner)
            return false;
        
        bool hasItems = CheckEndingItemCondition(ending);
        bool hasNPCStatus = CheckEndingNPCCondition(ending);
        bool hasLocation = CheckEndingLocationCondition(ending);
        bool hasTime = CheckEndingTimeCondition(ending);
        
        return hasItems && hasNPCStatus && hasLocation && hasTime;
    }

    /// <summary>
    /// 현재 상태에서 달성 가능한 엔딩을 체크합니다.
    /// 주의: 백엔드에서 엔딩 트리거를 받아오는 경우 이 메서드는 폴백(fallback) 용도로만 사용됩니다.
    /// </summary>
    /// <returns>달성 가능한 엔딩 타입 (없으면 None)</returns>
    public EndingType CheckEndingConditions()
    {
        // 불완전한 박제 체크 (최우선)
        if (humanity <= MIN_HUMANITY)
        {
            return EndingType.UnfinishedDoll;
        }
        
        // 1일차는 엔딩 진입 불가
        if (CurrentDay == 1)
        {
            return EndingType.None;
        }
        
        // 각 엔딩 체크 (우선순위: StealthExit > ChaoticBreakout > SiblingsHelp)
        if (CanAchieveEnding(EndingType.StealthExit))
        {
            return EndingType.StealthExit;
        }
        
        if (CanAchieveEnding(EndingType.ChaoticBreakout))
        {
            return EndingType.ChaoticBreakout;
        }
        
        if (CanAchieveEnding(EndingType.SiblingsHelp))
        {
            return EndingType.SiblingsHelp;
        }
        
        return EndingType.None;
    }

    /// <summary>
    /// 엔딩을 트리거합니다.
    /// 백엔드에서 받은 엔딩 트리거 정보를 적용합니다.
    /// </summary>
    /// <param name="ending">엔딩 타입 - 백엔드에서 제공</param>
    public void TriggerEnding(EndingType ending)
    {
        if (ending == EndingType.None)
        {
            Debug.LogWarning("[GameStateManager] 유효하지 않은 엔딩 타입입니다.");
            return;
        }
        
        currentEnding = ending;
        OnEndingTriggered?.Invoke(ending);
        
        Debug.Log($"[GameStateManager] 엔딩 트리거: {ending}");
        
        // 엔딩별 씬 전환 (씬 이름은 Inspector에서 설정)
        string endingSceneName = GetEndingSceneName(ending);
        if (!string.IsNullOrEmpty(endingSceneName))
        {
            SceneFadeManager fadeManager = FindObjectOfType<SceneFadeManager>();
            if (fadeManager != null)
            {
                fadeManager.LoadSceneWithFade(endingSceneName, gameOverFadeDuration);
            }
            else
            {
                SceneManager.LoadScene(endingSceneName);
            }
        }
    }

    /// <summary>
    /// 엔딩 타입에 따른 씬 이름을 반환합니다.
    /// </summary>
    /// <param name="ending">엔딩 타입</param>
    /// <returns>씬 이름</returns>
    private string GetEndingSceneName(EndingType ending)
    {
        // TODO: Inspector에서 설정 가능하도록 수정
        switch (ending)
        {
            case EndingType.StealthExit:
                return "Ending_StealthExit";
            case EndingType.ChaoticBreakout:
                return "Ending_ChaoticBreakout";
            case EndingType.SiblingsHelp:
                return "Ending_SiblingsHelp";
            case EndingType.UnfinishedDoll:
                return "GameOver";
            case EndingType.EternalDinner:
                return "Ending_EternalDinner";
            default:
                return "";
        }
    }

    // ============================================
    // 이벤트 플래그 관리 메서드
    // ============================================

    /// <summary>
    /// 이벤트 플래그를 설정합니다.
    /// 백엔드에서 받은 이벤트 플래그 정보를 적용합니다.
    /// </summary>
    /// <param name="flagName">플래그 이름</param>
    /// <param name="value">값 (백엔드에서 제공)</param>
    public void SetEventFlag(string flagName, bool value)
    {
        switch (flagName.ToLower())
        {
            case "grandmothercooperation":
                eventFlags.grandmotherCooperation = value;
                break;
            case "holeunlocked":
                eventFlags.holeUnlocked = value;
                break;
            case "firestarted":
                eventFlags.fireStarted = value;
                break;
            case "familyasleep":
                eventFlags.familyAsleep = value;
                break;
            case "teawithsleepingpill":
                eventFlags.teaWithSleepingPill = value;
                break;
            case "keystolen":
                eventFlags.keyStolen = value;
                break;
            case "caughtbyfather":
                eventFlags.caughtByFather = value;
                break;
            case "caughtbymother":
                eventFlags.caughtByMother = value;
                break;
            default:
                Debug.LogWarning($"[GameStateManager] 알 수 없는 플래그 이름: {flagName}");
                return;
        }
        
        Debug.Log($"[GameStateManager] 이벤트 플래그 설정: {flagName} = {value}");
    }

    /// <summary>
    /// 이벤트 플래그 값을 반환합니다.
    /// </summary>
    /// <param name="flagName">플래그 이름</param>
    /// <returns>플래그 값</returns>
    public bool GetEventFlag(string flagName)
    {
        switch (flagName.ToLower())
        {
            case "grandmothercooperation":
                return eventFlags.grandmotherCooperation;
            case "holeunlocked":
                return eventFlags.holeUnlocked;
            case "firestarted":
                return eventFlags.fireStarted;
            case "familyasleep":
                return eventFlags.familyAsleep;
            case "teawithsleepingpill":
                return eventFlags.teaWithSleepingPill;
            case "keystolen":
                return eventFlags.keyStolen;
            case "caughtbyfather":
                return eventFlags.caughtByFather;
            case "caughtbymother":
                return eventFlags.caughtByMother;
            default:
                Debug.LogWarning($"[GameStateManager] 알 수 없는 플래그 이름: {flagName}");
                return false;
        }
    }

    /// <summary>
    /// 커스텀 이벤트를 설정합니다.
    /// </summary>
    /// <param name="eventName">이벤트 이름</param>
    /// <param name="value">값</param>
    public void SetCustomEvent(string eventName, bool value)
    {
        if (eventFlags.customEvents == null)
        {
            eventFlags.customEvents = new Dictionary<string, bool>();
        }
        
        eventFlags.customEvents[eventName] = value;
        Debug.Log($"[GameStateManager] 커스텀 이벤트 설정: {eventName} = {value}");
    }

    /// <summary>
    /// 커스텀 이벤트 값을 반환합니다.
    /// </summary>
    /// <param name="eventName">이벤트 이름</param>
    /// <returns>이벤트 값 (없으면 false)</returns>
    public bool GetCustomEvent(string eventName)
    {
        if (eventFlags.customEvents == null || !eventFlags.customEvents.ContainsKey(eventName))
        {
            return false;
        }
        
        return eventFlags.customEvents[eventName];
    }

    /// <summary>
    /// 현재 인간성 수치를 반환합니다 (0~100).
    /// </summary>
    /// <returns>현재 인간성 수치 (0~100)</returns>
    public float GetHumanity()
    {
        return humanity;
    }

    /// <summary>
    /// 현재 날짜를 반환합니다.
    /// </summary>
    public int GetCurrentDay()
    {
        return CurrentDay;
    }

    /// <summary>
    /// 다음 날로 진행합니다. 시간 흐름에 따른 인간성 감소가 적용됩니다.
    /// 밤을 지나면 자동으로 다음 날로 넘어갑니다.
    /// </summary>
    /// <returns>게임 오버가 발생했으면 true, 정상 진행이면 false</returns>
    public bool AdvanceToNextDay()
    {
        if (currentDay < MAX_DAY)
        {
            currentDay++;
            CurrentDay = currentDay; // CurrentDay 프로퍼티를 통해 값 설정 (클램프 포함)
            
            // 인간성 10% 감소 (시간 경과 페널티)
            float oldHumanity = humanity;
            ModifyHumanity(-10f);
            
            // 게임 오버가 발생했는지 확인 (ModifyHumanity 내부에서 TriggerGameOver가 호출됨)
            bool gameOverOccurred = humanity <= MIN_HUMANITY && oldHumanity > MIN_HUMANITY;
            
            if (!gameOverOccurred)
            {
                // 턴 수 리셋
                currentTurn = 0;
                OnTurnChanged?.Invoke(GetRemainingTurns());
                
                // 시간대를 Day로 설정
                SetTimeOfDay(TimeOfDay.Day);
                
                // 아이템 리스폰
                RespawnDailyItems(CurrentDay);
                
                OnDayChanged?.Invoke(CurrentDay);
                Debug.Log($"[GameStateManager] 다음 날로 진행: {CurrentDay}일차 (최대 {MAX_DAY}일차)");
            }
            
            return gameOverOccurred;
        }
        else
        {
            // 5일차 종료 시 엔딩 체크
            Debug.LogWarning($"[GameStateManager] 최대 일수({MAX_DAY}일)에 도달했습니다. 엔딩 조건을 체크합니다.");
            
            EndingType achievableEnding = CheckEndingConditions();
            
            if (achievableEnding != EndingType.None && 
                achievableEnding != EndingType.EternalDinner)
            {
                // 달성 가능한 엔딩이 있으면 트리거
                TriggerEnding(achievableEnding);
                return false;
            }
            else
            {
                // 달성 가능한 엔딩이 없으면 영원한 식사 시간 배드 엔딩
                TriggerEnding(EndingType.EternalDinner);
                return false;
            }
        }
    }

    /// <summary>
    /// 다음 날로 진행합니다. AdvanceToNextDay()의 별칭 메서드입니다.
    /// </summary>
    /// <returns>게임 오버가 발생했으면 true, 정상 진행이면 false</returns>
    public bool AdvanceDay()
    {
        return AdvanceToNextDay();
    }

    /// <summary>
    /// 인간성 수치를 변경합니다.
    /// 백엔드에서 받은 변화량을 적용합니다.
    /// </summary>
    /// <param name="changeAmount">변화량 (양수: 증가, 음수: 감소)</param>
    public void ModifyHumanity(float changeAmount)
    {
        float oldValue = humanity;
        humanity = Mathf.Clamp(humanity + changeAmount, MIN_HUMANITY, MAX_HUMANITY);

        // 이벤트 발생
        OnHumanityChanged?.Invoke(humanity);

        // 게임 오버 체크
        if (humanity <= MIN_HUMANITY && oldValue > MIN_HUMANITY)
        {
            OnHumanityReachedZero?.Invoke();
            TriggerGameOver();
        }

        Debug.Log($"[GameStateManager] 인간성 변경: {oldValue:F1}% → {humanity:F1}% (변화량: {changeAmount:F1})");
    }

    /// <summary>
    /// 테스트용: 인간성 수치를 직접 설정합니다.
    /// </summary>
    /// <param name="value">설정할 인간성 수치 (0~100)</param>
    public void SetHumanity(float value)
    {
        float oldValue = humanity;
        humanity = Mathf.Clamp(value, MIN_HUMANITY, MAX_HUMANITY);
        
        // 이벤트 발생
        OnHumanityChanged?.Invoke(humanity);
        
        // 게임 오버 체크
        if (humanity <= MIN_HUMANITY && oldValue > MIN_HUMANITY)
        {
            OnHumanityReachedZero?.Invoke();
            TriggerGameOver();
        }
        
        Debug.Log($"[GameStateManager] 테스트: 인간성 수치 설정 {oldValue:F1}% → {humanity:F1}%");
    }

    [Header("Game Over Settings")]
    [SerializeField] private string gameOverSceneName = "GameOver";
    [SerializeField] private float gameOverFadeDuration = 1f;

    /// <summary>
    /// 인간성 0% 도달 시 배드 엔딩을 트리거합니다.
    /// </summary>
    private void TriggerGameOver()
    {
        Debug.LogWarning("[GameStateManager] 인간성이 0%에 도달했습니다. 게임 오버!");
        
        // SceneFadeManager를 찾아서 GameOver 씬으로 전환
        SceneFadeManager fadeManager = FindObjectOfType<SceneFadeManager>();
        if (fadeManager != null)
        {
            fadeManager.LoadSceneWithFade(gameOverSceneName, gameOverFadeDuration);
        }
        else
        {
            Debug.LogWarning("[GameStateManager] SceneFadeManager를 찾을 수 없습니다. 페이드 없이 씬을 전환합니다.");
            SceneManager.LoadScene(gameOverSceneName);
        }
    }

    /// <summary>
    /// 최대 일수를 반환합니다.
    /// </summary>
    public int GetMaxDay()
    {
        return MAX_DAY;
    }

    // ============================================
    // NPC 호감도 관리 메서드
    // ============================================

    /// <summary>
    /// NPC 호감도를 변경합니다.
    /// 백엔드에서 받은 변화량을 적용합니다.
    /// </summary>
    /// <param name="npc">NPC 타입</param>
    /// <param name="changeAmount">변화량 (양수: 증가, 음수: 감소) - 백엔드에서 제공</param>
    public void ModifyAffection(NPCType npc, float changeAmount)
    {
        if (!npcStatuses.ContainsKey(npc))
        {
            Debug.LogWarning($"[GameStateManager] NPC 상태를 찾을 수 없습니다: {npc}");
            return;
        }
        
        NPCStatus status = npcStatuses[npc];
        float oldAffection = status.affection;
        status.affection = Mathf.Clamp(status.affection + changeAmount, 0f, 100f);
        
        OnNPCStatusChanged?.Invoke(npc, status);
        Debug.Log($"[GameStateManager] {npc} 호감도 변경: {oldAffection:F1} → {status.affection:F1} (변화량: {changeAmount:F1})");
    }

    /// <summary>
    /// NPC 호감도를 반환합니다.
    /// </summary>
    /// <param name="npc">NPC 타입</param>
    /// <returns>호감도 (0~100)</returns>
    public float GetAffection(NPCType npc)
    {
        if (!npcStatuses.ContainsKey(npc))
        {
            Debug.LogWarning($"[GameStateManager] NPC 상태를 찾을 수 없습니다: {npc}");
            return 0f;
        }
        
        return npcStatuses[npc].affection;
    }

    // ============================================
    // NPC 인간성 관리 메서드
    // ============================================

    /// <summary>
    /// NPC 인간성을 변경합니다. 새엄마는 변경 불가합니다.
    /// 백엔드에서 받은 변화량을 적용합니다.
    /// </summary>
    /// <param name="npc">NPC 타입</param>
    /// <param name="changeAmount">변화량 (양수: 증가, 음수: 감소) - 백엔드에서 제공</param>
    public void ModifyNPCHumanity(NPCType npc, float changeAmount)
    {
        if (npc == NPCType.NewMother)
        {
            Debug.LogWarning("[GameStateManager] 새엄마의 인간성은 변경할 수 없습니다.");
            return;
        }
        
        if (!npcStatuses.ContainsKey(npc))
        {
            Debug.LogWarning($"[GameStateManager] NPC 상태를 찾을 수 없습니다: {npc}");
            return;
        }
        
        NPCStatus status = npcStatuses[npc];
        float oldHumanity = status.humanity;
        status.humanity = Mathf.Clamp(status.humanity + changeAmount, -100f, 100f);
        
        OnNPCStatusChanged?.Invoke(npc, status);
        Debug.Log($"[GameStateManager] {npc} 인간성 변경: {oldHumanity:F1} → {status.humanity:F1} (변화량: {changeAmount:F1})");
    }

    /// <summary>
    /// NPC 인간성을 반환합니다.
    /// </summary>
    /// <param name="npc">NPC 타입</param>
    /// <returns>NPC 인간성 (-100~100)</returns>
    public float GetNPCHumanity(NPCType npc)
    {
        if (!npcStatuses.ContainsKey(npc))
        {
            Debug.LogWarning($"[GameStateManager] NPC 상태를 찾을 수 없습니다: {npc}");
            return 0f;
        }
        
        return npcStatuses[npc].humanity;
    }

    // ============================================
    // NPC 상태 조회 메서드
    // ============================================

    /// <summary>
    /// NPC의 전체 상태를 반환합니다.
    /// </summary>
    /// <param name="npc">NPC 타입</param>
    /// <returns>NPC 상태 (복사본)</returns>
    public NPCStatus GetNPCStatus(NPCType npc)
    {
        if (!npcStatuses.ContainsKey(npc))
        {
            Debug.LogWarning($"[GameStateManager] NPC 상태를 찾을 수 없습니다: {npc}");
            return null;
        }
        
        // 복사본 반환 (원본 수정 방지)
        NPCStatus original = npcStatuses[npc];
        return new NPCStatus
        {
            npcType = original.npcType,
            affection = original.affection,
            humanity = original.humanity,
            isAvailable = original.isAvailable,
            isDisabled = original.isDisabled,
            disabledRemainingTurns = original.disabledRemainingTurns,
            disabledReason = original.disabledReason
        };
    }

    // ============================================
    // NPC 무력화 상태 관리
    // ============================================

    /// <summary>
    /// NPC를 무력화 상태로 설정합니다 (수면제 등).
    /// 백엔드에서 받은 무력화 상태 정보를 적용합니다.
    /// </summary>
    /// <param name="npc">NPC 타입</param>
    /// <param name="turns">무력화 지속 턴 수 (백엔드에서 제공)</param>
    /// <param name="reason">무력화 이유 (백엔드에서 제공)</param>
    public void SetNPCDisabled(NPCType npc, int turns, string reason)
    {
        if (!npcStatuses.ContainsKey(npc))
        {
            Debug.LogWarning($"[GameStateManager] NPC 상태를 찾을 수 없습니다: {npc}");
            return;
        }
        
        NPCStatus status = npcStatuses[npc];
        status.isDisabled = true;
        status.disabledRemainingTurns = turns;
        status.disabledReason = reason;
        status.isAvailable = false;
        
        OnNPCStatusChanged?.Invoke(npc, status);
        Debug.Log($"[GameStateManager] {npc} 무력화: {turns}턴 동안 ({reason})");
    }

    /// <summary>
    /// 턴 경과에 따라 NPC 무력화 상태를 업데이트합니다.
    /// ConsumeTurn()에서 호출됩니다.
    /// </summary>
    public void UpdateNPCDisabledStates()
    {
        foreach (var kvp in npcStatuses)
        {
            NPCStatus status = kvp.Value;
            if (status.isDisabled && status.disabledRemainingTurns > 0)
            {
                status.disabledRemainingTurns--;
                
                if (status.disabledRemainingTurns <= 0)
                {
                    status.isDisabled = false;
                    status.isAvailable = true;
                    status.disabledReason = "";
                    Debug.Log($"[GameStateManager] {kvp.Key} 무력화 해제");
                }
                
                OnNPCStatusChanged?.Invoke(kvp.Key, status);
            }
        }
    }

    // ============================================
    // NPC 위치 관리 메서드
    // ============================================

    /// <summary>
    /// NPC의 현재 위치를 설정합니다.
    /// 백엔드에서 받은 위치 정보를 적용합니다.
    /// 백엔드 응답이 항상 우선: 백엔드에서 NPC 위치를 제공하면 씬 전환 업데이트보다 우선 적용됩니다.
    /// </summary>
    /// <param name="npc">NPC 타입</param>
    /// <param name="location">위치 (백엔드에서 제공)</param>
    public void SetNPCLocation(NPCType npc, GameLocation location)
    {
        if (!npcLocations.ContainsKey(npc))
        {
            npcLocations[npc] = location;
        }
        else
        {
            GameLocation oldLocation = npcLocations[npc];
            npcLocations[npc] = location;
            
            if (oldLocation != location)
            {
                Debug.Log($"[GameStateManager] {npc} 위치 변경: {oldLocation} → {location} (백엔드 응답)");
            }
        }
    }

    /// <summary>
    /// NPC의 현재 위치를 반환합니다.
    /// </summary>
    /// <param name="npc">NPC 타입</param>
    /// <returns>위치</returns>
    public GameLocation GetNPCLocation(NPCType npc)
    {
        if (!npcLocations.ContainsKey(npc))
        {
            Debug.LogWarning($"[GameStateManager] NPC 위치를 찾을 수 없습니다: {npc}");
            return GameLocation.Hallway; // 기본값
        }
        
        return npcLocations[npc];
    }

    /// <summary>
    /// 싱글톤 인스턴스를 완전히 초기화합니다. 게임 재시작 시 사용됩니다.
    /// </summary>
    public static void ClearInstance()
    {
        if (Instance != null)
        {
            GameObject oldInstance = Instance.gameObject;
            Destroy(oldInstance);
            
            // 리플렉션을 사용하여 private setter로 Instance를 null로 설정
            PropertyInfo propertyInfo = typeof(GameStateManager).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            if (propertyInfo != null)
            {
                propertyInfo.SetValue(null, null);
            }
            
            Debug.Log("[GameStateManager] Instance가 초기화되었습니다.");
        }
    }
}

