using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventoryItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Ссылки на компоненты")]
    public Image itemIcon;
    public TextMeshProUGUI amountText;

    [HideInInspector] public int originalSlotIndex;

    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Transform originalParent;
    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }
    public void Refresh(InventorySlot slot)
    {
        if (slot == null || slot.IsEmpty)
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);
        itemIcon.sprite = slot.itemData.icon;
        amountText.text = slot.amount > 1 ? slot.amount.ToString() : "";
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalSlotIndex = originalParent.GetComponent<InventorySlotUI>().slotIndex;
        transform.SetParent(canvas.transform);

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = Vector2.zero;

        GameObject targetGO = eventData.pointerCurrentRaycast.gameObject;

        if (targetGO != null)
        {
            InventorySlotUI targetSlotUI = targetGO.GetComponent<InventorySlotUI>();
            if (targetSlotUI == null) targetSlotUI = targetGO.GetComponentInParent<InventorySlotUI>();

            InventorySlotUI sourceSlotUI = originalParent.GetComponent<InventorySlotUI>();

            if (targetSlotUI != null && sourceSlotUI != null)
            {
                if (sourceSlotUI.associatedInventory == targetSlotUI.associatedInventory)
                {
                    sourceSlotUI.associatedInventory.SwapSlots(sourceSlotUI.slotIndex, targetSlotUI.slotIndex);
                }
                else
                {
                    sourceSlotUI.associatedInventory.MoveItemToOtherInventory(
                        sourceSlotUI.slotIndex,
                        targetSlotUI.associatedInventory,
                        targetSlotUI.slotIndex
                    );
                }
                if (sourceSlotUI.ownerUI != null) sourceSlotUI.ownerUI.RefreshAll();
                else if (sourceSlotUI.ownerChestUI != null) sourceSlotUI.ownerChestUI.RefreshChest();

                if (targetSlotUI.ownerUI != null) targetSlotUI.ownerUI.RefreshAll();
                else if (targetSlotUI.ownerChestUI != null) targetSlotUI.ownerChestUI.RefreshChest();
                return; 
            }
        }

        InventorySlotUI fallbackSlotUI = originalParent.GetComponent<InventorySlotUI>();
        if (fallbackSlotUI != null && fallbackSlotUI.ownerUI != null)
        {
            fallbackSlotUI.ownerUI.RefreshAll();
        }
    }
}