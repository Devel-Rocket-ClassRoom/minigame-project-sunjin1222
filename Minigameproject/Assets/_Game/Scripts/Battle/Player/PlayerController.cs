using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public int maxHealth = 50;
    private int currentHealth;
    public int block;
    public GameObject gameover;
    public Slider slider;

    public GameObject aemorOB;

    public TextMeshProUGUI sliderHealth;
    public TextMeshProUGUI Armor;

    public PlayerAnimator playerAnimator;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        maxHealth = RunData.maxHp;
        currentHealth = RunData.currentHp;
        ApplyCharacterVisual();

        block = 0;
        slider.maxValue = maxHealth;
        slider.value = currentHealth;
        sliderHealth.text = $"HP: {currentHealth}/{maxHealth}";
    }

    private void ApplyCharacterVisual()
    {
        if (spriteRenderer == null || RunData.currentCharacter == null)
            return;

        Sprite characterSprite = RunData.currentCharacter.battleSprite != null
            ? RunData.currentCharacter.battleSprite
            : RunData.currentCharacter.image;

        if (characterSprite != null)
            spriteRenderer.sprite = characterSprite;
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

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        RunData.currentHp = currentHealth;

        slider.value = currentHealth;
        sliderHealth.text = $"HP: {currentHealth}/{maxHealth}";
    }

    public bool IsHealthAtOrBelowPercent(int percent)
    {
        if (maxHealth <= 0)
            return false;

        return currentHealth * 100 <= maxHealth * percent;
    }

    public void TakeDamage(int damage)
    {
        int remainingDamage = damage;

        if (block > 0)
        {
            int blockedDamage = Mathf.Min(block, remainingDamage);
            block -= blockedDamage;
            remainingDamage -= blockedDamage;
            aemorOB.SetActive(true);
            Armor.text = block.ToString();
            if (block <= 0)
                aemorOB.SetActive(false);
        }

        currentHealth -= remainingDamage;
        if (currentHealth < 0) currentHealth = 0;
      

        // RunData에 HP 반영
        RunData.currentHp = currentHealth;

        slider.value = currentHealth;
        sliderHealth.text = $"HP: {currentHealth}/{maxHealth}";

        if (playerAnimator != null)
            playerAnimator.PlayHit();

        if (currentHealth <= 0)
            Die();
    }

    public void ResetBlock()
    {
        block = 0;
        aemorOB.SetActive(false);
    }

    private void Die()
    {
        BattleController bc = FindObjectOfType<BattleController>();
        if (bc != null) bc.Didie();
        RunData.Clear();
        gameover.SetActive(true);
    }
}
