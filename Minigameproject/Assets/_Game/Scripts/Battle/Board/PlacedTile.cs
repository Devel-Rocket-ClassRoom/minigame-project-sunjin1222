using UnityEngine;
using UnityEngine.EventSystems;

public class PlacedTile : MonoBehaviour, IPointerClickHandler
{
    private CardData cardData;
    private BoardManager boardManager;
    private HandManager handManager;
    private int placedId = -1;
    private int originIndex = -1;

    public bool IsActivePlacement => cardData != null;

    public void Setup(CardData data, BoardManager board, HandManager hand, int id, int origin)
    {
        cardData = data;
        boardManager = board;
        handManager = hand;
        placedId = id;
        originIndex = origin;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (BattleController.IsTurnProcessing) return;
        if (!IsActivePlacement) return;
        if (!IsPointerOnTile(eventData)) return;

        boardManager.RemoveCard(originIndex);
        handManager.AddCard(cardData);
        Destroy(gameObject);
    }

    private bool IsPointerOnTile(PointerEventData eventData)
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
                eventData.position,
                eventData.pressEventCamera
            ))
            {
                return boardManager.IsCellInPlacement(originIndex, i);
            }
        }

        return false;
    }
}
