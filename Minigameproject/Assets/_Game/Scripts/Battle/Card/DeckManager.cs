using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class DeckManager : MonoBehaviour
{
    public CharacterData defaultCharacter;

    private List<CardData> deck = new List<CardData>();
    private List<CardData> discardPile = new List<CardData>();

    private const int StartHand = 6;
    public HandManager handManager;

    public TextMeshProUGUI Deckcount;
    public TextMeshProUGUI DisCardcount;

    [Header("Pile View")]
    public GameObject pilePanel;
    public TextMeshProUGUI pileTitle;
    public Transform pileContent;
    public GameObject pileCardPrefab;
    public TextMeshProUGUI pileEmptyText;

    private void Start()
    {
        if (RunData.currentCharacter == null && defaultCharacter != null)
            RunData.SetCharacter(defaultCharacter);

        if (!RunData.IsInitialized)
            RunData.Init(false);

        RunData.ApplyPendingRewardCards();
        InitializeDeck();
        DrawCards(StartHand);

        if (pilePanel != null)
            pilePanel.SetActive(false);
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
        counter();
    }

    public void AddCardToDeck(CardData card)
    {
        RunData.AddCard(card);
        RunData.AddedCard += 1;
        deck.Add(card);
        counter();
    }

    public void counter()
    {
        if (Deckcount != null)
            Deckcount.text = deck.Count.ToString();

        if (DisCardcount != null)
            DisCardcount.text = discardPile.Count.ToString();
    }

    public void ShowDeck()
    {
        List<CardData> sourceDeck =
            RunData.currentDeck != null && RunData.currentDeck.Count > 0
                ? RunData.currentDeck
                : deck;

        ShowPile("전체 덱", sourceDeck);
    }

    public void ShowDrawPile()
    {
        ShowPile("뽑을 더미", deck);
    }

    public void ShowDiscardPile()
    {
        ShowPile("버린 더미", discardPile);
    }

    public void ClosePileView()
    {
        if (pilePanel != null)
            pilePanel.SetActive(false);
    }

    private void ShowPile(string title, List<CardData> cards)
    {
        if (pilePanel == null || pileContent == null)
        {
            Debug.LogWarning("[DeckManager] Pile View UI가 연결되지 않았습니다.");
            return;
        }

        pilePanel.SetActive(true);
        pilePanel.transform.SetAsLastSibling();

        if (pileTitle != null)
            pileTitle.text = $"{title} ({cards.Count})";

        ClearPileContent();

        if (pileEmptyText != null)
        {
            pileEmptyText.text = "비어 있음";
            pileEmptyText.gameObject.SetActive(cards.Count == 0);
        }

        if (cards.Count == 0)
            return;

        foreach (CardData card in cards)
            CreatePileCard(card);
    }

    private void ClearPileContent()
    {
        for (int i = pileContent.childCount - 1; i >= 0; i--)
            Destroy(pileContent.GetChild(i).gameObject);
    }

    private void CreatePileCard(CardData card)
    {
        GameObject prefab = pileCardPrefab != null
            ? pileCardPrefab
            : handManager != null ? handManager.cardPrefab : null;

        if (prefab == null)
        {
            Debug.LogWarning("[DeckManager] Pile Card Prefab이 연결되지 않았습니다.");
            return;
        }

        GameObject cardObject = Instantiate(prefab, pileContent);
        CardDragHandler dragHandler = cardObject.GetComponent<CardDragHandler>();
        if (dragHandler != null)
            dragHandler.enabled = false;

        CardView cardView = cardObject.GetComponent<CardView>();
        if (cardView != null)
            cardView.Setup(card);

        RectTransform rect = cardObject.GetComponent<RectTransform>();
        if (rect != null)
            rect.localScale = Vector3.one;
    }
}
