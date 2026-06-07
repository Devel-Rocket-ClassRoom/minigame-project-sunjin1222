using UnityEngine;

[CreateAssetMenu(
    fileName = "EvidenceGainEffect",
    menuName = "Game/Effect/Mystery/Evidence Gain"
)]
public class EvidenceGainEffect : EffectSO
{
    public int evidence;

    public override void Apply(EffectContext context)
    {
        if (context.battleController == null)
        {
            Debug.LogError("[EvidenceGainEffect] BattleController가 없습니다.");
            return;
        }

        context.battleController.AddEvidence(evidence);
    }
}
