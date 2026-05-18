
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
    public CardType cardType;
    public Vector2Int[] tileShape;
    public int power;
    [TextArea] public string description;
    public Sprite icon;

    public GameObject gameObject;

}