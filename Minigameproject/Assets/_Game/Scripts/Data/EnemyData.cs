using UnityEngine;

public enum EnemyType
{
    Normal,
    Elite,
    Boss
}

public enum EnemyActionType
{
    Attack,
    Defense,
    Buff
}

public enum EnemyPatternMode
{
    Sequential,
    PeriodicBuffRandom
}

[System.Serializable]
public class EnemyPattern
{
    public EnemyActionType actionType;
    public int value;
    public bool firstLoopOnly;
}

[CreateAssetMenu(fileName = "Data", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public EnemyType enemyType;

    public int maxHealth;
    [Min(0)] public int maxDamagePerTurn;

    [TextArea] public string description;

    public EnemyPatternMode patternMode;
    [Min(0)] public int attackIncreasePerPatternLoop;
    [Min(0)] public int periodicBuffInterval;
    [Min(0)] public int periodicBuffAmount;

    public EnemyPattern[] patterns;

    public Sprite portrait;
    public GameObject prefab;
}
