using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    public EnemyData enemyData;
    public EnemyUI enemyUI;

    private int currentHealth;

    private void Start()
    {
        currentHealth = enemyData.maxHealth;

        enemyUI.UpdateUI(currentHealth, enemyData.maxHealth);
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

    private void Die()
    {
        Debug.Log($"{enemyData.enemyName} 사망");
    }
}
