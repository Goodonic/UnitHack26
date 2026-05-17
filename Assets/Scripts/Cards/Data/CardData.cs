using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Cards/Card")]
public class CardData : ScriptableObject
{
    [Header("Info")]
    public string cardName;
    [TextArea]
    public string description;

    [Header("Gameplay")]
    public CardEffectType effectType;

    public int value;
    public int cost;

    public bool isConsumable;

    [Header("Visual")]
    public Sprite artwork;
}