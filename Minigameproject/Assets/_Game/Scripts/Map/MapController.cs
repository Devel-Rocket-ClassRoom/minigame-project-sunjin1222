using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using TMPro;

// EP 후보 선택, 실행 순서, 전투 복귀 결과를 관리한다.
public class MapController : MonoBehaviour
{
    public MapGenerator mapGenerator;
    public MapView mapView;
    public MapDragHandler mapDragHandler;
    public GameObject eventPanel;
    public Button healChoiceButton;
    public Button cardChoiceButton;

    private MapData currentMap;
    private MapNodeData activeEventNode;
    private GameObject bigMap;

    private void Start()
    {
        if (mapGenerator == null || mapView == null)
        {
            Debug.LogError("[MapController] MapGenerator 또는 MapView가 연결되지 않았습니다.");
            return;
        }

        HideEventPanel();
        SetupBigMapButtons();

        if (RunData.currentMap == null)
        {
            ShowBigMap();
            return;
        }

        currentMap = RunData.currentMap;
        HideBigMap();

        bool hasCompletedBattle = ApplyCompletedBattleResult();

        if (hasCompletedBattle && currentMap.planConfirmed)
        {
            CompleteEpisodeIfFinished();
        }

        if (AdvanceEpisodeIfCompleted())
            return;

        DrawEpisodePlan();

        if (mapDragHandler != null)
            mapDragHandler.FocusOn(Vector2.zero);
    }

    private void SetupBigMapButtons()
    {
        bigMap = GameObject.Find("BigMap");

        if (bigMap == null)
        {
            Debug.LogWarning("[MapController] BigMap 오브젝트를 찾지 못했습니다.");
            return;
        }

        SetupBigMapPanZoom();

        Button[] episodeButtons = bigMap.GetComponentsInChildren<Button>(true);

        foreach (Button button in episodeButtons)
        {
            if (!TryGetEpisodeNumber(button.name, out int episodeNumber))
                continue;

            int selectedEpisode = episodeNumber;
            button.onClick.AddListener(() => OpenEpisode(selectedEpisode));
        }

        RefreshBigMapButtons();
    }

    private void SetupBigMapPanZoom()
    {
        RectTransform viewport = bigMap.transform as RectTransform;

        if (viewport == null || viewport.childCount == 0)
        {
            Debug.LogWarning("[MapController] BigMap 콘텐츠를 찾지 못했습니다.");
            return;
        }

        RectTransform content = viewport.GetChild(0) as RectTransform;
        BigMapPanZoom panZoom = bigMap.GetComponent<BigMapPanZoom>();

        if (panZoom == null)
            panZoom = bigMap.AddComponent<BigMapPanZoom>();

        panZoom.Configure(viewport, content);
    }

