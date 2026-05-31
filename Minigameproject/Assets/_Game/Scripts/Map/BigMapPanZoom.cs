using UnityEngine;
using UnityEngine.EventSystems;

public class BigMapPanZoom : MonoBehaviour, IBeginDragHandler, IDragHandler, IScrollHandler
{
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;
    [SerializeField] private float minScale = 1f;
    [SerializeField] private float maxScale = 2.5f;
    [SerializeField] private float zoomStep = 0.15f;
    [SerializeField] private float panPadding = 220f;

    private Canvas canvas;
    private float currentScale = 1f;

    public void Configure(RectTransform mapViewport, RectTransform mapContent)
    {
        viewport = mapViewport;
        content = mapContent;
        canvas = GetComponentInParent<Canvas>();
        ResetView();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (content == null)
            Debug.LogError("[BigMapPanZoom] 이동할 BigMap 콘텐츠가 연결되지 않았습니다.");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (content == null)
            return;

        float scaleFactor = canvas != null ? canvas.scaleFactor : 1f;
        content.anchoredPosition += eventData.delta / scaleFactor;
        ClampContentPosition();
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (content == null || viewport == null)
            return;

        float scroll = eventData.scrollDelta.y;

        if (Mathf.Approximately(scroll, 0f))
            return;

        float previousScale = currentScale;
        currentScale = Mathf.Clamp(
            currentScale + Mathf.Sign(scroll) * zoomStep,
            minScale,
            maxScale
        );

        if (Mathf.Approximately(previousScale, currentScale))
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            viewport,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 pointerPosition
        );

        float scaleRatio = currentScale / previousScale;
        content.anchoredPosition +=
            (pointerPosition - content.anchoredPosition) * (1f - scaleRatio);
        content.localScale = Vector3.one * currentScale;
        ClampContentPosition();
    }

    public void ResetView()
    {
        if (content == null)
            return;

        currentScale = minScale;
        content.localScale = Vector3.one * currentScale;
        content.anchoredPosition = Vector2.zero;
    }

    private void ClampContentPosition()
    {
        if (content == null || viewport == null)
            return;

        Vector2 scaledContentSize = content.rect.size * currentScale;
        Vector2 viewportSize = viewport.rect.size;
        Vector2 maxOffset = new Vector2(
            Mathf.Max(panPadding, (scaledContentSize.x - viewportSize.x) * 0.5f),
            Mathf.Max(panPadding, (scaledContentSize.y - viewportSize.y) * 0.5f)
        );

        Vector2 position = content.anchoredPosition;
        position.x = Mathf.Clamp(position.x, -maxOffset.x, maxOffset.x);
        position.y = Mathf.Clamp(position.y, -maxOffset.y, maxOffset.y);
        content.anchoredPosition = position;
    }
}
