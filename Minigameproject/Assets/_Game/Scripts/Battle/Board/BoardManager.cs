using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;


public class BoardCardEntry
{
    public CardData card;
    public int originIndex;

    public BoardCardEntry(CardData card, int originIndex)
    {
        this.card = card;
        this.originIndex = originIndex;
    }
}

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
        RefreshCardPreviewTexts();
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

    public List<BoardCardEntry> GetActivationOrder()
    {
        List<BoardCardEntry> result = new List<BoardCardEntry>();
        HashSet<int> seenOrigins = new HashSet<int>();

        for (int i = 0; i < placedCards.Length; i++)
        {
            CardData card = placedCards[i];

            if (card == null)
                continue;

            int origin = cardOrigin[i];

            if (!seenOrigins.Add(origin))
                continue;

            result.Add(new BoardCardEntry(card, origin));
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

    public int CountAdjacentCards(int originIndex)
    {
        HashSet<int> adjacentOrigins = new HashSet<int>();

        for (int i = 0; i < placedCards.Length; i++)
        {
            if (cardOrigin[i] != originIndex)
                continue;

            int row = i / Width;
            int col = i % Width;

            AddAdjacentOrigin(row - 1, col, originIndex, adjacentOrigins);
            AddAdjacentOrigin(row + 1, col, originIndex, adjacentOrigins);
            AddAdjacentOrigin(row, col - 1, originIndex, adjacentOrigins);
            AddAdjacentOrigin(row, col + 1, originIndex, adjacentOrigins);
        }

        return adjacentOrigins.Count;
    }

    private void AddAdjacentOrigin(
        int row,
        int col,
        int currentOrigin,
        HashSet<int> adjacentOrigins)
    {
        if (row < 0 || row >= Height || col < 0 || col >= Width)
            return;

        int index = col + row * Width;

        if (placedCards[index] == null)
            return;

        int neighborOrigin = cardOrigin[index];

        if (neighborOrigin == currentOrigin)
            return;

        adjacentOrigins.Add(neighborOrigin);
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

    public void RefreshCardPreviewTexts()
    {
        placedTileObjects.RemoveAll(tile => tile == null);

        List<BoardCardEntry> entries = GetActivationOrder();

        foreach (GameObject tileObject in placedTileObjects)
        {
            PlacedTile tile = tileObject.GetComponent<PlacedTile>();

            if (tile != null)
            {
                tile.SetPreviewText("", "");
            }
        }

        for (int i = 0; i < entries.Count; i++)
        {
            BoardCardEntry entry = entries[i];

            EffectContext context = new EffectContext
            {
                activationOrder = i + 1,
                adjacentCardCount = CountAdjacentCards(entry.originIndex)
            };

            EffectPreviewResult result = new EffectPreviewResult();

            if (entry.card.effects != null)
            {
                foreach (EffectSO effect in entry.card.effects)
                {
                    if (effect != null)
                    {
                        effect.Preview(context, result);
                    }
                }
            }

            string valueText = BuildPreviewText(result);

            foreach (GameObject tileObject in placedTileObjects)
            {
                PlacedTile tile = tileObject.GetComponent<PlacedTile>();

                if (tile != null && tile.OriginIndex == entry.originIndex)
                {
                    tile.SetPreviewText($"{i + 1}", valueText);
                    break;
                }
            }
        }
    }
    public void UnregisterPlacedTile(GameObject tile)
    {
        placedTileObjects.Remove(tile);
    }

    private string BuildPreviewText(EffectPreviewResult result)
    {
        if (result.damage > 0 && result.block > 0)
            return $"피해 {result.damage} / 방어 {result.block}";

        if (result.damage > 0)
            return $"피해 {result.damage}";

        if (result.block > 0)
            return $"방어 {result.block}";

        return "";
    }

}
