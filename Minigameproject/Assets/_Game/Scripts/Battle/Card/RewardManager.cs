using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class RewardManager : MonoBehaviour
{
    public List<CardData> cardPool;
    public List<RelicData> relicPool;
    public GameObject rewardPanel;
    public Transform rewardContent;
    public CardView cardRewardPrefab;
    public RelicRewardView relicRewardPrefab;
    public CardView[] rewardSlots;
    public DeckManager deckManager;
    public GameObject rewardbutton;
    public GameObject relicRewardButton;
    public GameObject relicRewardArea;
    public Image relicRewardIcon;
    public TMP_Text relicRewardName;
    public TMP_Text relicRewardDescription;
    public Button relicRewardConfirmButton;

    private bool eliteRelicRewardGiven;
    private RelicData pendingEliteRelic;
    private readonly List<GameObject> spawnedRewardObjects = new List<GameObject>();

    private void Start()
    {
        BindCardRewardButton();
        BindRelicRewardUI();
        ShowReward();
        HideRelicRewardUI();
    }


    public void ShowReward()
    {
        ClearRewards();

        List<CardData> rewardCardPool = RunData.GetRewardCardPool(cardPool);

        if (rewardCardPool == null || rewardCardPool.Count == 0)
        {
            Debug.LogWarning("[RewardManager] 보상 카드풀이 비어있습니다.");
            return;
        }

        List<CardData> copy = new List<CardData>(rewardCardPool);
        ShuffleReward(copy);


        int count = Mathf.Min(3, copy.Count);
        for (int i = 0; i < count; i++)
        {
            CreateCardReward(copy[i]);
        }
    }

    public void OnShowReward()
    {
        ShowReward();
        if (relicRewardArea != null)
            relicRewardArea.SetActive(false);

        if (rewardPanel != null)
            rewardPanel.SetActive(true);
    }

    public void ShowRewardButtons()
    {
        bool isEliteBattle = RunData.IsSelectedNodeType(MapNodeType.EliteBattle);

        if (rewardbutton != null)
            rewardbutton.SetActive(true);

        if (relicRewardButton != null)
            relicRewardButton.SetActive(isEliteBattle && !eliteRelicRewardGiven);

        if (isEliteBattle && pendingEliteRelic == null)
            pendingEliteRelic = GetRandomAvailableRelic();

        if (relicRewardArea != null)
            relicRewardArea.SetActive(false);
    }

    public void OnShowEliteRelicReward()
    {
        if (!RunData.IsSelectedNodeType(MapNodeType.EliteBattle) || eliteRelicRewardGiven)
            return;

        if (pendingEliteRelic == null)
            pendingEliteRelic = GetRandomAvailableRelic();

        if (pendingEliteRelic == null)
            return;

        if (relicRewardArea == null)
        {
            Debug.LogWarning("[RewardManager] RelicRewardArea가 없습니다.");
            return;
        }

        if (rewardPanel != null)
            rewardPanel.SetActive(false);

        relicRewardArea.SetActive(true);

        if (relicRewardIcon != null)
        {
            relicRewardIcon.sprite = pendingEliteRelic.icon;
            relicRewardIcon.enabled = pendingEliteRelic.icon != null;
        }

        if (relicRewardName != null)
            relicRewardName.text = pendingEliteRelic.relicName;

        if (relicRewardDescription != null)
            relicRewardDescription.text = pendingEliteRelic.description;

        if (relicRewardConfirmButton != null)
            relicRewardConfirmButton.gameObject.SetActive(true);
    }

    public void OnclauesReward()
    {
        if (rewardPanel != null)
            rewardPanel.SetActive(false);

        if (relicRewardArea != null)
            relicRewardArea.SetActive(false);
    }

    public void OnConfirmEliteRelicReward()
    {
        if (pendingEliteRelic == null || eliteRelicRewardGiven)
            return;

        GiveEliteRelicReward(pendingEliteRelic);

        if (relicRewardArea != null)
            relicRewardArea.SetActive(false);

        if (relicRewardButton != null)
            relicRewardButton.SetActive(false);
    }


    public void OnCardSelected(CardData card)
    {
        ClaimCardReward(card);
        ClearCardRewards();
        HideCardRewardUI();
    }

    private void OnDynamicCardSelected(CardView cardView, CardData card)
    {
        ClaimCardReward(card);
        ClearCardRewards();
        HideCardRewardUI();
    }

    private void ClaimCardReward(CardData card)
    {
        if (card == null)
            return;

        if (deckManager != null)
            deckManager.AddCardToDeck(card);
    }

    private void ShuffleReward(List<CardData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            CardData temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }

    private void GiveEliteRelicReward(RelicData relic)
    {
        if (relicPool == null || relicPool.Count == 0)
        {
            Debug.LogWarning("[RewardManager] 엘리트 유물 보상 풀이 비어있습니다.");
            return;
        }

        if (relic == null)
        {
            Debug.LogWarning("[RewardManager] 획득 가능한 엘리트 유물이 없습니다.");
            return;
        }

        RunData.AddRelic(relic);
        eliteRelicRewardGiven = true;
        pendingEliteRelic = null;

        Debug.Log($"[RewardManager] 엘리트 보상 유물 획득: {relic.relicName}");
    }

    private void BindRelicRewardUI()
    {
        if (relicRewardArea == null)
        {
            Debug.LogWarning("[RewardManager] relicRewardArea가 연결되지 않았습니다.");
            return;
        }

        if (relicRewardButton == null)
            Debug.LogWarning("[RewardManager] relicRewardButton이 연결되지 않았습니다.");

        if (relicRewardConfirmButton != null)
        {
            relicRewardConfirmButton.onClick.RemoveListener(OnConfirmEliteRelicReward);
            relicRewardConfirmButton.onClick.AddListener(OnConfirmEliteRelicReward);
        }

        Button button = relicRewardButton != null ? relicRewardButton.GetComponent<Button>() : null;
        if (button != null)
        {
            button.onClick.RemoveListener(OnShowEliteRelicReward);
            button.onClick.AddListener(OnShowEliteRelicReward);
        }
    }

    private void BindCardRewardButton()
    {
        Button button = rewardbutton != null ? rewardbutton.GetComponent<Button>() : null;
        if (button == null)
            return;

        button.onClick.RemoveListener(OnShowReward);
        button.onClick.AddListener(OnShowReward);
    }

    private void HideCardRewardUI()
    {
        if (rewardbutton != null)
            rewardbutton.SetActive(false);

        if (rewardPanel != null)
            rewardPanel.SetActive(false);
    }

    private void HideRelicRewardUI()
    {
        if (relicRewardArea != null)
            relicRewardArea.SetActive(false);

        if (relicRewardButton != null)
            relicRewardButton.SetActive(false);
    }

    private GameObject CreateRelicRewardButton()
    {
        return CreateRelicRewardButton(rewardbutton);
    }

    private GameObject CreateRelicRewardButton(GameObject sourceButton)
    {
        if (sourceButton == null)
            return null;

        Transform parent = rewardbutton != null && rewardbutton.transform.parent != null
            ? rewardbutton.transform.parent
            : rewardPanel != null ? rewardPanel.transform : null;

        if (parent == null)
            return null;

        GameObject buttonObject = Instantiate(sourceButton, parent);
        buttonObject.name = "RelicRewardButton";
        buttonObject.SetActive(false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        RectTransform cardButtonRect = rewardbutton != null ? rewardbutton.GetComponent<RectTransform>() : null;
        if (buttonRect != null && cardButtonRect != null)
        {
            buttonRect.anchoredPosition = cardButtonRect.anchoredPosition + new Vector2(180f, 0f);
            buttonRect.localScale = Vector3.one;
        }

        TMP_Text buttonText = buttonObject.GetComponentInChildren<TMP_Text>(true);
        if (buttonText != null)
            buttonText.text = "유물 보상";

        Button button = buttonObject.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnShowEliteRelicReward);
        }

        return buttonObject;
    }

    private GameObject CreateRelicRewardArea()
    {
        if (rewardPanel == null)
            return null;

        GameObject areaObject = new GameObject("RelicRewardPanel", typeof(RectTransform), typeof(Image));
        areaObject.transform.SetParent(rewardPanel.transform, false);

        RectTransform areaRect = areaObject.GetComponent<RectTransform>();
        areaRect.anchorMin = new Vector2(0.5f, 0.5f);
        areaRect.anchorMax = new Vector2(0.5f, 0.5f);
        areaRect.pivot = new Vector2(0.5f, 0.5f);
        areaRect.anchoredPosition = new Vector2(0f, -120f);
        areaRect.sizeDelta = new Vector2(420f, 140f);

        Image background = areaObject.GetComponent<Image>();
        background.color = new Color(0.08f, 0.08f, 0.08f, 0.92f);

        HorizontalLayoutGroup layoutGroup = areaObject.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.padding = new RectOffset(14, 14, 12, 12);
        layoutGroup.spacing = 12f;
        layoutGroup.childAlignment = TextAnchor.MiddleLeft;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;

        GameObject iconObject = new GameObject("Relicimage", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        iconObject.transform.SetParent(areaObject.transform, false);
        relicRewardIcon = iconObject.GetComponent<Image>();
        relicRewardIcon.preserveAspect = true;

        LayoutElement iconLayout = iconObject.GetComponent<LayoutElement>();
        iconLayout.preferredWidth = 72f;
        iconLayout.preferredHeight = 72f;
        iconLayout.flexibleWidth = 0f;

        GameObject textRoot = new GameObject("Text", typeof(RectTransform), typeof(LayoutElement));
        textRoot.transform.SetParent(areaObject.transform, false);

        VerticalLayoutGroup textLayout = textRoot.AddComponent<VerticalLayoutGroup>();
        textLayout.spacing = 4f;
        textLayout.childControlWidth = true;
        textLayout.childControlHeight = true;
        textLayout.childForceExpandWidth = true;
        textLayout.childForceExpandHeight = false;

        LayoutElement textLayoutElement = textRoot.GetComponent<LayoutElement>();
        textLayoutElement.preferredWidth = 300f;
        textLayoutElement.flexibleWidth = 1f;

        relicRewardName = CreateRuntimeText(textRoot.transform, "RelicName", 24f, FontStyles.Bold);
        relicRewardName.color = new Color(1f, 0.83f, 0.35f);

        relicRewardDescription = CreateRuntimeText(textRoot.transform, "RelicDe", 17f, FontStyles.Normal);
        relicRewardDescription.color = Color.white;

        areaObject.SetActive(false);
        return areaObject;
    }

    private void CreateCardReward(CardData card)
    {
        if (rewardSlots != null)
        {
            for (int i = 0; i < rewardSlots.Length; i++)
            {
                if (rewardSlots[i] == null || rewardSlots[i].gameObject.activeSelf)
                    continue;

                rewardSlots[i].Setup(card, OnCardSelected);
                rewardSlots[i].gameObject.SetActive(true);
                return;
            }
        }

        if (cardRewardPrefab != null && rewardContent != null)
        {
            CardView cardView = Instantiate(cardRewardPrefab, rewardContent);
            cardView.gameObject.SetActive(true);
            cardView.Setup(card, selectedCard => OnDynamicCardSelected(cardView, selectedCard));
            spawnedRewardObjects.Add(cardView.gameObject);
        }
    }

    private void CreateRelicReward(RelicData relic)
    {
        if (relic == null)
        {
            Debug.LogWarning("[RewardManager] 획득 가능한 엘리트 유물이 없습니다.");
            return;
        }

        if (relicRewardPrefab != null && rewardContent != null)
        {
            RelicRewardView relicView = Instantiate(relicRewardPrefab, rewardContent);
            relicView.gameObject.SetActive(true);
            relicView.Setup(relic, OnRelicSelected);
            spawnedRewardObjects.Add(relicView.gameObject);
            return;
        }

        CreateRuntimeRelicReward(relic);
    }

    private void OnRelicSelected(RelicRewardView relicView, RelicData relic)
    {
        GiveEliteRelicReward(relic);

        if (relicView != null)
            RemoveRewardObject(relicView.gameObject);

        CloseRewardPanelIfEmpty();
    }

    private void OnRuntimeRelicSelected(GameObject rewardObject, RelicData relic)
    {
        GiveEliteRelicReward(relic);
        RemoveRewardObject(rewardObject);
        CloseRewardPanelIfEmpty();
    }

    private void CreateRuntimeRelicReward(RelicData relic)
    {
        Transform parent = rewardContent != null ? rewardContent : rewardPanel != null ? rewardPanel.transform : null;

        if (parent == null)
        {
            Debug.LogWarning("[RewardManager] 엘리트 유물 보상을 표시할 패널이 없습니다.");
            return;
        }

        GameObject rewardObject = new GameObject("Elite Relic Reward", typeof(RectTransform), typeof(Image), typeof(Button));
        rewardObject.transform.SetParent(parent, false);
        rewardObject.SetActive(true);

        RectTransform rewardRect = rewardObject.GetComponent<RectTransform>();
        rewardRect.sizeDelta = new Vector2(360f, 120f);
        rewardRect.anchorMin = new Vector2(0.5f, 0.5f);
        rewardRect.anchorMax = new Vector2(0.5f, 0.5f);
        rewardRect.pivot = new Vector2(0.5f, 0.5f);

        Image background = rewardObject.GetComponent<Image>();
        background.color = new Color(0.08f, 0.08f, 0.08f, 0.92f);

        HorizontalLayoutGroup layoutGroup = rewardObject.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.padding = new RectOffset(14, 14, 12, 12);
        layoutGroup.spacing = 12f;
        layoutGroup.childAlignment = TextAnchor.MiddleLeft;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;

        CreateRuntimeRelicIcon(rewardObject.transform, relic);
        CreateRuntimeRelicText(rewardObject.transform, relic);

        Button button = rewardObject.GetComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(() => OnRuntimeRelicSelected(rewardObject, relic));

        if (RelicTooltipBuilder.TryBuild(relic, out string title, out string body))
        {
            TooltipTrigger tooltipTrigger = rewardObject.AddComponent<TooltipTrigger>();
            tooltipTrigger.SetTooltip(title, body);
        }

        spawnedRewardObjects.Add(rewardObject);
    }

    private void CreateRuntimeRelicIcon(Transform parent, RelicData relic)
    {
        GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        iconObject.transform.SetParent(parent, false);

        Image icon = iconObject.GetComponent<Image>();
        icon.sprite = relic.icon;
        icon.enabled = relic.icon != null;
        icon.preserveAspect = true;

        LayoutElement layoutElement = iconObject.GetComponent<LayoutElement>();
        layoutElement.preferredWidth = 72f;
        layoutElement.preferredHeight = 72f;
        layoutElement.flexibleWidth = 0f;
    }

    private void CreateRuntimeRelicText(Transform parent, RelicData relic)
    {
        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(LayoutElement));
        textObject.transform.SetParent(parent, false);

        VerticalLayoutGroup layoutGroup = textObject.AddComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = 4f;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;

        LayoutElement layoutElement = textObject.GetComponent<LayoutElement>();
        layoutElement.preferredWidth = 240f;
        layoutElement.flexibleWidth = 1f;

        TMP_Text nameText = CreateRuntimeText(textObject.transform, "Name", 24f, FontStyles.Bold);
        nameText.text = relic.relicName;
        nameText.color = new Color(1f, 0.83f, 0.35f);

        TMP_Text descriptionText = CreateRuntimeText(textObject.transform, "Description", 17f, FontStyles.Normal);
        descriptionText.text = relic.description;
        descriptionText.color = Color.white;
    }

    private TMP_Text CreateRuntimeText(Transform parent, string objectName, float fontSize, FontStyles fontStyle)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;

        return text;
    }

    private void ClearRewards()
    {
        for (int i = spawnedRewardObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedRewardObjects[i] != null)
                Destroy(spawnedRewardObjects[i]);
        }

        spawnedRewardObjects.Clear();

        if (rewardSlots == null)
            return;

        foreach (CardView rewardSlot in rewardSlots)
        {
            if (rewardSlot != null)
                rewardSlot.gameObject.SetActive(false);
        }
    }

    private void ClearCardRewards()
    {
        for (int i = spawnedRewardObjects.Count - 1; i >= 0; i--)
        {
            GameObject rewardObject = spawnedRewardObjects[i];
            if (rewardObject != null && rewardObject.GetComponent<CardView>() != null)
                RemoveRewardObject(rewardObject);
        }

        if (rewardSlots == null)
            return;

        foreach (CardView rewardSlot in rewardSlots)
        {
            if (rewardSlot != null)
                rewardSlot.gameObject.SetActive(false);
        }
    }

    private void RemoveRewardObject(GameObject rewardObject)
    {
        spawnedRewardObjects.Remove(rewardObject);

        if (rewardObject != null)
            Destroy(rewardObject);
    }

    private bool HasRemainingRewards()
    {
        spawnedRewardObjects.RemoveAll(rewardObject => rewardObject == null);
        return spawnedRewardObjects.Count > 0 ||
            (relicRewardButton != null && relicRewardButton.activeSelf) ||
            (relicRewardArea != null && relicRewardArea.activeSelf);
    }

    private void CloseRewardPanelIfEmpty()
    {
        if (HasRemainingRewards())
            return;

        if (rewardbutton != null)
            rewardbutton.SetActive(false);

        if (relicRewardButton != null)
            relicRewardButton.SetActive(false);

        if (rewardPanel != null)
            rewardPanel.SetActive(false);
    }

    private RelicData GetRandomAvailableRelic()
    {
        List<RelicData> availableRelics = new List<RelicData>();

        foreach (RelicData relic in relicPool)
        {
            if (relic != null && !RunData.HasRelic(relic))
                availableRelics.Add(relic);
        }

        if (availableRelics.Count == 0)
            return null;

        return availableRelics[Random.Range(0, availableRelics.Count)];
    }

}
