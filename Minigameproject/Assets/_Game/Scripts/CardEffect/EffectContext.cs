using UnityEngine;
using System.Collections.Generic;

public class EffectContext
{
    public EnemyController enemyController;
    public PlayerController playerController;
    public HandManager handManager;
    public DeckManager deckManager;
    public List<CardData> activatedCards = new List<CardData>();
    public CardData currentCard;

    public bool IsCurrentCardAttack => currentCard != null && currentCard.cardType == CardType.Attack;

    public int activationOrder;

    public int adjacentCardCount;

    public int sagaRequiredOrderReduction;

    public int GetAdjustedRequiredOrder(int requiredOrder)
    {
        return Mathf.Max(1, requiredOrder - sagaRequiredOrderReduction);
    }
}
