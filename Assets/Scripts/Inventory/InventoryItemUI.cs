using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public ItemData itemData;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;

    private Vector2 lastAnchoredPosition;
    private float currentCellSize;

    private Vector2 dragOffset;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void SetItem(ItemData data, float cellSize, float spacing = 0f)
    {
        itemData = data;
        currentCellSize = cellSize;
        GetComponent<Image>().sprite = data.icon;

        float totalWidth = data.width * cellSize + (data.width - 1) * spacing;
        float totalHeight = data.height * cellSize + (data.height - 1) * spacing;

        rectTransform.sizeDelta = new Vector2(totalWidth, totalHeight);
    }

    public void SetPosition(int x, int y, float cellSize, float spacing = 0f)
    {
        float step = cellSize + spacing;

        rectTransform.anchoredPosition = new Vector2(x * step, -y * step);
        lastAnchoredPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;

        dragOffset = (Vector2)rectTransform.position - eventData.position;

        transform.SetParent(transform.root);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.7f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position + dragOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        transform.SetParent(originalParent);

        rectTransform.anchoredPosition = lastAnchoredPosition;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Вызываем нашу новую панель и передаем ей данные предмета
            if (ItemInfoPanel.Instance != null)
            {
                ItemInfoPanel.Instance.ShowInfo(itemData);
            }
        }
        // Твой старый код для правой кнопки (контекстное меню)
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            ContextMenu.Instance.Show(itemData, eventData.position);
        }
    }
}
