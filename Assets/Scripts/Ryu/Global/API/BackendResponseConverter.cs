using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// 백엔드 응답을 Unity 구조로 변환하는 클래스입니다.
/// </summary>
public class BackendResponseConverter
{
    private string mockResponse;

    /// <summary>
    /// BackendResponseConverter 생성자
    /// </summary>
    /// <param name="mockResponse">목업 응답 텍스트</param>
    public BackendResponseConverter(string mockResponse)
    {
        this.mockResponse = mockResponse;
    }

    /// <summary>
    /// 백엔드 응답을 현재 구조로 변환합니다.
    /// </summary>
    public void ConvertBackendResponseToCurrentFormat(
        BackendGameResponse backendResponse,
        GameStateManager gameStateManager,
        out string response,
        out float humanityChange,
        out NPCAffectionChanges affectionChanges,
        out NPCHumanityChanges humanityChanges,
        out NPCDisabledStates disabledStates,
        out ItemChanges itemChanges,
        out EventFlags eventFlags,
        out string endingTrigger)
    {
        // 1. narrative → response
        response = !string.IsNullOrEmpty(backendResponse.narrative)
            ? backendResponse.narrative
            : mockResponse;

        // 2. ending_info → ending_trigger
        endingTrigger = backendResponse.ending_info?.ending_id;

        // 3. state_result 가져오기
        BackendStateDelta stateResult = backendResponse.state_result;
        
        if (stateResult == null)
        {
            Debug.LogWarning("[BackendResponseConverter] state_result가 null입니다!");
            humanityChange = 0f;
            affectionChanges = new NPCAffectionChanges();
            humanityChanges = new NPCHumanityChanges();
            disabledStates = new NPCDisabledStates();
            itemChanges = new ItemChanges();
            eventFlags = null;
            return;
        }

        // 4. humanity 처리 (현재 값 → 변화량 변환)
        if (stateResult.humanity.HasValue && gameStateManager != null)
        {
            float currentHumanity = gameStateManager.GetHumanity();
            float newHumanity = stateResult.humanity.Value;
            humanityChange = newHumanity - currentHumanity;
            Debug.Log($"[BackendResponseConverter] humanity 변화량 계산: {currentHumanity} → {newHumanity} (변화량: {humanityChange})");
        }
        else if (stateResult.humanity.HasValue)
        {
            Debug.LogWarning("[BackendResponseConverter] humanity (현재 값)는 GameStateManager가 없어 변화량을 계산할 수 없습니다. 변화량을 0으로 설정합니다.");
            humanityChange = 0f;
        }
        else
        {
            humanityChange = 0f;
        }

        // 5. state_result.npc_stats → npc_affection_changes, npc_humanity_changes
        affectionChanges = new NPCAffectionChanges();
        humanityChanges = new NPCHumanityChanges();

        if (stateResult.npc_stats != null)
        {
            foreach (var kvp in stateResult.npc_stats)
            {
                string npcName = kvp.Key;
                BackendNPCStats stats = kvp.Value;

                // NPC 이름 매핑 (백엔드 이름 → Unity enum)
                switch (npcName.ToLower())
                {
                    case "stepmother":
                    case "new_mother":
                        // trust를 affection으로 매핑 (값이 0이 아닐 때만 적용)
                        if (stats.trust != 0f)
                            affectionChanges.new_mother = stats.trust;
                        // suspicion은 현재 구조에 없으므로 무시하거나 로깅
                        if (stats.suspicion != 0f)
                            Debug.Log($"[BackendResponseConverter] stepmother suspicion: {stats.suspicion} (현재 구조에 없어 무시됨)");
                        break;

                    case "new_father":
                    case "father":
                        if (stats.trust != 0f)
                            affectionChanges.new_father = stats.trust;
                        if (stats.fear != 0f)
                            humanityChanges.new_father = stats.fear;  // fear를 humanity로 매핑 (임시)
                        break;

                    case "sibling":
                    case "brother":
                        // 백엔드 예시: brother는 fear만 있음
                        if (stats.trust != 0f)
                            affectionChanges.sibling = stats.trust;
                        if (stats.fear != 0f)
                            humanityChanges.sibling = stats.fear;
                        break;

                    case "dog":
                    case "baron":
                        if (stats.trust != 0f)
                            affectionChanges.dog = stats.trust;
                        if (stats.fear != 0f)
                            humanityChanges.dog = stats.fear;
                        break;

                    case "grandmother":
                        if (stats.trust != 0f)
                            affectionChanges.grandmother = stats.trust;
                        if (stats.fear != 0f)
                            humanityChanges.grandmother = stats.fear;
                        break;

                    default:
                        Debug.LogWarning($"[BackendResponseConverter] 알 수 없는 NPC 이름: {npcName}");
                        break;
                }
            }
        }

        // 6. state_result.flags → eventFlags
        eventFlags = null;
        if (stateResult.flags != null && stateResult.flags.Count > 0)
        {
            eventFlags = new EventFlags();

            // Dictionary의 각 플래그를 EventFlags의 고정 필드에 매핑
            foreach (var kvp in stateResult.flags)
            {
                string flagName = kvp.Key;
                bool flagValue = kvp.Value;

                switch (flagName.ToLower())
                {
                    case "met_mother":
                    case "metmother":
                        // 새로운 플래그 - customEvents에 저장
                        if (eventFlags.customEvents == null)
                            eventFlags.customEvents = new Dictionary<string, bool>();
                        eventFlags.customEvents["met_mother"] = flagValue;
                        Debug.Log($"[BackendResponseConverter] 플래그 설정: met_mother = {flagValue}");
                        break;

                    case "heard_rumor":
                    case "heardrumor":
                        // 새로운 플래그 - customEvents에 저장
                        if (eventFlags.customEvents == null)
                            eventFlags.customEvents = new Dictionary<string, bool>();
                        eventFlags.customEvents["heard_rumor"] = flagValue;
                        Debug.Log($"[BackendResponseConverter] 플래그 설정: heard_rumor = {flagValue}");
                        break;

                    case "grandmother_cooperation":
                    case "grandmothercooperation":
                        eventFlags.grandmotherCooperation = flagValue;
                        break;

                    case "hole_unlocked":
                    case "holeunlocked":
                        eventFlags.holeUnlocked = flagValue;
                        break;

                    case "fire_started":
                    case "firestarted":
                        eventFlags.fireStarted = flagValue;
                        break;

                    case "family_asleep":
                    case "familyasleep":
                        eventFlags.familyAsleep = flagValue;
                        break;

                    case "tea_with_sleeping_pill":
                    case "teawithsleepingpill":
                        eventFlags.teaWithSleepingPill = flagValue;
                        break;

                    case "key_stolen":
                    case "keystolen":
                        eventFlags.keyStolen = flagValue;
                        break;

                    default:
                        // 커스텀 플래그는 customEvents Dictionary에 저장
                        if (eventFlags.customEvents == null)
                            eventFlags.customEvents = new Dictionary<string, bool>();
                        eventFlags.customEvents[flagName] = flagValue;
                        Debug.Log($"[BackendResponseConverter] 커스텀 플래그 설정: {flagName} = {flagValue}");
                        break;
                }
            }
        }

        // 7. state_result.inventory_add/remove → itemChanges
        itemChanges = new ItemChanges();

        if (stateResult.inventory_add != null && stateResult.inventory_add.Count > 0)
        {
            List<ItemAcquisition> acquisitions = new List<ItemAcquisition>();
            foreach (string itemName in stateResult.inventory_add)
            {
                acquisitions.Add(new ItemAcquisition
                {
                    item_name = itemName,
                    count = 1
                });
            }
            itemChanges.acquired_items = acquisitions.ToArray();
        }

        if (stateResult.inventory_remove != null && stateResult.inventory_remove.Count > 0)
        {
            List<ItemConsumption> consumptions = new List<ItemConsumption>();
            foreach (string itemName in stateResult.inventory_remove)
            {
                consumptions.Add(new ItemConsumption
                {
                    item_name = itemName,
                    count = 1
                });
            }
            itemChanges.consumed_items = consumptions.ToArray();
        }

        // item_state_changes 처리
        if (stateResult.item_state_changes != null && stateResult.item_state_changes.Count > 0)
        {
            List<ItemStateChange> stateChanges = new List<ItemStateChange>();
            foreach (var stateChange in stateResult.item_state_changes)
            {
                stateChanges.Add(new ItemStateChange
                {
                    item_name = stateChange.item_name,
                    new_state = stateChange.new_state
                });
            }
            itemChanges.state_changes = stateChanges.ToArray();
        }

        // 8. npc_disabled_states 변환
        disabledStates = new NPCDisabledStates();
        if (stateResult.npc_disabled_states != null)
        {
            foreach (var kvp in stateResult.npc_disabled_states)
            {
                string npcName = kvp.Key;
                NPCDisabledState disabledState = kvp.Value;
                
                switch (npcName.ToLower())
                {
                    case "stepmother":
                    case "new_mother":
                        disabledStates.new_mother = disabledState;
                        break;
                    case "new_father":
                    case "father":
                        disabledStates.new_father = disabledState;
                        break;
                    case "sibling":
                    case "brother":
                        disabledStates.sibling = disabledState;
                        break;
                    case "dog":
                    case "baron":
                        disabledStates.dog = disabledState;
                        break;
                    case "grandmother":
                        disabledStates.grandmother = disabledState;
                        break;
                    default:
                        Debug.LogWarning($"[BackendResponseConverter] 알 수 없는 NPC 이름 (disabled_states): {npcName}");
                        break;
                }
            }
        }
    }
}

