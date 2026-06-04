using UnityEngine;
using System.Collections;

public class BattleController : MonoBehaviour
{
    public BoardManager boardManager;
    public HandManager handManager;
    public EnemyController enemyController;
    public PlayerController playerController;
    public DeckManager deckManager;

    public EnemyHealthController enemyHealthController;


    public static bool IsTurnProcessing = false;
    private int turnCount = 0;
    private BoardCardActivator boardCardActivator;

    private void Start()
    {
        if (BattleManager.CurrentEnemy != null)
            enemyController = BattleManager.CurrentEnemy;
        else
            Debug.LogError("[BattleController] BattleManager.CurrentEnemy가 null입니다.");

        boardCardActivator = new BoardCardActivator(boardManager, enemyController);
        StartCoroutine(ApplyBattleStartRelics());
    }

    private IEnumerator ApplyBattleStartRelics()
    {
        yield return null;
        RelicManager.ApplyRelics(RelicTriggerType.BattleStart, playerController, deckManager);
        ApplyTurnStartRelics();
    }

    private void ApplyTurnStartRelics()
    {
        RelicManager.ApplyRelics(RelicTriggerType.TurnStart, playerController, deckManager);
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
        boardCardActivator = new BoardCardActivator(boardManager, enemyController);
        turnCount++;

        EffectContext context = new EffectContext
        {
            enemyController = enemyController,
            playerController = playerController,
            handManager = handManager,
            deckManager = deckManager,
            sagaRequiredOrderReduction = BattleRelicResolver.GetSagaRequiredOrderReduction(playerController)
        };

        handManager.DiscardAll();

        var cards = boardManager.GetActivationOrder();
        bool shouldRepeatFirstTile = BattleRelicResolver.ShouldRepeatFirstTile(turnCount, playerController);
        
        BattleRelicResolver.ApplyEndTurnBoardRelics(boardManager, playerController, enemyController);

        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < cards.Count; i++)
        {
            BoardCardEntry entry = cards[i];

            yield return boardCardActivator.Execute(entry, context, i + 1);

            if (i == 0 && shouldRepeatFirstTile && enemyController.currentHealth > 0)
                yield return boardCardActivator.Execute(entry, context, i + 1);
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
            boardManager.RefreshCardPreviewTexts();

            deckManager.DrawCards(6);
            ApplyTurnStartRelics();
            enemyController.ResetDamageTakenThisTurn();
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
