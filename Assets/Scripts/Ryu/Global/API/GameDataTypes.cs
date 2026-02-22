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
/// step 요청에 담을 현재 월드 상태 (백엔드 world_state 동기화용).
/// NPC 무력화 상태가 적용된 뒤 다음 step 요청에 포함되어 백엔드 world_state에 반영됩니다.
/// </summary>
[Serializable]
public class StepRequestWorldState
{
    [JsonProperty("npc_disabled_states")]
    public Dictionary<string, NPCDisabledState> npc_disabled_states;
}

/// <summary>
/// 아이템 획득 정보
/// </summary>
[Serializable]
public class ItemAcquisition
{
    public string item_name;  // 백엔드에서 사용하는 아이템 이름 (예: "sleeping_pill", "oil_bottle", "siblings_toy")
    public int count;         // 획득 개수
}

/// <summary>
/// 아이템 소모 정보
/// </summary>
[Serializable]
public class ItemConsumption
{
    public string item_name;  // 백엔드에서 사용하는 아이템 이름 (예: "sleeping_pill", "oil_bottle", "siblings_toy")
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
/// 백엔드 응답의 엔딩 정보 (선택적)
/// null이면 엔딩 트리거 없음
/// </summary>
[Serializable]
public class BackendEndingInfo
{
    /// <summary>
    /// 엔딩 ID (백엔드가 실제로 보내는 필드)
    /// 예: "stealth_exit_test", "stealth_exit", "chaotic_breakout" 등
    /// </summary>
    [JsonProperty("ending_id")]
    public string ending_id;
    
    /// <summary>
    /// 엔딩 이름 (백엔드가 실제로 보내는 필드)
    /// </summary>
    [JsonProperty("name")]
    public string name;
    
    /// <summary>
    /// 엔딩 에필로그 프롬프트 (백엔드가 실제로 보내는 필드)
    /// </summary>
    [JsonProperty("epilogue_prompt")]
    public string epilogue_prompt;
}

/// <summary>
/// 백엔드 응답의 상태 결과값 (State Result)
/// 스펙 문서 기준: 모든 필드는 선택적(optional)이며, 변화가 없으면 생략 가능합니다.
/// </summary>
[Serializable]
public class BackendStateDelta
{
    /// <summary>
    /// NPC 통계 현재 값 (호감도, 의심도, 공포도)
    /// Key: NPC 이름 (예: "new_mother", "grandmother")
    /// Value: 통계 현재 값 (trust, suspicion, fear)
    /// </summary>
    [JsonProperty("npc_stats")]
    public Dictionary<string, BackendNPCStats> npc_stats;
    
    /// <summary>
    /// 이벤트 플래그 (게임 이벤트 발생 여부)
    /// Key: 플래그 이름 (예: "tea_with_sleeping_pill", "fire_started")
    /// Value: 플래그 값 (bool)
    /// </summary>
    [JsonProperty("flags")]
    public Dictionary<string, bool> flags;
    
    /// <summary>
    /// 획득한 아이템 목록
    /// 예: ["sleeping_pill", "earl_grey_tea"]
    /// </summary>
    [JsonProperty("inventory_add")]
    public List<string> inventory_add;
    
    /// <summary>
    /// 소모/사용한 아이템 목록
    /// 예: ["sleeping_pill", "earl_grey_tea"]
    /// </summary>
    [JsonProperty("inventory_remove")]
    public List<string> inventory_remove;
    
    /// <summary>
    /// 아이템 상태 변경 목록
    /// 예: [{"item_name": "earl_grey_tea", "new_state": "used"}]
    /// </summary>
    [JsonProperty("item_state_changes")]
    public List<ItemStateChange> item_state_changes;
    
    /// <summary>
    /// NPC 무력화 상태
    /// Key: NPC 이름 (예: "grandmother", "new_father")
    /// Value: 무력화 상태 정보 (is_disabled, remaining_turns, reason)
    /// </summary>
    [JsonProperty("npc_disabled_states")]
    public Dictionary<string, NPCDisabledState> npc_disabled_states;
    
    /// <summary>
    /// 플레이어 인간성 현재 값 (스펙 문서 기준)
    /// 0~100 범위의 값
    /// </summary>
    [JsonProperty("humanity")]
    public float? humanity;
    
    /// <summary>
    /// 기타 커스텀 변수
    /// Key: 변수 이름
    /// Value: 변수 값 (float)
    /// </summary>
    [JsonProperty("vars")]
    public Dictionary<string, float> vars;
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
/// 백엔드 응답의 디버그 정보 (선택적)
/// </summary>
[Serializable]
public class BackendDebugInfo
{
    [JsonProperty("game_id")]
    public int game_id;
    
    /// <summary>
    /// 백엔드가 판단한 근거 (디버깅용)
    /// 예: "플레이어가 할머니에게 수면제를 탄 홍차를 제공했습니다..."
    /// </summary>
    [JsonProperty("reasoning")]
    public string reasoning;
    
    [JsonProperty("steps")]
    public List<BackendStepInfo> steps;
    
    [JsonProperty("turn_after")]
    public int turn_after;
}

/// <summary>
/// 백엔드 전체 응답 구조 (스펙 문서 기준)
/// </summary>
[Serializable]
public class BackendGameResponse
{
    [JsonProperty("narrative")]
    public string narrative;  // → response로 매핑
    
    [JsonProperty("ending_info")]
    public BackendEndingInfo ending_info;  // → ending_trigger로 변환
    
    [JsonProperty("state_result")]
    public BackendStateDelta state_result;  // 스펙 문서 기준: state_result (필수)
    
    [JsonProperty("debug")]
    public BackendDebugInfo debug;  // 선택적 (로깅용)
}

/// <summary>
/// 시나리오 시작 API 응답 구조
/// </summary>
[Serializable]
public class ScenarioStartResponse
{
    [JsonProperty("game_id")]
    public int game_id;
    
    [JsonProperty("user_id")]
    public int user_id;
}
