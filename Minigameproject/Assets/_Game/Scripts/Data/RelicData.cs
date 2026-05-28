using UnityEngine;

public enum RelicTriggerType
{
    BattleStart,
    BattleVictory,
    TurnActivation
}

public enum RelicEffectType
{
    GainBlock,
    DrawCards,
    Heal,
    RepeatFirstTileEveryTurns,
    ReduceSagaRequiredOrder,
    GainBlockPerUnusedBoardCell
}

[CreateAssetMenu(fileName = "RelicData", menuName = "Game/Relic Data")]
public class RelicData : ScriptableObject
{
    public string relicName;
    [TextArea] public string description;
    public Sprite icon;
    public RelicTriggerType triggerType = RelicTriggerType.BattleStart;
    public RelicEffectType effectType = RelicEffectType.GainBlock;
    public int amount = 1;
}
