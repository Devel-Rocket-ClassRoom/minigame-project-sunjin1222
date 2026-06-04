using UnityEngine;

public class EnemyDeathHandler
{
    private readonly GameObject enemyObject;
    private readonly PlayerController playerController;
    private readonly GameObject rewardPanel;
    private readonly RewardManager rewardManager;

    public EnemyDeathHandler(
        GameObject owner,
        PlayerController player,
        GameObject panel,
        RewardManager rewards)
    {
        enemyObject = owner;
        playerController = player;
        rewardPanel = panel;
        rewardManager = rewards;
    }

    public void HandleDeath()
    {
        RelicManager.ApplyRelics(RelicTriggerType.BattleVictory, playerController, null);

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
