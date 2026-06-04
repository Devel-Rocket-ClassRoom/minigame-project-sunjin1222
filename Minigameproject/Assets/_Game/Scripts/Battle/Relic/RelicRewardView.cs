using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RelicRewardView : MonoBehaviour, IPointerClickHandler
{
    public Image relicIcon;
    public TMP_Text relicName;
    public TMP_Text relicDescription;

    private RelicData relicData;
    private Action<RelicRewardView, RelicData> onClickCallback;

    public void Setup(RelicData data, Action<RelicRewardView, RelicData> onClick)
    {
        relicData = data;
        onClickCallback = onClick;

        if (data == null)
        {
            UpdateTooltip(null);
            return;
        }

        if (relicIcon != null)
        {
            relicIcon.sprite = data.icon;
            relicIcon.enabled = data.icon != null;
        }

        if (relicName != null)
            relicName.text = data.relicName;

        if (relicDescription != null)
            relicDescription.text = data.description;

        UpdateTooltip(data);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClickCallback?.Invoke(this, relicData);
    }

    private void UpdateTooltip(RelicData data)
    {
        TooltipTrigger tooltipTrigger = GetComponent<TooltipTrigger>();

        if (RelicTooltipBuilder.TryBuild(data, out string title, out string body))
        {
            if (tooltipTrigger == null)
                tooltipTrigger = gameObject.AddComponent<TooltipTrigger>();

                

            tooltipTrigger.SetTooltip(title,body);
            return;
        }

        if (tooltipTrigger != null)
            Destroy(tooltipTrigger);
    }
}
