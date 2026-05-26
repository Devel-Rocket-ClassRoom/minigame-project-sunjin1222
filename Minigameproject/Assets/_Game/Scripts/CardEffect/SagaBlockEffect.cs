using UnityEngine;

[CreateAssetMenu(
    fileName = "SagaBlockEffect",
    menuName = "Game/Effect/SagaBlockEffect"
)]
public class SagaBlockEffect : EffectSO
{
    public int originblock;
    public int requiredOrder;
    public int bonusblock;

    public override void Apply(EffectContext context)
    {
        int finalblock = originblock;
        if (context.activationOrder >= requiredOrder)
        {
            finalblock += bonusblock;
        }
        context.playerController.GainBlock(finalblock);
    }

    public override void Preview(EffectContext context, EffectPreviewResult result)
    {
        int finalBlock = originblock;
        if (context.activationOrder >= requiredOrder)
        {
            finalBlock += bonusblock;
        }
        result.block += finalBlock;
    }
}
