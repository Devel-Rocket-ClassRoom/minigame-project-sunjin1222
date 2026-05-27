using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// EP 후보 선택, 실행 순서, 전투 복귀 결과를 관리한다.
public class MapController : MonoBehaviour
{
    public MapGenerator mapGenerator;
    public MapView mapView;
    public MapDragHandler mapDragHandler;

    private MapData currentMap;
    private MapEventPopup activeEventPopup;

    private void Start()
    {
        if (mapGenerator == null || mapView == null)
        {
            Debug.LogError("[MapController] MapGenerator 또는 MapView가 연결되지 않았습니다.");
            return;
        }

        if (RunData.currentMap == null)
            RunData.currentMap = mapGenerator.GenerateMap();

        currentMap = RunData.currentMap;

        bool hasCompletedBattle = ApplyCompletedBattleResult();

        if (hasCompletedBattle && currentMap.planConfirmed)
        {
            CompleteEpisodeIfFinished();
        }

        DrawEpisodePlan();

        if (mapDragHandler != null)
            mapDragHandler.FocusOn(Vector2.zero);
    }

    private void DrawEpisodePlan()
    {
        mapView.Draw(currentMap, OnCandidateSelected, AdvanceSelectedRoute);
        RefreshHud();
    }

    private void OnCandidateSelected(MapNodeData selectedNode)
    {
        if (currentMap.planConfirmed || currentMap.episodeCompleted)
            return;

        if (selectedNode.state == MapNodeState.Selected)
        {
            currentMap.selectedNodeIds.Remove(selectedNode.id);
            selectedNode.state = MapNodeState.Available;
        }
        else
        {
            if (currentMap.selectedNodeIds.Count >= 3)
                return;

            currentMap.selectedNodeIds.Add(selectedNode.id);
            selectedNode.state = MapNodeState.Selected;
        }

        UpdateSelectionOrders();
        mapView.Refresh(currentMap, OnCandidateSelected, AdvanceSelectedRoute);
        RefreshHud();
    }

    private void UpdateSelectionOrders()
    {
        foreach (MapNodeData node in currentMap.nodes)
            node.selectionOrder = 0;

        for (int i = 0; i < currentMap.selectedNodeIds.Count; i++)
        {
            MapNodeData node = FindNodeById(currentMap.selectedNodeIds[i]);

            if (node != null)
                node.selectionOrder = i + 1;
        }
    }

    private void AdvanceSelectedRoute()
    {
        if (currentMap.selectedNodeIds.Count != 3 || currentMap.episodeCompleted)
            return;

        if (!currentMap.planConfirmed)
        {
            currentMap.planConfirmed = true;
            currentMap.executionIndex = 0;
        }

        ExecuteNextSelectedZone();
    }

    // 예약된 다음 구역 하나만 실행하고, 완료 후에는 다시 맵에서 진행을 기다린다.
    private void ExecuteNextSelectedZone()
    {
        if (currentMap.executionIndex >= currentMap.selectedNodeIds.Count)
        {
            CompleteEpisodeIfFinished();
            DrawEpisodePlan();
            return;
        }

        MapNodeData selectedNode =
            FindNodeById(currentMap.selectedNodeIds[currentMap.executionIndex]);

        currentMap.executionIndex++;

        if (selectedNode == null)
        {
            CompleteEpisodeIfFinished();
            DrawEpisodePlan();
            return;
        }

        if (selectedNode.nodeType == MapNodeType.Rest)
        {
            ResolveRest(selectedNode);
            CompleteEpisodeIfFinished();
            DrawEpisodePlan();
            return;
        }

        if (selectedNode.nodeType == MapNodeType.Event)
        {
            ShowShrineEvent(selectedNode);
            return;
        }

        StartBattle(selectedNode);
    }

    private bool ApplyCompletedBattleResult()
    {
        if (!RunData.selectedBattleWon || RunData.selectedNodeId < 0)
            return false;

        MapNodeData completedNode = FindNodeById(RunData.selectedNodeId);

        if (completedNode != null)
            completedNode.state = MapNodeState.Cleared;

        RunData.selectedBattleWon = false;
        RunData.selectedNodeId = -1;
        RunData.selectedEnemy = null;

        return completedNode != null;
    }

    private void ResolveRest(MapNodeData restNode)
    {
        RunData.currentHp = Mathf.Min(RunData.maxHp, RunData.currentHp + 10);
        restNode.state = MapNodeState.Cleared;
        Debug.Log($"[EP.1] {restNode.zoneName}: HP 10 회복");
    }

