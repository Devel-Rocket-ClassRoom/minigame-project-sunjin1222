using TMPro;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public EnemyUI enemyUI;
    public PlayerController playerController;
    public GameObject panul;
    public RewardManager rewardManager;
    public EnemyAnimator enemyAnimator;

    private EnemyData enemyData;

    public GameObject aemorOB;
    public TextMeshProUGUI Armor;


    public int currentHealth;
    private int patternIndex = 0;
    private int patternLoopCount = 0;
    private int attackBonus = 0;
    private int turnCount = 0;
    private int randomActionCount = 0;
    private int currentRandomPatternIndex = -1;
    private EnemyPattern currentPattern;
    private readonly EnemyPattern periodicBuffPattern = new EnemyPattern
    {
        actionType = EnemyActionType.Buff
    };
    private bool hasAvailablePattern = true;
    private bool isDead;

    public int block;
    private int damageTakenThisTurn;

    public void Initialize(EnemyData data)
    {
        if (enemyUI == null)
            enemyUI = GetComponent<EnemyUI>();

        if (enemyAnimator == null)
            enemyAnimator = GetComponentInChildren<EnemyAnimator>();

        enemyData = data;
        currentHealth = enemyData.maxHealth;
        patternIndex = 0;
        patternLoopCount = 0;
        attackBonus = 0;
        turnCount = 0;
        randomActionCount = 0;
        currentRandomPatternIndex = -1;
        damageTakenThisTurn = 0;
        hasAvailablePattern = enemyData.patterns != null && enemyData.patterns.Length > 0;
        isDead = false;

        if (hasAvailablePattern)
            hasAvailablePattern = PrepareInitialPattern();

        enemyUI.UpdateUI(currentHealth, enemyData.maxHealth);
        UpdateDamageLimitUI();

        if (hasAvailablePattern)
            UpdateIntent();
        else
            enemyUI.ClearIntent();
    }

    public void DoTurn()
    {
        if (!isDead)
        {
            if (!hasAvailablePattern || enemyData.patterns == null || enemyData.patterns.Length == 0) return;

            if (playerController == null)
            {
                Debug.LogError("[EnemyController] playerController가 null입니다.");
                return;
            }

            EnemyPattern pattern = currentPattern;

            switch (pattern.actionType)
            {
                case EnemyActionType.Attack:
                    int damage = GetAttackDamage(pattern);
                    if (enemyAnimator != null) enemyAnimator.PlayAttack();
                    playerController.TakeDamage(damage);
                    Debug.Log($"{enemyData.enemyName} 공격! {damage} 데미지");
                    break;

                case EnemyActionType.Defense:
                    GainBlock(pattern.value);
                    Debug.Log($"{enemyData.enemyName} {pattern.value}방어 준비");
                    break;

                case EnemyActionType.Buff:
                    attackBonus += pattern.value;
                    Debug.Log($"{enemyData.enemyName} 힘 증가! 공격력 +{pattern.value}");
                    break;
            }

            turnCount++;
            hasAvailablePattern = enemyData.patternMode == EnemyPatternMode.PeriodicBuffRandom
                ? MoveToNextRandomPattern()
                : MoveToNextSequentialPattern();

            if (hasAvailablePattern)
                UpdateIntent();
            else
                enemyUI.ClearIntent();
        }
    }

    public void TakeDamage(int damage)
    {
        if (enemyUI == null)
        {
            Debug.LogError("[EnemyController] enemyUI가 null입니다.");
            return;
        }



        if (block > 0)
        {
            int blockedDamage = Mathf.Min(block, damage);
            block -= blockedDamage;
            damage -= blockedDamage;
            aemorOB.SetActive(true);
            Armor.text = block.ToString();
            if (block <= 0)
                aemorOB.SetActive(false);
        }

        if (damage > 0 && enemyData.maxDamagePerTurn > 0)
        {
            int remainingDamage = Mathf.Max(0, enemyData.maxDamagePerTurn - damageTakenThisTurn);
            damage = Mathf.Min(damage, remainingDamage);
            damageTakenThisTurn += damage;
        }

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        enemyUI.UpdateUI(currentHealth, enemyData.maxHealth);
        UpdateDamageLimitUI();

        if (enemyAnimator != null) enemyAnimator.PlayHit();

        if (currentHealth <= 0)
            Die();
    }

    public void GainBlock(int amount)
    {
        block += amount;
        if (block > 0)
        {
            aemorOB.SetActive(true);
            Armor.text = block.ToString();
        }
        else
        {
            aemorOB.SetActive(false);
        }
    }

    private void UpdateIntent()
    {
        if (enemyData.patterns == null || enemyData.patterns.Length == 0) return;
        enemyUI.UpdateIntent(currentPattern, GetAttackDamage(currentPattern));
    }

    public void ResetDamageTakenThisTurn()
    {
        damageTakenThisTurn = 0;
        UpdateDamageLimitUI();
    }

    private void UpdateDamageLimitUI()
    {
        if (enemyUI != null)
            enemyUI.UpdateDamageLimit(enemyData != null ? enemyData.maxDamagePerTurn : 0, damageTakenThisTurn);
    }

    private int GetAttackDamage(EnemyPattern pattern)
    {
        if (pattern.actionType != EnemyActionType.Attack)
            return pattern.value;

        return pattern.value +
            attackBonus +
            patternLoopCount * enemyData.attackIncreasePerPatternLoop;
    }

    private bool PrepareInitialPattern()
    {
        if (enemyData.patternMode == EnemyPatternMode.PeriodicBuffRandom)
            return SelectRandomPatternForTurn(1);

        currentPattern = enemyData.patterns[patternIndex];
        return true;
    }

    private bool MoveToNextSequentialPattern()
    {
        for (int i = 0; i < enemyData.patterns.Length; i++)
        {
            patternIndex = (patternIndex + 1) % enemyData.patterns.Length;

            if (patternIndex == 0)
                patternLoopCount++;

            if (!enemyData.patterns[patternIndex].firstLoopOnly || patternLoopCount == 0)
            {
                currentPattern = enemyData.patterns[patternIndex];
                return true;
            }
        }

        return false;
    }

    private bool MoveToNextRandomPattern()
    {
        if (currentRandomPatternIndex >= 0)
        {
            randomActionCount++;

            if (randomActionCount % enemyData.patterns.Length == 0)
                patternLoopCount++;
        }

        return SelectRandomPatternForTurn(turnCount + 1);
    }

    private bool SelectRandomPatternForTurn(int turnNumber)
    {
        if (enemyData.periodicBuffInterval > 0 &&
            turnNumber % enemyData.periodicBuffInterval == 0)
        {
            periodicBuffPattern.value = enemyData.periodicBuffAmount;
            currentPattern = periodicBuffPattern;
            currentRandomPatternIndex = -1;
            return true;
        }

        bool allowImmediateRepeat = false;
        int candidateCount = CountRandomPatternCandidates(allowImmediateRepeat);

        if (candidateCount == 0)
        {
            allowImmediateRepeat = true;
            candidateCount = CountRandomPatternCandidates(allowImmediateRepeat);
        }

        if (candidateCount == 0)
            return false;

        int selectedCandidate = Random.Range(0, candidateCount);

        for (int i = 0; i < enemyData.patterns.Length; i++)
        {
            if (!IsRandomPatternCandidate(i, allowImmediateRepeat))
                continue;

            if (selectedCandidate > 0)
            {
                selectedCandidate--;
                continue;
            }

            currentRandomPatternIndex = i;
            currentPattern = enemyData.patterns[i];
            return true;
        }

        return false;
    }

    private int CountRandomPatternCandidates(bool allowImmediateRepeat)
    {
        int count = 0;

        for (int i = 0; i < enemyData.patterns.Length; i++)
        {
            if (IsRandomPatternCandidate(i, allowImmediateRepeat))
                count++;
        }

        return count;
    }

    private bool IsRandomPatternCandidate(int index, bool allowImmediateRepeat)
    {
        EnemyPattern pattern = enemyData.patterns[index];

        if (pattern.firstLoopOnly && patternLoopCount > 0)
            return false;

        if (!allowImmediateRepeat && index == currentRandomPatternIndex)
            return false;

        return true;
    }

    private void Die()
    {
        isDead = true;
        RelicManager.ApplyRelics(RelicTriggerType.BattleVictory, playerController, null);
        panul.SetActive(true);
        rewardManager.rewardbutton.SetActive(true);

        if (RunData.currentMap != null && RunData.selectedNodeId >= 0)
        {
            RunData.selectedBattleWon = true;
        }

        BattleController bc = FindObjectOfType<BattleController>();
        if (bc != null) bc.Didie();

        gameObject.SetActive(false);
    }
    public void ResetBlock()
    {
        block = 0;
        aemorOB.SetActive(false);
    }
}
