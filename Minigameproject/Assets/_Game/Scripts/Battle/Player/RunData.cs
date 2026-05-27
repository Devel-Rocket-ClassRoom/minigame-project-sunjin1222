using System.Collections.Generic;
using UnityEngine;

public static class RunData
{
    // 덱
    public static List<CardData> currentDeck = new List<CardData>();
    public static List<CardData> pendingRewardCards = new List<CardData>();
    public static bool IsInitialized => currentDeck.Count > 0;

    // 플레이어 상태
    public static int currentHp = 50;
    public static int maxHp = 50;

    public static int currentFloor = 1;
    public static int AddedCard;

    public static MapData currentMap;
    public static int selectedNodeId;
    public static bool selectedBattleWon;
    public static EnemyData selectedEnemy;

    public static void Init(List<CardData> startDeck, int hp)
    {
        currentDeck = new List<CardData>(startDeck);
        maxHp = hp;
        currentHp = Mathf.Min(currentHp, maxHp);
        currentFloor = 1;
        AddedCard = pendingRewardCards.Count;
    }

    public static void AddCard(CardData card) => currentDeck.Add(card);

    public static void AddEventCard(CardData card)
    {
        if (card == null)
            return;

        if (IsInitialized)
            currentDeck.Add(card);
        else
            pendingRewardCards.Add(card);

        AddedCard += 1;
    }

    public static void ApplyPendingRewardCards()
    {
        if (pendingRewardCards.Count == 0)
            return;

        currentDeck.AddRange(pendingRewardCards);
        pendingRewardCards.Clear();
    }

    public static void Clear()
    {
        currentDeck.Clear();
        pendingRewardCards.Clear();
        currentHp = 50;
        maxHp = 50;
        currentFloor = 1;
        AddedCard = 0;
        currentMap = null;
        selectedNodeId = -1;
        selectedBattleWon = false;
        selectedEnemy = null;
    }
}

