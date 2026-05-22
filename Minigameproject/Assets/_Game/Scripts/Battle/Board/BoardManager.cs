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



    private List<GameObject> placedTileObjects = new List<GameObject>();

    public void RegisterPlacedTile(GameObject tile)
    {
        if (tile != null) placedTileObjects.Add(tile);
    }

    public void ReturnAllToHand(HandManager handManager)
    {
  
        foreach (GameObject tile in placedTileObjects)
        {
            if (tile != null) Destroy(tile);
        }
        placedTileObjects.Clear();


        HashSet<int> seenOrigins = new HashSet<int>();
        for (int i = 0; i < placedCards.Length; i++)
        {
            if (placedCards[i] == null) continue;
            if (!seenOrigins.Add(cardOrigin[i])) continue;
            handManager.AddCard(placedCards[i]);
        }

        for (int i = 0; i < placedCards.Length; i++)
        {
            placedCards[i] = null;
            cardOrigin[i] = -1;
        }
    }

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
            int targetCol = startCol + offset.x - card.tileOrigin.x;
            int targetRow = startRow + offset.y - card.tileOrigin.y;

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
            int targetCol = startCol + offset.x - card.tileOrigin.x;
            int targetRow = startRow + offset.y - card.tileOrigin.y;
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

    public void RemoveCard(int originIndex)
    {
        if (originIndex < 0) return;

        for (int i = 0; i < placedCards.Length; i++)
        {
            if (cardOrigin[i] == originIndex)
            {
                placedCards[i] = null;
                cardOrigin[i] = -1;
            }
        }
    }

    public bool IsCellInPlacement(int originIndex, int cellIndex)
    {
        if (originIndex < 0)
            return false;

        if (cellIndex < 0 || cellIndex >= cardOrigin.Length)
            return false;

        return cardOrigin[cellIndex] == originIndex;
    }

    public void DiscardBoard(DeckManager deckManager)
    {
        HashSet<int> seenOrigins = new HashSet<int>();
        for (int i = 0; i < placedCards.Length; i++)
        {
            if (placedCards[i] == null) continue;
            if (!seenOrigins.Add(cardOrigin[i])) continue;
            deckManager.DiscardCard(placedCards[i]);
        }
    }

    public void DestroyTiles()
    {
        foreach (GameObject tile in placedTileObjects)
        {
            if (tile != null) Destroy(tile);
        }
        placedTileObjects.Clear();
    }

}
