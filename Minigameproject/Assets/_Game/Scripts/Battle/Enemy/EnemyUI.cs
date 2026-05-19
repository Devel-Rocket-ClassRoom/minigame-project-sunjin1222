using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUI : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI healthText;

    public void UpdateUI(int currentHealth, int maxHealth)
    {
        slider.maxValue = maxHealth;
        slider.value = currentHealth;

        healthText.text =
            $"{currentHealth} / {maxHealth}";
    }
}