/*
using UnityEngine;
using System.Collections;

public class BurnEffect : MonoBehaviour
{
    private bool burning;

    public void Apply(float duration, int damagePerTick)
    {
        if (burning)
        {
            return;
        }

        StartCoroutine(BurnRoutine(duration, damagePerTick));
    }

    private IEnumerator BurnRoutine(float duration, int damagePerTick)
    {
        burning = true;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        Color originalColor = sr.color;
        sr.color = Color.red;

        Hittable hp = GetComponent<HittableComponent>().hp;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            hp.Damage(
                new Damage(
                    damagePerTick,
                    Damage.Type.FIRE,
                    GameManager.Instance.player
                )
            );

            yield return new WaitForSeconds(0.5f);

            elapsed += 0.5f;
        }

        sr.color = originalColor;

        burning = false;
    }
}
*/