using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventPanelController
{
    private readonly GameObject eventPanel;
    private readonly GameSceneManager gameSceneManager;
    private readonly Action<MapNodeData> onEventCompleted;

    private MapNodeData activeEventNode;
    private TMP_Text eventNameText;
    private TMP_Text eventDescriptionText;
    private Image eventImage;
    private Button[] eventChoiceButtons = Array.Empty<Button>();
    private readonly EventRewardPanelController rewardPanelController;

    public EventPanelController(
        GameObject panel,
        GameSceneManager sceneManager,
        EventRewardPanelController rewardPanel,
        Action<MapNodeData> eventCompleted)
    {
        eventPanel = panel;
        gameSceneManager = sceneManager;
        rewardPanelController = rewardPanel;
        onEventCompleted = eventCompleted;
    }

    public void Initialize()
    {
        BindEventPanel();
        BindRewardPanel();
        Hide();
        rewardPanelController?.Hide();
    }

    public void Show(MapNodeData eventNode)
    {
        EventData eventData = eventNode.eventData;
        

        if (eventPanel == null)
        {
            Debug.LogError("[EventPanelController] 이벤트 패널이 연결되지 않았습니다.");
            Complete(eventNode);
            return;
        }

        if (eventData == null)
        {
            Debug.LogWarning("[EventPanelController] 이벤트 노드에 배정된 EventData가 없습니다.");
            Complete(eventNode);
            return;
        }

        if (eventChoiceButtons.Length == 0)
        {
            Debug.LogError("[EventPanelController] 이벤트 선택 버튼을 찾지 못했습니다.");
            Complete(eventNode);
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

    public void Hide()
    {
        foreach (Button button in eventChoiceButtons)
            button.onClick.RemoveAllListeners();

        if (eventPanel != null)
            eventPanel.SetActive(false);
    }

    private void BindEventPanel()
    {
        if (eventPanel == null)
            return;

        List<Button> choiceButtons = new List<Button>();

        foreach (Button button in eventPanel.GetComponentsInChildren<Button>(true))
        {
            if (rewardPanelController != null && button == rewardPanelController.ConfirmButton)
                continue;

            if (button.onClick.GetPersistentEventCount() > 0)
                continue;

            choiceButtons.Add(button);
        }

        eventChoiceButtons = choiceButtons.ToArray();

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

    private void BindRewardPanel()
    {
        if (rewardPanelController != null)
            rewardPanelController.Initialize();
    }

    private void ChooseEventChoice(int choiceIndex)
    {
        if (activeEventNode == null || activeEventNode.eventData == null)
            return;

        Debug.Log($"이벤트 선택지 눌림: {choiceIndex}");

        EventChoiceData[] choices = activeEventNode.eventData.choices;

        if (choices == null || choiceIndex < 0 || choiceIndex >= choices.Length)
            return;

        EventChoiceData choice = choices[choiceIndex];
        RunData.SetCurrentHp(RunData.currentHp + choice.hpChange);

        List<CardData> rewardCards = choice.GetRandomRewardCards();
        RelicData rewardRelic = choice.GetRandomRewardRelic();

        if (rewardCards.Count > 0)
        {
            ShowCardReward(rewardCards);
            return;
        }

        if (rewardRelic != null)
        {
            ShowRelicReward(rewardRelic);
            return;
        }

        Complete(activeEventNode);
    }

    private void ShowCardReward(List<CardData> cards)
    {
        Hide();

        if (rewardPanelController == null ||
            !rewardPanelController.TryShowCards(cards, ConfirmReward))
        {
            Debug.LogWarning("[EventPanelController] 이벤트 카드 보상 패널을 찾지 못해 첫 번째 카드를 즉시 지급합니다.");
            if (cards != null && cards.Count > 0)
                RunData.AddEventCard(cards[0]);

            Complete(activeEventNode);
        }
    }

    private void ShowRelicReward(RelicData relic)
    {
        Hide();

        if (rewardPanelController == null ||
            !rewardPanelController.TryShowRelic(relic, ConfirmReward))
        {
            Debug.LogWarning("[EventPanelController] 이벤트 유물 보상 패널을 찾지 못해 유물을 즉시 지급합니다.");
            if (relic != null)
                RunData.AddRelic(relic);

            Complete(activeEventNode);
        }
    }

    private void ConfirmReward(CardData card, RelicData relic)
    {
        if (activeEventNode == null)
            return;

        if (card != null)
            RunData.AddEventCard(card);

        if (relic != null)
            RunData.AddRelic(relic);

        Complete(activeEventNode);
    }

    private void Complete(MapNodeData eventNode)
    {
        Hide();
        rewardPanelController?.Hide();
        activeEventNode = null;
        onEventCompleted?.Invoke(eventNode);
    }
}
