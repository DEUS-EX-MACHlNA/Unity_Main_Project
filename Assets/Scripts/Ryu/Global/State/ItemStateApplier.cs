using UnityEngine;

/// <summary>
/// 아이템 상태 적용을 담당하는 클래스입니다.
/// 아이템 획득, 소모, 상태 변경을 처리합니다.
/// </summary>
public static class ItemStateApplier
{
    /// <summary>
    /// 아이템 변화량을 적용합니다.
    /// </summary>
    /// <param name="manager">GameStateManager 인스턴스</param>
    /// <param name="changes">아이템 변화량</param>
    public static void ApplyItemChanges(GameStateManager manager, ItemChanges changes)
    {
        if (manager == null || changes == null)
            return;

        // 아이템 획득 처리
        if (changes.acquired_items != null)
        {
            foreach (var acquisition in changes.acquired_items)
            {
                ItemType itemType = NameMapper.ConvertItemNameToType(acquisition.item_name);
                if (itemType != ItemType.None) // None이 아닌 경우만 처리
                {
                    manager.AddItem(itemType, acquisition.count);
                    Debug.Log($"[ItemStateApplier] 아이템 획득: {itemType} x{acquisition.count}");
                }
            }
        }

        // 아이템 사용/소모 처리
        if (changes.consumed_items != null)
        {
            foreach (var consumption in changes.consumed_items)
            {
                ItemType itemType = NameMapper.ConvertItemNameToType(consumption.item_name);
                if (itemType != ItemType.None)
                {
                    manager.RemoveItem(itemType, consumption.count);
                    Debug.Log($"[ItemStateApplier] 아이템 사용/소모: {itemType} x{consumption.count}");
                }
            }
        }

        // 아이템 상태 변경 처리
        if (changes.state_changes != null)
        {
            foreach (var stateChange in changes.state_changes)
            {
                ItemType itemType = NameMapper.ConvertItemNameToType(stateChange.item_name);
                ItemState newState = NameMapper.ConvertItemStateNameToType(stateChange.new_state);
                if (itemType != ItemType.None)
                {
                    manager.SetItemState(itemType, newState);
                    Debug.Log($"[ItemStateApplier] 아이템 상태 변경: {itemType} → {newState}");
                }
            }
        }
    }
}

