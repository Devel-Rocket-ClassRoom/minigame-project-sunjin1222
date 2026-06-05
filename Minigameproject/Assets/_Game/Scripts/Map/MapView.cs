using System;
using System.Collections.Generic;
using UnityEngine;

// EP 후보 구역과 복구 시작 버튼을 기존 노드 프리팹으로 표시한다.
public class MapView : MonoBehaviour
{
    public MapNode nodePrefab;
    public RectTransform nodeContainer;

    private readonly Dictionary<int, MapNode> nodeViews =
        new Dictionary<int, MapNode>();
    private readonly List<GameObject> createdObjects =
        new List<GameObject>();

    private MapNode startButton;

    private const float HorizontalSpacing = 400f;
    private const float CandidateY = 280f;
    private const float StartButtonY = -20f;

    public void Draw(
        MapData mapData,
        Action<MapNodeData> onSelected,
        Action onStart)
    {
        ClearRenderedObjects();
        CreateCandidateViews(mapData, onSelected);
        CreateStartButton(mapData, onStart);
    }

    public void Refresh(
        MapData mapData,
        Action<MapNodeData> onSelected,
        Action onStart)
    {
        foreach (MapNodeData node in mapData.nodes)
        {
            if (nodeViews.TryGetValue(node.id, out MapNode nodeView))
                nodeView.Setup(node, onSelected);
        }

        UpdateStartButton(mapData, onStart);
    }

    private void CreateCandidateViews(
        MapData mapData,
        Action<MapNodeData> onSelected)
    {
        foreach (MapNodeData node in mapData.nodes)
        {
            MapNode nodeView = Instantiate(nodePrefab, nodeContainer);
            nodeView.Setup(node, onSelected);

            RectTransform rect = nodeView.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400f, 310f);
            rect.anchoredPosition = new Vector2(
                node.column * HorizontalSpacing,
                CandidateY
            );

            nodeViews.Add(node.id, nodeView);
            createdObjects.Add(nodeView.gameObject);
        }
    }

    private void CreateStartButton(MapData mapData, Action onStart)
    {
        startButton = Instantiate(nodePrefab, nodeContainer);
        RectTransform rect = startButton.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(380f, 120f);
        rect.anchoredPosition = new Vector2(0f, StartButtonY);
        createdObjects.Add(startButton.gameObject);

        UpdateStartButton(mapData, onStart);
    }

    private void UpdateStartButton(MapData mapData, Action onStart)
    {
        if (startButton == null)
            return;

        if (mapData.episodeCompleted)
        {
            startButton.SetupAction($"EP.{mapData.episodeNumber} 복구 완료", false, null);
            return;
        }

        bool selectedRequiredNodes = HasSelectedRequiredNodes(mapData);
        bool canStart = mapData.planConfirmed ||
            mapData.selectedNodeIds.Count == 3 && selectedRequiredNodes;
        string label = mapData.planConfirmed
            ? $"다음 장면 진행 ({Mathf.Min(mapData.executionIndex + 1, 3)}/3)"
            : selectedRequiredNodes
                ? $"복구 시작 ({mapData.selectedNodeIds.Count}/3)"
                : $"필수 구역 선택 ({mapData.selectedNodeIds.Count}/3)";

        startButton.SetupAction(label, canStart, onStart);
    }

    private bool HasSelectedRequiredNodes(MapData mapData)
    {
        foreach (MapNodeData node in mapData.nodes)
        {
            if (node.requiredToProgress &&
                !mapData.selectedNodeIds.Contains(node.id))
                return false;
        }

        return true;
    }

    private void ClearRenderedObjects()
    {
        foreach (GameObject createdObject in createdObjects)
        {
            if (createdObject != null)
                Destroy(createdObject);
        }

        createdObjects.Clear();
        nodeViews.Clear();
        startButton = null;
    }
}
