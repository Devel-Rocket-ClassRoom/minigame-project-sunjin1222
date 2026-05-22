using UnityEngine;
using UnityEngine.UI;

public class CardPreviewBuilder : MonoBehaviour
{
    [SerializeField] private RectTransform tilesRoot;
    [SerializeField] private Image artworkImage;

    public void Build(CardData cardData, float tileSize, bool canPlace, bool normalizeShape = true)
    {
        if (cardData == null || cardData.tileBlockPrefab == null)
            return;

        RectTransform root = GetTilesRoot();
        ClearRoot(root);

        Vector2Int min = Vector2Int.zero;
        Vector2Int max = Vector2Int.zero;

        for (int i = 0; i < cardData.tileShape.Length; i++)
        {
            Vector2Int offset = cardData.tileShape[i];
            min = Vector2Int.Min(min, offset);
            max = Vector2Int.Max(max, offset);
        }

        root.anchorMin = new Vector2(0f, 1f);
        root.anchorMax = new Vector2(0f, 1f);
        root.pivot = new Vector2(0f, 1f);
        root.anchoredPosition = Vector2.zero;
        root.sizeDelta = new Vector2(
            (max.x - min.x + 1) * tileSize,
            (max.y - min.y + 1) * tileSize
        );

        RectTransform previewRect = transform as RectTransform;
        if (previewRect != null)
        {
            previewRect.sizeDelta = root.sizeDelta;
        }

        for (int i = 0; i < cardData.tileShape.Length; i++)
        {
            Vector2Int offset = cardData.tileShape[i];
            GameObject block = Instantiate(cardData.tileBlockPrefab, root);

            PlacedTile blockPlacedTile = block.GetComponent<PlacedTile>();
            if (blockPlacedTile != null)
                Destroy(blockPlacedTile);

            RectTransform blockRect = block.GetComponent<RectTransform>();
            if (blockRect != null)
            {
                int x = normalizeShape ? offset.x - min.x : offset.x - cardData.tileOrigin.x;
                int y = normalizeShape ? offset.y - min.y : offset.y - cardData.tileOrigin.y;

                blockRect.anchorMin = new Vector2(0f, 1f);
                blockRect.anchorMax = new Vector2(0f, 1f);
                blockRect.pivot = new Vector2(0f, 1f);
                blockRect.sizeDelta = new Vector2(tileSize, tileSize);
                blockRect.anchoredPosition = new Vector2(
                    x * tileSize,
                    -y * tileSize
                );
            }
        }

        if (artworkImage != null)
            artworkImage.sprite = cardData.icon;

        SetBorderColor(canPlace);
    }

    private RectTransform GetTilesRoot()
    {
        if (tilesRoot != null)
            return tilesRoot;

        Transform existingRoot = transform.Find("TilesRoot");
        if (existingRoot != null)
        {
            tilesRoot = existingRoot as RectTransform;
            return tilesRoot;
        }

        GameObject rootObject = new GameObject("TilesRoot", typeof(RectTransform));
        tilesRoot = rootObject.GetComponent<RectTransform>();
        tilesRoot.SetParent(transform, false);
        return tilesRoot;
    }

    private void ClearRoot(RectTransform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Destroy(root.GetChild(i).gameObject);
        }
    }

    private void SetBorderColor(bool canPlace)
    {
        Image[] images = GetComponentsInChildren<Image>();

        foreach (Image image in images)
        {
            if (!image.name.Contains("Border"))
                continue;

            Color color = image.color;
            image.color = canPlace
                ? new Color(0f, 1f, 0f, color.a)
                : new Color(1f, 0f, 0f, color.a);
        }
    }
}
