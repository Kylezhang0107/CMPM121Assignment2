using UnityEngine;
using System.Collections;

public class BurnEffect : MonoBehaviour
{
    private Coroutine burnRoutine;

    public void Apply(Hittable targetHp, float duration, int damagePerTick)
    {
        if (burnRoutine != null)
        {
            StopCoroutine(burnRoutine);
        }

        burnRoutine = StartCoroutine(BurnRoutine(targetHp, duration, damagePerTick));
    }

    private IEnumerator BurnRoutine(Hittable targetHp, float duration, int damagePerTick)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (sr == null)
        {
            yield break;
        }

        Color originalColor = sr.color;
        sr.color = Color.red;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            targetHp.Damage(new Damage(damagePerTick, Damage.Type.FIRE, GameManager.Instance.player));

            yield return new WaitForSeconds(0.5f);
            elapsed += 0.5f;
        }

        sr.color = originalColor;

        burnRoutine = null;
    }
}