using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUI : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI intentText;
    public Image intentIcon;
    public TextMeshProUGUI damageLimitText;
    public TextMeshProUGUI thornsText;

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

    public void UpdateIntent(EnemyPattern pattern, int attackDamage)
    {
        switch (pattern.actionType)
        {
            case EnemyActionType.Attack:
                intentText.text = $"공격 {attackDamage}";
                break;
            case EnemyActionType.Defense:
                intentText.text = $"방어{pattern.value}";
                break;
            case EnemyActionType.Buff:
                intentText.text = $"힘 + {pattern.value}";
                break;
            case EnemyActionType.ThornsBuff:
                intentText.text = $"가시 + {pattern.value}";
                break;
            case EnemyActionType.Charge:
                intentText.text = "힘 모으는 중";
                break;
        }
    }

    public void ClearIntent()
    {
        intentText.text = "";
    }

    public void UpdateDamageLimit(int maxDamagePerTurn, int damageTakenThisTurn)
    {
        if (damageLimitText == null && maxDamagePerTurn > 0)
            damageLimitText = CreateRuntimeStatusText("DamageLimitText", -55f);

        if (damageLimitText == null)
            return;

        bool hasLimit = maxDamagePerTurn > 0;
        damageLimitText.gameObject.SetActive(hasLimit);

        if (!hasLimit)
            return;

        int remainingDamage = Mathf.Max(0, maxDamagePerTurn - damageTakenThisTurn);
        damageLimitText.text = $"피해한도\n{remainingDamage}/{maxDamagePerTurn}";
    }

    public void UpdateThorns(int thorns)
    {
        if (thornsText == null && thorns > 0)
            thornsText = CreateRuntimeStatusText("ThornsText", -95f);

        if (thornsText == null)
            return;

        bool hasThorns = thorns > 0;
        thornsText.gameObject.SetActive(hasThorns);

        if (hasThorns)
            thornsText.text = $"반사 {thorns}";
    }

    private TextMeshProUGUI CreateRuntimeStatusText(string objectName, float yOffset)
    {
        if (intentText == null)
            return null;

        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(intentText.transform.parent, false);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = intentText.rectTransform.anchorMin;
        rectTransform.anchorMax = intentText.rectTransform.anchorMax;
        rectTransform.pivot = intentText.rectTransform.pivot;
        rectTransform.anchoredPosition = intentText.rectTransform.anchoredPosition + new Vector2(0f, yOffset);
        rectTransform.sizeDelta = new Vector2(160f, 45f);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = intentText.font;
        text.fontSize = Mathf.Max(18f, intentText.fontSize * 0.75f);
        text.color = intentText.color;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;

        return text;
    }
}
