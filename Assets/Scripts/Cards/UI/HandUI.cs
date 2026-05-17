using System.Collections.Generic;
using UnityEngine;

public class HandUI : MonoBehaviour
{
    [Header("References")]
    public GameObject cardPrefab;
    public Transform handPanel;

    [Header("Deck")]
    public List<CardData> startingDeck;

    private List<CardView> handCards = new();

    public void ClearHand()
    {
        foreach (var card in handCards)
        {
            if (card != null)
                Destroy(card.gameObject);
        }

        handCards.Clear();
    }

    public void DrawHand()
    {
        Debug.Log("DRAW HAND START");
        Debug.Log("Panel: " + handPanel);
        Debug.Log("Prefab: " + cardPrefab);
        Debug.Log("Deck size: " + startingDeck.Count);

        ClearHand();

        foreach (var cardData in startingDeck)
        {
            if (cardData == null)
            {
                Debug.LogError("NULL CARD DATA IN DECK!");
                continue;
            }

            CardInstance instance = new CardInstance(cardData);

            GameObject cardObj = Instantiate(cardPrefab, handPanel);

            CardView view = cardObj.GetComponent<CardView>();
            view.Setup(instance);

            handCards.Add(view);
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