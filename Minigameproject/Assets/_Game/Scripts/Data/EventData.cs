using System;
using UnityEngine;

[Serializable]
public class EventChoiceData
{
    public string choiceText;

    [TextArea]
    public string resultText;

    [Tooltip("양수는 회복, 음수는 피해입니다.")]
    public int hpChange;

    public CardData rewardCard;
    public RelicData rewardRelic;
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
