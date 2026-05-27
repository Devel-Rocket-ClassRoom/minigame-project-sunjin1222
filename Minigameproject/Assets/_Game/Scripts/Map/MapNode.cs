using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum MapNodeType
{
    NormalBattle,
    EliteBattle,
    Rest,
    Boss
}

public enum MapNodeState
{
    Locked,
    Available,
    Cleared
}

/// <summary>
/// 맵의 노드 하나. UI 버튼으로 동작하며 클릭 시 MapManager에 진입 요청.
/// </summary>
public class MapNode : MonoBehaviour
{
    [Header("데이터")]
    public int floorIndex;          // 0 = 1층 (1번 칸)
    public MapNodeType nodeType;
    public MapNodeState nodeState = MapNodeState.Locked;

    [Header("UI 참조")]
    public Button button;
    public Image iconImage;
    public TextMeshProUGUI floorLabel;

    [Header("노드 타입별 색상")]
    public Color colorNormal  = new Color(0.4f, 0.7f, 1f);
    public Color colorElite   = new Color(1f, 0.5f, 0.2f);
    public Color colorRest    = new Color(0.4f, 0.85f, 0.4f);
    public Color colorBoss    = new Color(0.9f, 0.2f, 0.2f);
    public Color colorLocked  = new Color(0.3f, 0.3f, 0.3f);
    public Color colorCleared = new Color(0.6f, 0.6f, 0.6f);

    private MapManager mapManager;

    public void Setup(int floor, MapNodeType type, MapNodeState state, MapManager manager)
    {
        floorIndex = floor;
        nodeType   = type;
        nodeState  = state;
        mapManager = manager;

        if (floorLabel != null)
            floorLabel.text = $"{floor + 1}F";

        RefreshVisual();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    public void SetState(MapNodeState state)
    {
        nodeState = state;
        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (iconImage == null) return;

        if (nodeState == MapNodeState.Locked)
        {
            iconImage.color = colorLocked;
            if (button != null) button.interactable = false;
            return;
        }

        if (nodeState == MapNodeState.Cleared)
        {
            iconImage.color = colorCleared;
            if (button != null) button.interactable = false;
            return;
        }

        // Available
        if (button != null) button.interactable = true;
        iconImage.color = nodeType switch
        {
            MapNodeType.EliteBattle => colorElite,
            MapNodeType.Rest        => colorRest,
            MapNodeType.Boss        => colorBoss,
            _                       => colorNormal
        };
    }

    private void OnClick()
    {
        if (nodeState != MapNodeState.Available) return;
        mapManager.EnterNode(this);
    }
}
