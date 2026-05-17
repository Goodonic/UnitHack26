using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Ссылки на логику и объекты")]
    public Inventory inventoryLogic;
    public GameObject inventoryWindow;

    [Header("Панели UI")]
    [Tooltip("Панель для обычных ячеек (0-149)")]
    public Transform contentPanel;
    [Tooltip("Панель для ячеек экипировки (150-155)")]
    public Transform equipmentPanel;

    public ScrollRect scrollRect;

    [Header("Префабы")]
    public InventorySlotUI slotPrefab;
    private InventorySlotUI[] uiSlots;

    public ItemData testItem;
    public int testAmount = 5;

    private void Start()
    {
        InitializeUI();
        if (testItem != null)
        {
            inventoryLogic.AddItem(testItem, testAmount);
        }
        RefreshAll();
        inventoryWindow.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            bool isWindowOpen = inventoryWindow.activeSelf;
            inventoryWindow.SetActive(!isWindowOpen);
            if (!isWindowOpen)
            {
                RefreshAll();
                if (scrollRect != null)
                {
                    scrollRect.verticalNormalizedPosition = 1f;
                }
            }
        }
    }

    private void InitializeUI()
    {
        int totalSlots = inventoryLogic.TotalSlots;
        uiSlots = new InventorySlotUI[totalSlots];

        for (int i = 0; i < totalSlots; i++)
        {
            Transform targetPanel = inventoryLogic.IsEquipmentSlot(i) ? equipmentPanel : contentPanel;

            InventorySlotUI newSlotUI = Instantiate(slotPrefab, targetPanel);
            newSlotUI.slotIndex = i;

            newSlotUI.associatedInventory = inventoryLogic;
            newSlotUI.ownerUI = this;

            uiSlots[i] = newSlotUI;
        }
    }

    public void RefreshAll()
    {
        for (int i = 0; i < inventoryLogic.TotalSlots; i++)
        {
            InventorySlot logicSlot = inventoryLogic.slots[i];
            InventorySlotUI uiSlot = uiSlots[i];

            InventoryItemUI itemUI = uiSlot.GetComponentInChildren<InventoryItemUI>(true);

            if (itemUI != null)
            {
                itemUI.Refresh(logicSlot);
            }
        }
    }
}
