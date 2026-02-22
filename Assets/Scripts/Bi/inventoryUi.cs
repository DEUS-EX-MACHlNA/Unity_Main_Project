using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 인벤토리 UI를 관리합니다.
/// 버튼 슬롯에 아이템을 획득 순서대로 이미지로 표시하고,
/// 클릭 시 InputField에 @아이템id 형식으로 입력합니다.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("인벤토리 슬롯 버튼 (순서대로)")]
    [SerializeField] private Button[] slots;

    [Header("각 슬롯의 Image 컴포넌트 (순서대로)")]
    [SerializeField] private GameObject[] slotImages;

    [Header("아이템 스프라이트")]
    [SerializeField] private Sprite sprSleepingPills;
    [SerializeField] private Sprite sprEarlGreyTea;
    [SerializeField] private Sprite sprRealFamilyPhoto;
    [SerializeField] private Sprite sprWhaleOilCan;
    [SerializeField] private Sprite sprSilverLighter;
    [SerializeField] private Sprite sprOldRobotToy;
    [SerializeField] private Sprite sprBrassKey;

    [Header("아이템 사용 시 입력될 InputField")]
    [SerializeField] private TMP_InputField inputField;

    [Header("인벤토리 패널 (열고 닫을 대상)")]
    [SerializeField] private GameObject invenPanel;

    [Header("인벤토리 열기 버튼 (패널 닫혔을 때 보임)")]
    [SerializeField] private GameObject inventoryButton;

    [Header("인벤토리 닫기 버튼 (패널 열렸을 때 보임)")]
    [SerializeField] private Button inventoryCloseButton;

    [Header("인벤토리 열릴 때 꺼질 오브젝트들")]
    [SerializeField] private GameObject inputFieldBackground;
    [SerializeField] private GameObject inputFieldTMP;
    [SerializeField] private GameObject resultText;

    private List<ItemType> acquiredItems = new List<ItemType>();

    private void Start()
    {
        RefreshUI();

        // 처음엔 패널 닫고, 열기버튼 보이고, 닫기버튼 숨기기
        if (invenPanel != null) invenPanel.SetActive(false);
        if (inventoryButton != null) inventoryButton.SetActive(true);
        if (inventoryCloseButton != null)
        {
            inventoryCloseButton.gameObject.SetActive(false);
            inventoryCloseButton.onClick.AddListener(CloseInventory);
        }

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnInventoryChanged += OnItemAcquired;
    }

    /// <summary>
    /// 인벤토리 열기 (inventoryButton에 연결)
    /// </summary>
    public void OpenInventory()
    {
        if (invenPanel != null) invenPanel.SetActive(true);
        if (inventoryButton != null) inventoryButton.SetActive(false);
        if (inventoryCloseButton != null) inventoryCloseButton.gameObject.SetActive(true);
        if (inputFieldBackground != null) inputFieldBackground.SetActive(false);
        if (inputFieldTMP != null) inputFieldTMP.SetActive(false);
        if (resultText != null) resultText.SetActive(false);
    }

    /// <summary>
    /// 인벤토리 닫기 (inventoryCloseButton에 연결)
    /// </summary>
    public void CloseInventory()
    {
        if (invenPanel != null) invenPanel.SetActive(false);
        if (inventoryButton != null) inventoryButton.SetActive(true);
        if (inventoryCloseButton != null) inventoryCloseButton.gameObject.SetActive(false);
        if (inputFieldBackground != null) inputFieldBackground.SetActive(true);
        if (inputFieldTMP != null) inputFieldTMP.SetActive(true);
        if (resultText != null) resultText.SetActive(true);
    }

    private void OnDestroy()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnInventoryChanged -= OnItemAcquired;
    }

    public void OnItemAcquired(ItemType item, int count)
    {
        if (count > 0 && !acquiredItems.Contains(item))
            acquiredItems.Add(item);
        else if (count <= 0)
            acquiredItems.Remove(item);

        RefreshUI();
    }

    private void RefreshUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < acquiredItems.Count)
            {
                slots[i].gameObject.SetActive(true);
                slots[i].interactable = true;

                ItemType item = acquiredItems[i];

                if (slotImages != null && i < slotImages.Length)
                {
                    Image img = slotImages[i].GetComponent<Image>();
                    if (img != null)
                    {
                        img.sprite = GetItemSprite(item);
                        img.enabled = img.sprite != null;
                        img.preserveAspect = true;
                    }
                }

                int index = i;
                slots[i].onClick.RemoveAllListeners();
                slots[i].onClick.AddListener(() => OnSlotClicked(acquiredItems[index]));
            }
            else
            {
                slots[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnSlotClicked(ItemType item)
    {
        if (inputField == null) return;
        inputField.text += $"@{GetItemId(item)} ";
        inputField.ActivateInputField();
    }

    private Sprite GetItemSprite(ItemType item)
    {
        return item switch
        {
            ItemType.SleepingPills   => sprSleepingPills,
            ItemType.EarlGreyTea     => sprEarlGreyTea,
            ItemType.RealFamilyPhoto => sprRealFamilyPhoto,
            ItemType.WhaleOilCan     => sprWhaleOilCan,
            ItemType.SilverLighter   => sprSilverLighter,
            ItemType.OldRobotToy     => sprOldRobotToy,
            ItemType.BrassKey        => sprBrassKey,
            _                        => null
        };
    }

    private string GetItemId(ItemType item)
    {
        return item switch
        {
            ItemType.SleepingPills   => "sleeping_pill",
            ItemType.EarlGreyTea     => "earl_grey_tea",
            ItemType.RealFamilyPhoto => "real_family_photo",
            ItemType.WhaleOilCan     => "oil_bottle",
            ItemType.SilverLighter   => "silver_lighter",
            ItemType.OldRobotToy     => "siblings_toy",
            ItemType.BrassKey        => "brass_key",
            _                        => item.ToString().ToLower()
        };
    }
}