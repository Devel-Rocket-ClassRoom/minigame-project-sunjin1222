
using UnityEngine;

[CreateAssetMenu(
    fileName = "SageBlockEffect",
    menuName = "Game/Effect/SageBlockEffect"
)]
public class SageBlockEffect : EffectSO
{
    public int originblock;
    public int requiredOrder;
    public int bonusblock;

    public override void Apply(EffectContext context)
    {
        int finalblock = originblock;
        if (context.activationOrder >= context.GetAdjustedRequiredOrder(requiredOrder))
        {
            finalblock += bonusblock;
        }
        context.playerController.GainBlock(finalblock);
    }
    
    public override void Preview(EffectContext context, EffectPreviewResult result)
    {
        int finalBlock = originblock;

        if (context.activationOrder >= context.GetAdjustedRequiredOrder(requiredOrder))
        {
            finalBlock += bonusblock;
        }

        result.block += finalBlock;
    }
}

