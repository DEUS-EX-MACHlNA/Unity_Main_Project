using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 인벤토리를 관리하는 매니저입니다.
/// </summary>
public class InventoryManager
{
    private Dictionary<ItemType, int> inventory;

    /// <summary>
    /// 인벤토리 변경 시 호출되는 이벤트입니다.
    /// </summary>
    public event System.Action<ItemType, int> OnInventoryChanged;

    /// <summary>
    /// 초기화합니다.
    /// </summary>
    public void Initialize()
    {
        inventory = new Dictionary<ItemType, int>();
    }

    /// <summary>
    /// 인벤토리에 아이템을 추가합니다.
    /// </summary>
    public void AddItem(ItemType item, int count = 1)
    {
        if (count <= 0)
        {
            Debug.LogWarning($"[InventoryManager] 잘못된 아이템 개수: {count}");
            return;
        }
        
        if (!inventory.ContainsKey(item))
        {
            inventory[item] = 0;
        }
        
        inventory[item] += count;
        
        OnInventoryChanged?.Invoke(item, inventory[item]);
        Debug.Log($"[InventoryManager] 아이템 추가: {item} x{count} (총 {inventory[item]}개)");
    }

    /// <summary>
    /// 인벤토리에서 아이템을 제거합니다.
    /// </summary>
    public bool RemoveItem(ItemType item, int count = 1)
    {
        if (count <= 0)
        {
            Debug.LogWarning($"[InventoryManager] 잘못된 아이템 개수: {count}");
            return false;
        }
        
        if (!inventory.ContainsKey(item) || inventory[item] < count)
        {
            Debug.LogWarning($"[InventoryManager] 아이템이 부족합니다: {item} (보유: {inventory.GetValueOrDefault(item, 0)}, 요구: {count})");
            return false;
        }
        
        inventory[item] -= count;
        
        if (inventory[item] <= 0)
        {
            inventory.Remove(item);
        }
        
        OnInventoryChanged?.Invoke(item, inventory.GetValueOrDefault(item, 0));
        Debug.Log($"[InventoryManager] 아이템 제거: {item} x{count} (남은 개수: {inventory.GetValueOrDefault(item, 0)})");
        
        return true;
    }

    /// <summary>
    /// 인벤토리에 아이템이 있는지 확인합니다.
    /// </summary>
    public bool HasItem(ItemType item)
    {
        return inventory.ContainsKey(item) && inventory[item] > 0;
    }

    /// <summary>
    /// 인벤토리에 있는 아이템의 개수를 반환합니다.
    /// </summary>
    public int GetItemCount(ItemType item)
    {
        return inventory.GetValueOrDefault(item, 0);
    }
}

