using UnityEngine;
using UnityEngine.EventSystems;

public class PlayZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        CardView card = eventData.pointerDrag.GetComponent<CardView>();

        if (card == null) return;

        Debug.Log("Card played!");

        CombatManager.Instance.PlayCard(card.GetCardInstance());
    }
}