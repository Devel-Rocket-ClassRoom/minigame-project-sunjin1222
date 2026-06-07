using System;
using System.Collections.Generic;

[Serializable]
public class RunSaveData
{
    public string characterId;
    public List<string> deckCardIds = new List<string>();
    public List<string> pendingRewardCardIds = new List<string>();
    public List<string> relicIds = new List<string>();
    public List<string> seenQuestIds = new List<string>();
    public List<string> clearedQuestIds = new List<string>();
    public List<string> seenEventIds = new List<string>();
    public List<int> clearedEpisodeNumbers = new List<int>();
    public int currentHp;
    public int maxHp;
    public int currentFloor;
    public int highestUnlockedEpisode;
    public int addedCard;
    public SavedMapData currentMap;
    public int selectedNodeId;
    public bool selectedBattleWon;
    public string selectedEnemyId;
}

[Serializable]
public class SavedMapData
{
    public int episodeNumber;
    public string episodeTitle;
    public List<SavedMapNodeData> nodes = new List<SavedMapNodeData>();
    public List<int> selectedNodeIds = new List<int>();
    public bool planConfirmed;
    public int executionIndex;
    public bool episodeCompleted;
}

[Serializable]
public class SavedMapNodeData
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
    public string fixedEventId;
    public MapNodeType nodeType;
    public MapNodeState state;
    public string enemyId;
    public string eventId;
}
