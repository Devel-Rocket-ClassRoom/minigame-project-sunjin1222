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

    public RectTransform shapePreviewRoot;
    public Image miniTileTemplate;
    public float miniTileSize = 7f;

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
            BuildShapePreview(data);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClickCallback?.Invoke(cardData);
    }
    private void BuildShapePreview(CardData data)
    {
        if (shapePreviewRoot == null || miniTileTemplate == null || data == null)
            return;

        for (int i = shapePreviewRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = shapePreviewRoot.GetChild(i);

            if (child.gameObject != miniTileTemplate.gameObject)
                Destroy(child.gameObject);
        }

        if (data.tileShape == null || data.tileShape.Length == 0)
            return;

        Vector2Int min = data.tileShape[0];
        Vector2Int max = data.tileShape[0];

        for (int i = 1; i < data.tileShape.Length; i++)
        {
            min = Vector2Int.Min(min, data.tileShape[i]);
            max = Vector2Int.Max(max, data.tileShape[i]);
        }

        float shapeWidth = (max.x - min.x + 1) * miniTileSize;
        float shapeHeight = (max.y - min.y + 1) * miniTileSize;
        float startX = (shapePreviewRoot.rect.width - shapeWidth) * 0.5f;
        float startY = (shapePreviewRoot.rect.height - shapeHeight) * 0.5f;

        for (int i = 0; i < data.tileShape.Length; i++)
        {
            Vector2Int offset = data.tileShape[i] - min;

            Image tile = Instantiate(miniTileTemplate, shapePreviewRoot);
            tile.gameObject.SetActive(true);

            RectTransform tileRect = tile.rectTransform;
            tileRect.anchorMin = new Vector2(0f, 1f);
            tileRect.anchorMax = new Vector2(0f, 1f);
            tileRect.pivot = new Vector2(0f, 1f);
            tileRect.sizeDelta = new Vector2(miniTileSize, miniTileSize);
            tileRect.anchoredPosition = new Vector2(
                startX + offset.x * miniTileSize,
                -startY - offset.y * miniTileSize
            );
        }
    }
}
