using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    public const int Width = 3;
    public const int Height = 4;

    private CardData[] placedCards = new CardData[Width * Height];
    private int[] cardOrigin;
    public GameObject[] gridCells;


    private void Awake()
    {
        cardOrigin = new int[Width * Height];
        for (int i = 0; i < cardOrigin.Length; i++)
            cardOrigin[i] = -1;
    }

    public bool CanPlace(CardData card, int startIndex)
    {
        int startRow = startIndex / Width;
        int startCol = startIndex % Width;

        foreach (Vector2Int offset in card.tileShape)
        {
            int targetCol = startCol + offset.x;
            int targetRow = startRow + offset.y;

            if (targetCol < 0 || targetCol >= Width || targetRow < 0 || targetRow >= Height)
            {
                return false;
            }

            int targetIndex = targetCol + targetRow * Width;

            if (placedCards[targetIndex] != null)
            {
                return false;
            }
        }

        return true;
    }

    public bool PlaceCard(CardData card, int startIndex)
    {
        if (!CanPlace(card, startIndex))
        {
            return false;
        }
        int startRow = startIndex / Width;
        int startCol = startIndex % Width;

        foreach (Vector2Int offset in card.tileShape)
        {
            int targetCol = startCol + offset.x;
            int targetRow = startRow + offset.y;
            int targetIndex = targetCol + targetRow * Width;

            placedCards[targetIndex] = card;
            cardOrigin[targetIndex] = startIndex;
        }

        return true;
    }

    public void ClearBoard()
    {
        for (int i = 0; i < placedCards.Length; i++)
        {
            placedCards[i] = null;
            cardOrigin[i] = -1;
            GameObject cell = gridCells != null && i < gridCells.Length ? gridCells[i] : null;
            if (cell != null)
            {
                for (int c = cell.transform.childCount - 1; c >= 0; c--)
                {
                    Destroy(cell.transform.GetChild(c).gameObject);
                }
            }
        }
    }

    public List<CardData> GetActivationOrder()
    {
        List<CardData> result = new List<CardData>();
        HashSet<int> seenOrigins = new HashSet<int>();

        for (int i = 0; i < placedCards.Length; i++)
        {
            CardData card = placedCards[i];

            if (card == null)
                continue;

            int origin = cardOrigin[i];

            if (!seenOrigins.Add(origin))
                continue;

            result.Add(card);
        }

        return result;
    }

    public void RemoveCard(CardData card)
    {
        for (int i = 0; i < placedCards.Length; i++)
        {
            if (placedCards[i] == card)
            {
                placedCards[i] = null;
                cardOrigin[i] = -1;
            }
        }
    }
    public void ReturnAllToHand(HandManager handManager)
{
    if (handManager == null)
    {
        Debug.LogError("[BoardManager] ReturnAllToHand에 handManager가 null입니다.");
        return;
    }

    // 1) 보드 위의 모든 PlacedTile GameObject 파괴
    //    (placedTile은 canvasRect의 자식이라 gridCells 순회로는 못 잡음)
    PlacedTile[] allTiles = FindObjectsByType<PlacedTile>(FindObjectsSortMode.None);
    foreach (PlacedTile tile in allTiles)
    {
        if (tile != null)
            Destroy(tile.gameObject);
    }

    // 2) cardOrigin으로 중복 제거하면서 카드만 핸드로 복귀
    HashSet<int> seenOrigins = new HashSet<int>();
    for (int i = 0; i < placedCards.Length; i++)
    {
        CardData card = placedCards[i];
        if (card == null) continue;

        int origin = cardOrigin[i];
        if (!seenOrigins.Add(origin)) continue;

        handManager.AddCard(card);
    }

    // 3) 보드 상태 초기화
    for (int i = 0; i < placedCards.Length; i++)
    {
        placedCards[i] = null;
        cardOrigin[i] = -1;
    }
}
}
