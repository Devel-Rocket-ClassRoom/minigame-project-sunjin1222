using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;

// EP 후보 선택, 실행 순서, 전투 복귀 결과를 관리한다.
public class MapController : MonoBehaviour
{
    public MapGenerator mapGenerator;
    public MapView mapView;
    public MapDragHandler mapDragHandler;
    public GameObject eventPanel;

    private MapData currentMap;
    private MapNodeData activeEventNode;
    private GameObject bigMap;
    private TMP_Text eventNameText;
    private TMP_Text eventDescriptionText;
    private Image eventImage;
    private Button[] eventChoiceButtons = Array.Empty<Button>();
    private GameObject restPanel;
    private GameObject cardRemovePanel;
    private Transform cardRemoveContent;
    private GameObject cardRemoveTemplate;
    private MapNodeData activeRestNode;
    private Button cardRemoveConfirmButton;
    private CardData selectedRemoveCard;
    private RectTransform selectedRemoveCardRect;

    public TMP_Text HralText;

    private void Start()
    {
        if (mapGenerator == null || mapView == null)
        {
            Debug.LogError("[MapController] MapGenerator 또는 MapView가 연결되지 않았습니다.");
            return;
        }

        HideEventPanel();
        BindRestPanel();
        HideRestPanels();
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
            ShowRestPanel(selectedNode);
            HralText.text=$"{Mathf.CeilToInt(RunData.maxHp * 0.3f)}";
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
        restPanel = FindSceneObject("RestPanel");
        cardRemovePanel = FindSceneObject("CardRemovePanel");

        if (restPanel != null)
        {
            Button healButton = FindButton(restPanel, "HealButton");
            Button removeCardButton = FindButton(restPanel, "RemoveCardButton");

            if (healButton != null)
                healButton.onClick.AddListener(ChooseRestHeal);

            if (removeCardButton != null)
                removeCardButton.onClick.AddListener(ShowCardRemovePanel);
        }

        if (cardRemovePanel == null)
            return;

        Transform content = FindChild(cardRemovePanel.transform, "Content");

        if (content == null)
            return;

        cardRemoveContent = content;
        CardView templateView = content.GetComponentInChildren<CardView>(true);

        if (templateView != null)
        {
            cardRemoveTemplate = templateView.gameObject;
            cardRemoveTemplate.SetActive(false);
        }

        cardRemoveConfirmButton = FindButton(cardRemovePanel, "ConfirmButton");

        if (cardRemoveConfirmButton == null)
        {
            Debug.LogError("[MapController] CardRemovePanel 아래 ConfirmButton을 찾지 못했습니다.");
            return;
        }

        cardRemoveConfirmButton.onClick.AddListener(ConfirmCardRemove);
        cardRemoveConfirmButton.gameObject.SetActive(false);
    }

    private GameObject FindSceneObject(string objectName)
    {
        foreach (Transform transform in FindObjectsByType<Transform>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None))
        {
            if (transform.gameObject.scene.IsValid() &&
                string.Equals(transform.name.Trim(), objectName, StringComparison.Ordinal))
                return transform.gameObject;
        }

