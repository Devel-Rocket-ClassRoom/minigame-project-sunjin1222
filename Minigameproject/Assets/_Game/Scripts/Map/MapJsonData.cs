using System;

[Serializable]
public class MapJsonData
{
    public int episodeNumber = 1;
    public string episodeTitle = "사라진 출발 장면";
    public MapNodeJsonData[] nodes = Array.Empty<MapNodeJsonData>();
}

[Serializable]
public class MapNodeJsonData
{
    public int id = -1;
    public int floor = 1;
    public string zoneName = string.Empty;
    public string nodeType = nameof(MapNodeType.NormalBattle);
    public int riskLevel = 1;
    public string rewardHint = string.Empty;
    public string questId = string.Empty;
    public int[] availableEpisodes = Array.Empty<int>();
    public bool showOnce = false;
    public int mustAppearByEpisode = -1;
    public string[] requiredClearedQuestIds = Array.Empty<string>();
    public bool forceInclude = false;
    public bool requiredToProgress = false;
    public bool placeLast = false;
    public int fixedSelectionOrder = 0;
    public string fixedEventId = string.Empty;
}
