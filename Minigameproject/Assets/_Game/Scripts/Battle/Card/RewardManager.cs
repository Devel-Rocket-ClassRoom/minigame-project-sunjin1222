using UnityEngine;
using System.Collections.Generic;

public class RewardManager : MonoBehaviour
{
    public List<CardData> cardPool;
    public GameObject rewardPanel;
    public CardView[] rewardSlots;
    public DeckManager deckManager;

    private List<CardData> currentRewards = new List<CardData>();

    public void OnShowReward()
    {
        rewardPanel.SetActive(true);
        currentRewards.Clear();


        List<CardData> Copy = new List<CardData>(cardPool);
        ShuffleReward(Copy);


        int count = Mathf.Min(3, cardPool.Count);
        for (int i = 0; i < count; i++)
        {

            currentRewards.Add(Copy[i]);
            rewardSlots[i].Setup(Copy[i], OnCardSelected);
            rewardSlots[i].gameObject.SetActive(true);
        }
    }

    public void OnCardSelected(CardData card)
    {
        deckManager.AddCardToDeck(card);
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
}