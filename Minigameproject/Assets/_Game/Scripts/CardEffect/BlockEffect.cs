using UnityEngine;

[CreateAssetMenu(
    fileName = "BlockEffect",
    menuName = "Game/Effect/Block"
)]
public class BlockEffect : EffectSO
{
    public int block;

    public override void Apply(EffectContext context)
    {
        context.playerController.GainBlock(block);
    }
        public override void Preview(EffectContext context, EffectPreviewResult result)
    {
        result.block += block;
    }
}
