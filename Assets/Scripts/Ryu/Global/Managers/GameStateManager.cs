using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 게임 전반의 상태를 중앙에서 관리하는 싱글톤 매니저입니다.
/// 각 시스템 매니저들을 조율하는 오케스트레이터 역할을 합니다.
/// </summary>
public class GameStateManager : MonoBehaviour
{
    // Singleton 인스턴스
    public static GameStateManager Instance { get; private set; }

    // 각 시스템 매니저 인스턴스
    private HumanityManager humanityManager;
    private DayManager dayManager;
    private NPCManager npcManager;
    private InventoryManager inventoryManager;
    private ItemStateManager itemStateManager;
    private TurnManager turnManager;
    private LocationManager locationManager;
    private EventFlagManager eventFlagManager;
    private EndingManager endingManager;

    // 잠금 상태 저장 (문, 상자 등)
    private Dictionary<string, bool> locks;

    [Header("Game Over Settings")]
    [SerializeField] private string gameOverSceneName = "GameOver";
    [SerializeField] private float gameOverFadeDuration = 1f;

    // ============================================
    // 이벤트 시스템 (기존 코드와의 호환성을 위해 유지)
    // ============================================

    /// <summary>
    /// 인간성 변경 시 호출되는 이벤트입니다.
    /// </summary>
    public event Action<float> OnHumanityChanged;

    /// <summary>
    /// 인간성 0% 도달 시 호출되는 이벤트입니다.
    /// </summary>
    public event Action OnHumanityReachedZero;

    /// <summary>
    /// 날짜가 변경될 때 호출되는 이벤트입니다.
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
    /// 현재 날짜를 반환합니다 (1~5일차).
    /// </summary>
    public int CurrentDay 
    { 
        get { return dayManager?.GetCurrentDay() ?? 1; }
    }

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

        // 각 매니저 인스턴스 생성 및 초기화
        InitializeManagers();
        
