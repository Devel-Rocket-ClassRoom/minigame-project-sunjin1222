using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUI : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI healthText;


    public TextMeshProUGUI intentText;
    public Image intentIcon;

    public void UpdateUI(int currentHealth, int maxHealth)
    {
        slider.maxValue = maxHealth;
        slider.value = currentHealth;

        healthText.text =
            $"{currentHealth} / {maxHealth}";
    }


    public void UpdateIntent(EnemyPattern pattern)
    {
        switch (pattern.actionType)
        {
            case EnemyActionType.Attack:
                intentText.text = $"공격 {pattern.value}";
                break;
            case EnemyActionType.Defense:
                intentText.text = "방어";
                break;
        }
    }
}