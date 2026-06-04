using System;
using TMPro;
using UnityEngine;

public class EnemyHealthController
{
    private readonly EnemyUI enemyUI;
    private readonly EnemyAnimator enemyAnimator;
    private readonly GameObject armorObject;
    private readonly TextMeshProUGUI armorText;
    private readonly Action onDied;

    private EnemyData enemyData;
    private int damageTakenThisTurn;

    public int CurrentHealth { get; private set; }
    public int Block { get; private set; }

    public EnemyHealthController(
        EnemyUI ui,
        EnemyAnimator animator,
        GameObject armorView,
        TextMeshProUGUI armorLabel,
        Action diedCallback)
    {
        enemyUI = ui;
        enemyAnimator = animator;
        armorObject = armorView;
        armorText = armorLabel;
        onDied = diedCallback;
    }

    public void Initialize(EnemyData data)
    {
        enemyData = data;
        CurrentHealth = enemyData.maxHealth;
        Block = 0;
        damageTakenThisTurn = 0;

        UpdateArmorUI();
        enemyUI.UpdateUI(CurrentHealth, enemyData.maxHealth);
        UpdateDamageLimitUI();
    }

    public void TakeDamage(int damage)
    {
        if (enemyUI == null)
        {
            Debug.LogError("[EnemyHealthController] enemyUI가 null입니다.");
            return;
        }

        if (Block > 0)
        {
            int blockedDamage = Mathf.Min(Block, damage);
            Block -= blockedDamage;
            damage -= blockedDamage;
            UpdateArmorUI();
        }

        if (damage > 0 && enemyData.maxDamagePerTurn > 0)
        {
            int remainingDamage = Mathf.Max(0, enemyData.maxDamagePerTurn - damageTakenThisTurn);
            damage = Mathf.Min(damage, remainingDamage);
            damageTakenThisTurn += damage;
        }

        CurrentHealth -= damage;
        if (CurrentHealth < 0)
            CurrentHealth = 0;

        enemyUI.UpdateUI(CurrentHealth, enemyData.maxHealth);
        UpdateDamageLimitUI();

        if (enemyAnimator != null)
            enemyAnimator.PlayHit();

        if (CurrentHealth <= 0)
            onDied?.Invoke();
    }

    public void GainBlock(int amount)
    {
        Block += amount;
        UpdateArmorUI();
    }

    public void ResetDamageTakenThisTurn()
    {
        damageTakenThisTurn = 0;
        UpdateDamageLimitUI();
    }

    public void ResetBlock()
    {
        Block = 0;
        UpdateArmorUI();
    }

    private void UpdateArmorUI()
    {
        if (armorObject == null)
            return;

        bool hasBlock = Block > 0;
        armorObject.SetActive(hasBlock);

        if (hasBlock && armorText != null)
            armorText.text = Block.ToString();
    }

    private void UpdateDamageLimitUI()
    {
        if (enemyUI != null)
            enemyUI.UpdateDamageLimit(enemyData != null ? enemyData.maxDamagePerTurn : 0, damageTakenThisTurn);
    }
}
