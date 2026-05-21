using System.Collections.Generic;

public static class RunDeck
{
    public static List<CardData> currentDeck = new List<CardData>();

    public static bool IsInitialized => currentDeck.Count > 0;

    public static void Init(List<CardData> startDeck)
    {
        currentDeck = new List<CardData>(startDeck);
    }

    public static void AddCard(CardData card)
    {
        currentDeck.Add(card);
    }

    public static void Clear()
    {
        currentDeck.Clear();
    }
}
