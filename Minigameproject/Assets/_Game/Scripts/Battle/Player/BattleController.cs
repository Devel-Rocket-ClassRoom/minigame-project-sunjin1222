using UnityEngine;
using System.Collections.Generic;

public class BattleController : MonoBehaviour
{
    public BoardManager boardManager;
    public HandManager handManager;
    public EnemyController enemyController;
    public PlayerController playerController;
    public DeckManager deckManager;

    private void Start()
    {
        // BattleManager.Awake()에서 생성된 적 컨트롤러 가져오기
        if (BattleManager.CurrentEnemy != null)
            enemyController = BattleManager.CurrentEnemy;
        else
            Debug.LogError("[BattleController] BattleManager.CurrentEnemy가 null입니다.");
    }

    public void OnResetClicked()
    {
        if (boardManager == null || handManager == null)
        {
            Debug.LogError("[BattleController] BoardManager 또는 HandManager가 미할당입니다.");
            return;
        }
        boardManager.ReturnAllToHand(handManager);
    }

    public void OnClickEndTurnButton()
    {
        if (enemyController == null)
        {
            Debug.LogError("[BattleController] enemyController가 null입니다.");
            return;
        }

        EffectContext context = new EffectContext
        {
            enemyController = enemyController,
            playerController = playerController,
            handManager = handManager,
            deckManager = deckManager
        };

        var cards = boardManager.GetActivationOrder();
        foreach (CardData card in cards)
        {
            if (card.effects == null || card.effects.Length == 0) continue;
            foreach (EffectSO effect in card.effects)
            {
                effect.Apply(context);
            }
        }
        enemyController.ResetBlock();
        boardManager.DiscardBoard(deckManager);
        boardManager.DestroyTiles();
        boardManager.ClearBoard();
        handManager.DiscardAll();

        enemyController.DoTurn();
        playerController.ResetBlock();

        deckManager.DrawCards(6);
    }
}
