using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Play 모드에서 Inspector에 현재 보유 아이템 목록을 표시합니다.
/// 아무 GameObject에 붙이면 GameStateManager 인벤토리를 주기적으로 읽어와 갱신합니다.
/// </summary>
public class InventoryInspectorDisplay : MonoBehaviour
{
    [Serializable]
    public struct InventoryEntry
    {
        public ItemType itemType;
        public int count;

        public InventoryEntry(ItemType itemType, int count)
        {
            this.itemType = itemType;
            this.count = count;
        }
    }

    [Header("인벤토리 (Play 모드에서 자동 갱신)")]
    [Tooltip("보유 중인 아이템만 표시됩니다. 개수는 GameStateManager에서 읽어옵니다.")]
    [SerializeField] private List<InventoryEntry> heldItems = new List<InventoryEntry>();

    [Header("갱신 설정")]
    [Tooltip("갱신 간격(초). 0이면 매 프레임 갱신")]
    [SerializeField] private float updateInterval = 0.3f;

    private float _lastUpdateTime;

    private void Update()
    {
        if (!Application.isPlaying)
            return;

        if (updateInterval > 0f && Time.time - _lastUpdateTime < updateInterval)
            return;

        RefreshFromGameState();
        _lastUpdateTime = Time.time;
    }

    private void OnEnable()
    {
        if (Application.isPlaying)
            RefreshFromGameState();
    }

    /// <summary>
    /// GameStateManager 인벤토리에서 보유 목록을 읽어와 heldItems를 채웁니다.
    /// </summary>
    public void RefreshFromGameState()
    {
        heldItems.Clear();

        if (GameStateManager.Instance == null)
            return;

        foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
        {
            if (type == ItemType.None)
                continue;

            int count = GameStateManager.Instance.GetItemCount(type);
            if (count > 0)
                heldItems.Add(new InventoryEntry(type, count));
        }
    }
}
