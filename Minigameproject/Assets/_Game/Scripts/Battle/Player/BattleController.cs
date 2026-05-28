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
    private int turnCount = 0;

    private void Start()
    {
        if (BattleManager.CurrentEnemy != null)
            enemyController = BattleManager.CurrentEnemy;
        else
            Debug.LogError("[BattleController] BattleManager.CurrentEnemy가 null입니다.");

        StartCoroutine(ApplyBattleStartRelics());
    }

    private IEnumerator ApplyBattleStartRelics()
    {
        yield return null;
        RelicManager.ApplyRelics(RelicTriggerType.BattleStart, playerController, deckManager);
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
        turnCount++;

        EffectContext context = new EffectContext
        {
            enemyController = enemyController,
            playerController = playerController,
            handManager = handManager,
            deckManager = deckManager,
            sagaRequiredOrderReduction = GetSagaRequiredOrderReduction()
        };

        handManager.DiscardAll();

        var cards = boardManager.GetActivationOrder();
        bool shouldRepeatFirstTile = ShouldRepeatFirstTileThisTurn();

        for (int i = 0; i < cards.Count; i++)
        {
            BoardCardEntry entry = cards[i];

            yield return ExecuteBoardCard(entry, context, i + 1);

            if (i == 0 && shouldRepeatFirstTile && enemyController.currentHealth > 0)
                yield return ExecuteBoardCard(entry, context, i + 1);
        }
        if (enemyController.currentHealth > 0)
        {
            enemyController.ResetBlock();

            yield return new WaitForSeconds(0.5f);

            ApplyEndTurnBoardRelics();

            enemyController.DoTurn();

            yield return new WaitForSeconds(1f);

            playerController.ResetBlock();


            boardManager.DiscardBoard(deckManager);
            boardManager.DestroyTiles();
            boardManager.ClearBoard();
            boardManager.RefreshCardPreviewTexts();

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

    private IEnumerator ExecuteBoardCard(
        BoardCardEntry entry,
        EffectContext context,
        int activationOrder)
    {
        if (entry == null || entry.card == null)
            yield break;

        context.activationOrder = activationOrder;
        context.adjacentCardCount = boardManager.CountAdjacentCards(entry.originIndex);

        if (enemyController.currentHealth <= 0)
            yield break;

        if (entry.card.effects == null || entry.card.effects.Length == 0)
            yield break;

        foreach (EffectSO effect in entry.card.effects)
        {
            if (enemyController.currentHealth <= 0)
                yield break;

            effect.Apply(context);
            yield return new WaitForSeconds(0.5f);
        }
    }

    private bool ShouldRepeatFirstTileThisTurn()
    {
        if (RunData.currentRelics == null)
            return false;

        foreach (RelicData relic in RunData.currentRelics)
        {
            if (relic == null)
                continue;

            if (relic.triggerType != RelicTriggerType.TurnActivation)
                continue;

            if (relic.effectType != RelicEffectType.RepeatFirstTileEveryTurns)
                continue;

            int interval = Mathf.Max(1, relic.amount);
            if (turnCount % interval == 0)
            {
                Debug.Log($"[RelicManager] {relic.relicName} 발동");
                return true;
            }
        }

        return false;
    }

    private int GetSagaRequiredOrderReduction()
    {
        if (RunData.currentRelics == null)
            return 0;

        int reduction = 0;

        foreach (RelicData relic in RunData.currentRelics)
        {
            if (relic == null)
                continue;

            if (relic.effectType == RelicEffectType.ReduceSagaRequiredOrder)
                reduction += Mathf.Max(0, relic.amount);
        }

        return reduction;
    }

    private void ApplyEndTurnBoardRelics()
    {
        if (RunData.currentRelics == null || boardManager == null || playerController == null)
            return;

        int unusedCellCount = boardManager.CountUnusedCells();

        if (unusedCellCount <= 0)
            return;

        foreach (RelicData relic in RunData.currentRelics)
        {
            if (relic == null)
                continue;

            if (relic.triggerType != RelicTriggerType.TurnActivation)
                continue;

            if (relic.effectType != RelicEffectType.GainBlockPerUnusedBoardCell)
                continue;

            int blockAmount = unusedCellCount * Mathf.Max(1, relic.amount);
            playerController.GainBlock(blockAmount);
            Debug.Log($"[RelicManager] {relic.relicName} 발동: 방어도 {blockAmount}");
        }
    }

}
