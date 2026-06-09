using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapDeckViewController
{
    private readonly GameObject deckPanel;
    private readonly Transform deckContent;
    private readonly GameObject cardTemplate;
    private readonly TMP_Text deckTitle;
    private readonly TMP_Text emptyText;
    private readonly Button closeButton;
    private readonly Button deckButton;

    public MapDeckViewController(
        GameObject deckPanel,
        Transform deckContent,
        CardView cardTemplate,
        TMP_Text deckTitle,
        TMP_Text emptyText,
        Button closeButton,
        Button deckButton)
    {
        this.deckPanel = deckPanel;
        this.deckContent = deckContent;
        this.cardTemplate = cardTemplate != null ? cardTemplate.gameObject : null;
        this.deckTitle = deckTitle;
        this.emptyText = emptyText;
        this.closeButton = closeButton;
        this.deckButton = deckButton;
    }

    public void Initialize()
    {
        if (deckPanel == null || deckContent == null || cardTemplate == null)
        {
            Debug.LogWarning("[MapDeckViewController] 덱 보기 패널, Content, 카드 템플릿 중 연결되지 않은 항목이 있습니다.");
            return;
        }

        cardTemplate.SetActive(false);

        if (deckButton != null)
        {
            deckButton.onClick.RemoveListener(Show);
            deckButton.onClick.AddListener(Show);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Hide);
            closeButton.onClick.AddListener(Hide);
        }

        Hide();
    }

    public void Show()
    {
        if (deckPanel == null || deckContent == null || cardTemplate == null)
        {
            Debug.LogWarning("[MapDeckViewController] 덱 보기 패널, Content, 카드 템플릿 중 연결되지 않은 항목이 있습니다.");
            return;
        }

        ClearContent();

        int deckCount = RunData.currentDeck != null ? RunData.currentDeck.Count : 0;
        if (deckTitle != null)
            deckTitle.text = $"전체 덱 ({deckCount})";

        if (closeButton != null)
            closeButton.gameObject.SetActive(true);

        if (emptyText != null)
        {
            emptyText.text = "비어 있음";
            emptyText.gameObject.SetActive(deckCount == 0);
        }

        if (RunData.currentDeck != null)
        {
            foreach (CardData card in RunData.currentDeck)
                CreateDeckCard(card);
        }

        deckPanel.SetActive(true);
        deckPanel.transform.SetAsLastSibling();
    }

    public void Hide()
    {
        if (deckPanel != null)
            deckPanel.SetActive(false);
    }

    private void CreateDeckCard(CardData card)
    {
        if (card == null)
            return;

        GameObject cardObject = UnityEngine.Object.Instantiate(cardTemplate, deckContent);
        cardObject.SetActive(true);

        CardDragHandler dragHandler = cardObject.GetComponent<CardDragHandler>();
        if (dragHandler != null)
            dragHandler.enabled = false;

        CardView cardView = cardObject.GetComponent<CardView>();
        if (cardView != null)
            cardView.Setup(card);

        RectTransform rectTransform = cardObject.GetComponent<RectTransform>();
        if (rectTransform != null)
            rectTransform.localScale = Vector3.one;
    }

    private void ClearContent()
    {
        if (deckContent == null)
            return;

        for (int i = deckContent.childCount - 1; i >= 0; i--)
        {
            GameObject child = deckContent.GetChild(i).gameObject;

            if (child != cardTemplate)
                UnityEngine.Object.Destroy(child);
        }
    }
}
