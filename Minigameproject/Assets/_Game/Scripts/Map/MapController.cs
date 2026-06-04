using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using TMPro;

// EP 후보 선택, 실행 순서, 전투 복귀 결과를 관리한다.
public class MapController : MonoBehaviour
{
    public MapGenerator mapGenerator;
    public MapView mapView;
    public MapDragHandler mapDragHandler;
    public GameObject eventPanel;

    public GameSceneManager gameSceneManager;

    private MapData currentMap;
    private BigMapEpisodeView bigMapView;
    private EventPanelController eventPanelController;
    private RestPanelController restPanelController;
    private MapDeckViewController mapDeckViewController;

    public TMP_Text HralText;

    private void Start()
    {
        if (mapGenerator == null || mapView == null)
        {
            Debug.LogError("[MapController] MapGenerator 또는 MapView가 연결되지 않았습니다.");
            return;
        }

        BindEventPanel();
        BindRestPanel();
        BindMapDeckView();
        SetupBigMapView();

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

    private void SetupBigMapView()
    {
        GameObject bigMap = GameObject.Find("BigMap");

        if (bigMap == null)
        {
            Debug.LogWarning("[MapController] BigMap 오브젝트를 찾지 못했습니다.");
            return;
        }

        bigMapView = bigMap.GetComponent<BigMapEpisodeView>();

        if (bigMapView == null)
            bigMapView = bigMap.AddComponent<BigMapEpisodeView>();

        bigMapView.Initialize(OpenEpisode);
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
        if (mapView != null && mapView.nodeContainer != null)
            mapView.nodeContainer.gameObject.SetActive(false);

        if (bigMapView != null)
            bigMapView.Show();
    }

    private void HideBigMap()
    {
        if (bigMapView != null)
            bigMapView.Hide();

        if (mapView != null && mapView.nodeContainer != null)
            mapView.nodeContainer.gameObject.SetActive(true);
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
            ShowRestPanel(selectedNode);
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

    private void BindRestPanel()
    {
        restPanelController = new RestPanelController(
            gameSceneManager,
            HralText,
            CompleteRest);
        restPanelController.Initialize();
    }

    private void BindMapDeckView()
    {
        mapDeckViewController = new MapDeckViewController();
        mapDeckViewController.Initialize();
    }

    private void BindEventPanel()
    {
        eventPanelController = new EventPanelController(
            eventPanel,
            gameSceneManager,
            CompleteEvent);
        eventPanelController.Initialize();
    }

    private void ShowRestPanel(MapNodeData restNode)
    {
        if (restPanelController == null)
        {
            Debug.LogError("[MapController] RestPanelController가 초기화되지 않았습니다.");
            CompleteRest(restNode);
            return;
        }

        restPanelController.Show(restNode);
    }

    private void CompleteRest(MapNodeData restNode)
    {
        MarkNodeCleared(restNode);

        CompleteEpisodeIfFinished();
        if (AdvanceEpisodeIfCompleted())
            return;

        DrawEpisodePlan();
    }

    private void ShowShrineEvent(MapNodeData eventNode)
    {
        if (eventPanelController == null)
        {
            Debug.LogError("[MapController] EventPanelController가 초기화되지 않았습니다.");
            CompleteEvent(eventNode);
            return;
        }

        eventPanelController.Show(eventNode);
    }

    private void CompleteEvent(MapNodeData eventNode)
    {
        MarkNodeCleared(eventNode);

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
        EnsureEnemyMatchesNodeType(selectedNode);

        if (selectedNode.enemyData == null)
        {
            Debug.LogError($"{selectedNode.zoneName} 구역에 적 데이터가 없습니다.");
            return;
        }

        RunData.selectedNodeId = selectedNode.id;
        RunData.selectedEnemy = selectedNode.enemyData;

        SceneManager.LoadScene("BattleScene");
    }

    private void EnsureEnemyMatchesNodeType(MapNodeData selectedNode)
    {
        if (mapGenerator == null ||
            selectedNode == null ||
            mapGenerator.IsEnemyValidForNode(selectedNode))
            return;

        EnemyData replacementEnemy = mapGenerator.GetRandomEnemyForNode(selectedNode);

        Debug.LogWarning(
            $"[MapController] {selectedNode.nodeType} 노드에 맞지 않는 적 '{selectedNode.enemyData?.enemyName}'이 배정되어 교체합니다.");

        if (replacementEnemy != null)
            selectedNode.enemyData = replacementEnemy;
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