    private bool TryGetEpisodeNumber(string buttonName, out int episodeNumber)
    {
        episodeNumber = 0;

        return !string.IsNullOrEmpty(buttonName) &&
            buttonName.StartsWith("Ep", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(buttonName.Substring(2), out episodeNumber);
    }

    public void OpenEpisode(int episodeNumber)
    {
        if (episodeNumber < 1 ||
            episodeNumber > mapGenerator.MaxEpisodeNumber ||
            !RunData.IsEpisodeUnlocked(episodeNumber) ||
            RunData.IsEpisodeCleared(episodeNumber))
        {
            Debug.LogWarning($"[MapController] 열 수 없는 EP입니다: {episodeNumber}");
            return;
        }

        RunData.currentFloor = episodeNumber;
        RunData.currentMap = mapGenerator.GenerateMap(episodeNumber);
        currentMap = RunData.currentMap;

        HideBigMap();
        DrawEpisodePlan();

        if (mapDragHandler != null)
            mapDragHandler.FocusOn(Vector2.zero);
    }

    private void ShowBigMap()
    {
        RefreshBigMapButtons();

        if (mapView != null && mapView.nodeContainer != null)
            mapView.nodeContainer.gameObject.SetActive(false);

        if (bigMap != null)
            bigMap.SetActive(true);
    }

    private void HideBigMap()
    {
        if (bigMap != null)
            bigMap.SetActive(false);

        if (mapView != null && mapView.nodeContainer != null)
            mapView.nodeContainer.gameObject.SetActive(true);
    }

    private void RefreshBigMapButtons()
    {
        if (bigMap == null)
            return;

        Button[] episodeButtons = bigMap.GetComponentsInChildren<Button>(true);

        foreach (Button button in episodeButtons)
        {
            if (TryGetEpisodeNumber(button.name, out int episodeNumber))
                UpdateBigMapButtonState(button, episodeNumber);
        }
    }

    private void UpdateBigMapButtonState(Button button, int episodeNumber)
    {
        bool isCleared = RunData.IsEpisodeCleared(episodeNumber);
        bool isUnlocked = RunData.IsEpisodeUnlocked(episodeNumber);

        ColorBlock colors = button.colors;
        colors.disabledColor = isCleared
            ? new Color(0.85f, 0.72f, 0.22f, 1f)
            : new Color(0.45f, 0.45f, 0.45f, 0.65f);
        button.colors = colors;
        button.interactable = isUnlocked && !isCleared;

        TMP_Text episodeName = button.GetComponentInChildren<TMP_Text>(true);

        if (episodeName != null)
        {
            episodeName.color = isCleared
                ? new Color(0.2f, 0.8f, 0.3f, 1f)
                : isUnlocked
                    ? new Color(0.95f, 0.2f, 0.2f, 1f)
                    : new Color(0.55f, 0.55f, 0.55f, 1f);
        }
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

        if (selectedNode.fixedSelectionOrder > 0)
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
        System.Collections.Generic.List<int> previousSelectedNodeIds =
            new System.Collections.Generic.List<int>(currentMap.selectedNodeIds);

        currentMap.selectedNodeIds.Clear();

        foreach (MapNodeData node in currentMap.nodes)
        {
            if (node.fixedSelectionOrder > 0 && node.state == MapNodeState.Selected)
                node.selectionOrder = node.fixedSelectionOrder;
            else
                node.selectionOrder = 0;
        }

        int nextOrder = 1;

        foreach (int nodeId in previousSelectedNodeIds)
        {
            MapNodeData node = FindNodeById(nodeId);

            if (node == null ||
                node.state != MapNodeState.Selected ||
                node.fixedSelectionOrder > 0)
                continue;

            while (IsFixedSelectionOrder(nextOrder))
                nextOrder++;

            node.selectionOrder = nextOrder;
            nextOrder++;
        }

        for (int order = 1; order <= 3; order++)
        {
            MapNodeData node = FindNodeBySelectionOrder(order);

            if (node != null)
                currentMap.selectedNodeIds.Add(node.id);
        }
    }

    private bool IsFixedSelectionOrder(int selectionOrder)
    {
        foreach (MapNodeData node in currentMap.nodes)
        {
            if (node.fixedSelectionOrder == selectionOrder &&
                node.state == MapNodeState.Selected)
                return true;
        }

        return false;
    }

    private MapNodeData FindNodeBySelectionOrder(int selectionOrder)
    {
        foreach (MapNodeData node in currentMap.nodes)
        {
            if (node.selectionOrder == selectionOrder)
                return node;
        }

        return null;
    }

    private void AdvanceSelectedRoute()
    {
        if (currentMap.selectedNodeIds.Count != 3 ||
            !HasSelectedRequiredNodes() ||
            currentMap.episodeCompleted)
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
            if (AdvanceEpisodeIfCompleted())
                return;

            DrawEpisodePlan();
            return;
        }

        MapNodeData selectedNode =
            FindNodeById(currentMap.selectedNodeIds[currentMap.executionIndex]);

        currentMap.executionIndex++;

        if (selectedNode == null)
        {
            CompleteEpisodeIfFinished();
            if (AdvanceEpisodeIfCompleted())
                return;

            DrawEpisodePlan();
            return;
        }

        if (selectedNode.nodeType == MapNodeType.Rest)
        {
            ResolveRest(selectedNode);
            CompleteEpisodeIfFinished();
            if (AdvanceEpisodeIfCompleted())
                return;

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
            MarkNodeCleared(completedNode);

        RunData.selectedBattleWon = false;
        RunData.selectedNodeId = -1;
        RunData.selectedEnemy = null;

        return completedNode != null;
    }

    private void ResolveRest(MapNodeData restNode)
    {
        RunData.currentHp = Mathf.Min(RunData.maxHp, RunData.currentHp + 10);
        MarkNodeCleared(restNode);
        Debug.Log($"[EP.{currentMap.episodeNumber}] {restNode.zoneName}: HP 10 회복");
    }

    private void ShowShrineEvent(MapNodeData eventNode)
    {
        if (eventPanel == null || healChoiceButton == null || cardChoiceButton == null)
        {
            Debug.LogError("[MapController] 이벤트 패널 또는 선택 버튼이 연결되지 않았습니다.");
            CompleteEvent(eventNode);
            return;
        }

        activeEventNode = eventNode;
        RunData.MarkEventSeen(eventNode.eventData);

        healChoiceButton.onClick.RemoveListener(ChooseEventHeal);
        cardChoiceButton.onClick.RemoveListener(ChooseEventCard);
        healChoiceButton.onClick.AddListener(ChooseEventHeal);
        cardChoiceButton.onClick.AddListener(ChooseEventCard);

        eventPanel.SetActive(true);
    }

    private void ChooseEventHeal()
    {
        if (activeEventNode == null)
            return;

        RunData.currentHp = Mathf.Min(RunData.maxHp, RunData.currentHp + 10);
        Debug.Log($"[EP.{currentMap.episodeNumber} Event] {activeEventNode.zoneName}: HP 10 회복");
        CompleteEvent(activeEventNode);
    }

    private void ChooseEventCard()
    {
        if (activeEventNode == null)
            return;

        if (mapGenerator.shrineRewardCard != null)
        {
            RunData.AddEventCard(mapGenerator.shrineRewardCard);
            Debug.Log($"[EP.{currentMap.episodeNumber} Event] {activeEventNode.zoneName}: {mapGenerator.shrineRewardCard.cardName} 획득");
        }
        else
        {
            Debug.LogWarning("[MapController] 이벤트 보상 카드가 연결되지 않았습니다.");
        }

        CompleteEvent(activeEventNode);
    }

    private void CompleteEvent(MapNodeData eventNode)
    {
        MarkNodeCleared(eventNode);

        HideEventPanel();
        activeEventNode = null;

        CompleteEpisodeIfFinished();
        if (AdvanceEpisodeIfCompleted())
            return;

        DrawEpisodePlan();
    }

    private void MarkNodeCleared(MapNodeData node)
    {
        if (node == null)
            return;

        node.state = MapNodeState.Cleared;

        if (!string.IsNullOrEmpty(node.questId))
            RunData.clearedQuestIds.Add(node.questId);
    }

    private void HideEventPanel()
    {
        if (healChoiceButton != null)
            healChoiceButton.onClick.RemoveListener(ChooseEventHeal);

        if (cardChoiceButton != null)
            cardChoiceButton.onClick.RemoveListener(ChooseEventCard);

        if (eventPanel != null)
            eventPanel.SetActive(false);
    }

    private void CompleteEpisodeIfFinished()
    {
        if (currentMap.executionIndex >= currentMap.selectedNodeIds.Count)
            currentMap.episodeCompleted = true;
    }

    private bool AdvanceEpisodeIfCompleted()
    {
        if (!currentMap.episodeCompleted)
            return false;

        RunData.MarkEpisodeCleared(currentMap.episodeNumber);

        if (currentMap.episodeNumber >= mapGenerator.MaxEpisodeNumber)
            return false;

        int nextEpisodeNumber = currentMap.episodeNumber + 1;
        RunData.UnlockEpisode(nextEpisodeNumber);
        RunData.currentMap = null;
        ShowBigMap();
        return true;
    }

    private bool HasSelectedRequiredNodes()
    {
        foreach (MapNodeData node in currentMap.nodes)
        {
            if (node.requiredToProgress &&
                !currentMap.selectedNodeIds.Contains(node.id))
                return false;
        }

        return true;
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
