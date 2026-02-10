using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

// ============================================
// 백엔드 API 응답 데이터 타입 정의
// ============================================

/// <summary>
/// NPC 호감도 변화량
/// </summary>
[Serializable]
public class NPCAffectionChanges
{
    public float new_mother;    // 새엄마 (엘리노어) 호감도 변화량
    public float new_father;    // 새아빠 (아더) 호감도 변화량
    public float sibling;       // 동생 (루카스) 호감도 변화량
    public float dog;           // 강아지 (바론) 호감도 변화량
    public float grandmother;   // 할머니 (마가렛) 호감도 변화량
}

/// <summary>
/// NPC 인간성 변화량
/// </summary>
[Serializable]
public class NPCHumanityChanges
{
    // 새엄마는 인간성 변경 불가 (최종보스)
    public float new_father;    // 새아빠 (아더) 인간성 변화량
    public float sibling;       // 동생 (루카스) 인간성 변화량
    public float dog;           // 강아지 (바론) 인간성 변화량
    public float grandmother;   // 할머니 (마가렛) 인간성 변화량
}

/// <summary>
/// NPC 무력화 상태
/// </summary>
[Serializable]
public class NPCDisabledState
{
    public bool is_disabled;        // 무력화 여부
    public int remaining_turns;      // 무력화 남은 턴 수
    public string reason;           // 무력화 이유
}

/// <summary>
/// NPC 무력화 상태 집합
/// </summary>
[Serializable]
public class NPCDisabledStates
{
    // Unity JsonUtility는 Dictionary를 지원하지 않으므로, 각 NPC별로 개별 필드 사용
    // 백엔드에서 제공하지 않는 NPC는 null 또는 기본값으로 처리
    public NPCDisabledState new_mother;     // 새엄마 무력화 상태 (선택적)
    public NPCDisabledState new_father;     // 새아빠 무력화 상태 (선택적)
    public NPCDisabledState sibling;        // 동생 무력화 상태 (선택적)
    public NPCDisabledState dog;            // 강아지 무력화 상태 (선택적)
    public NPCDisabledState grandmother;   // 할머니 무력화 상태 (선택적)
}

/// <summary>
/// 아이템 획득 정보
/// </summary>
[Serializable]
public class ItemAcquisition
{
    public string item_name;  // 백엔드에서 사용하는 아이템 이름 (예: "sleeping_pill")
    public int count;         // 획득 개수
}

/// <summary>
/// 아이템 소모 정보
/// </summary>
[Serializable]
public class ItemConsumption
{
    public string item_name;  // 백엔드에서 사용하는 아이템 이름 (예: "sleeping_pill")
    public int count;         // 소모 개수
}

/// <summary>
/// 아이템 상태 변경 정보
/// </summary>
[Serializable]
public class ItemStateChange
{
    public string item_name;  // 백엔드에서 사용하는 아이템 이름
    public string new_state;  // 새로운 상태 (예: "used", "in_inventory")
}

/// <summary>
/// 아이템 변화량
/// </summary>
[Serializable]
public class ItemChanges
{
    public ItemAcquisition[] acquired_items;   // 획득된 아이템 목록 (선택적)
    public ItemConsumption[] consumed_items;   // 사용/소모된 아이템 목록 (선택적)
    public ItemStateChange[] state_changes;   // 아이템 상태 변경 목록 (선택적)
}

/// <summary>
/// NPC 위치 정보
/// </summary>
[Serializable]
public class NPCLocations
{
    // Unity JsonUtility는 Dictionary를 지원하지 않으므로, 각 NPC별로 개별 필드 사용
    // 백엔드에서 제공하지 않는 NPC는 null 또는 빈 문자열로 처리
    public string new_mother;     // 새엄마 위치 (예: "kitchen", 선택적)
    public string new_father;     // 새아빠 위치 (예: "living_room", 선택적)
    public string sibling;        // 동생 위치 (예: "siblings_room", 선택적)
    public string dog;            // 강아지 위치 (예: "backyard", 선택적)
    public string grandmother;    // 할머니 위치 (예: "basement", 선택적)
}

// ============================================
// 백엔드 API 응답 구조체 (Json.NET 사용)
// ============================================

/// <summary>
/// 백엔드 응답의 NPC 통계 정보
/// </summary>
[Serializable]
public class BackendNPCStats
{
    public float trust;         // 신뢰도 (→ affection으로 매핑)
    public float suspicion;     // 의심도 (새로운 개념)
    public float fear;          // 공포도 (새로운 개념)
}

/// <summary>
/// 백엔드 응답의 엔딩 정보
/// </summary>
[Serializable]
public class BackendEndingInfo
{
    [JsonProperty("ending_type")]
    public string ending_type;  // 예: "stealth_exit"
    
    [JsonProperty("description")]
    public string description;  // 선택적
}

/// <summary>
/// 백엔드 응답의 상태 변화량 (State Delta)
/// </summary>
[Serializable]
public class BackendStateDelta
{
    [JsonProperty("npc_stats")]
    public Dictionary<string, BackendNPCStats> npc_stats;  // Dictionary 지원!
    
    [JsonProperty("flags")]
    public Dictionary<string, bool> flags;  // Dictionary 지원!
    
    [JsonProperty("inventory_add")]
    public List<string> inventory_add;  // string[] 대신 List 사용 권장
    
    [JsonProperty("inventory_remove")]
    public List<string> inventory_remove;
    
    [JsonProperty("locks")]
    public Dictionary<string, bool> locks;  // 새로운 필드
    
    [JsonProperty("vars")]
    public Dictionary<string, float> vars;  // 새로운 필드
    
    [JsonProperty("turn_increment")]
    public int turn_increment;  // 새로운 필드
}

/// <summary>
/// 백엔드 응답의 스텝 정보
/// </summary>
[Serializable]
public class BackendStepInfo
{
    [JsonProperty("step")]
    public string step;
    
    [JsonProperty("state_delta")]
    public object state_delta;  // 동적 타입
    
    [JsonProperty("reached")]
    public bool reached;
    
    [JsonProperty("newly_unlocked")]
    public List<string> newly_unlocked;
}

/// <summary>
/// 백엔드 응답의 디버그 정보
/// </summary>
[Serializable]
public class BackendDebugInfo
{
    [JsonProperty("game_id")]
    public int game_id;
    
    [JsonProperty("steps")]
    public List<BackendStepInfo> steps;
    
    [JsonProperty("turn_after")]
    public int turn_after;
}

/// <summary>
/// 백엔드 전체 응답 구조
/// </summary>
[Serializable]
public class BackendGameResponse
{
    [JsonProperty("narrative")]
    public string narrative;  // → response로 매핑
    
    [JsonProperty("ending_info")]
    public BackendEndingInfo ending_info;  // → ending_trigger로 변환
    
    [JsonProperty("state_delta")]
    public BackendStateDelta state_delta;
    
    [JsonProperty("debug")]
    public BackendDebugInfo debug;  // 선택적 (로깅용)
}

// ============================================
// Scenario API 응답 구조체
// ============================================

/// <summary>
/// 시나리오 시작 응답
/// </summary>
[Serializable]
public class ScenarioStartResponse
{
    [JsonProperty("game_id")]
    public int game_id;  // 시작된 게임 ID
    
    // 백엔드 응답에 따라 추가 필드 가능
}
