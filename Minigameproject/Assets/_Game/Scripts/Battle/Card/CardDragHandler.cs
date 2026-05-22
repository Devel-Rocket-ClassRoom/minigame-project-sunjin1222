using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private static int nextPlacedId = 0;

    private CardData cardData;
    private Vector2 originalPosition;
    private RectTransform rect;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    public BoardManager boardManager;
    public HandManager handManager;
    public CardView cardView;

    private List<GameObject> previewTiles = new List<GameObject>();
    private int currentPreviewId;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Setup(CardData data)
    {
        cardData = data;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (BattleController.IsTurnProcessing)
        {
            eventData.pointerDrag = null;
            return;
        }

        originalPosition = rect.anchoredPosition;

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        currentPreviewId = nextPlacedId++;

        CreateFloatingPreview(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        int cellIndex = GetNearestCellIndex(eventData.position);

        if (cellIndex == -1)
        {
            CreateFloatingPreview(eventData.position);
            return;
        }

        CreateBoardPreview(cellIndex);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        int cellIndex = GetNearestCellIndex(eventData.position);

        if (cellIndex == -1)
        {
            DestroyPreview();
            ReturnToHand();
            return;
        }

        bool success = boardManager.PlaceCard(cardData, cellIndex);

        if (success)
        {
            ReplacePreviewWithPlacedPrefab(cellIndex);
            ResetPreviewColor();

            foreach (GameObject tile in previewTiles)
            {
                boardManager.RegisterPlacedTile(tile);
            }

            previewTiles.Clear();
            handManager.RemoveCard(cardView);
            Destroy(gameObject);
        }
        else
        {
            DestroyPreview();
            ReturnToHand();
        }
    }

    private void ReturnToHand()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        rect.anchoredPosition = originalPosition;
    }

    private void CreateFloatingPreview(Vector2 screenPosition)
    {
        DestroyPreview();

        if (cardData == null || cardData.tileBlockPrefab == null)
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
        {
            builder.Build(cardData, cardData.floatingPreviewTileSize, true);
        }

        previewTiles.Add(preview);
    }

    private void CreateBoardPreview(int cellIndex)
    {
        DestroyPreview();

        if (cardData == null || cardData.tileBlockPrefab == null)
            return;

        if (boardManager == null || boardManager.gridCells == null)
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

        GameObject preview =
            CreatePreviewObject(canvasRect);

        RectTransform previewRect =
            preview.GetComponent<RectTransform>();

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

        PlacedTile placedTile =
            preview.GetComponent<PlacedTile>();

        if (placedTile == null)
            placedTile = preview.AddComponent<PlacedTile>();

        if (placedTile != null)
        {
            placedTile.Setup(
                cardData,
                boardManager,
                handManager,
                currentPreviewId,
                cellIndex
            );
        }

        previewTiles.Add(preview);
    }

    private void ReplacePreviewWithPlacedPrefab(int cellIndex)
    {
        if (cardData == null || cardData.boardPreviewPrefab == null)
            return;

        DestroyPreview();

        RectTransform canvasRect = canvas.transform as RectTransform;

        GameObject placed =
            Instantiate(cardData.boardPreviewPrefab, canvasRect);

        RectTransform placedRect =
            placed.GetComponent<RectTransform>();

        if (placedRect != null && TryGetBoardCellTopLeft(cellIndex, canvasRect, out Vector2 localPos, out Vector2 cellSize))
        {
            placedRect.anchorMin = new Vector2(0.5f, 0.5f);
            placedRect.anchorMax = new Vector2(0.5f, 0.5f);
            placedRect.pivot = new Vector2(0f, 1f);
            placedRect.anchoredPosition = localPos + new Vector2(
                -cardData.tileOrigin.x * cellSize.x,
                cardData.tileOrigin.y * cellSize.y
            );
        }

        PlacedTile placedTile =
            placed.GetComponent<PlacedTile>();

        if (placedTile == null)
            placedTile = placed.AddComponent<PlacedTile>();

        placedTile.Setup(
            cardData,
            boardManager,
            handManager,
            currentPreviewId,
            cellIndex
        );

        previewTiles.Add(placed);
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

    private void DestroyPreview()
    {
        for (int i = 0; i < previewTiles.Count; i++)
        {
            if (previewTiles[i] != null)
                Destroy(previewTiles[i]);
        }

        previewTiles.Clear();
    }

    private int GetNearestCellIndex(Vector2 screenPosition)
    {
        if (boardManager == null || boardManager.gridCells == null)
            return -1;

        int nearestIndex = -1;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < boardManager.gridCells.Length; i++)
        {
            GameObject cell = boardManager.gridCells[i];

            if (cell == null)
                continue;

            RectTransform cellRect = cell.GetComponent<RectTransform>();

            Vector2 cellScreenPosition =
                RectTransformUtility.WorldToScreenPoint(
                    canvas.worldCamera,
                    cellRect.position
                );

            float distance = Vector2.Distance(screenPosition, cellScreenPosition);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }

        float threshold = 60f;

        if (nearestDistance > threshold)
            return -1;

        return nearestIndex;
    }

    private void ResetPreviewColor()
    {
        foreach (GameObject preview in previewTiles)
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
}
