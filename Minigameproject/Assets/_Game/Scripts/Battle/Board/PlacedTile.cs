using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlacedTile : MonoBehaviour, IPointerClickHandler, ICanvasRaycastFilter
{
    private CardData cardData;
    private BoardManager boardManager;
    private HandManager handManager;


    public TextMeshProUGUI ActivationOrder;
    public TextMeshProUGUI Value;

    public int OriginIndex => originIndex;

    private int placedId = -1;
    private int originIndex = -1;
    private readonly Dictionary<Image, Color> originalImageColors =
        new Dictionary<Image, Color>();
    private bool isHighlighted;

    public bool IsActivePlacement => cardData != null;

    public void Setup(CardData data, BoardManager board, HandManager hand, int id, int origin)
    {
        cardData = data;
        boardManager = board;
        handManager = hand;
        placedId = id;
        originIndex = origin;
        DisableChildRaycastTargets();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (BattleController.IsTurnProcessing) return;
        if (!IsActivePlacement) return;
        if (!IsPointerOnTile(eventData)) return;

        boardManager.RemoveCard(originIndex);
        boardManager.UnregisterPlacedTile(gameObject);
        handManager.AddCard(cardData);

        gameObject.SetActive(false);
        boardManager.RefreshCardPreviewTexts();

        Destroy(gameObject);

    }
    private bool IsPointerOnTile(PointerEventData eventData)
    {
        return IsRaycastLocationValid(eventData.position, eventData.pressEventCamera);
    }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (boardManager == null || boardManager.gridCells == null)
            return true;

        for (int i = 0; i < boardManager.gridCells.Length; i++)
        {
            GameObject cell = boardManager.gridCells[i];

            if (cell == null)
                continue;

            RectTransform cellRect = cell.GetComponent<RectTransform>();

            if (cellRect == null)
                continue;

            if (RectTransformUtility.RectangleContainsScreenPoint(
                cellRect,
                screenPoint,
                eventCamera
            ))
            {
                return boardManager.IsCellInPlacement(originIndex, i);
            }
        }

        return false;
    }

    private void DisableChildRaycastTargets()
    {
        Graphic[] graphics = GetComponentsInChildren<Graphic>(true);

        foreach (Graphic graphic in graphics)
        {
            if (graphic.gameObject != gameObject)
                graphic.raycastTarget = false;
        }
    }

    public void SetPreviewText(string orderText, string valueText = "")
    {
        if (ActivationOrder != null)
            ActivationOrder.text = orderText;

        if (Value != null)
            Value.text = valueText;
    }

    public void SetActivationHighlight(bool shouldHighlight)
    {
        if (isHighlighted == shouldHighlight)
            return;

        isHighlighted = shouldHighlight;

        if (shouldHighlight)
        {
            transform.SetAsLastSibling();

            Image[] images = GetComponentsInChildren<Image>(true);

            foreach (Image image in images)
            {
                if (!originalImageColors.ContainsKey(image))
                    originalImageColors.Add(image, image.color);

                Color originalColor = originalImageColors[image];
                Color grayColor = new Color(0.8f, 0.8f, 0.8f, originalColor.a);
                image.color = Color.Lerp(originalColor, grayColor, 0.75f);
            }

            return;
        }

        foreach (KeyValuePair<Image, Color> entry in originalImageColors)
        {
            if (entry.Key != null)
                entry.Key.color = entry.Value;
        }
    }
}
