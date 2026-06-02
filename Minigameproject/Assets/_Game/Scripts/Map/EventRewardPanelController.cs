using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventRewardPanelController : MonoBehaviour
{
    private GameObject cardRewardArea;
    private Transform cardContent;
    private GameObject cardTemplate;
    private GameObject relicRewardArea;
    private Image relicImage;
    private TMP_Text relicName;
    private TMP_Text relicDescription;
    private Button confirmButton;
    private CardData pendingCard;
    private RelicData pendingRelic;
    private RectTransform selectedCardRect;
    private Action<CardData, RelicData> confirmedCallback;
    private bool initialized;

    public void Initialize()
    {
        if (initialized)
            return;

        cardRewardArea = FindChild(transform, "CardRewardArea")?.gameObject;
        relicRewardArea = FindChild(transform, "RelicRewardArea")?.gameObject;

        if (cardRewardArea != null)
        {
            cardContent = FindChild(cardRewardArea.transform, "Panel");
            CardView templateView = cardContent?.GetComponentInChildren<CardView>(true);

            if (templateView != null)
            {
                cardTemplate = templateView.gameObject;
                cardTemplate.SetActive(false);
            }
        }

        if (relicRewardArea != null)
        {
            relicImage = FindChild(relicRewardArea.transform, "Relicimage")?.GetComponent<Image>();
            relicName = FindChild(relicRewardArea.transform, "RelicName")?.GetComponent<TMP_Text>();
            relicDescription = FindChild(relicRewardArea.transform, "RelicDe")?.GetComponent<TMP_Text>();
        }

        confirmButton = FindButton("ConfirmButton");

        if (confirmButton != null)
            confirmButton.onClick.AddListener(ConfirmReward);

        initialized = true;
    }

    public bool TryShowCards(List<CardData> cards, Action<CardData, RelicData> onConfirmed)
    {
        Initialize();

        if (cards == null ||
            cards.Count == 0 ||
            cardRewardArea == null ||
            cardContent == null ||
            cardTemplate == null ||
            confirmButton == null)
            return false;

        ClearCards();
        pendingCard = cards.Count == 1 ? cards[0] : null;
        pendingRelic = null;
        confirmedCallback = onConfirmed;

        foreach (CardData card in cards)
            CreateCard(card);

        cardRewardArea.SetActive(true);

        if (relicRewardArea != null)
            relicRewardArea.SetActive(false);

        confirmButton.gameObject.SetActive(cards.Count == 1);
        gameObject.SetActive(true);
        return true;
    }

    public bool TryShowRelic(RelicData relic, Action<CardData, RelicData> onConfirmed)
    {
        Initialize();

        if (relic == null || relicRewardArea == null || confirmButton == null)
            return false;

        pendingCard = null;
        pendingRelic = relic;
        confirmedCallback = onConfirmed;

        if (cardRewardArea != null)
            cardRewardArea.SetActive(false);

        relicRewardArea.SetActive(true);

        if (relicImage != null)
        {
            relicImage.sprite = relic.icon;
            relicImage.enabled = relic.icon != null;
        }

        if (relicName != null)
            relicName.text = relic.relicName;

        if (relicDescription != null)
            relicDescription.text = relic.description;

        confirmButton.gameObject.SetActive(true);
        gameObject.SetActive(true);
        return true;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        pendingCard = null;
        pendingRelic = null;
        confirmedCallback = null;
        ClearCards();
    }

    private void CreateCard(CardData card)
    {
        GameObject cardObject = Instantiate(cardTemplate, cardContent);
        cardObject.SetActive(true);

        CardDragHandler dragHandler = cardObject.GetComponent<CardDragHandler>();
        if (dragHandler != null)
            dragHandler.enabled = false;

        CardView cardView = cardObject.GetComponent<CardView>();
        if (cardView != null)
            cardView.Setup(card, selectedCard => SelectCard(selectedCard, cardObject));
    }

    private void SelectCard(CardData card, GameObject cardObject)
    {
        if (card == null)
            return;

        ClearSelectedCard();
        pendingCard = card;
        selectedCardRect = cardObject.GetComponent<RectTransform>();

        if (selectedCardRect != null)
            selectedCardRect.localScale = Vector3.one * 1.2f;

        confirmButton.gameObject.SetActive(true);
    }

    private void ConfirmReward()
    {
        if (pendingCard == null && pendingRelic == null)
            return;

        CardData card = pendingCard;
        RelicData relic = pendingRelic;
        Action<CardData, RelicData> callback = confirmedCallback;
        Hide();
        callback?.Invoke(card, relic);
    }

    private void ClearCards()
    {
        ClearSelectedCard();

        if (cardContent == null)
            return;

        for (int i = cardContent.childCount - 1; i >= 0; i--)
        {
            GameObject child = cardContent.GetChild(i).gameObject;

            if (child != cardTemplate)
                Destroy(child);
        }
    }

    private void ClearSelectedCard()
    {
        if (selectedCardRect != null)
            selectedCardRect.localScale = Vector3.one;

        selectedCardRect = null;
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

    private Button FindButton(string buttonName)
    {
        foreach (Button button in GetComponentsInChildren<Button>(true))
        {
            if (string.Equals(button.name.Trim(), buttonName, StringComparison.Ordinal))
                return button;
        }

        return null;
    }
}
