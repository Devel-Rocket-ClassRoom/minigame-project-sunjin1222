using UnityEngine;

public class EnemyDeathHandler
{
    private readonly GameObject enemyObject;
    private readonly PlayerController playerController;
    private readonly GameObject rewardPanel;
    private readonly RewardManager rewardManager;
    private readonly GameObject gameClearPanel;
    private readonly bool isBossBattle;

    public EnemyDeathHandler(
     GameObject owner,
     PlayerController player,
     GameObject panel,
     RewardManager rewards,
     GameObject clearPanel,
     bool bossBattle)
    {
        enemyObject = owner;
        playerController = player;
        rewardPanel = panel;
        rewardManager = rewards;
        gameClearPanel = clearPanel;
        isBossBattle = bossBattle;
    }

    public void HandleDeath()
    {
        RelicManager.ApplyRelics(RelicTriggerType.BattleVictory, playerController, null);

        if (isBossBattle || RunData.IsSelectedNodeType(MapNodeType.Boss))
        {
            if (gameClearPanel != null)
                gameClearPanel.SetActive(true);

            if (RunData.currentMap != null && RunData.selectedNodeId >= 0)
                RunData.selectedBattleWon = true;

            if (enemyObject != null)
                enemyObject.SetActive(false);

            return;
        }

        if (rewardPanel != null)
            rewardPanel.SetActive(true);

        if (rewardManager != null)
            rewardManager.ShowRewardButtons();

        if (RunData.currentMap != null && RunData.selectedNodeId >= 0)
            RunData.selectedBattleWon = true;

        BattleController battleController = Object.FindFirstObjectByType<BattleController>();
        if (battleController != null)
            battleController.Didie();

        if (enemyObject != null)
            enemyObject.SetActive(false);
    }
}
