using UnityEngine;
using System.Collections;

public class PlayerStatusEffects : MonoBehaviour
{
    private Coroutine burnRoutine;
    private Coroutine freezeRoutine;

    private PlayerController player;
    private SpriteRenderer spriteRenderer;
    private Color baseColor;

    private int frozenOriginalSpeed;
    private bool frozen;

    private void Awake()
    {
        player = GetComponent<PlayerController>();

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            baseColor = spriteRenderer.color;
        }
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
        if (!frozen)
        {
            frozenOriginalSpeed = player.speed;
        }

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
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            player.hp.Damage(
                new Damage(
                    damagePerTick,
                    Damage.Type.FIRE,
                    gameObject
                )
            );

            yield return new WaitForSeconds(0.5f);

            elapsed += 0.5f;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = baseColor;
        }

        burnRoutine = null;
    }

    private IEnumerator FreezeRoutine(float duration, float slowAmount)
    {
        frozen = true;

        player.speed = Mathf.RoundToInt(
            frozenOriginalSpeed * (1f - slowAmount)
        );

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.cyan;
        }

        yield return new WaitForSeconds(duration);

        player.speed = frozenOriginalSpeed;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = baseColor;
        }

        frozen = false;
        freezeRoutine = null;
    }
}