using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

// EP 후보 선택, 실행 순서, 전투 복귀 결과를 관리한다.
public class MapController : MonoBehaviour
{
    public MapGenerator mapGenerator;
    public MapView mapView;
    public MapDragHandler mapDragHandler;
    public GameObject eventPanel;
    public EventRewardPanelController eventRewardPanelController;

    public GameSceneManager gameSceneManager;

    [Header("Deck View")]
    public GameObject deckPanel;
    public Transform deckContent;
    public CardView deckCardTemplate;
    public TMP_Text deckTitle;
    public TMP_Text deckEmptyText;
    public Button deckCloseButton;
    public Button deckButton;

    [Header("Rest View")]
    public GameObject restPanel;
    public Button restHealButton;
    public Button restRemoveCardButton;
    public GameObject cardRemovePanel;
    public Transform cardRemoveContent;
    public CardView cardRemoveTemplate;
    public Button cardRemoveConfirmButton;

    [Header("Big Map")]
    public BigMapEpisodeView bigMapView;

    private MapData currentMap;
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

        if (RunData.currentMap == null ||
            RunData.currentMap.episodeNumber <= 0 ||
            RunData.currentMap.nodes == null ||
            RunData.currentMap.nodes.Count == 0)
        {
            RunData.currentMap = null;
            ShowBigMap();
            return;
        }

        currentMap = RunData.currentMap;
        HideBigMap();

        if (RunData.selectedNodeId >= 0 && !RunData.selectedBattleWon)
        {
            currentMap.executionIndex =
                Mathf.Max(0, currentMap.executionIndex - 1);

            RunData.selectedNodeId = -1;
            RunData.selectedEnemy = null;
        }

        bool hasCompletedBattle = ApplyCompletedBattleResult();

        if (hasCompletedBattle && currentMap.planConfirmed)
        {
            CompleteEpisodeIfFinished();
        }

        bool advancedEpisode = AdvanceEpisodeIfCompleted();

        if (hasCompletedBattle || advancedEpisode)
            RunSaveSystem.SaveToFirebaseAsync().Forget();

        if (advancedEpisode)
            return;

        DrawEpisodePlan();

        if (mapDragHandler != null)
            mapDragHandler.FocusOn(Vector2.zero);
    }

    public void TestEventButtonClick()
    {
        Debug.Log("이벤트 버튼 눌림");
    }
    private void SetupBigMapView()
    {
        if (bigMapView == null)
        {
            Debug.LogWarning("[MapController] BigMapEpisodeView가 연결되지 않았습니다.");
            return;
        }

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
        RunSaveSystem.SaveToFirebaseAsync().Forget();

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
        RunData.currentMap = currentMap;
        mapView.Refresh(currentMap, OnCandidateSelected, AdvanceSelectedRoute);
        RefreshHud();
        RunSaveSystem.SaveToFirebaseAsync().Forget();
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
            restPanel,
            restHealButton,
            restRemoveCardButton,
            cardRemovePanel,
            cardRemoveContent,
            cardRemoveTemplate,
            cardRemoveConfirmButton,
            CompleteRest);
        restPanelController.Initialize();
    }

    private void BindMapDeckView()
    {
        mapDeckViewController = new MapDeckViewController(
            deckPanel,
            deckContent,
            deckCardTemplate,
            deckTitle,
            deckEmptyText,
            deckCloseButton,
            deckButton);
        mapDeckViewController.Initialize();
    }

    private void BindEventPanel()
    {
        eventPanelController = new EventPanelController(
            eventPanel,
            gameSceneManager,
            eventRewardPanelController,
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
        bool advancedEpisode = AdvanceEpisodeIfCompleted();
        RunSaveSystem.SaveToFirebaseAsync().Forget();

        if (advancedEpisode)
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
        bool advancedEpisode = AdvanceEpisodeIfCompleted();
        RunSaveSystem.SaveToFirebaseAsync().Forget();

        if (advancedEpisode)
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

    private async void StartBattle(MapNodeData selectedNode)
    {
        EnsureEnemyMatchesNodeType(selectedNode);

        if (selectedNode.enemyData == null)
        {
            Debug.LogError($"{selectedNode.zoneName} 구역에 적 데이터가 없습니다.");
            return;
        }

        RunData.selectedNodeId = selectedNode.id;
        RunData.selectedEnemy = selectedNode.enemyData;

        if (!await RunSaveSystem.SaveToFirebaseAsync())
            return;

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
        if (gameSceneManager != null)
            gameSceneManager.RefreshMapHud();
    }
}
