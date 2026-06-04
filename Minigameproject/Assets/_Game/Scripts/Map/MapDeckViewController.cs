using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapDeckViewController
{
    public GameObject deckPanel;
    private Transform deckContent;
    private GameObject cardTemplate;
    private TMP_Text deckTitle;
    private TMP_Text emptyText;
    private Button closeButton;
    private Button deckButton;

    public void Initialize()
    {
        if (deckPanel == null)
            deckPanel = FindSceneObject("PileContent");

        if (deckPanel == null)
            deckPanel = FindSceneObject("Pile Content");

        if (deckPanel == null)
            deckPanel = FindSceneObject("MapDeckPanel");

        if (deckPanel == null)
            deckPanel = FindSceneObject("CardRemovePanel");

        BindDeckButton();
        BindDeckPanel();
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

    private void BindDeckButton()
    {
        deckButton = FindButtonByNameOrText("DeckButton");

        if (deckButton == null)
            deckButton = FindButtonByNameOrText("Deck");

        if (deckButton == null)
            return;

        deckButton.onClick.RemoveListener(Show);
        deckButton.onClick.AddListener(Show);
    }

    private void BindDeckPanel()
    {
        if (deckPanel == null)
            return;

        Transform pileContentRoot = IsNamed(deckPanel.transform, "PileContent") ||
            IsNamed(deckPanel.transform, "Pile Content")
                ? deckPanel.transform
                : FindChild(deckPanel.transform, "PileContent");

        if (pileContentRoot == null)
            pileContentRoot = FindChild(deckPanel.transform, "Pile Content");

        deckContent = ResolvePileContent(pileContentRoot);

        if (deckContent == null)
            deckContent = FindChild(deckPanel.transform, "Content");

        if (deckContent == null)
            return;

        CardView templateView = deckContent.GetComponentInChildren<CardView>(true);
        if (templateView == null)
            templateView = deckPanel.GetComponentInChildren<CardView>(true);
        if (templateView == null)
            templateView = FindCardTemplateFromScene();

        if (templateView != null)
        {
            cardTemplate = templateView.gameObject;
            cardTemplate.SetActive(false);
        }

        deckTitle = FindText(deckPanel.transform, "DeckTitle");
        if (deckTitle == null)
            deckTitle = FindText(deckPanel.transform, "Title");

        emptyText = FindText(deckPanel.transform, "EmptyText");

        closeButton = FindButton(deckPanel, "CloseButton");
        if (closeButton == null)
            closeButton = FindButton(deckPanel, "CancelButton");
        if (closeButton == null)
            closeButton = FindButtonByNameOrText(deckPanel, "닫기");
        if (closeButton == null)
            closeButton = FindButtonByNameOrText(deckPanel, "Close");
        if (closeButton == null)
            closeButton = FindButtonByNameOrText(deckPanel, "Cancel");
        if (closeButton == null)
            closeButton = FindButton(deckPanel, "ConfirmButton");
        if (closeButton == null && (IsNamed(deckPanel.transform, "PileContent") || IsNamed(deckPanel.transform, "Pile Content")))
            closeButton = deckPanel.GetComponentInChildren<Button>(true);

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Hide);
            closeButton.onClick.AddListener(Hide);
        }

        Button confirmButton = FindButton(deckPanel, "ConfirmButton");
        if (confirmButton != null && confirmButton != closeButton)
            confirmButton.gameObject.SetActive(false);
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

    private GameObject FindSceneObject(string objectName)
    {
        foreach (Transform transform in UnityEngine.Object.FindObjectsByType<Transform>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None))
        {
            if (transform.gameObject.scene.IsValid() &&
                string.Equals(transform.name.Trim(), objectName, StringComparison.Ordinal))
                return transform.gameObject;
        }

        return null;
    }

    private Transform FindChild(Transform parent, string childName)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (IsNamed(child, childName))
                return child;
        }

        return null;
    }

    private bool IsNamed(Transform target, string objectName)
    {
        return target != null &&
            string.Equals(target.name.Trim(), objectName, StringComparison.Ordinal);
    }

    private Transform ResolvePileContent(Transform pileContentRoot)
    {
        if (pileContentRoot == null)
            return null;

        ScrollRect scrollRect = pileContentRoot.GetComponentInChildren<ScrollRect>(true);
        if (scrollRect != null && scrollRect.content != null)
            return scrollRect.content;

        Transform content = FindChild(pileContentRoot, "Content");
        return content != null ? content : pileContentRoot;
    }

    private TMP_Text FindText(Transform parent, string textName)
    {
        foreach (TMP_Text text in parent.GetComponentsInChildren<TMP_Text>(true))
        {
            if (string.Equals(text.name.Trim(), textName, StringComparison.Ordinal))
                return text;
        }

        return null;
    }

    private Button FindButton(GameObject parent, string buttonName)
    {
        foreach (Button button in parent.GetComponentsInChildren<Button>(true))
        {
            if (string.Equals(button.name.Trim(), buttonName, StringComparison.Ordinal))
                return button;
        }

        return null;
    }

    private Button FindButtonByNameOrText(GameObject parent, string value)
    {
        foreach (Button button in parent.GetComponentsInChildren<Button>(true))
        {
            if (string.Equals(button.name.Trim(), value, StringComparison.OrdinalIgnoreCase))
                return button;

            TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
            if (text != null &&
                string.Equals(text.text.Trim(), value, StringComparison.OrdinalIgnoreCase))
                return button;
        }

        return null;
    }

    private Button FindButtonByNameOrText(string value)
    {
        foreach (Button button in UnityEngine.Object.FindObjectsByType<Button>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None))
        {
            if (!button.gameObject.scene.IsValid())
                continue;

            if (string.Equals(button.name.Trim(), value, StringComparison.OrdinalIgnoreCase))
                return button;

            TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
            if (text != null &&
                string.Equals(text.text.Trim(), value, StringComparison.OrdinalIgnoreCase))
                return button;
        }

        return null;
    }

    private CardView FindCardTemplateFromScene()
    {
        GameObject cardRemovePanel = FindSceneObject("CardRemovePanel");
        if (cardRemovePanel != null)
        {
            CardView cardView = cardRemovePanel.GetComponentInChildren<CardView>(true);
            if (cardView != null)
                return cardView;
        }

        foreach (CardView cardView in UnityEngine.Object.FindObjectsByType<CardView>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None))
        {
            if (cardView.gameObject.scene.IsValid())
                return cardView;
        }

        return null;
    }
}
