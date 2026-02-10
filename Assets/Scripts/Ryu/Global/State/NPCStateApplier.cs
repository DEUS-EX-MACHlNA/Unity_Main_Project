using UnityEngine;

/// <summary>
/// NPC 상태 적용을 담당하는 클래스입니다.
/// NPC 호감도, 인간성, 무력화 상태, 위치 변경을 처리합니다.
/// </summary>
public static class NPCStateApplier
{
    /// <summary>
    /// NPC 호감도 변화량을 적용합니다.
    /// </summary>
    /// <param name="manager">GameStateManager 인스턴스</param>
    /// <param name="changes">NPC 호감도 변화량</param>
    public static void ApplyAffectionChanges(GameStateManager manager, NPCAffectionChanges changes)
    {
        if (manager == null || changes == null)
            return;

        if (changes.new_mother != 0f)
            manager.ModifyAffection(NPCType.NewMother, changes.new_mother);
        if (changes.new_father != 0f)
            manager.ModifyAffection(NPCType.NewFather, changes.new_father);
        if (changes.sibling != 0f)
            manager.ModifyAffection(NPCType.Sibling, changes.sibling);
        if (changes.dog != 0f)
            manager.ModifyAffection(NPCType.Dog, changes.dog);
        if (changes.grandmother != 0f)
            manager.ModifyAffection(NPCType.Grandmother, changes.grandmother);
    }

    /// <summary>
    /// NPC 인간성 변화량을 적용합니다.
    /// </summary>
    /// <param name="manager">GameStateManager 인스턴스</param>
    /// <param name="changes">NPC 인간성 변화량</param>
    public static void ApplyHumanityChanges(GameStateManager manager, NPCHumanityChanges changes)
    {
        if (manager == null || changes == null)
            return;

        if (changes.new_father != 0f)
            manager.ModifyNPCHumanity(NPCType.NewFather, changes.new_father);
        if (changes.sibling != 0f)
            manager.ModifyNPCHumanity(NPCType.Sibling, changes.sibling);
        if (changes.dog != 0f)
            manager.ModifyNPCHumanity(NPCType.Dog, changes.dog);
        if (changes.grandmother != 0f)
            manager.ModifyNPCHumanity(NPCType.Grandmother, changes.grandmother);
    }

    /// <summary>
    /// NPC 무력화 상태를 적용합니다.
    /// </summary>
    /// <param name="manager">GameStateManager 인스턴스</param>
    /// <param name="states">NPC 무력화 상태</param>
    public static void ApplyDisabledStates(GameStateManager manager, NPCDisabledStates states)
    {
        if (manager == null || states == null)
            return;

        if (states.new_father != null && states.new_father.is_disabled)
        {
            manager.SetNPCDisabled(
                NPCType.NewFather,
                states.new_father.remaining_turns,
                states.new_father.reason
            );
        }

        if (states.sibling != null && states.sibling.is_disabled)
        {
            manager.SetNPCDisabled(
                NPCType.Sibling,
                states.sibling.remaining_turns,
                states.sibling.reason
            );
        }

        if (states.dog != null && states.dog.is_disabled)
        {
            manager.SetNPCDisabled(
                NPCType.Dog,
                states.dog.remaining_turns,
                states.dog.reason
            );
        }

        if (states.grandmother != null && states.grandmother.is_disabled)
        {
            manager.SetNPCDisabled(
                NPCType.Grandmother,
                states.grandmother.remaining_turns,
                states.grandmother.reason
            );
        }

        // 새엄마는 무력화 불가 (최종보스)
    }

    /// <summary>
    /// NPC 위치를 적용합니다.
    /// </summary>
    /// <param name="manager">GameStateManager 인스턴스</param>
    /// <param name="locations">NPC 위치 정보</param>
    public static void ApplyLocations(GameStateManager manager, NPCLocations locations)
    {
        if (manager == null || locations == null)
            return;

        if (!string.IsNullOrEmpty(locations.new_mother))
        {
            GameLocation location = NameMapper.ConvertLocationNameToType(locations.new_mother);
            manager.SetNPCLocation(NPCType.NewMother, location);
        }

        if (!string.IsNullOrEmpty(locations.new_father))
        {
            GameLocation location = NameMapper.ConvertLocationNameToType(locations.new_father);
            manager.SetNPCLocation(NPCType.NewFather, location);
        }

        if (!string.IsNullOrEmpty(locations.sibling))
        {
            GameLocation location = NameMapper.ConvertLocationNameToType(locations.sibling);
            manager.SetNPCLocation(NPCType.Sibling, location);
        }

        if (!string.IsNullOrEmpty(locations.dog))
        {
            GameLocation location = NameMapper.ConvertLocationNameToType(locations.dog);
            manager.SetNPCLocation(NPCType.Dog, location);
        }

        if (!string.IsNullOrEmpty(locations.grandmother))
        {
            GameLocation location = NameMapper.ConvertLocationNameToType(locations.grandmother);
            manager.SetNPCLocation(NPCType.Grandmother, location);
        }
    }
}

