using UnityEngine;

public class EnemyPatternRunner
{
    private readonly EnemyPattern periodicBuffPattern = new EnemyPattern
    {
        actionType = EnemyActionType.Buff
    };

    private EnemyData enemyData;
    private int patternIndex;
    private int patternLoopCount;
    private int attackBonus;
    private int turnCount;
    private int randomActionCount;
    private int currentRandomPatternIndex;
    private EnemyPattern currentPattern;
    private bool hasAvailablePattern;

    public EnemyPattern CurrentPattern => currentPattern;
    public bool HasAvailablePattern => hasAvailablePattern;

    public void Initialize(EnemyData data)
    {
        enemyData = data;
        patternIndex = 0;
        patternLoopCount = 0;
        attackBonus = 0;
        turnCount = 0;
        randomActionCount = 0;
        currentRandomPatternIndex = -1;
        currentPattern = null;
        hasAvailablePattern = enemyData != null &&
            enemyData.patterns != null &&
            enemyData.patterns.Length > 0;

        if (hasAvailablePattern)
            hasAvailablePattern = PrepareInitialPattern();
    }

    public void AddAttackBonus(int amount)
    {
        attackBonus += amount;
    }

    public int GetAttackDamage(EnemyPattern pattern)
    {
        if (pattern == null)
            return 0;

        if (pattern.actionType != EnemyActionType.Attack)
            return pattern.value;

        return pattern.value +
            attackBonus +
            patternLoopCount * enemyData.attackIncreasePerPatternLoop;
    }

    public void AdvanceAfterTurn()
    {
        turnCount++;
        hasAvailablePattern = enemyData.patternMode == EnemyPatternMode.PeriodicBuffRandom
            ? MoveToNextRandomPattern()
            : MoveToNextSequentialPattern();
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
}
