using System.Collections.Generic;
using UnityEngine;

public class BoardActivationHighlighter
{
    private readonly List<GameObject> placedTileObjects;

    public BoardActivationHighlighter(List<GameObject> tiles)
    {
        placedTileObjects = tiles;
    }

    public void Show(int originIndex)
    {
        Hide();

        if (originIndex < 0)
            return;

        foreach (GameObject tileObject in placedTileObjects)
        {
            if (tileObject == null)
                continue;

            PlacedTile tile = tileObject.GetComponent<PlacedTile>();

            if (tile != null && tile.OriginIndex == originIndex)
                tile.SetActivationHighlight(true);
        }
    }

    public void Hide()
    {
        foreach (GameObject tileObject in placedTileObjects)
        {
            if (tileObject == null)
                continue;

            PlacedTile tile = tileObject.GetComponent<PlacedTile>();

            if (tile != null)
                tile.SetActivationHighlight(false);
        }
    }
}
