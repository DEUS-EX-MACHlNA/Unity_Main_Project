using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 엔딩 조건을 체크하고 트리거하는 매니저입니다.
/// </summary>
public class EndingManager
{
    private Dictionary<EndingType, EndingCondition> endingConditions;
    private EndingType currentEnding = EndingType.None;
    
    private HumanityManager humanityManager;
    private DayManager dayManager;
    private InventoryManager inventoryManager;
    private NPCManager npcManager;
    private LocationManager locationManager;
    private TurnManager turnManager;
    private EventFlagManager eventFlagManager;
    
    private string gameOverSceneName = "GameOver";
    private float gameOverFadeDuration = 1f;

    /// <summary>
    /// 엔딩 트리거 시 호출되는 이벤트입니다.
    /// </summary>
    public event System.Action<EndingType> OnEndingTriggered;

    /// <summary>
    /// 초기화합니다.
    /// </summary>
    public void Initialize(
        HumanityManager humanityMgr,
        DayManager dayMgr,
        InventoryManager inventoryMgr,
        NPCManager npcMgr,
        LocationManager locationMgr,
        TurnManager turnMgr,
        EventFlagManager eventFlagMgr)
    {
        humanityManager = humanityMgr;
        dayManager = dayMgr;
        inventoryManager = inventoryMgr;
        npcManager = npcMgr;
        locationManager = locationMgr;
        turnManager = turnMgr;
        eventFlagManager = eventFlagMgr;
        
        endingConditions = new Dictionary<EndingType, EndingCondition>();
        InitializeEndingConditions();
    }

    /// <summary>
    /// 엔딩 조건을 초기화합니다.
    /// </summary>
    private void InitializeEndingConditions()
    {
        endingConditions[EndingType.StealthExit] = new EndingCondition
        {
            endingType = EndingType.StealthExit,
            hasRequiredItems = false,
            hasRequiredNPCStatus = false,
            hasRequiredLocation = false,
            hasRequiredTime = false,
            customFlags = new Dictionary<string, bool>()
        };
        
        endingConditions[EndingType.ChaoticBreakout] = new EndingCondition
        {
            endingType = EndingType.ChaoticBreakout,
            hasRequiredItems = false,
            hasRequiredNPCStatus = false,
            hasRequiredLocation = false,
            hasRequiredTime = false,
            customFlags = new Dictionary<string, bool>()
        };
        
        endingConditions[EndingType.SiblingsHelp] = new EndingCondition
        {
            endingType = EndingType.SiblingsHelp,
            hasRequiredItems = false,
            hasRequiredNPCStatus = false,
            hasRequiredLocation = false,
            hasRequiredTime = false,
            customFlags = new Dictionary<string, bool>()
        };
        
        endingConditions[EndingType.UnfinishedDoll] = new EndingCondition
        {
            endingType = EndingType.UnfinishedDoll,
            hasRequiredItems = false,
            hasRequiredNPCStatus = false,
            hasRequiredLocation = false,
            hasRequiredTime = false,
            customFlags = new Dictionary<string, bool>()
        };
        
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
    /// 엔딩의 아이템 조건을 체크합니다.
    /// </summary>
    private bool CheckEndingItemCondition(EndingType ending)
    {
        switch (ending)
        {
            case EndingType.StealthExit:
                return inventoryManager.HasItem(ItemType.SleepingPill) && 
                       eventFlagManager.GetEventFlag("teawithsleepingpill");
                
            case EndingType.ChaoticBreakout:
                return inventoryManager.HasItem(ItemType.OilBottle) && 
                       inventoryManager.HasItem(ItemType.SilverLighter);
                
            case EndingType.SiblingsHelp:
                return inventoryManager.HasItem(ItemType.RealFamilyPhoto) && 
                       inventoryManager.HasItem(ItemType.SiblingsToy);
                
            case EndingType.UnfinishedDoll:
                return humanityManager.IsHumanityZero();
                
            case EndingType.EternalDinner:
                return false;
                
            default:
                return false;
        }
    }

    /// <summary>
    /// 엔딩의 NPC 조건을 체크합니다.
    /// </summary>
    private bool CheckEndingNPCCondition(EndingType ending)
    {
        switch (ending)
        {
            case EndingType.StealthExit:
                return npcManager.GetAffection(NPCType.NewMother) >= 50f;
                
            case EndingType.ChaoticBreakout:
                return eventFlagManager.GetEventFlag("grandmothercooperation");
                
            case EndingType.SiblingsHelp:
                return npcManager.GetAffection(NPCType.Sibling) >= 90f;
                
            default:
                return true;
        }
    }

    /// <summary>
    /// 엔딩의 위치 조건을 체크합니다.
    /// </summary>
    private bool CheckEndingLocationCondition(EndingType ending)
    {
        switch (ending)
        {
            case EndingType.StealthExit:
                return true;
                
            case EndingType.ChaoticBreakout:
                GameLocation currentLoc = locationManager.GetCurrentLocation();
                return currentLoc == GameLocation.LivingRoom || 
                       currentLoc == GameLocation.Basement;
                
            case EndingType.SiblingsHelp:
                return locationManager.GetCurrentLocation() == GameLocation.SiblingsRoom &&
                       locationManager.GetNPCLocation(NPCType.Sibling) == GameLocation.SiblingsRoom;
                
            default:
                return true;
        }
    }

    /// <summary>
    /// 엔딩의 시간 조건을 체크합니다.
    /// </summary>
    private bool CheckEndingTimeCondition(EndingType ending)
    {
        switch (ending)
        {
            case EndingType.StealthExit:
            case EndingType.ChaoticBreakout:
                return true;
                
            case EndingType.SiblingsHelp:
                return turnManager.GetCurrentTimeOfDay() == TimeOfDay.Day;
                
            default:
                return true;
        }
    }

    /// <summary>
    /// 특정 엔딩을 달성할 수 있는지 확인합니다.
    /// </summary>
    public bool CanAchieveEnding(EndingType ending)
    {
        // 1일차는 엔딩 진입 불가
        if (dayManager.GetCurrentDay() == 1)
            return false;
        
        // 불완전한 박제는 이미 처리됨
        if (ending == EndingType.UnfinishedDoll)
            return humanityManager.IsHumanityZero();
        
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
    /// </summary>
    public EndingType CheckEndingConditions()
    {
        // 불완전한 박제 체크 (최우선)
        if (humanityManager.IsHumanityZero())
        {
            return EndingType.UnfinishedDoll;
        }
        
        // 1일차는 엔딩 진입 불가
        if (dayManager.GetCurrentDay() == 1)
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
    /// </summary>
    public void TriggerEnding(EndingType ending)
    {
        if (ending == EndingType.None)
        {
            Debug.LogWarning("[EndingManager] 유효하지 않은 엔딩 타입입니다.");
            return;
        }
        
        currentEnding = ending;
        OnEndingTriggered?.Invoke(ending);
        
        Debug.Log($"[EndingManager] 엔딩 트리거: {ending}");
        
        // 엔딩별 씬 전환
        string endingSceneName = GetEndingSceneName(ending);
        if (!string.IsNullOrEmpty(endingSceneName))
        {
            SceneFadeManager fadeManager = Object.FindObjectOfType<SceneFadeManager>();
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
    private string GetEndingSceneName(EndingType ending)
    {
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

    /// <summary>
    /// 게임 오버 설정을 설정합니다.
    /// </summary>
    public void SetGameOverSettings(string sceneName, float fadeDuration)
    {
        gameOverSceneName = sceneName;
        gameOverFadeDuration = fadeDuration;
    }
}

