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
    public bool requiredToProgress;
    public int fixedSelectionOrder;
    public string questId;

    public MapNodeType nodeType;
    public MapNodeState state;

    public EnemyData enemyData;
}
