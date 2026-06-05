using System.Collections;
using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Vector3 originalPosition;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalPosition = transform.localPosition;
    }

    private void Start()
    {
        StartCoroutine(IdleAnimation());
    }

    private IEnumerator IdleAnimation()
    {
        float time = 0f;
        while (true)
        {
            time += Time.deltaTime;
            float offsetY = Mathf.Sin(time * 1.2f) * 0.06f;
            transform.localPosition = originalPosition + new Vector3(0, offsetY, 0);
            yield return null;
        }
    }

    public IEnumerator HitAnimation()
    {
        StopAllCoroutines();

        for (int i = 0; i < 3; i++)
        {
            if (spriteRenderer != null)
                spriteRenderer.color = new Color(1f, 0.3f, 0.3f, 1f);
            transform.localPosition = originalPosition + new Vector3(0.12f, 0, 0);
            yield return new WaitForSeconds(0.05f);

            if (spriteRenderer != null)
                spriteRenderer.color = Color.white;
            transform.localPosition = originalPosition + new Vector3(-0.12f, 0, 0);
            yield return new WaitForSeconds(0.05f);
        }

        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;
        transform.localPosition = originalPosition;

        StartCoroutine(IdleAnimation());
    }

    public IEnumerator AttackAnimation()
    {
        StopAllCoroutines();

        Vector3 targetPos = originalPosition + new Vector3(-0.3f, 0, 0);
        float elapsed = 0f;
        float duration = 0.1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(originalPosition, targetPos, elapsed / duration);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(targetPos, originalPosition, elapsed / duration);
            yield return null;
        }

        transform.localPosition = originalPosition;
        StartCoroutine(IdleAnimation());
    }

    public void SetSprite(Sprite sprite)
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && sprite != null)
            spriteRenderer.sprite = sprite;
    }

    public void PlayHit() => StartCoroutine(HitAnimation());
    public void PlayAttack() => StartCoroutine(AttackAnimation());
}
