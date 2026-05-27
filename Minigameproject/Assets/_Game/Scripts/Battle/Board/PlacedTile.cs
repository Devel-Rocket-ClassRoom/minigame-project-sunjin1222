using TMPro;
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
}
