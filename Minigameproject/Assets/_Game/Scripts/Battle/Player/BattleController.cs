using UnityEngine;
using System.Collections.Generic;

public class BattleController : MonoBehaviour
{
    public BoardManager boardManager;
    public HandManager handManager;
    public EnemyController enemyController;
    public PlayerController playerController;
    public DeckManager deckManager;

    public void OnResetClicked()
    {
        if (boardManager == null || handManager == null)
        {
            Debug.LogError("[ButtonUI] BoardManager 또는 HandManager가 미할당입니다.");
            return;
        }

        boardManager.ReturnAllToHand(handManager);
    }

    public void OnClickEndTurnButton()
    {
        EffectContext context = new EffectContext
        {
            enemyController = enemyController,
            playerController = playerController,
            handManager = handManager,
            deckManager = deckManager
        };

        // 1. 카드 발동
        var cards = boardManager.GetActivationOrder();
        foreach (CardData card in cards)
        {
            if (card.effects == null || card.effects.Length == 0) continue;
            foreach (EffectSO effect in card.effects)
            {
                effect.Apply(context);
            }
        }
        boardManager.DiscardBoard(deckManager); 
        boardManager.DestroyTiles();        
        boardManager.ClearBoard();           
        handManager.DiscardAll();

        enemyController.DoTurn();
        playerController.ResetBlock();

        deckManager.DrawCards(6);
    }
}