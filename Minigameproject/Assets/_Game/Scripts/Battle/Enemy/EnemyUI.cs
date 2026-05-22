using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUI : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI intentText;
    public Image intentIcon;

    private Canvas worldCanvas;
    private Camera mainCamera;

    private void Awake()
    {
        worldCanvas = GetComponentInChildren<Canvas>();
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        // World Space Canvas가 항상 카메라를 향하도록
        if (worldCanvas != null && mainCamera != null)
        {
            worldCanvas.transform.LookAt(
                worldCanvas.transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up
            );
        }
    }

    public void UpdateUI(int currentHealth, int maxHealth)
    {
        slider.maxValue = maxHealth;
        slider.value = currentHealth;
        healthText.text = $"{currentHealth} / {maxHealth}";
    }

    public void UpdateIntent(EnemyPattern pattern)
    {
        switch (pattern.actionType)
        {
            case EnemyActionType.Attack:
                intentText.text = $"공격 {pattern.value}";
                break;
            case EnemyActionType.Defense:
                intentText.text = $"방어{pattern.value}";
                break;
        }
    }
}
