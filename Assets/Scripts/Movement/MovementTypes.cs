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
        float triggerRange = 9f;
        float chargeTime = 1f;
        float dashTime = 0.2f;
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
}