    private void ShowShrineEvent(MapNodeData eventNode)
    {
        RectTransform popupParent = mapView.nodeContainer.parent as RectTransform;

        if (popupParent == null)
        {
            Debug.LogError("[MapController] 이벤트 팝업을 표시할 Canvas가 없습니다.");
            CompleteEvent(eventNode);
            return;
        }

        activeEventPopup = MapEventPopup.Create(popupParent, mapView.nodePrefab.nodeText);
        activeEventPopup.ShowShrineEvent(
            () =>
            {
                RunData.currentHp = Mathf.Min(RunData.maxHp, RunData.currentHp + 10);
                Debug.Log("[EP.1 Event] 낡은 신전: HP 10 회복");
                CompleteEvent(eventNode);
            },
            () =>
            {
                if (mapGenerator.shrineRewardCard != null)
                {
                    RunData.AddEventCard(mapGenerator.shrineRewardCard);
                    Debug.Log($"[EP.1 Event] 낡은 신전: {mapGenerator.shrineRewardCard.cardName} 획득");
                }
                else
                {
                    Debug.LogWarning("[MapController] 낡은 신전 보상 카드가 연결되지 않았습니다.");
                }

                CompleteEvent(eventNode);
            }
        );
    }

    private void CompleteEvent(MapNodeData eventNode)
    {
        eventNode.state = MapNodeState.Cleared;

        if (activeEventPopup != null)
        {
            Destroy(activeEventPopup.gameObject);
            activeEventPopup = null;
        }

        CompleteEpisodeIfFinished();
        DrawEpisodePlan();
    }

    private void CompleteEpisodeIfFinished()
    {
        if (currentMap.executionIndex >= currentMap.selectedNodeIds.Count)
            currentMap.episodeCompleted = true;
    }

    private void StartBattle(MapNodeData selectedNode)
    {
        if (selectedNode.enemyData == null)
        {
            Debug.LogError($"{selectedNode.zoneName} 구역에 적 데이터가 없습니다.");
            return;
        }

        RunData.selectedNodeId = selectedNode.id;
        RunData.selectedEnemy = selectedNode.enemyData;

        SceneManager.LoadScene("BattleScene");
    }

    private MapNodeData FindNodeById(int nodeId)
    {
        foreach (MapNodeData node in currentMap.nodes)
        {
            if (node.id == nodeId)
                return node;
        }

        return null;
    }

    private void RefreshHud()
    {
        GameSceneManager sceneManager = FindFirstObjectByType<GameSceneManager>();

        if (sceneManager != null)
            sceneManager.RefreshMapHud();
    }
}

// MapScene 위에 표시되는 공통 이벤트 선택 팝업의 최소 구현이다.
public class MapEventPopup : MonoBehaviour
{
    private TextMeshProUGUI textTemplate;

    public static MapEventPopup Create(
        RectTransform parent,
        TextMeshProUGUI template)
    {
        GameObject overlayObject = new GameObject(
            "EventPopup",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(MapEventPopup)
        );

        RectTransform overlay = overlayObject.GetComponent<RectTransform>();
        overlay.SetParent(parent, false);
        overlay.anchorMin = Vector2.zero;
        overlay.anchorMax = Vector2.one;
        overlay.offsetMin = Vector2.zero;
        overlay.offsetMax = Vector2.zero;
        overlay.SetAsLastSibling();

        Image background = overlayObject.GetComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.72f);

        MapEventPopup popup = overlayObject.GetComponent<MapEventPopup>();
        popup.textTemplate = template;
        return popup;
    }

    public void ShowShrineEvent(Action onHeal, Action onTakeCard)
    {
        RectTransform panel = CreatePanel();

        CreateText(panel, "낡은 신전 - 잠든 수호검", new Vector2(0f, 145f), 32f);
        CreateText(
            panel,
            "금이 간 제단 위에서 오래된 수호검이 희미하게 빛난다.\n" +
            "검의 잔향을 몸에 담을지, 잘려 나간 각성의 컷을 회수할지 선택하라.",
            new Vector2(0f, 45f),
            22f
        );

        CreateChoiceButton(panel, "안식을 취한다\nHP 10 회복", new Vector2(-190f, -115f), onHeal);
        CreateChoiceButton(panel, "컷을 회수한다\n각성 카드 획득", new Vector2(190f, -115f), onTakeCard);
    }

    private RectTransform CreatePanel()
    {
        GameObject panelObject = new GameObject(
            "EventWindow",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        RectTransform panel = panelObject.GetComponent<RectTransform>();
        panel.SetParent(transform, false);
        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.sizeDelta = new Vector2(940f, 470f);

        Image image = panelObject.GetComponent<Image>();
        image.color = new Color(0.13f, 0.16f, 0.24f, 1f);
        return panel;
    }

    private void CreateChoiceButton(
        RectTransform parent,
        string label,
        Vector2 position,
        Action onClick)
    {
        GameObject buttonObject = new GameObject(
            "ChoiceButton",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button)
        );

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(320f, 92f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.91f, 0.87f, 0.72f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() => onClick?.Invoke());

        TextMeshProUGUI text = CreateText(rect, label, Vector2.zero, 21f);
        text.color = new Color(0.14f, 0.12f, 0.09f, 1f);
    }

    private TextMeshProUGUI CreateText(
        RectTransform parent,
        string value,
        Vector2 position,
        float fontSize)
    {
        GameObject textObject = new GameObject(
            "Text",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(parent.sizeDelta.x - 60f, 90f);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false;

        if (textTemplate != null)
            text.font = textTemplate.font;

        return text;
    }
}
