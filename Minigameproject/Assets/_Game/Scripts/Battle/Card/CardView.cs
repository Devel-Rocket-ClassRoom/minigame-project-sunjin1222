using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class CardView : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI cardName;
    public Image cardImage;
    public TextMeshProUGUI cardDescription;
    private CardData cardData;

    private Action<CardData> onClickCallback;

    public CardData GetCardData() => cardData;

    public void Setup(CardData data, Action<CardData> onClick = null)
    {
        cardData = data;
        onClickCallback = onClick;

        if (data == null) return;

        if (cardName != null) cardName.text = data.cardName;
        if (cardDescription != null) cardDescription.text = data.description;

        if (cardImage != null)
        {
            if (data.icon != null)
            {
                cardImage.sprite = data.icon;
                cardImage.enabled = true;
            }
            else
            {
                cardImage.sprite = null;
                cardImage.enabled = false;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClickCallback?.Invoke(cardData);
    }
}