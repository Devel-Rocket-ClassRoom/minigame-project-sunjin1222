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
}
