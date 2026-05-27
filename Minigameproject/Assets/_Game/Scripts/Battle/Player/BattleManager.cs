using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public EnemyData[] enemyPool;
    public Transform enemySpawnPoint;

    public BattleController battleController;
    public PlayerController playerController;
    public RewardManager rewardManager;
    public GameObject panul;

    public static EnemyController CurrentEnemy { get; private set; }

    private void Awake()
    {
        if (enemyPool == null || enemyPool.Length == 0)
        {
            Debug.LogWarning("[BattleManager] enemyPool이 비어있습니다.");
            return;
        }

        if (playerController == null)
            Debug.LogError("[BattleManager] playerController가 인스펙터에 연결되지 않았습니다!");

        EnemyData enemyToSpawn = RunData.selectedEnemy != null
            ? RunData.selectedEnemy
            : enemyPool[Random.Range(0, enemyPool.Length)];

        SpawnEnemy(enemyToSpawn);
    }

    private void SpawnEnemy(EnemyData data)
    {
        if (data.prefab == null)
        {
            Debug.LogError($"[BattleManager] {data.enemyName}의 prefab이 없습니다.");
            return;
        }

        GameObject instance = Instantiate(data.prefab);
        instance.transform.SetParent(enemySpawnPoint, false);

        EnemyController controller = instance.GetComponentInChildren<EnemyController>();

        if (controller == null)
        {
            Debug.LogError("[BattleManager] 생성된 프리팹에 EnemyController가 없습니다.");
            return;
        }

        // 외부 참조 연결
     
        controller.playerController = playerController;
        controller.rewardManager = rewardManager;
        controller.panul = panul;

        CurrentEnemy = controller;
        controller.Initialize(data);
    }
}
