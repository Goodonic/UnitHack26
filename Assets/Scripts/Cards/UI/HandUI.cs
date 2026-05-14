using System.Collections.Generic;
using UnityEngine;

public class HandUI : MonoBehaviour
{
    [Header("References")]
    public GameObject cardPrefab;
    public Transform handPanel;

    [Header("Test Deck")]
    public List<CardData> testDeck;

    private List<CardView> handCards = new();

    private void Start()
    {
        CombatManager.Instance.OnCardUsed += RemoveCard;
        DrawHand();
    }

    public void DrawHand()
    {
        foreach (var cardData in testDeck)
        {
            CardInstance instance = new CardInstance(cardData);

            GameObject cardObj = Instantiate(cardPrefab, handPanel);
            CardView view = cardObj.GetComponent<CardView>();

            view.Setup(instance);

            handCards.Add(view); //добавить карту в список колоды
        }
    }

    public void RemoveCard(CardInstance instance)
    {
        for (int i = 0; i < handCards.Count; i++)
        {
            if (handCards[i].GetCardInstance() == instance)
            {
                Destroy(handCards[i].gameObject);
                handCards.RemoveAt(i);
                break;
            }
        }
    }
}