using System.Collections.Generic;

using UnityEngine;

public class BoardPreviewTextUpdater
{
    private readonly BoardManager boardManager;
    private readonly List<GameObject> placedTileObjects;

    public BoardPreviewTextUpdater(BoardManager board, List<GameObject> tiles)
    {
        boardManager = board;
        placedTileObjects = tiles;
    }

    public void Refresh()
    {
        placedTileObjects.RemoveAll(tile => tile == null);

        List<BoardCardEntry> entries = boardManager.GetActivationOrder();
        ClearPreviewTexts();

        for (int i = 0; i < entries.Count; i++)
        {
            BoardCardEntry entry = entries[i];

            EffectContext context = new EffectContext
            {
                activationOrder = i + 1,
                adjacentCardCount = boardManager.CountAdjacentCards(entry.originIndex),
                sagaRequiredOrderReduction = BattleRelicResolver.GetSagaRequiredOrderReduction()
            };

            EffectPreviewResult result = new EffectPreviewResult();

            if (entry.card.effects != null)
            {
                foreach (EffectSO effect in entry.card.effects)
                {
                    if (effect != null)
                        effect.Preview(context, result);
                }
            }

            SetPreviewTextForOrigin(
                entry.originIndex,
                $"{i + 1}",
                BuildPreviewText(result)
            );
        }
    }

    private void ClearPreviewTexts()
    {
        foreach (GameObject tileObject in placedTileObjects)
        {
            PlacedTile tile = tileObject.GetComponent<PlacedTile>();

            if (tile != null)
                tile.SetPreviewText("", "");
        }
    }

    private void SetPreviewTextForOrigin(int originIndex, string orderText, string valueText)
    {
        foreach (GameObject tileObject in placedTileObjects)
        {
            PlacedTile tile = tileObject.GetComponent<PlacedTile>();

            if (tile != null && tile.OriginIndex == originIndex)
            {
                tile.SetPreviewText(orderText, valueText);
                break;
            }
        }
    }

    private string BuildPreviewText(EffectPreviewResult result)
    {
        if (result.damage > 0 && result.block > 0)
            return $"피해 {result.damage}\n방어 {result.block}";

        if (result.damage > 0)
            return $"피해 {result.damage}";

        if (result.block > 0)
            return $"방어 {result.block}";

        return "";
    }

}
