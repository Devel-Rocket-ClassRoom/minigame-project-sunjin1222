using UnityEngine;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    private List<CardData> deck = new List<CardData>();
    private List<CardData> discardPile = new List<CardData>();
    public List<CardData> startDeck;
    private const int StartHand = 6;
    public HandManager handManager;

    private void Start()
    {
        InitializeDeck();
        DrawCards(StartHand);
    }


    private void InitializeDeck()
    {
        deck.Clear();

        if (startDeck == null || startDeck.Count == 0)
        {
            Debug.LogWarning("[DeckManager] startDeck이 비어있거나 미할당입니다. 덱이 빈 상태로 시작합니다.");
            return;
        }

        foreach (CardData card in startDeck)
        {
            if (card == null) continue;
            deck.Add(card);
        }
        ShuffleDeck(deck);
    }

    private void DrawCards(int count)
    {
  
        if (handManager == null)
        {
            Debug.LogError("[DeckManager] handManager 참조가 비어있습니다.");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            if (deck.Count == 0)
            {
                if (discardPile.Count == 0)
                {
                    Debug.Log("[DeckManager] 덱과 버린 더미가 모두 비어 더 이상 카드를 뽑을 수 없습니다.");
                    break;
                }

                deck.AddRange(discardPile);
                discardPile.Clear();
                ShuffleDeck(deck);
            }

            if (deck.Count > 0)
            {
                handManager.AddCard(deck[0]);
                deck.RemoveAt(0);
            }
        }
    }

    private void ShuffleDeck(List<CardData> list)
    {

        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);

            CardData temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }

    public void DiscardCard(CardData card)
    {
        discardPile.Add(card);
    }



}