        return null;
    }

    private Transform FindChild(Transform parent, string childName)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (string.Equals(child.name.Trim(), childName, StringComparison.Ordinal))
                return child;
        }

        return null;
    }

    private Button FindButton(GameObject parent, string buttonName)
    {
        foreach (Button button in parent.GetComponentsInChildren<Button>(true))
        {
            if (string.Equals(button.name.Trim(), buttonName, StringComparison.Ordinal))
                return button;
        }

        return null;
    }

    private void ShowRestPanel(MapNodeData restNode)
    {
        if (restPanel == null)
        {
            Debug.LogError("[MapController] RestPanel을 찾지 못했습니다.");
            CompleteRest(restNode);
            return;
        }

        activeRestNode = restNode;
        restPanel.SetActive(true);
    }

    private void ChooseRestHeal()
    {
        if (activeRestNode == null)
            return;

        int healAmount = Mathf.CeilToInt(RunData.maxHp * 0.3f);
        RunData.currentHp = Mathf.Min(RunData.maxHp, RunData.currentHp + healAmount);
        CompleteRest(activeRestNode);
    }

    private void ShowCardRemovePanel()
    {
        if (activeRestNode == null || cardRemovePanel == null || cardRemoveContent == null)
        {
            Debug.LogError("[MapController] 카드 제거 패널 또는 Content를 찾지 못했습니다.");
            return;
        }

        if (cardRemoveTemplate == null)
        {
            Debug.LogError("[MapController] 카드 제거 화면에 템플릿 카드가 없습니다.");
            return;
        }

        ClearCardRemoveContent();
        ClearSelectedRemoveCard();

        foreach (CardData card in RunData.currentDeck)
            CreateRemoveCard(card);

        restPanel.SetActive(false);
        cardRemovePanel.SetActive(true);
    }

    private void ClearCardRemoveContent()
    {
        for (int i = cardRemoveContent.childCount - 1; i >= 0; i--)
        {
            GameObject child = cardRemoveContent.GetChild(i).gameObject;

            if (child != cardRemoveTemplate)
                Destroy(child);
        }
    }

    private void CreateRemoveCard(CardData card)
    {
        GameObject cardObject = Instantiate(cardRemoveTemplate, cardRemoveContent);
        cardObject.SetActive(true);

        CardDragHandler dragHandler = cardObject.GetComponent<CardDragHandler>();
        if (dragHandler != null)
            dragHandler.enabled = false;

        CardView cardView = cardObject.GetComponent<CardView>();
        if (cardView != null)
            cardView.Setup(card, selectedCard => SelectCardToRemove(selectedCard, cardObject));
    }

    private void SelectCardToRemove(CardData card, GameObject cardObject)
    {
        if (activeRestNode == null || card == null)
            return;

        if (RunData.currentDeck.Count <= 1)
        {
            Debug.LogWarning("[MapController] 덱의 마지막 카드는 제거할 수 없습니다.");
            return;
        }

        ClearSelectedRemoveCard();
        selectedRemoveCard = card;
        selectedRemoveCardRect = cardObject.GetComponent<RectTransform>();

        if (selectedRemoveCardRect != null)
            selectedRemoveCardRect.localScale = Vector3.one * 1.08f;

        cardRemoveConfirmButton.gameObject.SetActive(true);
    }

    private void ConfirmCardRemove()
    {
        if (activeRestNode == null || selectedRemoveCard == null)
            return;

        RunData.currentDeck.Remove(selectedRemoveCard);
        CompleteRest(activeRestNode);
    }

    private void ClearSelectedRemoveCard()
    {
        if (selectedRemoveCardRect != null)
            selectedRemoveCardRect.localScale = Vector3.one;

        selectedRemoveCard = null;
        selectedRemoveCardRect = null;

        if (cardRemoveConfirmButton != null)
            cardRemoveConfirmButton.gameObject.SetActive(false);
    }

    private void CompleteRest(MapNodeData restNode)
    {
        MarkNodeCleared(restNode);
        HideRestPanels();
        activeRestNode = null;

        CompleteEpisodeIfFinished();
        if (AdvanceEpisodeIfCompleted())
            return;

        DrawEpisodePlan();
    }

    private void HideRestPanels()
    {
        if (restPanel != null)
            restPanel.SetActive(false);

        if (cardRemovePanel != null)
            cardRemovePanel.SetActive(false);

        ClearSelectedRemoveCard();
    }

    private void ShowShrineEvent(MapNodeData eventNode)
    {
        EventData eventData = eventNode.eventData;

        if (eventPanel == null)
        {
            Debug.LogError("[MapController] 이벤트 패널이 연결되지 않았습니다.");
            CompleteEvent(eventNode);
            return;
        }

        if (eventData == null)
        {
            Debug.LogWarning("[MapController] 이벤트 노드에 배정된 EventData가 없습니다.");
            CompleteEvent(eventNode);
            return;
        }

        BindEventPanel();

        if (eventChoiceButtons.Length == 0)
        {
            Debug.LogError("[MapController] 이벤트 선택 버튼을 찾지 못했습니다.");
            CompleteEvent(eventNode);
            return;
        }

        activeEventNode = eventNode;
        RunData.MarkEventSeen(eventData);

        if (eventNameText != null)
            eventNameText.text = eventData.eventTitle;

        if (eventDescriptionText != null)
            eventDescriptionText.text = eventData.description;

        if (eventImage != null)
        {
            eventImage.sprite = eventData.illustration;
            eventImage.gameObject.SetActive(eventData.illustration != null);
        }

        for (int i = 0; i < eventChoiceButtons.Length; i++)
        {
            Button button = eventChoiceButtons[i];
            button.onClick.RemoveAllListeners();

            bool hasChoice = eventData.choices != null && i < eventData.choices.Length;
            button.gameObject.SetActive(hasChoice);

            if (!hasChoice)
                continue;

            int choiceIndex = i;
            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>(true);

            if (buttonText != null)
                buttonText.text = eventData.choices[i].choiceText;

            button.onClick.AddListener(() => ChooseEventChoice(choiceIndex));
        }

        eventPanel.SetActive(true);
    }

    private void BindEventPanel()
    {
        if (eventChoiceButtons.Length > 0)
            return;

        eventChoiceButtons = eventPanel.GetComponentsInChildren<Button>(true);

        foreach (TMP_Text text in eventPanel.GetComponentsInChildren<TMP_Text>(true))
        {
            if (text.transform.parent.name == "EventName")
                eventNameText = text;
            else if (text.transform.parent.name == "EventText")
                eventDescriptionText = text;
        }

        foreach (Image image in eventPanel.GetComponentsInChildren<Image>(true))
        {
            if (image.name == "EventImage")
            {
                eventImage = image;
                break;
            }
        }
    }

    private void ChooseEventChoice(int choiceIndex)
    {
        if (activeEventNode == null || activeEventNode.eventData == null)
            return;

        EventChoiceData[] choices = activeEventNode.eventData.choices;

        if (choices == null || choiceIndex < 0 || choiceIndex >= choices.Length)
            return;

        EventChoiceData choice = choices[choiceIndex];
        RunData.currentHp = Mathf.Clamp(RunData.currentHp + choice.hpChange, 0, RunData.maxHp);

        CardData rewardCard = choice.GetRandomRewardCard();
        RelicData rewardRelic = choice.GetRandomRewardRelic();

        if (rewardCard != null)
            RunData.AddEventCard(rewardCard);

        if (rewardRelic != null)
            RunData.AddRelic(rewardRelic);

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
        foreach (Button button in eventChoiceButtons)
            button.onClick.RemoveAllListeners();

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
