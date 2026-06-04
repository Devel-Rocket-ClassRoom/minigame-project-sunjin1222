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

    public int currentHealth => healthController != null ? healthController.CurrentHealth : 0;
    public int block => healthController != null ? healthController.Block : 0;

    private EnemyPatternRunner patternRunner = new EnemyPatternRunner();
    private EnemyHealthController healthController;
    private EnemyDeathHandler deathHandler;
    private int currentThorns;
    private bool isDead;

    public void Initialize(EnemyData data)
    {
        if (enemyUI == null)
            enemyUI = GetComponent<EnemyUI>();

        if (enemyAnimator == null)
            enemyAnimator = GetComponentInChildren<EnemyAnimator>();

        enemyData = data;
        deathHandler = new EnemyDeathHandler(
            gameObject,
            playerController,
            panul,
            rewardManager);
        healthController = new EnemyHealthController(
            enemyUI,
            enemyAnimator,
            aemorOB,
            Armor,
            Die);
        healthController.Initialize(enemyData);
        patternRunner.Initialize(enemyData);
        currentThorns = enemyData != null ? Mathf.Max(0, enemyData.thorns) : 0;
        enemyUI.UpdateThorns(currentThorns);
        isDead = false;

        if (patternRunner.HasAvailablePattern)
            UpdateIntent();
        else
            enemyUI.ClearIntent();
    }

    public void DoTurn()
    {
        if (!isDead)
        {
            if (!patternRunner.HasAvailablePattern) return;

            if (playerController == null)
            {
                Debug.LogError("[EnemyController] playerController가 null입니다.");
                return;
            }

            EnemyPattern pattern = patternRunner.CurrentPattern;

            switch (pattern.actionType)
            {
                case EnemyActionType.Attack:
                    int damage = patternRunner.GetAttackDamage(pattern);
                    if (enemyAnimator != null) enemyAnimator.PlayAttack();
                    playerController.TakeDamage(damage);
                    Debug.Log($"{enemyData.enemyName} 공격! {damage} 데미지");
                    break;

                case EnemyActionType.Defense:
                    GainBlock(pattern.value);
                    Debug.Log($"{enemyData.enemyName} {pattern.value}방어 준비");
                    break;

                case EnemyActionType.Buff:
                    patternRunner.AddAttackBonus(pattern.value);
                    Debug.Log($"{enemyData.enemyName} 힘 증가! 공격력 +{pattern.value}");
                    break;

                case EnemyActionType.ThornsBuff:
                    AddThorns(pattern.value);
                    Debug.Log($"{enemyData.enemyName} 가시 증가! 반사 피해 +{pattern.value} (현재 {currentThorns})");
                    break;

                case EnemyActionType.Charge:
                    Debug.Log($"{enemyData.enemyName} 힘 모으는 중");
                    break;
            }

            patternRunner.AdvanceAfterTurn();

            if (patternRunner.HasAvailablePattern)
                UpdateIntent();
            else
                enemyUI.ClearIntent();
        }
    }

    public void TakeDamage(int damage)
    {
        if (healthController == null)
            return;

        int actualDamage = healthController.TakeDamage(damage);

        if (!isDead && actualDamage > 0 && currentThorns > 0)
            ReflectThornsDamage();
    }

    public void GainBlock(int amount)
    {
        healthController?.GainBlock(amount);
    }

    public void AddThorns(int amount)
    {
        currentThorns = Mathf.Max(0, currentThorns + amount);
        enemyUI.UpdateThorns(currentThorns);
    }

    private void ReflectThornsDamage()
    {
        if (playerController == null)
        {
            Debug.LogError("[EnemyController] playerController가 null이라 가시 반사 피해를 줄 수 없습니다.");
            return;
        }

        playerController.TakeDamage(currentThorns);
        Debug.Log($"{enemyData.enemyName} 가시 반사! {currentThorns} 데미지");
    }

    private void UpdateIntent()
    {
        if (!patternRunner.HasAvailablePattern) return;
        EnemyPattern pattern = patternRunner.CurrentPattern;
        enemyUI.UpdateIntent(pattern, patternRunner.GetAttackDamage(pattern));
    }

    public void ResetDamageTakenThisTurn()
    {
        healthController?.ResetDamageTakenThisTurn();
    }

    public void RefreshDamageLimitUI()
    {
        healthController?.RefreshDamageLimitUI();
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        deathHandler.HandleDeath();
    }
    public void ResetBlock()
    {
        healthController?.ResetBlock();
    }
}
