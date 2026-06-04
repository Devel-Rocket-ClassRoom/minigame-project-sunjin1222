using UnityEngine;

// EP 후보 구역의 원본 데이터를 만들고 전투/이벤트 데이터를 연결한다.
public class MapGenerator : MonoBehaviour
{
    [Header("Map JSON")]
    [SerializeField] private string mapJsonResourceFormat = "Map/Episode{0}Map";
    [SerializeField] private int maxEpisodeNumber = 8;
    [SerializeField] private int visibleNodeCount = 5;

    [Header("Enemy Pools")]
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
        new MapEncounterAssigner(
            normalEnemyPool,
            eliteEnemyPool,
            bossEnemyPool,
            eventPool
        ).Assign(mapData);

        return mapData;
    }
}
