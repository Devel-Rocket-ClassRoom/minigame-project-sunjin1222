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

[System.Serializable]
public class EnemyPattern
{
    public EnemyActionType actionType;
    public int value;
}

[CreateAssetMenu(fileName = "Data", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public EnemyType enemyType;

    public int maxHealth;

    [TextArea] public string description;

    public EnemyPattern[] patterns;

    public Sprite portrait;
    public GameObject prefab;
}