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
        ClearHand();

        // 1. Создаем временную колоду для текущего боя на основе стартовой
        List<CardData> combatDeck = new List<CardData>(startingDeck);

        // 2. Добавляем в неё карты от надетого снаряжения
        if (EquipmentManager.Instance != null)
        {
            combatDeck.AddRange(EquipmentManager.Instance.GetEquipmentCards());
        }

        // 3. Спавним карты из получившейся объединенной колоды
        foreach (var cardData in combatDeck)
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