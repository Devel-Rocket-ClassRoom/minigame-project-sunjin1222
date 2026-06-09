using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventRewardPanelController : MonoBehaviour
{
    public GameObject cardRewardArea;
    public Transform cardContent;
    public CardView cardTemplate;
    public GameObject relicRewardArea;
    public Image relicImage;
    public TMP_Text relicName;
    public TMP_Text relicDescription;
    public Button confirmButton;
    private CardData pendingCard;
    private RelicData pendingRelic;
    private RectTransform selectedCardRect;
    private Action<CardData, RelicData> confirmedCallback;
    private bool initialized;

    public Button ConfirmButton => confirmButton;

    public void Initialize()
    {
        if (initialized)
            return;

        if (cardTemplate != null)
            cardTemplate.gameObject.SetActive(false);

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
        GameObject cardObject = Instantiate(cardTemplate.gameObject, cardContent);
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

            if (cardTemplate == null || child != cardTemplate.gameObject)
                Destroy(child);
        }
    }

    private void ClearSelectedCard()
    {
        if (selectedCardRect != null)
            selectedCardRect.localScale = Vector3.one;

        selectedCardRect = null;
    }

}
