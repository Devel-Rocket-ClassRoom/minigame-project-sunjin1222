using UnityEngine;

public static class RelicManager
{
    public static void ApplyRelics(
        RelicTriggerType triggerType,
        PlayerController playerController,
        DeckManager deckManager)
    {
        if (RunData.currentRelics == null || RunData.currentRelics.Count == 0)
            return;

        foreach (RelicData relic in RunData.currentRelics)
        {
            if (relic == null || relic.triggerType != triggerType)
                continue;

            if (!CanApplyRelic(relic, playerController))
                continue;

            ApplyRelic(relic, playerController, deckManager);
        }
    }

    public static bool CanApplyRelic(RelicData relic, PlayerController playerController)
    {
        if (relic == null)
            return false;

        switch (relic.conditionType)
        {
            case RelicConditionType.None:
                return true;

            case RelicConditionType.PlayerHealthAtOrBelowPercent:
                return playerController != null &&
                    playerController.IsHealthAtOrBelowPercent(relic.conditionAmount);

            default:
                Debug.LogWarning($"[RelicManager] 처리되지 않은 유물 조건입니다: {relic.conditionType}");
                return false;
        }
    }

    private static void ApplyRelic(
        RelicData relic,
        PlayerController playerController,
        DeckManager deckManager)
    {
        switch (relic.effectType)
        {
            case RelicEffectType.GainBlock:
                if (playerController != null)
                    playerController.GainBlock(relic.amount);
                break;

            case RelicEffectType.DrawCards:
                if (deckManager != null)
                    deckManager.DrawCards(relic.amount);
                break;

            case RelicEffectType.Heal:
                if (playerController != null)
                    playerController.Heal(relic.amount);
                break;

            default:
                Debug.LogWarning($"[RelicManager] 처리되지 않은 유물 효과입니다: {relic.effectType}");
                break;
        }

        Debug.Log($"[RelicManager] {relic.relicName} 발동");
    }
}
