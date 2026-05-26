using UnityEngine;
using System.Collections.Generic;

public class EffectContext
{
    public EnemyController enemyController;
    public PlayerController playerController;
    public HandManager handManager;
    public DeckManager deckManager;
    public List<CardData> activatedCards = new List<CardData>();

    // 체인: 보드에서 인접한 카드 수 계산용
    public BoardManager boardManager;
    public int currentCardOriginIndex = -1;

    // 서사: 현재 카드의 발동 순서 (1번째부터 시작)
    public int activationOrder = 0;

    // 반전 카드용: 이번 턴 받은 피해 누적
    public int damageTakenThisTurn = 0;
}
