using UnityEngine;
using System.Collections;

public class PlayerStatusEffects : MonoBehaviour
{
    private Coroutine burnRoutine;
    private Coroutine freezeRoutine;

    private PlayerController player;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        player = GetComponent<PlayerController>();

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

    }

    public void ApplyBurn(float duration, int damagePerTick)
    {

        if (burnRoutine != null)
        {
            StopCoroutine(burnRoutine);
        }

        burnRoutine = StartCoroutine(
            BurnRoutine(duration, damagePerTick)
        );
    }

    public void ApplyFreeze(float duration, float slowAmount)
    {

        if (freezeRoutine != null)
        {
            StopCoroutine(freezeRoutine);
        }

        freezeRoutine = StartCoroutine(
            FreezeRoutine(duration, slowAmount)
        );
    }

    private IEnumerator BurnRoutine(float duration, int damagePerTick)
    {
        Color originalColor = Color.white;

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            player.hp.Damage(
                new Damage(
                    damagePerTick,
                    Damage.Type.FIRE
                )
            );

            yield return new WaitForSeconds(0.5f);

            elapsed += 0.5f;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        burnRoutine = null;
    }

    private IEnumerator FreezeRoutine(float duration, float slowAmount)
    {
        int originalSpeed = player.speed;

        player.speed = Mathf.RoundToInt(
            originalSpeed * (1f - slowAmount)
        );

        Color originalColor = Color.white;

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.cyan;
        }

        yield return new WaitForSeconds(duration);

        player.speed = originalSpeed;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        freezeRoutine = null;
    }
}