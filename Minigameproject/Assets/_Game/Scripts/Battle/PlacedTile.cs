using UnityEngine;
using UnityEngine.EventSystems;

public class PlacedTile : MonoBehaviour, IPointerClickHandler
{
    private CardData cardData;
    private BoardManager boardManager;
    private HandManager handManager;
    // Setup이 호출되지 않은 PlacedTile이 placedId=0으로 우연히 매칭되는 것을 방지
    private int placedId = -1;

    // PlacedTile.cs
    public bool IsActivePlacement => cardData != null;

    public void Setup(CardData data, BoardManager board, HandManager hand, int id)
    {
        cardData = data;
        boardManager = board;
        handManager = hand;
        placedId = id;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Setup이 안 됐다면 이 PlacedTile은 클릭 대상이 아님 — 조용히 무시
        if (cardData == null || boardManager == null || handManager == null)
            return;

        boardManager.RemoveCard(cardData);
        handManager.AddCard(cardData);

        // 프리팹 1개가 타일 전체 모양을 담고 있으므로 자기 자신만 파괴하면 충분
        Destroy(gameObject);
    }
}