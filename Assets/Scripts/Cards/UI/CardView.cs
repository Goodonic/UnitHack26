using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardView : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("UI")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Image artworkImage;

    private CardInstance cardInstance;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas;

    private Transform startParent;
    private Vector3 startPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void Setup(CardInstance instance)
    {
        cardInstance = instance;

        nameText.text = cardInstance.data.cardName;
        descriptionText.text = cardInstance.data.description;
        costText.text = cardInstance.currentCost.ToString();

        if (artworkImage != null && cardInstance.data.artwork != null)
        {
            artworkImage.sprite = cardInstance.data.artwork;
        }
    }

    public CardInstance GetCardInstance()
    {
        return cardInstance;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startParent = transform.parent;
        startPosition = rectTransform.position;

        canvasGroup.alpha = 0.8f;
        canvasGroup.blocksRaycasts = false;

        transform.SetParent(rootCanvas.transform);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        GameObject target = eventData.pointerCurrentRaycast.gameObject;

        if (target == null)
        {
            ReturnToHand();
            return;
        }

        if (CombatManager.Instance != null && !CombatManager.Instance.IsPlayerTurn)
        {
            ReturnToHand();
            return;
        }

        if (target.CompareTag("Enemy"))
        {
            PlayCard();
            return;
        }

        ReturnToHand();
    }

    private void PlayCard()
    {
        CombatManager.Instance.PlayCard(cardInstance);

        Destroy(gameObject);
    }

    private void ReturnToHand()
    {
        transform.SetParent(startParent);
        rectTransform.position = startPosition;
    }
}