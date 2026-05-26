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
        int finalBlock = block;

        // 체인: 인접 카드 수 × 2 방어도 추가
        if (context.boardManager != null && context.currentCardOriginIndex >= 0)
        {
            int chainCount = context.boardManager.GetAdjacentCardCount(context.currentCardOriginIndex);
            finalBlock += chainCount * 2;
        }

        // 서사: N번째 이후 발동 시 keywordBonus 추가
        CardData card = context.activatedCards.Count > 0
            ? context.activatedCards[context.activatedCards.Count - 1]
            : null;

        if (card != null && card.keyword == KeywordType.Saga && card.sagaN > 0)
        {
            if (context.activationOrder >= card.sagaN)
            {
                finalBlock += card.keywordBonus;
            }
        }

        context.playerController.AddBlock(finalBlock);
    }
}
