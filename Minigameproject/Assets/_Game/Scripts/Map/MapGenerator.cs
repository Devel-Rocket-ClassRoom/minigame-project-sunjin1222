using System;
using System.Collections.Generic;
using UnityEngine;

// EP 후보 구역의 원본 데이터를 만들고 전투 구역에 적을 연결한다.
public class MapGenerator : MonoBehaviour
{
    [Header("Map JSON")]
    [SerializeField] private string mapJsonResourceFormat = "Map/Episode{0}Map";
    [SerializeField] private int maxEpisodeNumber = 8;
    [SerializeField] private int visibleNodeCount = 5;

    [Header("Enemy Pools")]
    public EnemyData[] normalEnemyPool;
    public EnemyData[] eliteEnemyPool;
    public EnemyData[] bossEnemyPool;

    [Header("Event Pool")]
    public EventData[] eventPool;

    public int MaxEpisodeNumber => maxEpisodeNumber;

    public MapData GenerateMap()
    {
        return GenerateMap(RunData.currentFloor);
    }

    public MapData GenerateMap(int episodeNumber)
    {
        MapData mapData = LoadMapFromJson(episodeNumber);
  
        AssignEnemies(mapData);
        AssignEvents(mapData);
        return mapData;
    }

    private MapData LoadMapFromJson(int episodeNumber)
    {
        string mapJsonResourcePath = string.Format(mapJsonResourceFormat, episodeNumber);
        TextAsset mapJson = Resources.Load<TextAsset>(mapJsonResourcePath);

        if (mapJson == null)
        {
            Debug.LogWarning($"[MapGenerator] Resources/{mapJsonResourcePath}.json 파일을 찾지 못했습니다.");
            return new MapData
            {
                episodeNumber = episodeNumber,
                episodeTitle = $"EP.{episodeNumber}"
            };
        }

        try
        {
            MapJsonData jsonData = JsonUtility.FromJson<MapJsonData>(mapJson.text);

            if (jsonData == null || jsonData.nodes == null)
            {
                Debug.LogWarning("[MapGenerator] 맵 JSON 데이터가 비어 있습니다.");
                return new MapData
                {
                    episodeNumber = episodeNumber,
                    episodeTitle = $"EP.{episodeNumber}"
                };
            }

            MapData mapData = new MapData
            {
                episodeNumber = jsonData.episodeNumber,
                episodeTitle = jsonData.episodeTitle
            };

            AddRandomNodes(mapData, jsonData.nodes);

            return mapData;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[MapGenerator] 맵 JSON 파싱 실패: {exception.Message}");

            return new MapData
            {
                episodeNumber = episodeNumber,
                episodeTitle = $"EP.{episodeNumber}"
            };
        }
    }

    private void AddRandomNodes(MapData mapData, MapNodeJsonData[] sourceNodes)
    {
        List<MapNodeJsonData> forcedNodes = new List<MapNodeJsonData>();
        List<MapNodeJsonData> lastForcedNodes = new List<MapNodeJsonData>();
        List<MapNodeJsonData> candidates = new List<MapNodeJsonData>();

        foreach (MapNodeJsonData node in sourceNodes)
        {
            if (!CanAppearInEpisode(node, mapData.episodeNumber) ||
                HasAlreadyAppeared(node) ||
                !HasRequiredClearedQuests(node))
                continue;

            if (node.forceInclude || MustAppearNow(node, mapData.episodeNumber))
            {
                if (node.placeLast)
                    lastForcedNodes.Add(node);
                else
                    forcedNodes.Add(node);
            }
            else
                candidates.Add(node);
        }

        Shuffle(candidates);

        int forcedCount = forcedNodes.Count + lastForcedNodes.Count;
        int count = Mathf.Min(visibleNodeCount, forcedCount + candidates.Count);
        List<MapNodeJsonData> selectedNodes = new List<MapNodeJsonData>(count);

        for (int i = 0; i < forcedNodes.Count && selectedNodes.Count < count; i++)
            selectedNodes.Add(forcedNodes[i]);

        int candidateSlots = Mathf.Max(0, count - selectedNodes.Count - lastForcedNodes.Count);

        for (int i = 0; i < candidates.Count && i < candidateSlots; i++)
            selectedNodes.Add(candidates[i]);

        for (int i = 0; i < lastForcedNodes.Count && selectedNodes.Count < count; i++)
            selectedNodes.Add(lastForcedNodes[i]);

        if (forcedCount > visibleNodeCount)
        {
            Debug.LogWarning($"[MapGenerator] EP.{mapData.episodeNumber}의 필수 등장 퀘스트가 표시 개수보다 많습니다.");
        }

        for (int i = 0; i < selectedNodes.Count; i++)
        {
            MapNodeJsonData node = selectedNodes[i];

            if (!Enum.TryParse(node.nodeType, true, out MapNodeType nodeType))
            {
                Debug.LogWarning($"[MapGenerator] 알 수 없는 노드 타입 '{node.nodeType}'입니다. NormalBattle로 처리합니다.");
                nodeType = MapNodeType.NormalBattle;
            }

            mapData.nodes.Add(CreateCandidate(
                node.id,
                node.floor,
                i - count / 2,
                node.zoneName,
                nodeType,
                node.riskLevel,
                node.rewardHint,
                node.requiredToProgress,
                node.fixedSelectionOrder,
                node.questId,
                node.fixedEventId
            ));

            MarkQuestAppeared(node);
        }

        ApplyFixedSelections(mapData);
    }

