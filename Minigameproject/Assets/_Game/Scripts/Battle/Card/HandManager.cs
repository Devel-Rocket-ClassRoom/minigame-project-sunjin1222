using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HandManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public BoardManager boardManager;
    public Transform handArea;

    public DeckManager deckManager;

    private List<CardView> handCards = new List<CardView>();
    public float cardSpacing = 80f;

    public void AddCard(CardData data)
    {
        if (cardPrefab == null)
        {
            Debug.LogError("[HandManager] cardPrefab이 미할당입니다.");
            return;
        }
        if (handArea == null)
        {
            Debug.LogError("[HandManager] handArea가 미할당입니다.");
            return;
        }

        GameObject cardObj = Instantiate(cardPrefab, handArea);
        CardView cardView = cardObj.GetComponent<CardView>();

        if (cardView == null)
        {
            Debug.LogError("[HandManager] cardPrefab에 CardView 컴포넌트가 없습니다.");
            Destroy(cardObj);
            return;
        }

        cardView.Setup(data);
        handCards.Add(cardView);
        CardDragHandler dragHandler = cardObj.GetComponent<CardDragHandler>();

        if (dragHandler == null)
        {
            Debug.LogError("[HandManager] cardPrefab에 CardDragHandler 컴포넌트가 없습니다.");
            handCards.Remove(cardView);
            Destroy(cardObj);
            return;
        }

        dragHandler.Setup(data);
        dragHandler.boardManager = boardManager;
        dragHandler.handManager = this;

        ArrangeHand();

    }

    public void RemoveCard(CardView cardView)
    {
        if (cardView != null)
        {
            handCards.Remove(cardView);
        }

        handCards.RemoveAll(card => card == null);

        ArrangeHand();
    }

    public void ArrangeHand()
    {
        handCards.RemoveAll(card => card == null);

        int cardCount = handCards.Count;
        if (cardCount == 0) return;



        LayoutGroup layoutGroup = handArea != null
            ? handArea.GetComponent<LayoutGroup>()
            : null;

        if (layoutGroup == null)
        {
            float spacing = Mathf.Min(cardSpacing, 700f / cardCount);
            float startX = -(cardCount - 1) * spacing / 2f;

            for (int i = 0; i < cardCount; i++)
            {
                float x = startX + (i * spacing);
                RectTransform rect = handCards[i].GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(x, 0);
                handCards[i].transform.SetSiblingIndex(i);
            }
        }
        else
        {
            for (int i = 0; i < cardCount; i++)
            {
                handCards[i].transform.SetSiblingIndex(i);
            }
            RectTransform handRect = handArea as RectTransform;
            if (handRect == null && handArea != null)
                handRect = handArea.GetComponent<RectTransform>();
            if (handRect != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(handRect);
        }
    }
}
