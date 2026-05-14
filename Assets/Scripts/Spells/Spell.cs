using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spell
{
    // configurable fields -start
    public string spellName = "Bolt";
    public string spellId;
    public string description = "";

    public int manaCost = 10;
    public int damageAmount = 100;

    public float cooldown = 0.75f;

    public int icon = 0;

    public float projectileSpeed = 15f;

    public string projectileTrajectory = "straight";

    public int projectileSprite = 0;

    public float projectileLifetime = -1f;

    public int secondaryDamageAmount = 0;
    public int secondaryProjectileCount = 0;
    public float sprayAngle = 0f;
    public float castDelay = 0f;

    public string secondaryProjectileTrajectory = "straight";
    public float secondaryProjectileSpeed = 0f;
    public int secondaryProjectileSprite = 0;
    public float secondaryProjectileLifetime = -1f;
    // end-
    public float last_cast;
    public SpellCaster owner;
    public Hittable.Team team;

    // Track last applied progression power for accurate gain calculation
    public int lastProgressionPower = 0;

    public Spell(SpellCaster owner)
    {
        this.owner = owner;
    }

    public virtual string GetName()
    {
        return spellName;
    }

    public virtual string GetDescription()
    {
        return description;
    }

    public virtual int GetManaCost()
    {
        return manaCost;
    }

    public virtual int GetDamage()
    {
        return damageAmount;
    }

    public virtual float GetCooldown()
    {
        return cooldown;
    }

    public virtual int GetIcon()
    {
        return icon;
    }

    public bool IsReady()
    {
        return (last_cast + GetCooldown() < Time.time);
    }

    public virtual IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        this.team = team;
        SpawnProjectile(
            projectileSprite,
            projectileTrajectory,
            where,
            target - where,
            projectileSpeed,
            OnHit,
            projectileLifetime
        );
        yield return new WaitForEndOfFrame();
    }

    protected void SpawnProjectile(
        int sprite,
        string trajectory,
        Vector3 where,
        Vector3 direction,
        float speed,
        System.Action<Hittable, Vector3> onHit,
        float lifetime = -1f
    )
    {
        if (lifetime > 0f)
        {
            GameManager.Instance.projectileManager.CreateProjectile(
                sprite,
                trajectory,
                where,
                direction,
                speed,
                onHit,
                lifetime
            );
            return;
        }

        GameManager.Instance.projectileManager.CreateProjectile(
            sprite,
            trajectory,
            where,
            direction,
            speed,
            onHit
        );
    }

    public virtual void OnHit(Hittable other, Vector3 impact)
    {
        if (other.team != team)
        {
            other.Damage(new Damage(GetDamage(), Damage.Type.ARCANE));
        }
    }

    public virtual void ApplyModifier(SpellData modifierData, int power = 0, int wave = 1)
    {

        float damageMultiplier = EvaluateFloatOptional(modifierData.damage_multiplier, power, wave, 1f);
        float manaMultiplier = EvaluateFloatOptional(modifierData.mana_multiplier, power, wave, 1f);
        float cooldownMultiplier = EvaluateFloatOptional(modifierData.cooldown_multiplier, power, wave, 1f);
        float speedMultiplier = EvaluateFloatOptional(modifierData.speed_multiplier, power, wave, 1f);
        float manaAdder = EvaluateFloatOptional(modifierData.mana_adder, power, wave, 0f);

        damageAmount = Mathf.Max(1, Mathf.RoundToInt(damageAmount * damageMultiplier));
        if (secondaryDamageAmount > 0)
        {
            secondaryDamageAmount = Mathf.Max(1, Mathf.RoundToInt(secondaryDamageAmount * damageMultiplier));
        }

        manaCost = Mathf.Max(0, Mathf.RoundToInt(manaCost * manaMultiplier + manaAdder));
        cooldown = Mathf.Max(0.05f, cooldown * cooldownMultiplier);

        projectileSpeed = Mathf.Max(0.1f, projectileSpeed * speedMultiplier);
        if (secondaryProjectileSpeed > 0f)
        {
            secondaryProjectileSpeed = Mathf.Max(0.1f, secondaryProjectileSpeed * speedMultiplier);
        }

        if (!string.IsNullOrWhiteSpace(modifierData.projectile_trajectory))
        {
            projectileTrajectory = modifierData.projectile_trajectory;
            if (secondaryProjectileSpeed > 0f)
            {
                secondaryProjectileTrajectory = modifierData.projectile_trajectory;
            }
        }

        if (!string.IsNullOrWhiteSpace(modifierData.name))
        {
            spellName = modifierData.name + " " + spellName;
        }

        if (!string.IsNullOrWhiteSpace(modifierData.description))
        {
            description = modifierData.description + "\n" + description;
        }
    }

    protected float EvaluateFloatOptional(string expression, int power, int wave, float fallback)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return fallback;
        }

        Dictionary<string, int> vars = new Dictionary<string, int>()
        {
            { "power", power },
            { "wave", wave }
        };

        return RPNEvaluator.RPNEvaluator.Evaluatef(expression, vars);
    }

}
