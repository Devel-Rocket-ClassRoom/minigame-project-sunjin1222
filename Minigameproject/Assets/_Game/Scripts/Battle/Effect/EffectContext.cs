using UnityEngine;
using System.Collections.Generic;

public class EffectContext
{
    public EnemyController enemyController;
    public PlayerController playerController;
    public HandManager handManager;
    public DeckManager deckManager;
    public List<CardData> activatedCards = new List<CardData>();
}
