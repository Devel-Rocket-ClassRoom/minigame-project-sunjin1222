using UnityEngine;
using System.Collections.Generic;


public class BoardManager : MonoBehaviour
{
    private const int Width = 3;
    private const int Height = 4;

    private CardData[] placedCards = new CardData[Width * Height];
    public GameObject[] gridCells;

    private bool CanPlace(CardData card, int startIndex)
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

    private bool PlaceCard(CardData card, int startIndex)
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
        }

        return true;
    }

    private void ClearBoard()
    {
        for (int i = 0; i < placedCards.Length; i++)
        {
            placedCards[i] = null;
        }
    }

    private List<CardData> GetActivationOrder()
    {
        List<CardData> result = new List<CardData>();

        for (int i = 0; i < placedCards.Length; i++)
        {
            CardData card = placedCards[i];

            if (card == null)
                continue;

            // 다칸 카드 중복 방지
            if (result.Contains(card))
                continue;

            result.Add(card);
        }

        return result;
    }
}