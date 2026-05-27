using System.Collections.Generic;

[System.Serializable]
public class MapData
{
    public int episodeNumber = 1;
    public string episodeTitle = "사라진 출발 장면";
    public List<MapNodeData> nodes = new List<MapNodeData>();
    public List<int> selectedNodeIds = new List<int>();
    public bool planConfirmed;
    public int executionIndex;
    public bool episodeCompleted;
}
