using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public EnemyUI enemyUI;
    public PlayerController playerController;
    public GameObject panul;
    public RewardManager rewardManager;
    public EnemyAnimator enemyAnimator;

    private EnemyData enemyData;
    private int currentHealth;
    private int patternIndex = 0;
    private bool isDead;

    private void Start() { }

    public void Initialize(EnemyData data)
    {
        if (enemyUI == null)
            enemyUI = GetComponent<EnemyUI>();

        if (enemyAnimator == null)
            enemyAnimator = GetComponentInChildren<EnemyAnimator>();

        enemyData = data;
        currentHealth = enemyData.maxHealth;
        patternIndex = 0;
        isDead = false;
        enemyUI.UpdateUI(currentHealth, enemyData.maxHealth);
        UpdateIntent();
    }

    public void DoTurn()
    {
        if (!isDead)
        {
            if (enemyData.patterns == null || enemyData.patterns.Length == 0) return;

            if (playerController == null)
            {
                Debug.LogError("[EnemyController] playerController가 null입니다.");
                return;
            }

            EnemyPattern pattern = enemyData.patterns[patternIndex];

            switch (pattern.actionType)
            {
                case EnemyActionType.Attack:
                    if (enemyAnimator != null) enemyAnimator.PlayAttack();
                    playerController.TakeDamage(pattern.value);
                    Debug.Log($"{enemyData.enemyName} 공격! {pattern.value} 데미지");
                    break;

                case EnemyActionType.Defense:
                    Debug.Log($"{enemyData.enemyName} 방어 준비");
                    break;
            }

            patternIndex = (patternIndex + 1) % enemyData.patterns.Length;
            UpdateIntent();
        }
    }

    public void TakeDamage(int damage)
    {
        if (enemyUI == null)
        {
            Debug.LogError("[EnemyController] enemyUI가 null입니다.");
            return;
        }

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        enemyUI.UpdateUI(currentHealth, enemyData.maxHealth);

        if (enemyAnimator != null) enemyAnimator.PlayHit();

        if (currentHealth <= 0)
            Die();
    }

    private void UpdateIntent()
    {
        if (enemyData.patterns == null || enemyData.patterns.Length == 0) return;
        enemyUI.UpdateIntent(enemyData.patterns[patternIndex]);
    }

    private void Die()
    {
        isDead = true;
        panul.SetActive(true);
        rewardManager.rewardbutton.SetActive(true);
        gameObject.SetActive(false);
    }
}
