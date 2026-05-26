using UnityEngine;

[CreateAssetMenu(
    fileName = "ChainBlockEffect",
    menuName = "Game/Effect/Chain Block"
)]
public class ChainBlockEffect : EffectSO
{
    public int originBlock;
    public int bonusPerAdjacentCard = 2;

    public override void Apply(EffectContext context)
    {
        int finalBlock =
            originBlock + context.adjacentCardCount * bonusPerAdjacentCard;
        context.playerController.GainBlock(finalBlock);
    }
    public override void Preview(EffectContext context, EffectPreviewResult result)
    {
        int finalBlock =
            originBlock + context.adjacentCardCount * bonusPerAdjacentCard;
        result.block += finalBlock;
    }
}
