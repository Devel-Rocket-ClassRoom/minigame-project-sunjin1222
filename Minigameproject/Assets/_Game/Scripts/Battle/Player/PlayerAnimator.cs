using System.Collections;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
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

    // Idle: 살짝 위아래로 둥실둥실
    private IEnumerator IdleAnimation()
    {
        float time = 0f;
        while (true)
        {
            time += Time.deltaTime;
            float offsetY = Mathf.Sin(time * 1.5f) * 0.05f;
            transform.localPosition = originalPosition + new Vector3(0, offsetY, 0);
            yield return null;
        }
    }

    // 피격: 빨간 깜빡임 + 흔들림
    public IEnumerator HitAnimation()
    {
        StopCoroutine(IdleAnimation());

        for (int i = 0; i < 3; i++)
        {
            spriteRenderer.color = new Color(1f, 0.3f, 0.3f, 1f);
            transform.localPosition = originalPosition + new Vector3(-0.1f, 0, 0);
            yield return new WaitForSeconds(0.02f);

            spriteRenderer.color = Color.white;
            transform.localPosition = originalPosition + new Vector3(0.1f, 0, 0);
            yield return new WaitForSeconds(0.1f);
        }

        spriteRenderer.color = Color.white;
        transform.localPosition = originalPosition;

        StartCoroutine(IdleAnimation());
    }

    public void PlayHit()
    {
        StartCoroutine(HitAnimation());
    }
}
