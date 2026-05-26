using UnityEngine;

public enum CardType 
{
    Attack,
    Defense,
    Skill,
}

public enum KeywordType
{
    None,
    Chain,  // 인접 카드 수 × 2 (데미지 or 방어도)
    Saga,   // N번째 이후 발동 시 추가 효과
}

[CreateAssetMenu(fileName = "CardData", menuName = "Game/Card Data")]
public class CardData : ScriptableObject
{
    public string cardName;
    public Vector2Int[] tileShape;
    public Vector2Int tileOrigin;
    public EffectSO[] effects;
    [TextArea] public string description;
    public Sprite icon;
    
    public CardType cardType;
    public GameObject tileBlockPrefab;
    public float floatingPreviewTileSize = 60f;
    public GameObject boardPreviewPrefab;

    [Header("Keyword")]
    public KeywordType keyword = KeywordType.None;
    public int sagaN = 0;       // 서사 N — N번째 이후 발동 시 추가 효과
    public int keywordBonus = 0; // 키워드 발동 시 추가 수치 (데미지 or 방어도)
}
