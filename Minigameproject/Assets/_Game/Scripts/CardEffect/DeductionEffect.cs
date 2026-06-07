using UnityEngine;

public enum DeductionSpendMode
{
    FixedAmount,
    AllEvidence
}

[CreateAssetMenu(
    fileName = "DeductionEffect",
    menuName = "Game/Effect/Mystery/Deduction"
)]
public class DeductionEffect : EffectSO
{
    public DeductionSpendMode spendMode = DeductionSpendMode.FixedAmount;

    [Header("Base")]
    public int baseDamage;
    public int baseBlock;

    [Header("Fixed Amount")]
    public int evidenceCost;
    public int bonusDamage;
    public int bonusBlock;

    [Header("All Evidence")]
    public int damagePerEvidence;
    public int blockPerEvidence;

    public override void Apply(EffectContext context)
    {
        if (context.battleController == null)
        {
            Debug.LogError("[DeductionEffect] BattleController가 없습니다.");
            return;
        }

        int damage = baseDamage;
        int block = baseBlock;

        if (spendMode == DeductionSpendMode.FixedAmount)
        {
            if (context.battleController.TrySpendEvidence(evidenceCost))
            {
                damage += bonusDamage;
                block += bonusBlock;
            }
        }
        else
        {
            int spentEvidence = context.battleController.SpendAllEvidence();
            damage += spentEvidence * damagePerEvidence;
            block += spentEvidence * blockPerEvidence;
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

        if (spendMode == DeductionSpendMode.FixedAmount)
        {
            if (context.battleController == null ||
                context.battleController.HasEvidence(evidenceCost))
            {
                damage += bonusDamage;
                block += bonusBlock;
            }

            result.damage += damage;
            result.block += block;
            return;
        }

        if (context.battleController != null)
        {
            damage += context.battleController.EvidenceCount * damagePerEvidence;
            block += context.battleController.EvidenceCount * blockPerEvidence;
        }

        result.damage += damage;
        result.block += block;
    }
}
