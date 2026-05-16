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
        // Двигаем иконку за мышкой
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        // Возвращаем предмет в иерархию ячейки
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = Vector2.zero;

        // ДОБАВЬ ЭТУ СТРОКУ:
        // Обновляем визуал всего инвентаря ПОСЛЕ того, как объект вернулся на место
        FindAnyObjectByType<InventoryUI>().RefreshAll();
    }
}
