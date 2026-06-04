using System.Collections.Generic;
using UnityEngine;

public class MapEncounterAssigner
{
    private readonly EnemyData[] normalEnemyPool;
    private readonly EnemyData[] eliteEnemyPool;
    private readonly EnemyData[] bossEnemyPool;
    private readonly EventData[] eventPool;

    public MapEncounterAssigner(
        EnemyData[] normalEnemies,
        EnemyData[] eliteEnemies,
        EnemyData[] bossEnemies,
        EventData[] events)
    {
        normalEnemyPool = normalEnemies;
        eliteEnemyPool = eliteEnemies;
        bossEnemyPool = bossEnemies;
        eventPool = events;
    }

    public void Assign(MapData mapData)
    {
        AssignEnemies(mapData);
        AssignEvents(mapData);
    }

    private void AssignEnemies(MapData mapData)
    {
        foreach (MapNodeData node in mapData.nodes)
        {
            if (node.nodeType == MapNodeType.NormalBattle)
                node.enemyData = GetRandomEnemy(normalEnemyPool);
            else if (node.nodeType == MapNodeType.EliteBattle)
                node.enemyData = GetRandomEnemy(eliteEnemyPool);
            else if (node.nodeType == MapNodeType.Boss)
                node.enemyData = GetRandomEnemy(bossEnemyPool);
        }
    }

    private EnemyData GetRandomEnemy(EnemyData[] enemyPool)
    {
        if (enemyPool == null || enemyPool.Length == 0)
            return null;

        return enemyPool[Random.Range(0, enemyPool.Length)];
    }

    private void AssignEvents(MapData mapData)
    {
        List<EventData> candidates = new List<EventData>();

        if (eventPool != null)
        {
            foreach (EventData eventData in eventPool)
            {
                if (eventData == null ||
                    eventData.fixedOnly ||
                    !eventData.CanAppear(mapData.episodeNumber) ||
                    (eventData.showOncePerRun && RunData.HasSeenEvent(eventData)))
                    continue;

                candidates.Add(eventData);
            }
        }

        foreach (MapNodeData node in mapData.nodes)
        {
            if (node.nodeType != MapNodeType.Event)
                continue;

            if (!string.IsNullOrEmpty(node.fixedEventId))
            {
                node.eventData = FindEventById(node.fixedEventId);

                if (node.eventData == null)
                    Debug.LogWarning($"[MapEncounterAssigner] 고정 이벤트 '{node.fixedEventId}'를 찾지 못했습니다.");

                continue;
            }

            node.eventData = GetRandomWeightedEvent(candidates);

            if (node.eventData != null)
                candidates.Remove(node.eventData);
        }
    }

    private EventData FindEventById(string eventId)
    {
        if (eventPool == null)
            return null;

        foreach (EventData eventData in eventPool)
        {
            if (eventData != null && eventData.eventId == eventId)
                return eventData;
        }

        return null;
    }

    private EventData GetRandomWeightedEvent(List<EventData> candidates)
    {
        int totalWeight = 0;

        foreach (EventData eventData in candidates)
            totalWeight += Mathf.Max(1, eventData.weight);

        if (totalWeight <= 0)
            return null;

        int randomWeight = Random.Range(0, totalWeight);

        foreach (EventData eventData in candidates)
        {
            randomWeight -= Mathf.Max(1, eventData.weight);

            if (randomWeight < 0)
                return eventData;
        }

        return null;
    }
}
