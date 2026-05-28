using UnityEngine;
using System.Collections.Generic;

public class RewardManager : MonoBehaviour
{
    public List<CardData> cardPool;
    public List<RelicData> relicPool;
    public GameObject rewardPanel;
    public CardView[] rewardSlots;
    public DeckManager deckManager;
    public GameObject rewardbutton;

    private List<CardData> currentRewards = new List<CardData>();

    private void Start()
    {
        ShowReward();
    }


    public void ShowReward()
    {

        currentRewards.Clear();

        List<CardData> rewardCardPool = RunData.GetRewardCardPool(cardPool);

        if (rewardCardPool == null || rewardCardPool.Count == 0)
        {
            Debug.LogWarning("[RewardManager] 보상 카드풀이 비어있습니다.");
            return;
        }

        List<CardData> Copy = new List<CardData>(rewardCardPool);
        ShuffleReward(Copy);


        int count = Mathf.Min(3, rewardCardPool.Count);
        for (int i = 0; i < count; i++)
        {

            currentRewards.Add(Copy[i]);
            rewardSlots[i].Setup(Copy[i], OnCardSelected);
            rewardSlots[i].gameObject.SetActive(true);
        }
    }

    public void OnShowReward()
    {
        if (RunData.IsSelectedNodeType(MapNodeType.EliteBattle))
        {
            GiveEliteRelicReward();
            return;
        }

        rewardPanel.SetActive(true);
    }

    public void OnclauesReward()
    {
        rewardPanel.SetActive(false);
    }


    public void OnCardSelected(CardData card)
    {
        deckManager.AddCardToDeck(card);
        rewardbutton.SetActive(false);
        rewardPanel.SetActive(false);
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

    private void GiveEliteRelicReward()
    {
        if (relicPool == null || relicPool.Count == 0)
        {
            Debug.LogWarning("[RewardManager] 엘리트 유물 보상 풀이 비어있습니다.");
            return;
        }

        RelicData relic = GetRandomAvailableRelic();

        if (relic == null)
        {
            Debug.LogWarning("[RewardManager] 획득 가능한 엘리트 유물이 없습니다.");
            return;
        }

        RunData.AddRelic(relic);

        if (rewardbutton != null)
            rewardbutton.SetActive(false);

        if (rewardPanel != null)
            rewardPanel.SetActive(false);

        Debug.Log($"[RewardManager] 엘리트 보상 유물 획득: {relic.relicName}");
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
