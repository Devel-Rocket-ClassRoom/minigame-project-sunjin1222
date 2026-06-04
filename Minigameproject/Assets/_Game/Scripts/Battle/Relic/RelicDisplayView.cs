using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RelicDisplayView : MonoBehaviour
{
    public RectTransform relicPosition;
    public Vector2 iconSize = new Vector2(64f, 64f);
    public Color emptyIconColor = new Color(0.85f, 0.75f, 0.35f, 1f);

    private readonly List<GameObject> createdIcons = new List<GameObject>();

    private void Awake()
    {
        if (relicPosition == null)
            relicPosition = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        RunData.RelicsChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        RunData.RelicsChanged -= Refresh;
        ClearIcons();
    }

    public void Refresh()
    {
        if (relicPosition == null)
            return;

        ClearIcons();

        if (RunData.currentRelics == null || RunData.currentRelics.Count == 0)
            return;

        foreach (RelicData relic in RunData.currentRelics)
        {
            if (relic != null)
                CreateRelicIcon(relic);
        }
    }

    private void CreateRelicIcon(RelicData relic)
    {
        GameObject iconObject = new GameObject(relic.relicName);
        iconObject.transform.SetParent(relicPosition, false);

        RectTransform rectTransform = iconObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = iconSize;

        CanvasRenderer canvasRenderer = iconObject.AddComponent<CanvasRenderer>();
        canvasRenderer.cullTransparentMesh = true;

        Image image = iconObject.AddComponent<Image>();
        image.sprite = relic.icon;
        image.color = relic.icon == null ? emptyIconColor : Color.white;
        image.preserveAspect = true;
        image.raycastTarget = true;

        LayoutElement layoutElement = iconObject.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = iconSize.x;
        layoutElement.preferredHeight = iconSize.y;
        layoutElement.minWidth = iconSize.x;
        layoutElement.minHeight = iconSize.y;

        if (RelicTooltipBuilder.TryBuild(relic, out string title, out string body))
        {
            TooltipTrigger tooltipTrigger = iconObject.AddComponent<TooltipTrigger>();
            tooltipTrigger.SetTooltip(title,body);
        }

        createdIcons.Add(iconObject);
    }

    private void ClearIcons()
    {
        for (int i = createdIcons.Count - 1; i >= 0; i--)
        {
            if (createdIcons[i] != null)
                Destroy(createdIcons[i]);
        }

        createdIcons.Clear();
    }
}
