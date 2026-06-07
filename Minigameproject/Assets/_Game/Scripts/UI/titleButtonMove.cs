using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class titleButtonMove : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private RectTransform moveTarget;
    [SerializeField] private Image hoverImage;

    [SerializeField] private float moveAmount = 40f;

    private Vector2 originalPos;

    private void Awake()
    {
        if (moveTarget == null)
            moveTarget = GetComponent<RectTransform>();

        if (moveTarget == null)
        {
            enabled = false;
            return;
        }

        originalPos = moveTarget.anchoredPosition;

        if (hoverImage != null)
        {
            hoverImage.raycastTarget = false;
            hoverImage.gameObject.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        moveTarget.anchoredPosition = originalPos + new Vector2(moveAmount, 0f);

        if (hoverImage != null)
            hoverImage.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetHoverState();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ResetHoverState();
    }

    private void OnDisable()
    {
        ResetHoverState();
    }

    private void ResetHoverState()
    {
        if (moveTarget != null)
            moveTarget.anchoredPosition = originalPos;

        if (hoverImage != null)
            hoverImage.gameObject.SetActive(false);
    }
}
