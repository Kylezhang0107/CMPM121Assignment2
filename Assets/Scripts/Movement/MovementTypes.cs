using UnityEngine;

public static class MovementTypes
{
    public static void Chase(Unit unit, Vector3 direction, int speed)
    {
        unit.movement = direction.normalized * speed;
    }

    public static void Circle(Unit unit, Vector3 direction, int speed)
    {
        Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0).normalized;

        Vector3 move = direction.normalized + perpendicular * Mathf.Sin(Time.time * 2f);

        unit.movement = move.normalized * speed;
    }

    public static void Dash(Unit unit, Vector3 direction, int speed, ref int dashPhase, ref float dashTimer, ref Vector3 dashDirection)
    {
        float triggerRange = 10f;
        float chargeTime = 1f;
        float dashTime = 0.8f;
        float recoveryTime = 0.6f;

        float dashSpeedMultiplier = 5f;
        float recoverySpeedMultiplier = 2f;

        switch (dashPhase)
        {
            case 0:
                unit.movement = direction.normalized * speed;

                if (direction.magnitude <= triggerRange)
                {
                    dashPhase = 1;
                    dashTimer = chargeTime;
                }
                break;

            case 1:
                unit.movement = Vector3.zero;
                dashTimer -= Time.deltaTime;

                if (dashTimer <= 0)
                {
                    dashDirection = direction.normalized;
                    dashPhase = 2;
                    dashTimer = dashTime;
                }
                break;

            case 2:
                unit.movement = dashDirection * speed * dashSpeedMultiplier;
                dashTimer -= Time.deltaTime;

                if (dashTimer <= 0)
                {
                    dashPhase = 3;
                    dashTimer = recoveryTime;
                }
                break;

            case 3:
                unit.movement = -dashDirection * speed * recoverySpeedMultiplier;
                dashTimer -= Time.deltaTime;

                if (dashTimer <= 0)
                {
                    dashPhase = 0;
                }
                break;
        }
    }

    public static void ZigZag(Unit unit, Vector3 direction, int speed)
    {
        Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0).normalized;
        Vector3 move = direction.normalized + perpendicular * Mathf.Sin(Time.time * 8f) * 0.8f;

        unit.movement = move.normalized * speed;
    }

    public static void Phase(Unit unit, Transform transform, SpriteRenderer renderer, Collider2D collider, EnemyController enemy, ref bool isPhasing, ref float phaseTimer, ref Vector3 phaseDirection, float speed)
    {
        float idleTime = 3f;
        float phaseTime = 0.8f;
        float moveSpeed = speed * 3f;

        if (!isPhasing)
        {
            unit.movement = Vector3.zero;
            if (!enemy.summonCheckedThisIdle)
            {
                enemy.summonCheckedThisIdle = true;

                if (!string.IsNullOrEmpty(enemy.summonEnemy) && UnityEngine.Random.value < 0.25f)
                {
                    EnemySpawner.Instance.SpawnEnemyAtPosition(enemy.summonEnemy, transform.position);
                }
            }
            phaseTimer -= Time.deltaTime;

            if (phaseTimer <= 0f)
            {
                isPhasing = true;
                enemy.summonCheckedThisIdle = false;
                phaseTimer = phaseTime;
                phaseDirection = Random.insideUnitCircle.normalized;

                if (renderer != null)
                    renderer.color = new Color(1f, 1f, 1f, 0.2f);
            }

            return;
        }

        unit.movement = phaseDirection * moveSpeed;
        phaseTimer -= Time.deltaTime;

        if (phaseTimer <= 0f)
        {
            isPhasing = false;
            phaseTimer = idleTime;
            unit.movement = Vector3.zero;

            if (renderer != null)
                renderer.color = new Color(1f, 1f, 1f, 1f);
        }
    }

    public static void Scary(Unit unit, Vector3 direction, int speed, Transform transform, Transform player, SpriteRenderer renderer, ref int phase, ref float timer, ref Vector3 phaseDir, ref bool isPhasing)
    {
        float idleTime = 1.2f;
        float zigzagTime = 0.9f;
        float phaseTime = 0.6f;
        float phaseSpeed = speed * 3f;
        float triggerRange = 8f;

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        switch (phase)
        {
            case 0:
                unit.movement = Vector3.zero;
                timer -= Time.deltaTime;

                if (timer <= 0f)
                {
                    phase = 1;
                    timer = zigzagTime;
                }
                break;

            case 1:
            {
                Vector3 perp = new Vector3(-direction.y, direction.x, 0).normalized;
                Vector3 zigzag = direction.normalized + perp * Mathf.Sin(Time.time * 18f) * 1.2f;

                unit.movement = zigzag.normalized * (speed * 2.1f);
                timer -= Time.deltaTime;

                if (timer <= 0f)
                {
                    if (distToPlayer <= triggerRange)
                    {

                        isPhasing = true;
                        phaseDir = Random.insideUnitCircle.normalized;

                        if (renderer != null)
                            renderer.color = new Color(1f, 1f, 1f, 0.2f);

                        phase = 2;
                        timer = phaseTime;
                    }
                    else
                    {
                        phase = 0;
                        timer = idleTime;
                    }
                }
                break;
            }

            case 2:
            {
                unit.movement = phaseDir * phaseSpeed;
                timer -= Time.deltaTime;

                if (timer <= 0f)
                {
                    isPhasing = false;

                    if (renderer != null)
                        renderer.color = Color.white;

                    phase = 0;
                    timer = idleTime;
                }
                break;
            }
        }
    }
}