using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class BattleController : MonoBehaviour
{
    private const int MaxRedrawUseCount = 2;
    private const int RedrawDamage = 5;

    public BoardManager boardManager;
    public HandManager handManager;
    public EnemyController enemyController;
    public PlayerController playerController;
    public DeckManager deckManager;
    public TextMeshProUGUI reDrow;
    public TextMeshProUGUI evidenceText;
    public Transform characterUISlot;
    public CharacterBattleUI[] characterBattleUIs;

    public EnemyHealthController enemyHealthController;


    public static bool IsTurnProcessing = false;
    private int turnCount = 0;
    private int redrawUseCount = 0;
    private int evidenceCount = 0;
    private BoardCardActivator boardCardActivator;

    public int EvidenceCount => evidenceCount;

    private void Start()
    {
        if (BattleManager.CurrentEnemy != null)
            enemyController = BattleManager.CurrentEnemy;
        else
            Debug.LogError("[BattleController] BattleManager.CurrentEnemy가 null입니다.");

        boardCardActivator = new BoardCardActivator(boardManager, enemyController);
        boardManager?.SetBattleController(this);
        UpdateCharacterBattleUIs();
        StartCoroutine(ApplyBattleStartRelics());
        UpdateRedrawText();
        UpdateEvidenceText();
    }

    private IEnumerator ApplyBattleStartRelics()
    {
        yield return null;
        RelicManager.ApplyRelics(RelicTriggerType.BattleStart, playerController, deckManager, this);
        ApplyTurnStartRelics();
    }

    private void ApplyTurnStartRelics()
    {
        RelicManager.ApplyRelics(RelicTriggerType.TurnStart, playerController, deckManager, this);
    }

    public void OnResetClicked()
    {
        if (IsTurnProcessing) return;
        if (boardManager == null || handManager == null)
        {
            Debug.LogError("[BattleController] BoardManager 또는 HandManager가 미할당입니다.");
            return;
        }
        boardManager.ReturnAllToHand(handManager);
    }

    public void OnClickRedrawButton()
    {
        if (IsTurnProcessing) return;
        if (redrawUseCount >= MaxRedrawUseCount) return;
        if (handManager == null || deckManager == null || playerController == null)
        {
            Debug.LogError("[BattleController] HandManager, DeckManager 또는 PlayerController가 미할당입니다.");
            return;
        }

        int discardedCount = handManager.DiscardAll();
        if (discardedCount <= 0) return;


        redrawUseCount++;
        UpdateRedrawText();
        playerController.LoseHealth(RedrawDamage);
        deckManager.DrawCards(discardedCount);
    }

    public void AddEvidence(int amount)
    {
        evidenceCount += Mathf.Max(0, amount);
        UpdateEvidenceText();
        boardManager?.RefreshCardPreviewTexts();
    }

    public bool HasEvidence(int amount)
    {
        return evidenceCount >= amount;
    }

    public bool TrySpendEvidence(int amount)
    {
        amount = Mathf.Max(0, amount);

        if (evidenceCount < amount)
            return false;

        evidenceCount -= amount;
        UpdateEvidenceText();
        boardManager?.RefreshCardPreviewTexts();
        return true;
    }

    public int SpendAllEvidence()
    {
        int spent = evidenceCount;
        evidenceCount = 0;
        UpdateEvidenceText();
        boardManager?.RefreshCardPreviewTexts();
        return spent;
    }

    public void SetEvidenceText(TextMeshProUGUI text)
    {
        evidenceText = text;
        UpdateEvidenceText();
    }

    private void UpdateRedrawText()
    {
        if (reDrow != null)
            reDrow.text = $"다시 뽑기\n({(MaxRedrawUseCount - redrawUseCount).ToString()})";
    }

    private void UpdateEvidenceText()
    {
        if (evidenceText != null)
            evidenceText.text = $"증거:{evidenceCount}";
    }

    private void UpdateCharacterBattleUIs()
    {
        if (RunData.currentCharacter == null)
            return;

        if (characterBattleUIs == null)
            return;

        foreach (CharacterBattleUI battleUI in characterBattleUIs)
        {
            if (battleUI == null)
                continue;

            bool shouldShow = battleUI.Matches(RunData.currentCharacter);
            battleUI.gameObject.SetActive(shouldShow);

            if (shouldShow)
                battleUI.Bind(this);
        }
    }

    public void OnClickEndTurnButton()
    {
        if (IsTurnProcessing) return;
        if (enemyController == null)
        {
            Debug.LogError("[BattleController] enemyController가 null입니다.");
            return;
        }
        StartCoroutine(TurnRoutine());
    }

    private IEnumerator TurnRoutine()
    {
        IsTurnProcessing = true;
        boardCardActivator = new BoardCardActivator(boardManager, enemyController);
        turnCount++;

        EffectContext context = new EffectContext
        {
            enemyController = enemyController,
            playerController = playerController,
            handManager = handManager,
            deckManager = deckManager,
            battleController = this,
            sagaRequiredOrderReduction = BattleRelicResolver.GetSagaRequiredOrderReduction(playerController)
        };

        handManager.DiscardAll();

        var cards = boardManager.GetActivationOrder();
        bool shouldRepeatFirstTile = BattleRelicResolver.ShouldRepeatFirstTile(turnCount, playerController);
        
        BattleRelicResolver.ApplyEndTurnBoardRelics(boardManager, playerController, enemyController);

        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < cards.Count; i++)
        {
            BoardCardEntry entry = cards[i];

            yield return boardCardActivator.Execute(entry, context, i + 1);

            if (i == 0 && shouldRepeatFirstTile && enemyController.currentHealth > 0)
                yield return boardCardActivator.Execute(entry, context, i + 1);
        }
        if (enemyController.currentHealth > 0)
        {
            enemyController.ResetBlock();

            yield return new WaitForSeconds(0.5f);


            enemyController.DoTurn();

            yield return new WaitForSeconds(1f);

            playerController.ResetBlock();


            boardManager.DiscardBoard(deckManager);
            boardManager.DestroyTiles();
            boardManager.ClearBoard();
            boardManager.RefreshCardPreviewTexts();

            deckManager.DrawCards(6);
            ApplyTurnStartRelics();
            enemyController.ResetDamageTakenThisTurn();
        }

        IsTurnProcessing = false;
    }

    public void Didie()
    {
        boardManager.DiscardBoard(deckManager);
        boardManager.DestroyTiles();
        boardManager.ClearBoard();
        handManager.DiscardAll();
        IsTurnProcessing = false;
    }

}
