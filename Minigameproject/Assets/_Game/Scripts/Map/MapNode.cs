using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum MapNodeType
{
    Start,
    NormalBattle,
    EliteBattle,
    Event,
    Rest,
    Boss
}

public enum MapNodeState
{
    Locked,
    Available,
    Selected,
    Cleared
}

public class MapNode : MonoBehaviour
{
    public TextMeshProUGUI nodeText;
    public Button button;
    public Image nodeImage;

    private MapNodeData nodeData;
    private Action<MapNodeData> onSelected;
    private Action onAction;

    public void Setup(MapNodeData data, Action<MapNodeData> selectedCallback)
    {
        nodeData = data;
        onSelected = selectedCallback;
        onAction = null;

        nodeText.fontSize = 18f;
        nodeText.text = BuildLabel(nodeData);

        button.onClick.RemoveListener(OnButtonClicked);
        button.onClick.AddListener(OnButtonClicked);

        switch (nodeData.state)
        {
            case MapNodeState.Locked:
                nodeImage.color = Color.gray;
                button.interactable = false;
                break;

            case MapNodeState.Available:
                nodeImage.color = Color.white;
                button.interactable = true;
                break;

            case MapNodeState.Selected:
                nodeImage.color = new Color(0.45f, 0.78f, 1f);
                button.interactable = true;
                break;

            case MapNodeState.Cleared:
                nodeImage.color = Color.green;
                button.interactable = false;
                break;
        }
    }

    public void SetupAction(string label, bool interactable, Action action)
    {
        nodeData = null;
        onSelected = null;
        onAction = action;
        nodeText.fontSize = 21f;
        nodeText.text = label;
        nodeImage.color = interactable ? new Color(0.95f, 0.82f, 0.36f) : Color.gray;
        button.interactable = interactable;
        button.onClick.RemoveListener(OnButtonClicked);
        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        if (onAction != null)
        {
            onAction.Invoke();
            return;
        }

        if (nodeData == null ||
            nodeData.state != MapNodeState.Available &&
            nodeData.state != MapNodeState.Selected)
            return;

        onSelected?.Invoke(nodeData);
    }

    private string BuildLabel(MapNodeData data)
    {
        string typeLabel;

        switch (data.nodeType)
        {
            case MapNodeType.Event:
                typeLabel = "이벤트";
                break;
            case MapNodeType.Rest:
                typeLabel = "휴식";
                break;
            default:
                typeLabel = "일반 전투";
                break;
        }

        string stars = new string('★', Mathf.Max(1, data.riskLevel));
        string selection = data.selectionOrder > 0 ? $"\n[{data.selectionOrder}번째 선택]" : string.Empty;

        return $"{data.zoneName}\n{typeLabel} {stars}\n{data.rewardHint}{selection}";
    }
}
