using System.Collections;
using UnityEngine;

public class ArcaneSpraySpell : Spell
{
    public ArcaneSpraySpell(SpellCaster owner)
        : base(owner)
    {
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        this.team = team;

        Vector3 baseDirection = target - where;
        if (baseDirection.sqrMagnitude < 0.0001f)
        {
            baseDirection = Vector3.right;
        }

        int count = Mathf.Max(1, secondaryProjectileCount);
        float halfAngle = sprayAngle * 0.5f * Mathf.Rad2Deg;

        for (int i = 0; i < count; i++)
        {
            float angle = Random.Range(-halfAngle, halfAngle);
            Vector3 direction = Quaternion.Euler(0f, 0f, angle) * baseDirection;
            SpawnProjectile(
                projectileSprite,
                projectileTrajectory,
                where,
                direction,
                projectileSpeed,
                OnHit,
                projectileLifetime
            );
        }

        yield return new WaitForEndOfFrame();
    }
}
