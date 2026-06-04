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
        healthController?.TakeDamage(damage);
    }

    public void GainBlock(int amount)
    {
        healthController?.GainBlock(amount);
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
