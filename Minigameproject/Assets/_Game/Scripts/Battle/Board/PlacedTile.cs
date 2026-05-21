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

        boardManager.RemoveCard(originIndex);
        handManager.AddCard(cardData);
        Destroy(gameObject);
    }
}