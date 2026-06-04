using UnityEngine;

// EP 후보 구역의 원본 데이터를 만들고 전투/이벤트 데이터를 연결한다.
public class MapGenerator : MonoBehaviour
{
    [Header("Map JSON")]
    [SerializeField] private string mapJsonResourceFormat = "Map/Episode{0}Map";
    [SerializeField] private int maxEpisodeNumber = 8;
    [SerializeField] private int visibleNodeCount = 5;

    [Header("Enemy Pools")]
    public EnemyData[] episode1NormalEnemyPool;
    public EnemyData[] normalEnemyPool;
    public EnemyData[] eliteEnemyPool;
    public EnemyData[] bossEnemyPool;

    [Header("Event Pool")]
    public EventData[] eventPool;

    public int MaxEpisodeNumber => maxEpisodeNumber;

    public MapData GenerateMap()
    {
        return GenerateMap(RunData.currentFloor);
    }

    public MapData GenerateMap(int episodeNumber)
    {
        MapJsonData jsonData = new MapJsonLoader(mapJsonResourceFormat).Load(episodeNumber);
        MapData mapData = new MapData
        {
            episodeNumber = jsonData.episodeNumber,
            episodeTitle = jsonData.episodeTitle
        };

        new MapNodeSelector(visibleNodeCount).AddRandomNodes(mapData, jsonData.nodes);
        EnemyData[] selectedNormalEnemyPool =
            episodeNumber == 1 &&
            episode1NormalEnemyPool != null &&
            episode1NormalEnemyPool.Length > 0
                ? episode1NormalEnemyPool
                : normalEnemyPool;

        new MapEncounterAssigner(
            selectedNormalEnemyPool,
            eliteEnemyPool,
            bossEnemyPool,
            eventPool
        ).Assign(mapData);

        return mapData;
    }

    public bool IsEnemyValidForNode(MapNodeData node)
    {
        if (node == null || node.enemyData == null)
            return false;

        return node.nodeType switch
        {
            MapNodeType.NormalBattle => node.enemyData.enemyType == EnemyType.Normal,
            MapNodeType.EliteBattle => node.enemyData.enemyType == EnemyType.Elite,
            MapNodeType.Boss => node.enemyData.enemyType == EnemyType.Boss,
            _ => true
        };
    }

    public EnemyData GetRandomEnemyForNode(MapNodeData node)
    {
        if (node == null)
            return null;

        EnemyData[] enemyPool = GetEnemyPoolForNode(node);
        EnemyType? expectedType = GetExpectedEnemyType(node.nodeType);

        if (enemyPool == null || enemyPool.Length == 0 || !expectedType.HasValue)
            return null;

        EnemyData[] candidates = System.Array.FindAll(
            enemyPool,
            enemy => enemy != null && enemy.enemyType == expectedType.Value);

        if (candidates.Length == 0)
            return null;

        return candidates[Random.Range(0, candidates.Length)];
    }

    private EnemyData[] GetEnemyPoolForNode(MapNodeData node)
    {
        if (node.nodeType == MapNodeType.NormalBattle)
        {
            int episodeNumber = RunData.currentMap != null
                ? RunData.currentMap.episodeNumber
                : RunData.currentFloor;

            return episodeNumber == 1 &&
                episode1NormalEnemyPool != null &&
                episode1NormalEnemyPool.Length > 0
                    ? episode1NormalEnemyPool
                    : normalEnemyPool;
        }

        if (node.nodeType == MapNodeType.EliteBattle)
            return eliteEnemyPool;

        if (node.nodeType == MapNodeType.Boss)
            return bossEnemyPool;

        return null;
    }

    private EnemyType? GetExpectedEnemyType(MapNodeType nodeType)
    {
        return nodeType switch
        {
            MapNodeType.NormalBattle => EnemyType.Normal,
            MapNodeType.EliteBattle => EnemyType.Elite,
            MapNodeType.Boss => EnemyType.Boss,
            _ => null
        };
    }
}
