
using UnityEngine;

[CreateAssetMenu(
    fileName = "ChainDamageEffect",
    menuName = "Game/Effect/Chain Damage"
)]
public class ChainDamageEffect : EffectSO
{
    public int originDamage;
    public int bonusPerAdjacentCard = 2;

    public override void Apply(EffectContext context)
    {
        int finalDamage = originDamage + context.adjacentCardCount * bonusPerAdjacentCard;


        context.enemyController.TakeDamage(finalDamage);
    }
    public override void Preview(EffectContext context, EffectPreviewResult result)
    {
        int finalDamage =
            originDamage + context.adjacentCardCount * bonusPerAdjacentCard;

        result.damage += finalDamage;
    }
}

