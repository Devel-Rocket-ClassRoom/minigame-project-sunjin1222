using UnityEngine;

public enum CardType 
{
    Attack,
    Skill,
    Support,
}

[CreateAssetMenu(fileName = "CardData", menuName = "Game/Card Data")]
public class CardData : ScriptableObject
{
    public string cardId;
    public string cardName;
    public Vector2Int[] tileShape;
    public Vector2Int tileOrigin;
    public EffectSO[] effects;
    [TextArea] public string description;
    [TextArea] public string tooltipText;
    public Sprite icon;
    
    public CardType cardType;
    public GameObject tileBlockPrefab;
    public float floatingPreviewTileSize = 60f;
    public GameObject boardPreviewPrefab;
}
