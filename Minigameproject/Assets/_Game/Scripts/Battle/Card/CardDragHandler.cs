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

        if (cardData == null || cardData.floatingPreviewPrefab == null)
            return;

        RectTransform canvasRect = canvas.transform as RectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            canvas.worldCamera,
            out Vector2 localPoint
        );

        // 프리팹 1번만 생성 (이미 전체 타일 모양을 담고 있음)
        GameObject preview = Instantiate(cardData.floatingPreviewPrefab, canvasRect);
        RectTransform previewRect = preview.GetComponent<RectTransform>();
        previewRect.anchoredPosition = localPoint;

        previewTiles.Add(preview);
    }

    private void CreateBoardPreview(int cellIndex)
    {
        DestroyPreview();

        if (cardData == null || cardData.boardPreviewPrefab == null)
            return;

        if (boardManager == null || boardManager.gridCells == null)
            return;

        if (cellIndex < 0 || cellIndex >= boardManager.gridCells.Length)
            return;

        // 배치 가능 여부 저장
        bool canPlace = boardManager.CanPlace(cardData, cellIndex);

        RectTransform canvasRect =
            canvas.transform as RectTransform;

        RectTransform cellRect =
            boardManager.gridCells[cellIndex].GetComponent<RectTransform>();

        // 칸의 월드 좌표 가져오기
        Vector3[] corners = new Vector3[4];
        cellRect.GetWorldCorners(corners);

        // corners[1] = 왼쪽 위
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

        // 프리뷰 생성
        GameObject preview =
            Instantiate(cardData.boardPreviewPrefab, canvasRect);

        RectTransform previewRect =
            preview.GetComponent<RectTransform>();

        previewRect.anchorMin = new Vector2(0.5f, 0.5f);
        previewRect.anchorMax = new Vector2(0.5f, 0.5f);

        // 왼쪽 위 기준
        previewRect.pivot = new Vector2(0f, 1f);

        // 칸 위치로 이동
        previewRect.anchoredPosition = localPos;

        // 색상 변경
        Image[] images = preview.GetComponentsInChildren<Image>();

        foreach (Image image in images)
        {
            if (!image.name.Contains("Border"))
                continue;

            Color color = image.color;

            if (canPlace)
            {
                image.color = new Color(
                    0f,
                    1f,
                    0f,
                    color.a
                );
            }
            else
            {
                image.color = new Color(
                    1f,
                    0f,
                    0f,
                    color.a
                );
            }
        }

        PlacedTile placedTile =
            preview.GetComponent<PlacedTile>();

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