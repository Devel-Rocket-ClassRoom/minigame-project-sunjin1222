using UnityEngine;
using UnityEngine.EventSystems;

// 2026-05-26: 커진 맵을 마우스나 터치 드래그로 둘러볼 수 있게 한다.
public class MapDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    public RectTransform content;
    public float horizontalLimit = 180f;
    public float verticalLimit = 650f;

    private Canvas canvas;
    private Vector2 initialPosition;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();

        if (content != null)
            initialPosition = content.anchoredPosition;
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

        nextPosition.x = Mathf.Clamp(
            nextPosition.x,
            initialPosition.x - horizontalLimit,
            initialPosition.x + horizontalLimit
        );
        nextPosition.y = Mathf.Clamp(
            nextPosition.y,
            initialPosition.y - verticalLimit,
            initialPosition.y + verticalLimit
        );

        content.anchoredPosition = nextPosition;
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

        nextPosition.y = Mathf.Clamp(
            nextPosition.y,
            initialPosition.y - verticalLimit,
            initialPosition.y + verticalLimit
        );

        content.anchoredPosition = nextPosition;
    }
}
