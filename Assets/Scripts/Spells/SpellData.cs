using UnityEngine;
using System;

[Serializable]
public class DamageData
{
    public string amount;
    public string type;
}

[Serializable]
public class ProjectileData
{
    public string trajectory;
    public string speed;
    public int sprite;

    // optional
    public string lifetime;
}

[Serializable]
public class SpellData
{
    public string id;
    public string name;
    public string description;

    public int icon;

    public string mana_cost;
    public string cooldown;

    // optional values
    public string N;
    public string spray;

    public string secondary_damage;

    public DamageData damage;

    public ProjectileData projectile;

    // optional
    public ProjectileData secondary_projectile;

    // modifier values (all optional)
    public string damage_multiplier;
    public string mana_multiplier;
    public string cooldown_multiplier;
    public string speed_multiplier;
    public string mana_adder;
    public string projectile_trajectory;
    public string delay;
    public string angle;

    // custom modifier fields
    public string burst_count;
    public string burst_spread;
}
