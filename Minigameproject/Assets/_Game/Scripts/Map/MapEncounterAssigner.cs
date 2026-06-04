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
        List<EnemyData> eliteEnemyBag = BuildEnemyBag(eliteEnemyPool);

        foreach (MapNodeData node in mapData.nodes)
        {
            if (node.nodeType == MapNodeType.NormalBattle)
                node.enemyData = GetRandomEnemy(normalEnemyPool, EnemyType.Normal);
            else if (node.nodeType == MapNodeType.EliteBattle)
                node.enemyData = GetRandomEnemyFromBag(eliteEnemyBag, eliteEnemyPool);
            else if (node.nodeType == MapNodeType.Boss)
                node.enemyData = GetRandomEnemy(bossEnemyPool, EnemyType.Boss);
        }
    }

    private EnemyData GetRandomEnemy(EnemyData[] enemyPool, EnemyType enemyType)
    {
        List<EnemyData> candidates = BuildEnemyBag(enemyPool, enemyType);

        if (candidates.Count == 0)
            return null;

        return candidates[Random.Range(0, candidates.Count)];
    }

    private List<EnemyData> BuildEnemyBag(EnemyData[] enemyPool, EnemyType? enemyType = null)
    {
        List<EnemyData> enemyBag = new List<EnemyData>();

        if (enemyPool == null)
            return enemyBag;

        foreach (EnemyData enemy in enemyPool)
        {
            if (enemy != null &&
                (!enemyType.HasValue || enemy.enemyType == enemyType.Value) &&
                !enemyBag.Contains(enemy))
                enemyBag.Add(enemy);
        }

        return enemyBag;
    }

    private EnemyData GetRandomEnemyFromBag(List<EnemyData> enemyBag, EnemyData[] enemyPool)
    {
        if (enemyPool == null || enemyPool.Length == 0)
            return null;

        if (enemyBag.Count == 0)
            enemyBag.AddRange(BuildEnemyBag(enemyPool));

        if (enemyBag.Count == 0)
            return null;

        int randomIndex = Random.Range(0, enemyBag.Count);
        EnemyData selectedEnemy = enemyBag[randomIndex];
        enemyBag.RemoveAt(randomIndex);
        return selectedEnemy;
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
