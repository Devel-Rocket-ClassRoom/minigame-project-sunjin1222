using UnityEngine;

public static class BattleRelicResolver
{
    public static bool ShouldRepeatFirstTile(int turnCount, PlayerController playerController)
    {
        if (RunData.currentRelics == null)
            return false;

        foreach (RelicData relic in RunData.currentRelics)
        {
            if (relic == null)
                continue;

            if (!RelicManager.CanApplyRelic(relic, playerController))
                continue;

            if (relic.triggerType != RelicTriggerType.TurnActivation)
                continue;

            if (relic.effectType != RelicEffectType.RepeatFirstTileEveryTurns)
                continue;

            int interval = Mathf.Max(1, relic.amount);
            if (turnCount % interval == 0)
            {
                Debug.Log($"[RelicManager] {relic.relicName} 발동");
                return true;
            }
        }

        return false;
    }

    public static int GetSagaRequiredOrderReduction(PlayerController playerController = null)
    {
        if (RunData.currentRelics == null)
            return 0;

        int reduction = 0;

        foreach (RelicData relic in RunData.currentRelics)
        {
            if (relic == null)
                continue;

            if (!RelicManager.CanApplyRelic(relic, playerController))
                continue;

            if (relic.effectType == RelicEffectType.ReduceSagaRequiredOrder)
                reduction += Mathf.Max(0, relic.amount);
        }

        return reduction;
    }

    public static void ApplyEndTurnBoardRelics(
        BoardManager boardManager,
        PlayerController playerController,
        EnemyController enemyController)
    {
        if (RunData.currentRelics == null || boardManager == null)
            return;

        int unusedCellCount = boardManager.CountUnusedCells();

        if (unusedCellCount <= 0)
            return;

        foreach (RelicData relic in RunData.currentRelics)
        {
            if (relic == null)
                continue;

            if (!RelicManager.CanApplyRelic(relic, playerController))
                continue;

            if (relic.triggerType != RelicTriggerType.TurnActivation)
                continue;

            if (relic.effectType == RelicEffectType.GainBlockPerUnusedBoardCell)
            {
                if (playerController == null)
                    continue;

                int blockAmount = unusedCellCount * Mathf.Max(1, relic.amount);
                playerController.GainBlock(blockAmount);

                Debug.Log($"[RelicManager] {relic.relicName} 발동: 방어도 {blockAmount}");
                continue;
            }

            if (relic.effectType == RelicEffectType.DealDamagePerUnusedBoardCell)
            {
                if (enemyController == null)
                    continue;

                int damageAmount = unusedCellCount * Mathf.Max(1, relic.amount);
                enemyController.TakeDamage(damageAmount);

                Debug.Log($"[RelicManager] {relic.relicName} 발동: 피해 {damageAmount}");
            }
        }
    }
}
