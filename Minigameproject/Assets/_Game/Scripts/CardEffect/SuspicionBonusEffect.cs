using UnityEngine;

[CreateAssetMenu(
    fileName = "SuspicionBonusEffect",
    menuName = "Game/Effect/Mystery/Suspicion Bonus"
)]
public class SuspicionBonusEffect : EffectSO
{
    [Header("Base")]
    public int baseDamage;
    public int baseBlock;

    [Header("Suspicion")]
    public int requiredEvidence;
    public int bonusDamage;
    public int bonusBlock;

    public override void Apply(EffectContext context)
    {
        if (context.battleController == null)
        {
            Debug.LogError("[SuspicionBonusEffect] BattleController가 없습니다.");
            return;
        }

        int damage = baseDamage;
        int block = baseBlock;

        if (context.battleController.HasEvidence(requiredEvidence))
        {
            damage += bonusDamage;
            block += bonusBlock;
        }

        if (damage > 0)
            context.enemyController.TakeDamage(damage, context.IsCurrentCardAttack);

        if (block > 0)
            context.playerController.GainBlock(block);
    }

    public override void Preview(EffectContext context, EffectPreviewResult result)
    {
        int damage = baseDamage;
        int block = baseBlock;

        if (context.battleController == null ||
            context.battleController.HasEvidence(requiredEvidence))
        {
            damage += bonusDamage;
            block += bonusBlock;
        }

        result.damage += damage;
        result.block += block;
    }
}
