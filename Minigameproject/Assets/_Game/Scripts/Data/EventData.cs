using System;
using System.Collections.Generic;
using UnityEngine;

public enum EventRewardType
{
    Card,
    Relic
}

[Serializable]
public class EventChoiceData
{
    public string choiceText;

    [Tooltip("양수는 회복, 음수는 피해입니다.")]
    public int hpChange;

    public EventRewardType rewardType;


    public bool useCharacterCardPool;
    public bool filterByCardType;

    public CardType rewardCardType;
    [Range(0, 5)] public int cardOptionCount = 1;
    public List<CardData> rewardCards = new List<CardData>();
    public List<RelicData> rewardRelics = new List<RelicData>();

    public CardData GetRandomRewardCard()
    {
        List<CardData> cards = GetRandomRewardCards();
        return cards.Count > 0 ? cards[0] : null;
    }

    public List<CardData> GetRandomRewardCards()
    {
        List<CardData> candidates = GetRewardCardCandidates();
        List<CardData> rewards = new List<CardData>();
        int optionCount = Mathf.Min(Mathf.Clamp(cardOptionCount, 1, 5), candidates.Count);

        for (int i = 0; i < optionCount; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, candidates.Count);
            CardData card = candidates[randomIndex];
            candidates[randomIndex] = candidates[i];
            candidates[i] = card;
            rewards.Add(card);
        }

        return rewards;
    }

    private List<CardData> GetRewardCardCandidates()
    {
        List<CardData> candidates = new List<CardData>();

        if (rewardType != EventRewardType.Card)
            return candidates;

        List<CardData> cardPool = useCharacterCardPool
            ? RunData.GetRewardCardPool(rewardCards)
            : rewardCards;

        if (cardPool == null)
            return candidates;

        foreach (CardData card in cardPool)
        {
            if (card != null && (!filterByCardType || card.cardType == rewardCardType))
                candidates.Add(card);
        }

        return candidates;
    }

    public RelicData GetRandomRewardRelic()
    {
        if (rewardType != EventRewardType.Relic || rewardRelics == null || rewardRelics.Count == 0)
            return null;

        List<RelicData> candidates = new List<RelicData>();

        foreach (RelicData relic in rewardRelics)
        {
            if (relic != null && !RunData.HasRelic(relic))
                candidates.Add(relic);
        }

        if (candidates.Count == 0)
            return null;

        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }
}

[CreateAssetMenu(fileName = "EventData", menuName = "Game/Event Data")]
public class EventData : ScriptableObject
{
    [Header("표시 정보")]
    public string eventId;
    public string eventTitle;

    [TextArea]
    public string description;

    public Sprite illustration;
    public EventChoiceData[] choices = Array.Empty<EventChoiceData>();

    [Header("등장 조건")]
    [Min(1)] public int minFloor = 1;

    [Tooltip("0이면 최대 층 제한이 없습니다.")]
    [Min(0)] public int maxFloor;

    [Min(1)] public int weight = 1;
    public bool showOncePerRun;
    public bool fixedOnly;

    public bool CanAppear(int floor)
    {
        return floor >= minFloor &&
            (maxFloor <= 0 || floor <= maxFloor);
    }
}
