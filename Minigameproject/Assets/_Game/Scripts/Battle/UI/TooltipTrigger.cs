using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string tooltipTitle;
    [SerializeField, TextArea] private string tooltipBody;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = transform as RectTransform;
    }

    public void SetTooltip(string title, string body)
    {
        tooltipBody = title;
        tooltipBody = body;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipUI.Show(this, rectTransform, tooltipTitle, tooltipBody);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipUI.Hide(this);
    }

    private void OnDisable()
    {
        TooltipUI.Hide(this);
    }
}
