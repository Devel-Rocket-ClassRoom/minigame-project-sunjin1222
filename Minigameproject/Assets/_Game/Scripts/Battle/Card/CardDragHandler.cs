using UnityEngine;
using UnityEngine.EventSystems;

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

    private CardPlacementPreview placementPreview;
    private int currentPreviewId;

    private bool isMovingPlacedCard;
    private int originalBoardIndex = -1;

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

    public void SetupPlacedCardMove(CardData data, BoardManager board, HandManager hand, int originIndex)
    {
        cardData = data;
        boardManager = board;
        handManager = hand;
        isMovingPlacedCard = true;
        originalBoardIndex = originIndex;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (BattleController.IsTurnProcessing)
        {
            eventData.pointerDrag = null;
            return;
        }

        originalPosition = rect.anchoredPosition;

        if (isMovingPlacedCard)
        {
            boardManager.RemoveCard(originalBoardIndex);
            boardManager.UnregisterPlacedTile(gameObject);
            boardManager.RefreshCardPreviewTexts();
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        currentPreviewId = nextPlacedId++;
        placementPreview = new CardPlacementPreview(canvas, boardManager, handManager);

        placementPreview.ShowFloating(cardData, eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        int cellIndex = GetNearestCellIndex(eventData.position);

        if (cellIndex == -1)
        {
            placementPreview.ShowFloating(cardData, eventData.position);
            return;
        }

        placementPreview.ShowBoard(cardData, cellIndex, currentPreviewId);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        int cellIndex = GetNearestCellIndex(eventData.position);

        if (cellIndex == -1)
        {
            placementPreview?.Destroy();
            CancelDrag();
            return;
        }

        bool success = boardManager.PlaceCard(cardData, cellIndex);

        if (success)
        {
            placementPreview.ReplaceWithPlacedPrefab(cardData, cellIndex, currentPreviewId);
            placementPreview.ResetBorderColor();

            foreach (GameObject tile in placementPreview.PreviewObjects)
            {
                boardManager.RegisterPlacedTile(tile);
            }

            placementPreview.ClearTrackedObjects();

            if (!isMovingPlacedCard)
            {
                handManager.RemoveCard(cardView);
            }

            Destroy(gameObject);
            boardManager.RefreshCardPreviewTexts();
        }
        else
        {
            placementPreview?.Destroy();
            CancelDrag();
        }
    }
    private void CancelDrag()
    {
        if (isMovingPlacedCard)
        {
            boardManager.PlaceCard(cardData, originalBoardIndex);
            boardManager.RegisterPlacedTile(gameObject);
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            boardManager.RefreshCardPreviewTexts();
            return;
        }

        ReturnToHand();
    }

    private void ReturnToHand()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        rect.anchoredPosition = originalPosition;
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

        float threshold = 90f;

        if (nearestDistance > threshold)
            return -1;

        return nearestIndex;
    }

}
