using System;
using System.Collections;
using UnityEngine;

public class ArcaneBlastSpell : Spell
{
    public ArcaneBlastSpell(SpellCaster owner)
        : base(owner)
    {
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        this.team = team;

        int primaryDamage = GetDamage();
        int splitCount = Mathf.Max(1, secondaryProjectileCount);
        int splitDamage = secondaryDamageAmount > 0 ? secondaryDamageAmount : Mathf.Max(1, primaryDamage / 2);

        SpawnProjectile(
            projectileSprite,
            projectileTrajectory,
            where,
            target - where,
            projectileSpeed,
            (other, impact) => OnPrimaryHit(other, impact, splitCount, splitDamage),
            projectileLifetime
        );

        yield return new WaitForEndOfFrame();
    }

    private void OnPrimaryHit(Hittable other, Vector3 impact, int splitCount, int splitDamage)
    {
        if (other.team != team)
        {
            other.Damage(new Damage(GetDamage(), Damage.Type.ARCANE));
        }

        float angleStep = 360f / splitCount;
        for (int i = 0; i < splitCount; i++)
        {
            float angle = angleStep * i;
            Vector3 direction = Quaternion.Euler(0f, 0f, angle) * Vector3.right;
            SpawnProjectile(
                secondaryProjectileSprite > 0 ? secondaryProjectileSprite : projectileSprite,
                string.IsNullOrWhiteSpace(secondaryProjectileTrajectory) ? projectileTrajectory : secondaryProjectileTrajectory,
                impact,
                direction,
                secondaryProjectileSpeed > 0f ? secondaryProjectileSpeed : projectileSpeed,
                (hit, point) => OnSecondaryHit(hit, point, splitDamage),
                secondaryProjectileLifetime > 0f ? secondaryProjectileLifetime : projectileLifetime
            );
        }
    }

    private void OnSecondaryHit(Hittable other, Vector3 impact, int splitDamage)
    {
        if (other.team != team)
        {
            other.Damage(new Damage(splitDamage, Damage.Type.ARCANE));
        }
    }
}
