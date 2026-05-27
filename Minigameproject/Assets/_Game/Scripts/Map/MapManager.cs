using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// MapScene 진입점. 노드 UI를 생성하고 클릭 이벤트를 처리한다.
/// </summary>
public class MapManager : MonoBehaviour
{
    [Header("노드 프리팹 & 컨테이너")]
    public GameObject mapNodePrefab;   // MapNode 컴포넌트가 붙은 프리팹
    public Transform nodesParent;      // 노드들이 들어갈 부모 Transform

    [Header("휴식 패널 (선택)")]
    public GameObject restPanel;       // 휴식 노드 진입 시 보여줄 패널 (없으면 무시)

    private MapNode[] nodeObjects;

    private void Start()
    {
        // 맵 데이터가 없으면 새로 생성
        if (RunData.mapNodeTypes == null || RunData.mapNodeTypes.Length == 0)
        {
            RunData.mapNodeTypes   = MapGenerator.Generate();
            RunData.mapNodeStates  = new MapNodeState[MapGenerator.TOTAL_FLOORS];
            RunData.currentFloor   = 0;

            // 첫 층만 Available
            RunData.mapNodeStates[0] = MapNodeState.Available;
            for (int i = 1; i < MapGenerator.TOTAL_FLOORS; i++)
                RunData.mapNodeStates[i] = MapNodeState.Locked;
        }

        BuildMapUI();
    }

    /// <summary>노드 UI를 인스턴스화하고 초기화한다.</summary>
    private void BuildMapUI()
    {
        if (mapNodePrefab == null || nodesParent == null)
        {
            Debug.LogError("[MapManager] mapNodePrefab 또는 nodesParent가 연결되지 않았습니다.");
            return;
        }

        nodeObjects = new MapNode[MapGenerator.TOTAL_FLOORS];

        for (int i = 0; i < MapGenerator.TOTAL_FLOORS; i++)
        {
            GameObject go = Instantiate(mapNodePrefab, nodesParent);
            MapNode node  = go.GetComponent<MapNode>();

            if (node == null)
            {
                Debug.LogError("[MapManager] mapNodePrefab에 MapNode 컴포넌트가 없습니다.");
                continue;
            }

            node.Setup(i, RunData.mapNodeTypes[i], RunData.mapNodeStates[i], this);
            nodeObjects[i] = node;
        }
    }

    /// <summary>노드 클릭 시 MapNode에서 호출된다.</summary>
    public void EnterNode(MapNode node)
    {
        // 현재 노드를 Cleared로
        RunData.mapNodeStates[node.floorIndex] = MapNodeState.Cleared;
        RunData.currentFloor = node.floorIndex;

        // 다음 층 Unlocking (마지막 층이 아닐 때)
        int next = node.floorIndex + 1;
        if (next < MapGenerator.TOTAL_FLOORS)
            RunData.mapNodeStates[next] = MapNodeState.Available;

        switch (node.nodeType)
        {
            case MapNodeType.NormalBattle:
            case MapNodeType.EliteBattle:
            case MapNodeType.Boss:
                RunData.currentNodeType = node.nodeType;
                SceneManager.LoadScene("BattleScene");
                break;

            case MapNodeType.Rest:
                HandleRest(node);
                break;
        }
    }

    private void HandleRest(MapNode node)
    {
        // HP 30% 회복
        int heal = Mathf.RoundToInt(RunData.maxHp * 0.3f);
        RunData.currentHp = Mathf.Min(RunData.currentHp + heal, RunData.maxHp);
        Debug.Log($"[MapManager] 휴식: {heal} HP 회복 → 현재 HP {RunData.currentHp}");

        if (restPanel != null)
        {
            // 패널이 연결되어 있으면 보여주고 플레이어가 닫으면 맵 갱신
            restPanel.SetActive(true);
        }
        else
        {
            // 패널 없으면 즉시 맵 갱신
            RefreshMapUI();
        }
    }

    /// <summary>휴식 패널의 '닫기' 버튼에서 호출한다.</summary>
    public void OnRestPanelClose()
    {
        if (restPanel != null)
            restPanel.SetActive(false);

        RefreshMapUI();
    }

    /// <summary>전투 복귀 후 맵을 다시 열었을 때 노드 상태를 갱신한다.</summary>
    private void RefreshMapUI()
    {
        if (nodeObjects == null) return;
        for (int i = 0; i < nodeObjects.Length; i++)
        {
            if (nodeObjects[i] != null)
                nodeObjects[i].SetState(RunData.mapNodeStates[i]);
        }
    }
}
