using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class DeckManager : MonoBehaviour
{
    public List<CardData> startDeck;

    private List<CardData> deck = new List<CardData>();
    private List<CardData> discardPile = new List<CardData>();

    private const int StartHand = 6;
    public HandManager handManager;

    public TextMeshProUGUI Deckcount;
    public TextMeshProUGUI DisCardcount;

    private void Start()
    {
        if (!RunData.IsInitialized)
            RunData.Init(startDeck, 50);

        InitializeDeck();
        DrawCards(StartHand);
    }

    private void InitializeDeck()
    {
        deck.Clear();
        discardPile.Clear();

        if (RunData.currentDeck == null || RunData.currentDeck.Count == 0)
        {
            Debug.LogWarning("[DeckManager] RunData가 비어있습니다.");
            return;
        }

        foreach (CardData card in RunData.currentDeck)
        {
            if (card == null) continue;
            deck.Add(card);
        }
        ShuffleDeck(deck);
        counter();
    }

    public void DrawCards(int count)
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
                if (discardPile.Count == 0) break;
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
        counter();
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

    public void AddCardToDeck(CardData card)
    {
        RunData.AddCard(card);
        deck.Add(card);
    }

    public void counter()
    {
        Deckcount.text = deck.Count.ToString();
        DisCardcount.text = discardPile.Count.ToString();
    }
}
