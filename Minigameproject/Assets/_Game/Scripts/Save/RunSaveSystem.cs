using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class RunSaveSystem
{
    private const string SaveFileName = "run_save.json";

    public static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    public static bool HasSave()
    {
        return File.Exists(SavePath);
    }

    public static void Save()
    {
        RunSaveData saveData = CreateSaveData();
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"[RunSaveSystem] 저장 완료: {SavePath}");
    }

    public static bool Load()
    {
        if (!HasSave())
        {
            Debug.LogWarning("[RunSaveSystem] 저장 파일이 없습니다.");
            return false;
        }

        string json = File.ReadAllText(SavePath);
        RunSaveData saveData = JsonUtility.FromJson<RunSaveData>(json);

        if (saveData == null)
        {
            Debug.LogWarning("[RunSaveSystem] 저장 데이터를 읽지 못했습니다.");
            return false;
        }

        ApplySaveData(saveData);
        Debug.Log($"[RunSaveSystem] 불러오기 완료: {SavePath}");
        return true;
    }

    public static void Delete()
    {
        if (HasSave())
            File.Delete(SavePath);
    }

    private static RunSaveData CreateSaveData()
    {
        RunSaveData saveData = new RunSaveData
        {
            characterId = RunData.currentCharacter != null ? RunData.currentCharacter.characterId : "",
            currentHp = RunData.currentHp,
            maxHp = RunData.maxHp,
            currentFloor = RunData.currentFloor,
            highestUnlockedEpisode = RunData.highestUnlockedEpisode,
            addedCard = RunData.AddedCard,
            currentMap = SaveMap(RunData.currentMap),
            selectedNodeId = RunData.selectedNodeId,
            selectedBattleWon = RunData.selectedBattleWon,
            selectedEnemyId = RunData.selectedEnemy != null ? RunData.selectedEnemy.enemyId : ""
        };

        AddCardIds(saveData.deckCardIds, RunData.currentDeck);
        AddCardIds(saveData.pendingRewardCardIds, RunData.pendingRewardCards);
        AddRelicIds(saveData.relicIds, RunData.currentRelics);

        saveData.seenQuestIds.AddRange(RunData.seenQuestIds);
        saveData.clearedQuestIds.AddRange(RunData.clearedQuestIds);
        saveData.seenEventIds.AddRange(RunData.seenEventIds);
        saveData.clearedEpisodeNumbers.AddRange(RunData.clearedEpisodeNumbers);

        return saveData;
    }

    private static void ApplySaveData(RunSaveData saveData)
    {
        RunData.Clear();

        RunData.currentCharacter = SaveAssetResolver.FindCharacter(saveData.characterId);
        RunData.currentDeck = LoadCards(saveData.deckCardIds);
        RunData.pendingRewardCards = LoadCards(saveData.pendingRewardCardIds);

        RunData.currentRelics.Clear();
        foreach (RelicData relic in LoadRelics(saveData.relicIds))
            RunData.AddRelic(relic);

        RunData.seenQuestIds = new HashSet<string>(saveData.seenQuestIds ?? new List<string>());
        RunData.clearedQuestIds = new HashSet<string>(saveData.clearedQuestIds ?? new List<string>());
        RunData.seenEventIds = new HashSet<string>(saveData.seenEventIds ?? new List<string>());
        RunData.clearedEpisodeNumbers = new HashSet<int>(saveData.clearedEpisodeNumbers ?? new List<int>());

        RunData.currentHp = saveData.currentHp;
        RunData.maxHp = saveData.maxHp;
        RunData.currentFloor = saveData.currentFloor;
        RunData.highestUnlockedEpisode = saveData.highestUnlockedEpisode;
        RunData.AddedCard = saveData.addedCard;
        RunData.currentMap = LoadMap(saveData.currentMap);
        RunData.selectedNodeId = saveData.selectedNodeId;
        RunData.selectedBattleWon = saveData.selectedBattleWon;
        RunData.selectedEnemy = SaveAssetResolver.FindEnemy(saveData.selectedEnemyId);
    }

    private static void AddCardIds(List<string> ids, List<CardData> cards)
    {
        if (cards == null)
            return;

        foreach (CardData card in cards)
        {
            if (card == null)
                continue;

            if (string.IsNullOrWhiteSpace(card.cardId))
                Debug.LogWarning($"[RunSaveSystem] cardId가 비어있는 카드가 있습니다: {card.name}");

            ids.Add(card.cardId);
        }
    }

    private static void AddRelicIds(List<string> ids, List<RelicData> relics)
    {
        if (relics == null)
            return;

        foreach (RelicData relic in relics)
        {
            if (relic == null)
                continue;

            if (string.IsNullOrWhiteSpace(relic.relicId))
                Debug.LogWarning($"[RunSaveSystem] relicId가 비어있는 유물이 있습니다: {relic.name}");

            ids.Add(relic.relicId);
        }
    }

    private static List<CardData> LoadCards(List<string> ids)
    {
        List<CardData> cards = new List<CardData>();

        if (ids == null)
            return cards;

        foreach (string id in ids)
        {
            CardData card = SaveAssetResolver.FindCard(id);
            if (card != null)
                cards.Add(card);
        }

        return cards;
    }

    private static List<RelicData> LoadRelics(List<string> ids)
    {
        List<RelicData> relics = new List<RelicData>();

        if (ids == null)
            return relics;

        foreach (string id in ids)
        {
            RelicData relic = SaveAssetResolver.FindRelic(id);
            if (relic != null)
                relics.Add(relic);
        }

        return relics;
    }

    private static SavedMapData SaveMap(MapData mapData)
    {
        if (mapData == null)
            return null;

        SavedMapData savedMap = new SavedMapData
        {
            episodeNumber = mapData.episodeNumber,
            episodeTitle = mapData.episodeTitle,
            selectedNodeIds = new List<int>(mapData.selectedNodeIds),
            planConfirmed = mapData.planConfirmed,
            executionIndex = mapData.executionIndex,
            episodeCompleted = mapData.episodeCompleted
        };

        foreach (MapNodeData node in mapData.nodes)
            savedMap.nodes.Add(SaveNode(node));

        return savedMap;
    }

    private static SavedMapNodeData SaveNode(MapNodeData node)
    {
        return new SavedMapNodeData
        {
            id = node.id,
            floor = node.floor,
            column = node.column,
            zoneName = node.zoneName,
            rewardHint = node.rewardHint,
            riskLevel = node.riskLevel,
            selectionOrder = node.selectionOrder,
            requiredToProgress = node.requiredToProgress,
            fixedSelectionOrder = node.fixedSelectionOrder,
            questId = node.questId,
            fixedEventId = node.fixedEventId,
            nodeType = node.nodeType,
            state = node.state,
            enemyId = node.enemyData != null ? node.enemyData.enemyId : "",
            eventId = node.eventData != null ? node.eventData.eventId : ""
        };
    }

    private static MapData LoadMap(SavedMapData savedMap)
    {
        if (savedMap == null)
            return null;

        MapData mapData = new MapData
        {
            episodeNumber = savedMap.episodeNumber,
            episodeTitle = savedMap.episodeTitle,
            selectedNodeIds = savedMap.selectedNodeIds ?? new List<int>(),
            planConfirmed = savedMap.planConfirmed,
            executionIndex = savedMap.executionIndex,
            episodeCompleted = savedMap.episodeCompleted
        };

        if (savedMap.nodes != null)
        {
            foreach (SavedMapNodeData savedNode in savedMap.nodes)
                mapData.nodes.Add(LoadNode(savedNode));
        }

        return mapData;
    }

    private static MapNodeData LoadNode(SavedMapNodeData savedNode)
    {
        return new MapNodeData
        {
            id = savedNode.id,
            floor = savedNode.floor,
            column = savedNode.column,
            zoneName = savedNode.zoneName,
            rewardHint = savedNode.rewardHint,
            riskLevel = savedNode.riskLevel,
            selectionOrder = savedNode.selectionOrder,
            requiredToProgress = savedNode.requiredToProgress,
            fixedSelectionOrder = savedNode.fixedSelectionOrder,
            questId = savedNode.questId,
            fixedEventId = savedNode.fixedEventId,
            nodeType = savedNode.nodeType,
            state = savedNode.state,
            enemyData = SaveAssetResolver.FindEnemy(savedNode.enemyId),
            eventData = SaveAssetResolver.FindEvent(savedNode.eventId)
        };
    }
}
