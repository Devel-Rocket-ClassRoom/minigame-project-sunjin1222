using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    public EnemyData enemyData;
    public EnemyUI enemyUI;
    public PlayerController playerController;

    public GameObject panul;

    private int currentHealth;
    private int patternIndex = 0;

    private bool isDead;

    private void Start()
    {
        currentHealth = enemyData.maxHealth;
        enemyUI.UpdateUI(currentHealth, enemyData.maxHealth);
        UpdateIntent();
    }

    public void DoTurn()
    {
        if (!isDead)
        {
            if (enemyData.patterns == null || enemyData.patterns.Length == 0) return;

            EnemyPattern pattern = enemyData.patterns[patternIndex];

            switch (pattern.actionType)
            {
                case EnemyActionType.Attack:
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
        currentHealth -= damage;

        if (currentHealth < 0)
            currentHealth = 0;

        enemyUI.UpdateUI(currentHealth, enemyData.maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
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
        gameObject.SetActive(false);
    }
}