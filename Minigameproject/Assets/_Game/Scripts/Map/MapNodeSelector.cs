using System;
using System.Collections.Generic;
using UnityEngine;

public class MapNodeSelector
{
    private readonly int visibleNodeCount;

    public MapNodeSelector(int visibleCount)
    {
        visibleNodeCount = visibleCount;
    }

    public void AddRandomNodes(MapData mapData, MapNodeJsonData[] sourceNodes)
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
            Debug.LogWarning($"[MapNodeSelector] EP.{mapData.episodeNumber}의 필수 등장 퀘스트가 표시 개수보다 많습니다.");
        }

        for (int i = 0; i < selectedNodes.Count; i++)
        {
            MapNodeJsonData node = selectedNodes[i];

            if (!Enum.TryParse(node.nodeType, true, out MapNodeType nodeType))
            {
                Debug.LogWarning($"[MapNodeSelector] 알 수 없는 노드 타입 '{node.nodeType}'입니다. NormalBattle로 처리합니다.");
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
}
