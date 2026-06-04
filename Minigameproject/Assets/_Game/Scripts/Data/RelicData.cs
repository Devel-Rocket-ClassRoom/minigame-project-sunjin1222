using UnityEngine;

public enum RelicTriggerType
{
    BattleStart,
    BattleVictory,
    TurnActivation,
    TurnStart
}

public enum RelicEffectType
{
    GainBlock,
    DrawCards,
    Heal,
    RepeatFirstTileEveryTurns,
    ReduceSagaRequiredOrder,
    GainBlockPerUnusedBoardCell,
    DealDamagePerUnusedBoardCell
}

public enum RelicConditionType
{
    None,
    PlayerHealthAtOrBelowPercent
}

[CreateAssetMenu(fileName = "RelicData", menuName = "Game/Relic Data")]
public class RelicData : ScriptableObject
{
    public string relicId;
    public string relicName;
    [TextArea] public string description;
    public Sprite icon;
    public RelicTriggerType triggerType = RelicTriggerType.BattleStart;
    public RelicEffectType effectType = RelicEffectType.GainBlock;
    public int amount = 1;
    public RelicConditionType conditionType = RelicConditionType.None;
    public int conditionAmount = 50;
}
