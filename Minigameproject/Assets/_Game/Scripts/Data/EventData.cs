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
    public List<CardData> rewardCards = new List<CardData>();
    public List<RelicData> rewardRelics = new List<RelicData>();

    public CardData GetRandomRewardCard()
    {
        if (rewardType != EventRewardType.Card || rewardCards == null || rewardCards.Count == 0)
            return null;

        return rewardCards[UnityEngine.Random.Range(0, rewardCards.Count)];
    }

    public RelicData GetRandomRewardRelic()
    {
        if (rewardType != EventRewardType.Relic || rewardRelics == null || rewardRelics.Count == 0)
            return null;

        return rewardRelics[UnityEngine.Random.Range(0, rewardRelics.Count)];
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

    public bool CanAppear(int floor)
    {
        return floor >= minFloor &&
            (maxFloor <= 0 || floor <= maxFloor);
    }
}