        // 이벤트 연결
        ConnectEvents();
    }

    /// <summary>
    /// 각 매니저를 초기화합니다.
    /// </summary>
    private void InitializeManagers()
    {
        // 기본 매니저들 초기화
        humanityManager = new HumanityManager();
        humanityManager.Initialize(10f); // 테스트용: 10%로 설정
        humanityManager.SetGameOverSettings(gameOverSceneName, gameOverFadeDuration);

        dayManager = new DayManager();
        dayManager.Initialize(1);

        npcManager = new NPCManager();
        npcManager.Initialize();

        inventoryManager = new InventoryManager();
        inventoryManager.Initialize();

        itemStateManager = new ItemStateManager();
        itemStateManager.Initialize(inventoryManager);

        turnManager = new TurnManager();

        locationManager = new LocationManager();
        locationManager.Initialize(npcManager);

        eventFlagManager = new EventFlagManager();
        eventFlagManager.Initialize();

        // 잠금 상태 초기화
        locks = new Dictionary<string, bool>();

        // EndingManager는 다른 매니저들에 의존하므로 마지막에 초기화
        endingManager = new EndingManager();
        endingManager.Initialize(
            humanityManager,
            dayManager,
            inventoryManager,
            npcManager,
            locationManager,
            turnManager,
            eventFlagManager);
        endingManager.SetGameOverSettings(gameOverSceneName, gameOverFadeDuration);
    }

    /// <summary>
    /// 각 매니저의 이벤트를 연결합니다.
    /// </summary>
    private void ConnectEvents()
    {
        if (humanityManager != null)
        {
            humanityManager.OnHumanityChanged += (value) => OnHumanityChanged?.Invoke(value);
            humanityManager.OnHumanityReachedZero += () => OnHumanityReachedZero?.Invoke();
        }

        if (dayManager != null)
        {
            dayManager.OnDayChanged += (day) => OnDayChanged?.Invoke(day);
        }

        if (npcManager != null)
        {
            npcManager.OnNPCStatusChanged += (npc, status) => OnNPCStatusChanged?.Invoke(npc, status);
        }

        if (inventoryManager != null)
        {
            inventoryManager.OnInventoryChanged += (item, count) => OnInventoryChanged?.Invoke(item, count);
        }

        if (itemStateManager != null)
        {
            itemStateManager.OnItemStateChanged += (item, state) => OnItemStateChanged?.Invoke(item, state);
        }

        if (turnManager != null)
        {
            turnManager.OnTimeOfDayChanged += (time) => OnTimeOfDayChanged?.Invoke(time);
            turnManager.OnTurnChanged += (remaining) => OnTurnChanged?.Invoke(remaining);
            turnManager.OnTurnsExhausted += () => OnTurnsExhausted?.Invoke();
        }

        if (locationManager != null)
        {
            locationManager.OnLocationChanged += (location) => OnLocationChanged?.Invoke(location);
        }

        if (endingManager != null)
        {
            endingManager.OnEndingTriggered += (ending) => OnEndingTriggered?.Invoke(ending);
        }
    }

    // ============================================
    // 인간성 관리 메서드 (위임)
    // ============================================

    public float GetHumanity()
    {
        return humanityManager?.GetHumanity() ?? 100f;
    }

    public void ModifyHumanity(float changeAmount)
    {
        humanityManager?.ModifyHumanity(changeAmount);
    }

    public void SetHumanity(float value)
    {
        humanityManager?.SetHumanity(value);
    }

    // ============================================
    // 날짜 관리 메서드 (위임)
    // ============================================

    public int GetCurrentDay()
    {
        return dayManager?.GetCurrentDay() ?? 1;
    }

    public int GetMaxDay()
    {
        return dayManager?.GetMaxDay() ?? 5;
    }

    public bool AdvanceToNextDay()
    {
        if (dayManager == null) return false;

        bool reachedMaxDay = dayManager.AdvanceToNextDay();
        
        if (!reachedMaxDay)
        {
            // 인간성 10% 감소 (시간 경과 페널티)
            float oldHumanity = humanityManager.GetHumanity();
            humanityManager.ModifyHumanity(-10f);
            
            // 게임 오버가 발생했는지 확인
            bool gameOverOccurred = humanityManager.IsHumanityZero() && oldHumanity > 0f;
            
            if (!gameOverOccurred)
            {
                // 턴 수 리셋
                turnManager?.ResetTurns();
                
                // 시간대를 Day로 설정
                turnManager?.SetTimeOfDay(TimeOfDay.Day);
                
                // 아이템 리스폰
                itemStateManager?.RespawnDailyItems(dayManager.GetCurrentDay());
            }
            
            return gameOverOccurred;
        }
        else
        {
            // 5일차 종료 시 엔딩 체크
            EndingType achievableEnding = endingManager?.CheckEndingConditions() ?? EndingType.None;
            
            if (achievableEnding != EndingType.None && 
                achievableEnding != EndingType.EternalDinner)
            {
                endingManager?.TriggerEnding(achievableEnding);
                return false;
            }
            else
            {
                endingManager?.TriggerEnding(EndingType.EternalDinner);
                return false;
            }
        }
    }

    public bool AdvanceDay()
    {
        return AdvanceToNextDay();
    }

    // ============================================
    // NPC 관리 메서드 (위임)
    // ============================================

    public void ModifyAffection(NPCType npc, float changeAmount)
    {
        npcManager?.ModifyAffection(npc, changeAmount);
    }

    public float GetAffection(NPCType npc)
    {
        return npcManager?.GetAffection(npc) ?? 0f;
    }

    public void ModifyNPCHumanity(NPCType npc, float changeAmount)
    {
        npcManager?.ModifyNPCHumanity(npc, changeAmount);
    }

    public float GetNPCHumanity(NPCType npc)
    {
        return npcManager?.GetNPCHumanity(npc) ?? 0f;
    }

    public NPCStatus GetNPCStatus(NPCType npc)
    {
        return npcManager?.GetNPCStatus(npc);
    }

    public void SetNPCDisabled(NPCType npc, int turns, string reason)
    {
        npcManager?.SetNPCDisabled(npc, turns, reason);
    }

    public void UpdateNPCDisabledStates()
    {
        npcManager?.UpdateNPCDisabledStates();
    }

    public void SetNPCLocation(NPCType npc, GameLocation location)
    {
        npcManager?.SetNPCLocation(npc, location);
    }

    public GameLocation GetNPCLocation(NPCType npc)
    {
        return npcManager?.GetNPCLocation(npc) ?? GameLocation.Hallway;
    }

    // ============================================
    // 인벤토리 관리 메서드 (위임)
    // ============================================

    public void AddItem(ItemType item, int count = 1)
    {
        if (inventoryManager != null)
        {
            inventoryManager.AddItem(item, count);
            itemStateManager?.OnItemAddedToInventory(item);
        }
    }

    public bool RemoveItem(ItemType item, int count = 1)
    {
        return inventoryManager?.RemoveItem(item, count) ?? false;
    }

    public bool HasItem(ItemType item)
    {
        return inventoryManager?.HasItem(item) ?? false;
    }

    public int GetItemCount(ItemType item)
    {
        return inventoryManager?.GetItemCount(item) ?? 0;
    }

    // ============================================
    // 아이템 상태 관리 메서드 (위임)
    // ============================================

    public void SetItemState(ItemType item, ItemState state, ItemLocation location = null)
    {
        itemStateManager?.SetItemState(item, state, location);
    }

    public ItemState GetItemState(ItemType item)
    {
        return itemStateManager?.GetItemState(item) ?? ItemState.InWorld;
    }

    public void RespawnDailyItems(int currentDay)
    {
        itemStateManager?.RespawnDailyItems(currentDay);
    }

    // ============================================
    // 턴 및 시간대 관리 메서드 (위임)
    // ============================================

    public void SetTimeOfDay(TimeOfDay time)
    {
        turnManager?.SetTimeOfDay(time);
    }

    public TimeOfDay GetCurrentTimeOfDay()
    {
        return turnManager?.GetCurrentTimeOfDay() ?? TimeOfDay.Day;
    }

    public bool ConsumeTurn(int amount = 1)
    {
        if (turnManager == null) return false;

        bool success = turnManager.ConsumeTurn(amount);
        
        if (success)
        {
            // NPC 무력화 상태 업데이트
            npcManager?.UpdateNPCDisabledStates();
            
            // 턴 소진 시 엔딩 체크
            if (!turnManager.HasRemainingTurns())
            {
                // 밤의 대화 전에 엔딩 조건 체크 (수면제 엔딩 등)
                if (eventFlagManager?.GetEventFlag("teawithsleepingpill") == true && 
                    inventoryManager?.HasItem(ItemType.SleepingPill) == true)
                {
                    EndingType preNightEnding = endingManager?.CheckEndingConditions() ?? EndingType.None;
                    if (preNightEnding != EndingType.None && 
                        preNightEnding != EndingType.EternalDinner &&
                        preNightEnding != EndingType.UnfinishedDoll)
                    {
                        endingManager?.TriggerEnding(preNightEnding);
                        return true;
                    }
                }
            }
        }
        
        return success;
    }

    public int GetRemainingTurns()
    {
        return turnManager?.GetRemainingTurns() ?? 0;
    }

    public bool HasRemainingTurns()
    {
        return turnManager?.HasRemainingTurns() ?? false;
    }

    // ============================================
    // 위치 관리 메서드 (위임)
    // ============================================

    public void SetCurrentLocation(GameLocation location)
    {
        locationManager?.SetCurrentLocation(location);
    }

    public GameLocation GetCurrentLocation()
    {
        return locationManager?.GetCurrentLocation() ?? GameLocation.Hallway;
    }

    // ============================================
    // 엔딩 관리 메서드 (위임)
    // ============================================

    public bool CanAchieveEnding(EndingType ending)
    {
        return endingManager?.CanAchieveEnding(ending) ?? false;
    }

    public EndingType CheckEndingConditions()
    {
        return endingManager?.CheckEndingConditions() ?? EndingType.None;
    }

    public void TriggerEnding(EndingType ending)
    {
        endingManager?.TriggerEnding(ending);
    }

    // ============================================
    // 이벤트 플래그 관리 메서드 (위임)
    // ============================================

    public void SetEventFlag(string flagName, bool value)
    {
        eventFlagManager?.SetEventFlag(flagName, value);
    }

    public bool GetEventFlag(string flagName)
    {
        return eventFlagManager?.GetEventFlag(flagName) ?? false;
    }

    public void SetCustomEvent(string eventName, bool value)
    {
        eventFlagManager?.SetCustomEvent(eventName, value);
    }

    public bool GetCustomEvent(string eventName)
    {
        return eventFlagManager?.GetCustomEvent(eventName) ?? false;
    }

    // ============================================
    // 잠금 상태 관리 메서드
    // ============================================

    /// <summary>
    /// 잠금 상태를 설정합니다.
    /// </summary>
    /// <param name="lockName">잠금 이름 (예: "basement_door", "siblings_room_door")</param>
    /// <param name="isLocked">잠금 여부 (true = 잠금, false = 해제)</param>
    public void SetLock(string lockName, bool isLocked)
    {
        if (locks == null)
        {
            locks = new Dictionary<string, bool>();
        }

        locks[lockName] = isLocked;
        Debug.Log($"[GameStateManager] 잠금 상태 변경: {lockName} = {isLocked}");
    }

    /// <summary>
    /// 잠금 상태를 조회합니다.
    /// </summary>
    /// <param name="lockName">잠금 이름</param>
    /// <returns>잠금 여부 (true = 잠금, false = 해제). 존재하지 않으면 false 반환</returns>
    public bool IsLocked(string lockName)
    {
        if (locks == null || !locks.ContainsKey(lockName))
        {
            return false; // 기본값: 잠금 해제
        }

        return locks[lockName];
    }

    /// <summary>
    /// 모든 잠금 상태를 일괄 적용합니다.
    /// </summary>
    /// <param name="locksToApply">적용할 잠금 상태 Dictionary</param>
    public void ApplyLocks(Dictionary<string, bool> locksToApply)
    {
        if (locksToApply == null || locksToApply.Count == 0)
            return;

        if (locks == null)
        {
            locks = new Dictionary<string, bool>();
        }

        foreach (var kvp in locksToApply)
        {
            locks[kvp.Key] = kvp.Value;
            Debug.Log($"[GameStateManager] 잠금 상태 적용: {kvp.Key} = {kvp.Value}");
        }
    }

    // ============================================
    // 싱글톤 정리
    // ============================================

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
