using UnityEngine;
using UnityEngine.EventSystems;

public class PlacedTile : MonoBehaviour, IPointerClickHandler
{
    private CardData cardData;
    private BoardManager boardManager;
    private HandManager handManager;
    private int placedId;

    public void Setup(CardData data, BoardManager board, HandManager hand, int id)
    {
        cardData = data;
        boardManager = board;
        handManager = hand;
        placedId = id;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (cardData == null || boardManager == null || handManager == null)
        {
            Debug.LogError("PlacedTile Setup이 안 되어 있습니다.");
            return;
        }

        boardManager.RemoveCard(cardData);
        handManager.AddCard(cardData);

        // 같은 카드로 배치된 타일 전부 삭제
        PlacedTile[] allTiles = FindObjectsByType<PlacedTile>(FindObjectsSortMode.None);

        foreach (PlacedTile tile in allTiles)
        {
            if (tile.placedId == placedId)
            {
                Destroy(tile.gameObject);
            }
        }
    }
}