    private void ApplyFixedSelections(MapData mapData)
    {
        foreach (MapNodeData node in mapData.nodes)
        {
            if (node.fixedSelectionOrder <= 0)
                continue;

            node.state = MapNodeState.Selected;
            node.selectionOrder = node.fixedSelectionOrder;

            if (!mapData.selectedNodeIds.Contains(node.id))
                mapData.selectedNodeIds.Add(node.id);
        }
    }

    private void Shuffle(List<MapNodeJsonData> candidates)
    {
        if (candidates.Count <= 1)
            return;

        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            (candidates[i], candidates[randomIndex]) = (candidates[randomIndex], candidates[i]);
        }
    }

    private bool CanAppearInEpisode(MapNodeJsonData node, int episodeNumber)
    {
        if (node.availableEpisodes == null || node.availableEpisodes.Length == 0)
            return true;

        foreach (int availableEpisode in node.availableEpisodes)
        {
            if (availableEpisode == episodeNumber)
                return true;
        }

        return false;
    }

    private bool HasAlreadyAppeared(MapNodeJsonData node)
    {
        return node.showOnce &&
            !string.IsNullOrEmpty(node.questId) &&
            RunData.seenQuestIds.Contains(node.questId);
    }

    private bool HasRequiredClearedQuests(MapNodeJsonData node)
    {
        if (node.requiredClearedQuestIds == null || node.requiredClearedQuestIds.Length == 0)
            return true;

        foreach (string questId in node.requiredClearedQuestIds)
        {
            if (string.IsNullOrEmpty(questId))
                continue;

            if (!RunData.clearedQuestIds.Contains(questId))
                return false;
        }

        return true;
    }

    private bool MustAppearNow(MapNodeJsonData node, int episodeNumber)
    {
        return node.showOnce &&
            !string.IsNullOrEmpty(node.questId) &&
            node.mustAppearByEpisode == episodeNumber;
    }

    private void MarkQuestAppeared(MapNodeJsonData node)
    {
        if (!node.showOnce || string.IsNullOrEmpty(node.questId))
            return;

        RunData.seenQuestIds.Add(node.questId);
    }

    private MapNodeData CreateCandidate(
        int id,
        int floor,
        int column,
        string zoneName,
        MapNodeType nodeType,
        int riskLevel,
        string rewardHint,
        bool requiredToProgress = false,
        int fixedSelectionOrder = 0,
        string questId = "",
        string fixedEventId = "")
    {
        return new MapNodeData
        {
            id = id,
            floor = floor,
            column = column,
            zoneName = zoneName,
            nodeType = nodeType,
            riskLevel = riskLevel,
            rewardHint = rewardHint,
            requiredToProgress = requiredToProgress,
            fixedSelectionOrder = fixedSelectionOrder,
            questId = questId,
            fixedEventId = fixedEventId,
            state = MapNodeState.Available
        };
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

        return enemyPool[UnityEngine.Random.Range(0, enemyPool.Length)];
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
                    Debug.LogWarning($"[MapGenerator] 고정 이벤트 '{node.fixedEventId}'를 찾지 못했습니다.");

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

        int randomWeight = UnityEngine.Random.Range(0, totalWeight);

        foreach (EventData eventData in candidates)
        {
            randomWeight -= Mathf.Max(1, eventData.weight);

            if (randomWeight < 0)
                return eventData;
        }

        return null;
    }

    [Serializable]
    private class MapJsonData
    {
        public int episodeNumber = 1;
        public string episodeTitle = "사라진 출발 장면";
        public MapNodeJsonData[] nodes = Array.Empty<MapNodeJsonData>();
    }

    [Serializable]
    private class MapNodeJsonData
    {
        public int id = -1;
        public int floor = 1;
        public string zoneName = string.Empty;
        public string nodeType = nameof(MapNodeType.NormalBattle);
        public int riskLevel = 1;
        public string rewardHint = string.Empty;
        public string questId = string.Empty;
        public int[] availableEpisodes = Array.Empty<int>();
        public bool showOnce = false;
        public int mustAppearByEpisode = -1;
        public string[] requiredClearedQuestIds = Array.Empty<string>();
        public bool forceInclude = false;
        public bool requiredToProgress = false;
        public bool placeLast = false;
        public int fixedSelectionOrder = 0;
        public string fixedEventId = string.Empty;
    }
}
