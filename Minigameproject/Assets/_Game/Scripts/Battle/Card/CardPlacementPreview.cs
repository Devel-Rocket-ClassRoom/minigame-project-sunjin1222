using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardPlacementPreview
{
    private readonly Canvas canvas;
    private readonly BoardManager boardManager;
    private readonly HandManager handManager;
    private readonly List<GameObject> previewObjects = new List<GameObject>();

    public IReadOnlyList<GameObject> PreviewObjects => previewObjects;

    public CardPlacementPreview(Canvas parentCanvas, BoardManager board, HandManager hand)
    {
        canvas = parentCanvas;
        boardManager = board;
        handManager = hand;
    }

    public void ShowFloating(CardData cardData, Vector2 screenPosition)
    {
        Destroy();

        if (cardData == null || cardData.tileBlockPrefab == null || canvas == null)
            return;

        RectTransform canvasRect = canvas.transform as RectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            canvas.worldCamera,
            out Vector2 localPoint
        );

        GameObject preview = CreatePreviewObject(canvasRect);
        RectTransform previewRect = preview.GetComponent<RectTransform>();
        previewRect.anchorMin = new Vector2(0.5f, 0.5f);
        previewRect.anchorMax = new Vector2(0.5f, 0.5f);
        previewRect.pivot = new Vector2(0.5f, 0.5f);
        previewRect.anchoredPosition = localPoint;

        CardPreviewBuilder builder = preview.GetComponent<CardPreviewBuilder>();
        if (builder != null)
            builder.Build(cardData, cardData.floatingPreviewTileSize, true);

        previewObjects.Add(preview);
    }

    public void ShowBoard(CardData cardData, int cellIndex, int previewId)
    {
        Destroy();

        if (cardData == null || cardData.tileBlockPrefab == null)
            return;

        if (boardManager == null || boardManager.gridCells == null || canvas == null)
            return;

        if (cellIndex < 0 || cellIndex >= boardManager.gridCells.Length)
            return;

        bool canPlace = boardManager.CanPlace(cardData, cellIndex);
        RectTransform canvasRect = canvas.transform as RectTransform;
        RectTransform cellRect =
            boardManager.gridCells[cellIndex].GetComponent<RectTransform>();

        Vector3[] corners = new Vector3[4];
        cellRect.GetWorldCorners(corners);

        Vector2 screenPos =
            RectTransformUtility.WorldToScreenPoint(
                canvas.worldCamera,
                corners[1]
            );

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            canvas.worldCamera,
            out Vector2 localPos
        );

        GameObject preview = CreatePreviewObject(canvasRect);
        RectTransform previewRect = preview.GetComponent<RectTransform>();
        previewRect.anchorMin = new Vector2(0.5f, 0.5f);
        previewRect.anchorMax = new Vector2(0.5f, 0.5f);
        previewRect.pivot = new Vector2(0f, 1f);
        previewRect.anchoredPosition = localPos;

        CardPreviewBuilder builder = preview.GetComponent<CardPreviewBuilder>();
        if (builder != null)
        {
            float tileSize = Mathf.Min(cellRect.rect.width, cellRect.rect.height);
            builder.Build(cardData, tileSize, canPlace, false);
        }

        PlacedTile placedTile = preview.GetComponent<PlacedTile>();

        if (placedTile == null)
            placedTile = preview.AddComponent<PlacedTile>();

        placedTile.Setup(
            cardData,
            boardManager,
            handManager,
            previewId,
            cellIndex
        );

        previewObjects.Add(preview);
    }

    public void ReplaceWithPlacedPrefab(CardData cardData, int cellIndex, int placedId)
    {
        if (cardData == null || cardData.boardPreviewPrefab == null || canvas == null)
            return;

        Destroy();

        RectTransform canvasRect = canvas.transform as RectTransform;
        GameObject placed = Object.Instantiate(cardData.boardPreviewPrefab, canvasRect);
        RectTransform placedRect = placed.GetComponent<RectTransform>();

        if (placedRect != null &&
            TryGetBoardCellTopLeft(cellIndex, canvasRect, out Vector2 localPos, out Vector2 cellSize))
        {
            placedRect.anchorMin = new Vector2(0.5f, 0.5f);
            placedRect.anchorMax = new Vector2(0.5f, 0.5f);
            placedRect.pivot = new Vector2(0f, 1f);
            placedRect.anchoredPosition = localPos + new Vector2(
                -cardData.tileOrigin.x * cellSize.x,
                cardData.tileOrigin.y * cellSize.y
            );
        }

        PlacedTile placedTile = placed.GetComponent<PlacedTile>();

        if (placedTile == null)
            placedTile = placed.AddComponent<PlacedTile>();

        placedTile.Setup(
            cardData,
            boardManager,
            handManager,
            placedId,
            cellIndex
        );

        CardDragHandler placedDragHandler = placed.GetComponent<CardDragHandler>();

        if (placedDragHandler == null)
            placedDragHandler = placed.AddComponent<CardDragHandler>();

        placedDragHandler.SetupPlacedCardMove(
            cardData,
            boardManager,
            handManager,
            cellIndex
        );

        previewObjects.Add(placed);
    }

    public void ResetBorderColor()
    {
        foreach (GameObject preview in previewObjects)
        {
            if (preview == null)
                continue;

            Image[] images = preview.GetComponentsInChildren<Image>();

            foreach (Image image in images)
            {
                if (!image.name.Contains("Border"))
                    continue;

                Color color = image.color;
                image.color = new Color(1f, 1f, 1f, color.a);
            }
        }
    }

    public void ClearTrackedObjects()
    {
        previewObjects.Clear();
    }

    public void Destroy()
    {
        for (int i = 0; i < previewObjects.Count; i++)
        {
            if (previewObjects[i] != null)
                Object.Destroy(previewObjects[i]);
        }

        previewObjects.Clear();
    }

    private GameObject CreatePreviewObject(RectTransform parent)
    {
        GameObject preview = new GameObject("CardPreview", typeof(RectTransform), typeof(CardPreviewBuilder));
        preview.transform.SetParent(parent, false);
        return preview;
    }

    private bool TryGetBoardCellTopLeft(int cellIndex, RectTransform canvasRect, out Vector2 localPos, out Vector2 cellSize)
    {
        localPos = Vector2.zero;
        cellSize = Vector2.zero;

        if (boardManager == null || boardManager.gridCells == null)
            return false;

        if (cellIndex < 0 || cellIndex >= boardManager.gridCells.Length)
            return false;

        RectTransform cellRect =
            boardManager.gridCells[cellIndex].GetComponent<RectTransform>();

        if (cellRect == null)
            return false;

        cellSize = cellRect.rect.size;

        Vector3[] corners = new Vector3[4];
        cellRect.GetWorldCorners(corners);

        Vector2 screenPos =
            RectTransformUtility.WorldToScreenPoint(
                canvas.worldCamera,
                corners[1]
            );

        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            canvas.worldCamera,
            out localPos
        );
    }
}
