using UnityEngine;
using UnityEngine.InputSystem;

public class ChestUI : MonoBehaviour
{
    [Header("Ссылки")]
    public GameObject chestWindow;
    public Transform slotsParent;
    public InventorySlotUI slotPrefab;
    public InventoryUI playerInventoryUI;

    private InventorySlotUI[] uiSlots;
    private Chest currentChest;

    private void Start()
    {
        uiSlots = new InventorySlotUI[4];
        for (int i = 0; i < 4; i++)
        {
            InventorySlotUI newSlotUI = Instantiate(slotPrefab, slotsParent);
            newSlotUI.slotIndex = i;
            newSlotUI.ownerChestUI = this;
            uiSlots[i] = newSlotUI;
        }
        chestWindow.SetActive(false);
    }

    private void Update()
    {
        if (chestWindow.activeSelf && Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            CloseAndDestroyChest();
        }
    }

    public void OpenChest(Chest chest)
    {
        currentChest = chest;
        Inventory chestInv = chest.GetInventory();

        for (int i = 0; i < 4; i++)
        {
            uiSlots[i].associatedInventory = chestInv;
        }

        chestWindow.SetActive(true);
        playerInventoryUI.inventoryWindow.SetActive(true);

        RefreshChest();
        playerInventoryUI.RefreshAll();
    }

    public void RefreshChest()
    {
        if (currentChest == null) return;

        for (int i = 0; i < 4; i++)
        {
            InventorySlot logicSlot = currentChest.GetInventory().slots[i];
            InventoryItemUI itemUI = uiSlots[i].GetComponentInChildren<InventoryItemUI>(true);
            if (itemUI != null)
            {
                itemUI.Refresh(logicSlot);
            }
        }
    }

    public void CloseAndDestroyChest()
    {
        if (currentChest != null)
        {
            currentChest.DestroyChest();
            currentChest = null;
        }

        chestWindow.SetActive(false);
        playerInventoryUI.inventoryWindow.SetActive(false);
    }
}
