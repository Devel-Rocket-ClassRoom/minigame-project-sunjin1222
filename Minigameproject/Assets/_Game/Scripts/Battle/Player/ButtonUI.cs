using UnityEngine;
using System.Collections.Generic;
public class ButtonUI : MonoBehaviour
{
    public BoardManager boardManager;
    public HandManager handManager;

    public EnemyController  enemyController;
    public void OnResetClicked()
    {
        if (boardManager == null || handManager == null)
        {
            Debug.LogError("[ButtonUI] BoardManager 또는 HandManager가 미할당입니다.");
            return;
        }

        boardManager.ReturnAllToHand(handManager);
    }
    public void OnUseCardsClicked()
    {
        if (boardManager == null || enemyController == null)
        {
            Debug.LogError("[ButtonUI] BoardManager 또는 EnemyUI가 미할당입니다.");
            return;
        }

        var cards = boardManager.GetActivationOrder();

        foreach (CardData card in cards)
        {
            Debug.Log($"{card.cardName} 발동!");

            if (card.cardType == CardType.Attack)
            {
                enemyController.TakeDamage(card.power);
                Debug.Log($"{card.cardName}으로 {card.power} 데미지!");
            }
        }
        boardManager.ClearBoard();
        boardManager.ReturnAllToHand(handManager);
    }
    public void OnClickEndTurnButton()
    {
        List<CardData> cards = boardManager.GetActivationOrder();

        foreach (CardData card in cards)
        {
            Debug.Log($"{card.cardName} 발동!");
        }

        boardManager.ClearBoard();
        boardManager.ReturnAllToHand(handManager);
    }
}