public class CardInstance
{
    public CardData data;

    public int currentCost;
    public int currentValue;

    public bool exhausted;

    public CardInstance(CardData cardData)
    {
        data = cardData;

        currentCost = data.cost;
        currentValue = data.value;

        exhausted = false;
    }
}