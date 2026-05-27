using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapNodeData
{
    public int id;
    public int floor;
    public int column;

    public string zoneName;
    public string rewardHint;
    public int riskLevel;
    public int selectionOrder;

    public MapNodeType nodeType;
    public MapNodeState state;

    public EnemyData enemyData;
    public List<int> nextNodeIds = new List<int>();
}
