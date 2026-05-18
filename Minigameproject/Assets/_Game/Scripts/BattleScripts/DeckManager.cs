using UnityEngine;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    private List<CardData> deck = new List<CardData>();
    public List<CardData> handCards = new List<CardData>();
    private List<CardData> discardPile = new List<CardData>();
    public List<CardData> startDeck;
    private const int StartHand = 6;


    private void Start()
    {
        InitializeDeck();
        DrawCards(StartHand);
    }


    private void InitializeDeck()
    {
        deck.Clear();
        foreach (CardData card in startDeck)
        {
            deck.Add(card);
        }
        ShuffleDeck(deck);
    }

    private void DrawCards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // 덱 소진 시 버리기 파일 재활용
            if (deck.Count == 0)
            {
                deck.AddRange(discardPile);
                discardPile.Clear();
                ShuffleDeck(deck);
            }

            if (deck.Count > 0)
            {
                handCards.Add(deck[0]);
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
        handCards.Remove(card);
        discardPile.Add(card);
    }



}
