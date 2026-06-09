using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipUI : MonoBehaviour
{
    private const float Offset = 0f;

    private static TooltipUI instance;

    private RectTransform rectTransform;
    private RectTransform canvasRectTransform;
    private Canvas canvas;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private TextMeshProUGUI singleText;
    private Object owner;

    public static void Show(Object tooltipOwner, RectTransform anchor, string title, string body)
    {
        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(body))
            return;

        TooltipUI tooltip = GetOrCreate(anchor);
        if (tooltip == null)
            return;

        tooltip.owner = tooltipOwner;
        tooltip.SetText(title, body);
        tooltip.gameObject.SetActive(true);
        tooltip.UpdatePosition(anchor);
    }

    public static void Hide(Object tooltipOwner)
    {
        if (instance == null)
            return;

        if (tooltipOwner != null && instance.owner != tooltipOwner)
            return;

        instance.owner = null;
        instance.gameObject.SetActive(false);
    }

    private static TooltipUI GetOrCreate(RectTransform anchor)
    {
        if (instance != null)
            return instance;

        Canvas targetCanvas = null;
        if (anchor != null)
            targetCanvas = anchor.GetComponentInParent<Canvas>();

        if (targetCanvas == null)
            return null;

        TooltipUI sceneTooltip = FindTooltipInCanvas(targetCanvas);
        if (sceneTooltip != null)
        {
            instance = sceneTooltip;
            instance.Initialize(targetCanvas);
            instance.gameObject.SetActive(false);
            return instance;
        }

        GameObject tooltipObject = new GameObject("RuntimeTooltip");
        tooltipObject.transform.SetParent(targetCanvas.transform, false);

        instance = tooltipObject.AddComponent<TooltipUI>();
        instance.BuildRuntimeTooltip(targetCanvas);
        tooltipObject.SetActive(false);

        return instance;
    }

    private static TooltipUI FindTooltipInCanvas(Canvas targetCanvas)
    {
        TooltipUI existingTooltip = targetCanvas.GetComponentInChildren<TooltipUI>(true);
        if (existingTooltip != null)
            return existingTooltip;

        foreach (Transform child in targetCanvas.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == "TooltipPanel")
                return child.gameObject.AddComponent<TooltipUI>();
        }

        return null;
    }

    private void Initialize(Canvas targetCanvas)
    {
        canvas = targetCanvas;
        canvasRectTransform = canvas.transform as RectTransform;
        rectTransform = transform as RectTransform;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

        if (singleText == null && titleText == null && bodyText == null)
            Debug.LogWarning("[TooltipUI] Tooltip text fields are not connected.");
    }

    private void BuildRuntimeTooltip(Canvas targetCanvas)
    {
        canvas = targetCanvas;
        canvasRectTransform = canvas.transform as RectTransform;
        rectTransform = gameObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.sizeDelta = new Vector2(280f, 120f);
        transform.SetAsLastSibling();

        CanvasRenderer canvasRenderer = gameObject.AddComponent<CanvasRenderer>();
        canvasRenderer.cullTransparentMesh = true;

        Image background = gameObject.AddComponent<Image>();
        background.color = new Color(0.08f, 0.08f, 0.1f, 0.94f);
        background.raycastTarget = false;

        VerticalLayoutGroup layout = gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(12, 12, 10, 10);
        layout.spacing = 4f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        titleText = CreateText("Title", 18f, FontStyles.Bold);
        bodyText = CreateText("Body", 15f, FontStyles.Normal);
    }

    private void SetText(string title, string body)
    {
        if (singleText != null)
        {
            singleText.text = string.IsNullOrWhiteSpace(title)
                ? body ?? string.Empty
                : string.IsNullOrWhiteSpace(body)
                    ? $"<b>{title}</b>"
                    : $"<b>{title}</b>\n{body}";
            singleText.gameObject.SetActive(true);
            return;
        }

        if (titleText != null)
        {
            titleText.text = title ?? string.Empty;
            titleText.gameObject.SetActive(!string.IsNullOrWhiteSpace(title));
        }

        if (bodyText != null)
        {
            bodyText.text = body ?? string.Empty;
            bodyText.gameObject.SetActive(!string.IsNullOrWhiteSpace(body));
        }
    }

    private TextMeshProUGUI CreateText(string objectName, float fontSize, FontStyles fontStyle)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(transform, false);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = Color.white;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;

        RectTransform textRect = text.rectTransform;
        textRect.sizeDelta = new Vector2(256f, 0f);

        LayoutElement layoutElement = textObject.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = 256f;

        return text;
    }

    private void UpdatePosition(RectTransform anchor)
    {
        if (anchor == null || canvasRectTransform == null || rectTransform == null)
            return;

        transform.SetAsLastSibling();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

        Vector3[] corners = new Vector3[4];
        anchor.GetWorldCorners(corners);

        Camera worldCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector2 bottomLeft = RectTransformUtility.WorldToScreenPoint(worldCamera, corners[0]);
        Vector2 topRight = RectTransformUtility.WorldToScreenPoint(worldCamera, corners[2]);
        Vector2 center = (bottomLeft + topRight) * 0.5f;

        bool showOnRight = center.x < Screen.width * 0.5f;
        Vector2 screenPosition = showOnRight
          ? new Vector2(topRight.x + Offset, center.y - 100f)
          : new Vector2(bottomLeft.x - Offset, center.y - 100f);
        rectTransform.pivot = showOnRight
            ? new Vector2(0f, 0.5f)
            : new Vector2(1f, 0.5f);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform,
            screenPosition,
            worldCamera,
            out Vector2 position);

        Vector2 size = rectTransform.rect.size;
        Vector2 canvasSize = canvasRectTransform.rect.size;
        Vector2 pivot = rectTransform.pivot;

        float minX = -canvasSize.x * 0.5f + size.x * pivot.x;
        float maxX = canvasSize.x * 0.5f - size.x * (1f - pivot.x);
        float minY = -canvasSize.y * 0.5f + size.y * pivot.y;
        float maxY = canvasSize.y * 0.5f - size.y * (1f - pivot.y);

        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);

        rectTransform.anchoredPosition = position;
    }
}
