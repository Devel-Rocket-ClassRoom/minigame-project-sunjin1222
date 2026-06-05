using UnityEngine;
using UnityEngine.EventSystems;

// 2026-05-26: 커진 맵을 마우스나 터치 드래그로 둘러볼 수 있게 한다.
public class MapDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IScrollHandler
{
    public RectTransform content;
    public float horizontalLimit = 180f;
    public float verticalLimit = 650f;
    public float minScale = 1f;
    public float maxScale = 2.5f;
    public float zoomStep = 0.15f;

    private Canvas canvas;
    private Vector2 initialPosition;
    private float currentScale = 1f;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();

        if (content != null)
        {
            initialPosition = content.anchoredPosition;
            currentScale = Mathf.Max(minScale, content.localScale.x);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (content == null)
            Debug.LogError("[MapDragHandler] 이동할 NodeContainer가 연결되지 않았습니다.");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (content == null)
            return;

        float scaleFactor = canvas != null ? canvas.scaleFactor : 1f;
        Vector2 nextPosition =
            content.anchoredPosition + eventData.delta / scaleFactor;

        content.anchoredPosition = ClampPosition(nextPosition);
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (content == null)
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

        RectTransform viewport = content.parent as RectTransform;
        Vector2 pointerPosition = content.anchoredPosition;

        if (viewport != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                viewport,
                eventData.position,
                eventData.pressEventCamera,
                out pointerPosition
            );
        }

        float scaleRatio = currentScale / previousScale;
        content.anchoredPosition +=
            (pointerPosition - content.anchoredPosition) * (1f - scaleRatio);
        content.localScale = Vector3.one * currentScale;
        content.anchoredPosition = ClampPosition(content.anchoredPosition);
    }

    // 2026-05-26: 전투 복귀 시 진행 가능한 층이 화면 중앙에 보이도록 맵 위치를 맞춘다.
    public void FocusOn(Vector2 nodePosition)
    {
        if (content == null)
            return;

        Vector2 nextPosition = new Vector2(
            initialPosition.x,
            -nodePosition.y
        );

        content.anchoredPosition = ClampPosition(nextPosition);
    }

    private Vector2 ClampPosition(Vector2 position)
    {
        float scaleLimit = Mathf.Max(1f, currentScale);

        position.x = Mathf.Clamp(
            position.x,
            initialPosition.x - horizontalLimit * scaleLimit,
            initialPosition.x + horizontalLimit * scaleLimit
        );
        position.y = Mathf.Clamp(
            position.y,
            initialPosition.y - verticalLimit * scaleLimit,
            initialPosition.y + verticalLimit * scaleLimit
        );

        return position;
    }
}
