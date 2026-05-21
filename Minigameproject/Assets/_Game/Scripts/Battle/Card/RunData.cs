using System.Collections.Generic;

public static class RunData
{
    // 덱
    public static List<CardData> currentDeck = new List<CardData>();
    public static bool IsInitialized => currentDeck.Count > 0;

    // 플레이어 상태
    public static int currentHp = 50;
    public static int maxHp = 50;

    public static void Init(List<CardData> startDeck, int hp)
    {
        currentDeck = new List<CardData>(startDeck);
        maxHp = hp;
        currentHp = hp;
    }

    public static void AddCard(CardData card) => currentDeck.Add(card);

    public static void Clear()
    {
        currentDeck.Clear();
        currentHp = 50;
        maxHp = 50;
    }
}
