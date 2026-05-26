using UnityEngine;

[CreateAssetMenu(
    fileName = "DamageEffect",
    menuName = "Game/Effect/Damage"
)]
public class DamageEffect : EffectSO
{
    public int damage;

    public override void Apply(EffectContext context)
    {
        context.enemyController.TakeDamage(damage);
    }
    public override void Preview(EffectContext context, EffectPreviewResult result)
    {
        result.damage += damage;
    }
}
