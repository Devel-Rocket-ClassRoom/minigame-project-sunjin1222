using System;
using System.Collections.Generic;
using UnityEngine;

// EP 후보 구역과 복구 시작 버튼을 기존 노드 프리팹으로 표시한다.
public class MapView : MonoBehaviour
{
    public MapNode nodePrefab;
    public RectTransform nodeContainer;
    public RectTransform fixedButtonContainer;

    private readonly Dictionary<int, MapNode> nodeViews =
        new Dictionary<int, MapNode>();
    private readonly List<GameObject> createdObjects =
        new List<GameObject>();

    private MapNode startButton;

    private const float StartButtonY = -400f;

    private static readonly Vector2[] nodeSlots =
    {
  new Vector2(-507f, 30f),
new Vector2(-161f, -120f),
new Vector2(-73f, 166f),
new Vector2(291f, -65f),
new Vector2(442f, 194f),
    };

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
        int index = 0;

        foreach (MapNodeData node in mapData.nodes)
        {
            MapNode nodeView = Instantiate(nodePrefab, nodeContainer);
            nodeView.gameObject.SetActive(true);
            nodeView.Setup(node, onSelected);

            RectTransform rect = nodeView.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200f, 110f);

            if (index < nodeSlots.Length)
                rect.anchoredPosition = nodeSlots[index];
            else
                rect.anchoredPosition = Vector2.zero;

            index++;

            nodeViews.Add(node.id, nodeView);
            createdObjects.Add(nodeView.gameObject);
        }
    }

    private void CreateStartButton(MapData mapData, Action onStart)
    {
        RectTransform parent = fixedButtonContainer != null
            ? fixedButtonContainer
            : nodeContainer.parent as RectTransform;

        startButton = Instantiate(nodePrefab, parent != null ? parent : nodeContainer);
        startButton.gameObject.SetActive(true);
        RectTransform rect = startButton.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(380f, 120f);
        rect.anchoredPosition = new Vector2(0f, StartButtonY);
        rect.localScale = Vector3.one;
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
