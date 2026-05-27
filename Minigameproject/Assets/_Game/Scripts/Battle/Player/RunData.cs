using System.Collections.Generic;

public static class RunData
{
    // ── 덱 ──────────────────────────────────────────
    public static List<CardData> currentDeck = new List<CardData>();
    public static bool IsInitialized => currentDeck.Count > 0;

    // ── 플레이어 상태 ────────────────────────────────
    public static int currentHp = 50;
    public static int maxHp     = 50;

    // ── 맵 상태 ─────────────────────────────────────
    public static int currentFloor = 0;                  // 0-based (0 = 1층)
    public static int AddedCard    = 0;

    /// <summary>10층 노드 타입 배열. MapGenerator.Generate()로 채운다.</summary>
    public static MapNodeType[]  mapNodeTypes;
    /// <summary>각 노드의 현재 상태 (Locked / Available / Cleared)</summary>
    public static MapNodeState[] mapNodeStates;
    /// <summary>방금 선택한 노드 타입. BattleScene에서 어떤 적을 불러올지 판단할 때 사용.</summary>
    public static MapNodeType    currentNodeType = MapNodeType.NormalBattle;

    // ── 초기화 ──────────────────────────────────────
    public static void Init(List<CardData> startDeck, int hp)
    {
        currentDeck = new List<CardData>(startDeck);
        maxHp       = hp;
        currentHp   = hp;
        currentFloor = 0;
        AddedCard    = 0;

        // 맵 새로 생성
        mapNodeTypes  = MapGenerator.Generate();
        mapNodeStates = new MapNodeState[MapGenerator.TOTAL_FLOORS];
        mapNodeStates[0] = MapNodeState.Available;
        for (int i = 1; i < MapGenerator.TOTAL_FLOORS; i++)
            mapNodeStates[i] = MapNodeState.Locked;

        currentNodeType = MapNodeType.NormalBattle;
    }

    public static void AddCard(CardData card) => currentDeck.Add(card);

    public static void Clear()
    {
        currentDeck.Clear();
        currentHp    = 50;
        maxHp        = 50;
        currentFloor = 0;
        AddedCard    = 0;
        mapNodeTypes  = null;
        mapNodeStates = null;
        currentNodeType = MapNodeType.NormalBattle;
    }
}
