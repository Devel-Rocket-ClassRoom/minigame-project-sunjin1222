using System.Collections.Generic;
using System;
using UnityEngine;

public static class RunData
{
    // 덱
    public static List<CardData> currentDeck = new List<CardData>();
    public static List<CardData> pendingRewardCards = new List<CardData>();
    public static List<RelicData> currentRelics = new List<RelicData>();
    public static event Action RelicsChanged;
    public static HashSet<string> seenQuestIds = new HashSet<string>();
    public static HashSet<string> clearedQuestIds = new HashSet<string>();
    public static bool IsInitialized => currentDeck.Count > 0;
    public static CharacterData currentCharacter;

    // 플레이어 상태
    public static int currentHp = 50;
    public static int maxHp = 50;

    public static int currentFloor = 1;
    public static int AddedCard;

    public static MapData currentMap;
    public static int selectedNodeId;
    public static bool selectedBattleWon;
    public static EnemyData selectedEnemy;

    public static void SetCharacter(CharacterData character)
    {
        currentCharacter = character;
    }

    public static void Init(bool resetRunState = true)
    {
        if (currentCharacter == null)
        {
            Debug.LogWarning("[RunData] 현재 캐릭터가 없어 시작 덱을 만들 수 없습니다.");
            currentDeck = new List<CardData>();
            maxHp = 50;
        }
        else
        {
            currentDeck = new List<CardData>(currentCharacter.startDeck);
            maxHp = currentCharacter.maxHp;
        }

        if (resetRunState)
        {
            pendingRewardCards.Clear();
            currentRelics.Clear();

            if (currentCharacter != null && currentCharacter.startingRelics != null)
            {
                foreach (RelicData relic in currentCharacter.startingRelics)
                    AddRelic(relic);
            }

            currentHp = maxHp;
            currentFloor = 1;
            currentMap = null;
            seenQuestIds.Clear();
            clearedQuestIds.Clear();
            selectedNodeId = -1;
            selectedBattleWon = false;
            selectedEnemy = null;
            AddedCard = 0;
        }
        else
        {
            currentHp = Mathf.Min(currentHp, maxHp);
            AddedCard = pendingRewardCards.Count;
        }
    }

    public static void AddCard(CardData card) => currentDeck.Add(card);

    public static void AddRelic(RelicData relic)
    {
        if (relic == null || HasRelic(relic))
            return;

        currentRelics.Add(relic);
        RelicsChanged?.Invoke();
    }

    public static bool HasRelic(RelicData relic)
    {
        return relic != null && currentRelics.Contains(relic);
    }

    public static bool IsSelectedNodeType(MapNodeType nodeType)
    {
        MapNodeData selectedNode = GetSelectedNode();
        return selectedNode != null && selectedNode.nodeType == nodeType;
    }

    public static MapNodeData GetSelectedNode()
    {
        if (currentMap == null || selectedNodeId < 0)
            return null;

        foreach (MapNodeData node in currentMap.nodes)
        {
            if (node.id == selectedNodeId)
                return node;
        }

        return null;
    }

    public static List<CardData> GetRewardCardPool(List<CardData> fallbackCardPool)
    {
        if (currentCharacter != null &&
            currentCharacter.rewardCardPool != null &&
            currentCharacter.rewardCardPool.Count > 0)
            return currentCharacter.rewardCardPool;

        return fallbackCardPool;
    }

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
        currentRelics.Clear();
        RelicsChanged?.Invoke();
        seenQuestIds.Clear();
        clearedQuestIds.Clear();
        currentHp = 50;
        maxHp = 50;
        currentFloor = 1;
        AddedCard = 0;
        currentMap = null;
        selectedNodeId = -1;
        selectedBattleWon = false;
        selectedEnemy = null;
        currentCharacter = null;
    }

}

