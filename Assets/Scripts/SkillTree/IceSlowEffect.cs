using UnityEngine;
using System.Collections;

public class IceSlowEffect : MonoBehaviour
{
    private bool slowed;

    public void Apply(float duration, float slowAmount)
    {
        if (slowed)
        {
            return;
        }

        StartCoroutine(SlowRoutine(duration, slowAmount));
    }

    private IEnumerator SlowRoutine(float duration, float slowAmount)
    {
        slowed = true;

        EnemyController enemy = GetComponent<EnemyController>();

        if (enemy == null)
        {
            yield break;
        }

        int originalSpeed = enemy.speed;

        enemy.speed = Mathf.RoundToInt(
            originalSpeed * (1f - slowAmount)
        );

        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        Color originalColor = sr.color;
        sr.color = Color.blue;

        yield return new WaitForSeconds(duration);

        enemy.speed = originalSpeed;
        sr.color = originalColor;

        slowed = false;
    }
}