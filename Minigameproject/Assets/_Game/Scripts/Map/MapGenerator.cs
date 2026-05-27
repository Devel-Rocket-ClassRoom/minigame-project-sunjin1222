using UnityEngine;

// EP.1 후보 구역의 원본 데이터를 만들고 전투 구역에 적을 연결한다.
public class MapGenerator : MonoBehaviour
{
    [Header("Enemy Pools")]
    public EnemyData[] normalEnemyPool;
    public EnemyData[] eliteEnemyPool;
    public EnemyData[] bossEnemyPool;

    [Header("EP.1 Event Reward")]
    public CardData shrineRewardCard;

    public MapData GenerateMap()
    {
        MapData mapData = new MapData();

        mapData.nodes.Add(CreateCandidate(0, -2, "마을 훈련장", MapNodeType.NormalBattle, 1, "기본 카드"));
        mapData.nodes.Add(CreateCandidate(1, -1, "숲길 입구", MapNodeType.NormalBattle, 1, "체인 카드"));
        mapData.nodes.Add(CreateCandidate(2, 0, "낡은 신전", MapNodeType.Event, 1, "회복 / 각성"));
        mapData.nodes.Add(CreateCandidate(3, 1, "오염된 늑대굴", MapNodeType.NormalBattle, 2, "공격 카드"));
        mapData.nodes.Add(CreateCandidate(4, 2, "무너진 초소", MapNodeType.NormalBattle, 2, "방어 카드"));

        AssignEnemies(mapData);
        return mapData;
    }

    private MapNodeData CreateCandidate(
        int id,
        int column,
        string zoneName,
        MapNodeType nodeType,
        int riskLevel,
        string rewardHint)
    {
        return new MapNodeData
        {
            id = id,
            floor = 1,
            column = column,
            zoneName = zoneName,
            nodeType = nodeType,
            riskLevel = riskLevel,
            rewardHint = rewardHint,
            state = MapNodeState.Available
        };
    }

    private void AssignEnemies(MapData mapData)
    {
        foreach (MapNodeData node in mapData.nodes)
        {
            if (node.nodeType == MapNodeType.NormalBattle)
                node.enemyData = GetRandomEnemy(normalEnemyPool);
        }
    }

    private EnemyData GetRandomEnemy(EnemyData[] enemyPool)
    {
        if (enemyPool == null || enemyPool.Length == 0)
            return null;

        return enemyPool[Random.Range(0, enemyPool.Length)];
    }
}
