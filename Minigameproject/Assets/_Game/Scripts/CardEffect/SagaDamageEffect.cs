
using UnityEngine;

[CreateAssetMenu(
    fileName = "SagaDamageEffect",
    menuName = "Game/Effect/SagaDamageEffect"
)]
public class SagaDamageEffect : EffectSO
{
    public int originDamage;
    public int requiredOrder;
    public int bonusDamage;

    public override void Apply(EffectContext context)
    {
        int finalDamage = originDamage;
        if (context.activationOrder >= requiredOrder)
        {
            finalDamage += bonusDamage;
        }
        context.enemyController.TakeDamage(finalDamage);
    }
    public override void Preview(EffectContext context, EffectPreviewResult result)
    {
        int finalDamage = originDamage;

        if (context.activationOrder >= requiredOrder)
        {
            finalDamage += bonusDamage;
        }

        result.damage += finalDamage;
    }
}

