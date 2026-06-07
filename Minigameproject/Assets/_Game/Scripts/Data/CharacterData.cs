using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Game/Character Data")]
public class CharacterData : ScriptableObject
{
    public string characterId;
    public string characterName;
    public int maxHp = 50;
    [TextArea] public string description;
    [TextArea] public string Playdescription;

    public Sprite image;
    public Sprite battleSprite;
    public GameObject battleUIPrefab;
    public List<CardData> startDeck = new List<CardData>();
    public List<CardData> rewardCardPool = new List<CardData>();
    public List<RelicData> startingRelics = new List<RelicData>();
}
