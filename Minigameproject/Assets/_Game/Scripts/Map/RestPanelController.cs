using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RestPanelController
{
    private readonly GameSceneManager gameSceneManager;
    private readonly TMP_Text healAmountText;
    private readonly Action<MapNodeData> onRestCompleted;

    private GameObject restPanel;
    private GameObject cardRemovePanel;
    private Transform cardRemoveContent;
    private GameObject cardRemoveTemplate;
    private Button cardRemoveConfirmButton;
    private MapNodeData activeRestNode;
    private CardData selectedRemoveCard;
    private RectTransform selectedRemoveCardRect;

    public RestPanelController(
        GameSceneManager sceneManager,
        TMP_Text healText,
        Action<MapNodeData> restCompleted)
    {
        gameSceneManager = sceneManager;
        healAmountText = healText;
        onRestCompleted = restCompleted;
    }

    public void Initialize()
    {
        restPanel = FindSceneObject("RestPanel");
        cardRemovePanel = FindSceneObject("CardRemovePanel");

        if (restPanel != null)
        {
            Button healButton = FindButton(restPanel, "HealButton");
            Button removeCardButton = FindButton(restPanel, "RemoveCardButton");

            if (healButton != null)
                healButton.onClick.AddListener(ChooseRestHeal);

            if (removeCardButton != null)
                removeCardButton.onClick.AddListener(ShowCardRemovePanel);
        }

        BindCardRemovePanel();
        Hide();
    }

    public void Show(MapNodeData restNode)
    {
        if (restPanel == null)
        {
            Debug.LogError("[RestPanelController] RestPanel을 찾지 못했습니다.");
            Complete(restNode);
            return;
        }

        activeRestNode = restNode;

        if (healAmountText != null)
            healAmountText.text = Mathf.CeilToInt(RunData.maxHp * 0.3f).ToString();

        restPanel.SetActive(true);
    }

    public void Hide()
    {
        if (restPanel != null)
            restPanel.SetActive(false);

        if (cardRemovePanel != null)
            cardRemovePanel.SetActive(false);

        ClearSelectedRemoveCard();
    }

    private void BindCardRemovePanel()
    {
        if (cardRemovePanel == null)
            return;

        Transform content = FindChild(cardRemovePanel.transform, "Content");

        if (content == null)
            return;

        cardRemoveContent = content;
        CardView templateView = content.GetComponentInChildren<CardView>(true);

        if (templateView != null)
        {
            cardRemoveTemplate = templateView.gameObject;
            cardRemoveTemplate.SetActive(false);
        }

        cardRemoveConfirmButton = FindButton(cardRemovePanel, "ConfirmButton");

        if (cardRemoveConfirmButton == null)
        {
            Debug.LogError("[RestPanelController] CardRemovePanel 아래 ConfirmButton을 찾지 못했습니다.");
            return;
        }

        cardRemoveConfirmButton.onClick.AddListener(ConfirmCardRemove);
        cardRemoveConfirmButton.gameObject.SetActive(false);
    }

    private void ChooseRestHeal()
    {
        if (activeRestNode == null)
            return;

        int healAmount = Mathf.CeilToInt(RunData.maxHp * 0.3f);
        RunData.currentHp = Mathf.Min(RunData.maxHp, RunData.currentHp + healAmount);

        if (gameSceneManager != null)
            gameSceneManager.RefreshMapHud();

        Complete(activeRestNode);
    }

    private void ShowCardRemovePanel()
    {
        if (activeRestNode == null || cardRemovePanel == null || cardRemoveContent == null)
        {
            Debug.LogError("[RestPanelController] 카드 제거 패널 또는 Content를 찾지 못했습니다.");
            return;
        }

        if (cardRemoveTemplate == null)
        {
            Debug.LogError("[RestPanelController] 카드 제거 화면에 템플릿 카드가 없습니다.");
            return;
        }

        ClearCardRemoveContent();
        ClearSelectedRemoveCard();

        foreach (CardData card in RunData.currentDeck)
            CreateRemoveCard(card);

        restPanel.SetActive(false);
        cardRemovePanel.SetActive(true);
    }

    private void ClearCardRemoveContent()
    {
        for (int i = cardRemoveContent.childCount - 1; i >= 0; i--)
        {
            GameObject child = cardRemoveContent.GetChild(i).gameObject;

            if (child != cardRemoveTemplate)
                UnityEngine.Object.Destroy(child);
        }
    }

    private void CreateRemoveCard(CardData card)
    {
        GameObject cardObject = UnityEngine.Object.Instantiate(cardRemoveTemplate, cardRemoveContent);
        cardObject.SetActive(true);

        CardDragHandler dragHandler = cardObject.GetComponent<CardDragHandler>();
        if (dragHandler != null)
            dragHandler.enabled = false;

        CardView cardView = cardObject.GetComponent<CardView>();
        if (cardView != null)
            cardView.Setup(card, selectedCard => SelectCardToRemove(selectedCard, cardObject));
    }

    private void SelectCardToRemove(CardData card, GameObject cardObject)
    {
        if (activeRestNode == null || card == null)
            return;

        if (RunData.currentDeck.Count <= 1)
        {
            Debug.LogWarning("[RestPanelController] 덱의 마지막 카드는 제거할 수 없습니다.");
            return;
        }

        ClearSelectedRemoveCard();
        selectedRemoveCard = card;
        selectedRemoveCardRect = cardObject.GetComponent<RectTransform>();

        if (selectedRemoveCardRect != null)
            selectedRemoveCardRect.localScale = Vector3.one * 1.08f;

        cardRemoveConfirmButton.gameObject.SetActive(true);
    }

    private void ConfirmCardRemove()
    {
        if (activeRestNode == null || selectedRemoveCard == null)
            return;

        RunData.currentDeck.Remove(selectedRemoveCard);
        Complete(activeRestNode);
    }

    private void ClearSelectedRemoveCard()
    {
        if (selectedRemoveCardRect != null)
            selectedRemoveCardRect.localScale = Vector3.one;

        selectedRemoveCard = null;
        selectedRemoveCardRect = null;

        if (cardRemoveConfirmButton != null)
            cardRemoveConfirmButton.gameObject.SetActive(false);
    }

    private void Complete(MapNodeData restNode)
    {
        Hide();
        activeRestNode = null;
        onRestCompleted?.Invoke(restNode);
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
            if (string.Equals(child.name.Trim(), childName, StringComparison.Ordinal))
                return child;
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
}
