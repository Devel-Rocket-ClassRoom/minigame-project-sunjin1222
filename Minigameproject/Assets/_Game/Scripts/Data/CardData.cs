
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
    public string cardName;
    public Vector2Int[] tileShape;
    public EffectSO[] effects;
    [TextArea] public string description;
    public Sprite icon;
    
    public CardType cardType;
    public GameObject floatingPreviewPrefab; 
    public GameObject boardPreviewPrefab;

}