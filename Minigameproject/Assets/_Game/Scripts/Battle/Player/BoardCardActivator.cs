using System.Collections;
using UnityEngine;

public class BoardCardActivator
{
    private readonly BoardManager boardManager;
    private readonly EnemyController enemyController;

    public BoardCardActivator(BoardManager boardManager, EnemyController enemyController)
    {
        this.boardManager = boardManager;
        this.enemyController = enemyController;
    }

    public IEnumerator Execute(
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

        context.currentCard = entry.card;
        boardManager.ShowActivationHighlight(entry.originIndex);
        yield return new WaitForSeconds(0.15f);

        foreach (EffectSO effect in entry.card.effects)
        {
            if (enemyController.currentHealth <= 0)
                break;

            if (effect == null)
                continue;

            effect.Apply(context);
            yield return new WaitForSeconds(0.5f);
        }

        boardManager.HideActivationHighlight();
        context.currentCard = null;
    }
}
