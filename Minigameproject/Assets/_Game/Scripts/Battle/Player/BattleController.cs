using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleController : MonoBehaviour
{
    public BoardManager boardManager;
    public HandManager handManager;
    public EnemyController enemyController;
    public PlayerController playerController;
    public DeckManager deckManager;



    public static bool IsTurnProcessing = false;

    private void Start()
    {
        if (BattleManager.CurrentEnemy != null)
            enemyController = BattleManager.CurrentEnemy;
        else
            Debug.LogError("[BattleController] BattleManager.CurrentEnemy가 null입니다.");
    }

    public void OnResetClicked()
    {
        if (IsTurnProcessing) return;
        if (boardManager == null || handManager == null)
        {
            Debug.LogError("[BattleController] BoardManager 또는 HandManager가 미할당입니다.");
            return;
        }
        boardManager.ReturnAllToHand(handManager);
    }

    public void OnClickEndTurnButton()
    {
        if (IsTurnProcessing) return;
        if (enemyController == null)
        {
            Debug.LogError("[BattleController] enemyController가 null입니다.");
            return;
        }
        StartCoroutine(TurnRoutine());
    }

 private IEnumerator TurnRoutine()
{
    IsTurnProcessing = true;

    EffectContext context = new EffectContext
    {
        enemyController = enemyController,
        playerController = playerController,
        handManager = handManager,
        deckManager = deckManager
    };

    handManager.DiscardAll();

    var cards = boardManager.GetActivationOrder();

    foreach (CardData card in cards)
    {
        if (enemyController.currentHealth <= 0)
            break;

        if (card.effects == null || card.effects.Length == 0)
            continue;

        foreach (EffectSO effect in card.effects)
        {
            if (enemyController.currentHealth <= 0)
                break;

            effect.Apply(context);
            yield return new WaitForSeconds(0.5f);
        }
    }

    if (enemyController.currentHealth > 0)
    {
        enemyController.ResetBlock();

        yield return new WaitForSeconds(0.5f);

        enemyController.DoTurn();

        yield return new WaitForSeconds(1f);

        playerController.ResetBlock();
    

    boardManager.DiscardBoard(deckManager);
    boardManager.DestroyTiles();
    boardManager.ClearBoard();

    deckManager.DrawCards(6);
    }

    IsTurnProcessing = false;
}

    public void Didie()
    {
        boardManager.DiscardBoard(deckManager);
        boardManager.DestroyTiles();
        boardManager.ClearBoard();
        handManager.DiscardAll();
        IsTurnProcessing = false;
    }

}