using UnityEngine;
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
            Debug.Log($"{cardData.cardName} 배치 성공 : {cellIndex}");

            // 프리뷰 오브젝트는 보드에 그대로 남김
            previewTiles.Clear();

            // 손패에서 제거
            handManager.RemoveCard(GetComponent<CardView>());

            // 손패 카드 UI 숨김
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
        else
        {
            Debug.Log($"{cardData.cardName} 배치 실패");

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

        if (cardData == null || cardData.previewPrefab == null)
            return;

        RectTransform canvasRect = canvas.transform as RectTransform;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            canvas.worldCamera,
            out localPoint
        );

        float tileSize = 64f;

        foreach (Vector2Int offset in cardData.tileShape)
        {
            GameObject preview = Instantiate(cardData.previewPrefab, canvasRect);

            RectTransform previewRect = preview.GetComponent<RectTransform>();

            previewRect.anchoredPosition =
                localPoint + new Vector2(offset.x * tileSize, -offset.y * tileSize);

            previewRect.sizeDelta = new Vector2(tileSize, tileSize);

            previewTiles.Add(preview);
        }
    }

    private void CreateBoardPreview(int cellIndex)
    {
        DestroyPreview();

        if (cardData == null || cardData.previewPrefab == null)
            return;

        int startRow = cellIndex / 3;
        int startCol = cellIndex % 3;

        foreach (Vector2Int offset in cardData.tileShape)
        {
            int targetCol = startCol + offset.x;
            int targetRow = startRow + offset.y;

            if (targetCol < 0 || targetCol >= 3 ||
                targetRow < 0 || targetRow >= 4)
                continue;

            int targetIndex = targetCol + targetRow * 3;

            GameObject cell = boardManager.gridCells[targetIndex];

            GameObject preview = Instantiate(cardData.previewPrefab, cell.transform);

            RectTransform previewRect = preview.GetComponent<RectTransform>();

            previewRect.anchorMin = Vector2.zero;
            previewRect.anchorMax = Vector2.one;
            previewRect.offsetMin = Vector2.zero;
            previewRect.offsetMax = Vector2.zero;

            PlacedTile placedTile = preview.GetComponent<PlacedTile>();
            if (placedTile != null)
            {
                placedTile.Setup(cardData, boardManager, handManager, currentPreviewId);
            }

            previewTiles.Add(preview);
        }
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
                RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, cellRect.position);

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
}