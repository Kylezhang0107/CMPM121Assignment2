using System.Collections.Generic;
using UnityEngine;

public class SpellBuilder
{
    private readonly Dictionary<string, SpellData> spells;

    private readonly List<string> baseSpellIds = new List<string>()
    {
        "arcane_bolt",
        "magic_missile",
        "arcane_blast",
        "arcane_spray"
    };

    private readonly List<string> modifierSpellIds = new List<string>()
    {
        "damage_amp",
        "speed_amp",
        "doubler",
        "splitter",
        "chaos",
        "homing",
        "sidecar",
        "efficiency",
        "overclock"
    };

    public SpellBuilder()
    {
        spells = SpellJsonLoader.LoadSpells();
    }

    public Spell BuildSpecific(SpellCaster owner, string spellId, int power = 0, int wave = 1)
    {
        if (!spells.TryGetValue(spellId, out SpellData data))
        {
            return BuildFallbackSpell(owner);
        }

        Spell spell;
        if (spellId == "arcane_blast")
        {
            spell = new ArcaneBlastSpell(owner);
        }
        else if (spellId == "arcane_spray")
        {
            spell = new ArcaneSpraySpell(owner);
        }
        else
        {
            spell = new Spell(owner);
        }

        Dictionary<string, int> vars = BuildVars(power, wave);

        spell.spellName = data.name;
        spell.description = data.description ?? "";
        spell.icon = data.icon;

        spell.manaCost = EvaluateInt(data.mana_cost, vars, 10);
        spell.cooldown = EvaluateFloat(data.cooldown, vars, 1f);
        spell.damageAmount = EvaluateInt(data.damage != null ? data.damage.amount : null, vars, 10);

        if (data.projectile != null)
        {
            spell.projectileTrajectory = data.projectile.trajectory;
            spell.projectileSpeed = EvaluateFloat(data.projectile.speed, vars, 10f);
            spell.projectileSprite = data.projectile.sprite;
            spell.projectileLifetime = EvaluateFloatOptional(data.projectile.lifetime, vars, -1f);
        }

        spell.secondaryProjectileCount = EvaluateIntOptional(data.N, vars, 1);
        spell.sprayAngle = EvaluateFloatOptional(data.spray, vars, 0f);
        spell.secondaryDamageAmount = EvaluateIntOptional(data.secondary_damage, vars, 0);

        if (data.secondary_projectile != null)
        {
            spell.secondaryProjectileTrajectory = data.secondary_projectile.trajectory;
            spell.secondaryProjectileSpeed = EvaluateFloat(data.secondary_projectile.speed, vars, 8f);
            spell.secondaryProjectileSprite = data.secondary_projectile.sprite;
            spell.secondaryProjectileLifetime = EvaluateFloatOptional(data.secondary_projectile.lifetime, vars, -1f);
        }

        return spell;
    }

    public Spell BuildSpecific(SpellCaster owner, SpellData data, int power = 0, int wave = 1)
    {
        foreach (KeyValuePair<string, SpellData> pair in spells)
        {
            if (pair.Value == data)
            {
                return BuildSpecific(owner, pair.Key, power, wave);
            }
        }

        return BuildFallbackSpell(owner);
    }

    public Spell BuildRandom(SpellCaster owner, int power = 0, int wave = 1)
    {
        List<string> validBases = new List<string>();
        foreach (string baseId in baseSpellIds)
        {
            if (spells.ContainsKey(baseId))
            {
                validBases.Add(baseId);
            }
        }

        if (validBases.Count == 0)
        {
            return BuildFallbackSpell(owner);
        }

        string chosenBase = validBases[Random.Range(0, validBases.Count)];
        Spell spell = BuildSpecific(owner, chosenBase, power, wave);

        List<string> validModifiers = new List<string>();
        foreach (string modifierId in modifierSpellIds)
        {
            if (spells.ContainsKey(modifierId))
            {
                validModifiers.Add(modifierId);
            }
        }

        int modifierCount = 0;
        while (validModifiers.Count > 0 && modifierCount < 4 && Random.value < 0.7f)
        {
            string modifierId = validModifiers[Random.Range(0, validModifiers.Count)];
            spell = ApplyModifier(owner, spell, modifierId, power, wave);
            modifierCount++;
        }

        return spell;
    }

    private Spell ApplyModifier(SpellCaster owner, Spell spell, string modifierId, int power, int wave)
    {
        if (!spells.TryGetValue(modifierId, out SpellData modifierData))
        {
            return spell;
        }

        spell.ApplyModifier(modifierData, power, wave);

        Dictionary<string, int> vars = BuildVars(power, wave);
        if (modifierId == "splitter")
        {
            float angle = EvaluateFloatOptional(modifierData.angle, vars, 7f);
            return new SplitterSpell(owner, spell, angle);
        }

        if (modifierId == "doubler")
        {
            float delay = EvaluateFloatOptional(modifierData.delay, vars, 0.25f);
            return new DoublerSpell(owner, spell, delay);
        }

        if (modifierId == "sidecar")
        {
            int count = EvaluateIntOptional(modifierData.burst_count, vars, 2);
            float spread = EvaluateFloatOptional(modifierData.burst_spread, vars, 25f);
            return new SidecarSpell(owner, spell, count, spread);
        }

        return spell;
    }

    private Spell BuildFallbackSpell(SpellCaster owner)
    {
        Spell fallback = new Spell(owner);
        fallback.spellName = "Arcane Bolt";
        fallback.description = "Fallback spell when a configured spell cannot be loaded.";
        fallback.icon = 0;
        fallback.manaCost = 10;
        fallback.cooldown = 1.5f;
        fallback.damageAmount = 20;
        fallback.projectileTrajectory = "straight";
        fallback.projectileSpeed = 8f;
        fallback.projectileSprite = 0;
        return fallback;
    }

    private Dictionary<string, int> BuildVars(int power, int wave)
    {
        return new Dictionary<string, int>()
        {
            { "power", power },
            { "wave", wave }
        };
    }

    private int EvaluateInt(string expression, Dictionary<string, int> vars, int fallback)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return fallback;
        }

        return RPNEvaluator.RPNEvaluator.Evaluate(expression, vars);
    }

    private int EvaluateIntOptional(string expression, Dictionary<string, int> vars, int fallback)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return fallback;
        }

        return Mathf.RoundToInt(RPNEvaluator.RPNEvaluator.Evaluatef(expression, vars));
    }

    private float EvaluateFloat(string expression, Dictionary<string, int> vars, float fallback)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return fallback;
        }

        return RPNEvaluator.RPNEvaluator.Evaluatef(expression, vars);
    }

    private float EvaluateFloatOptional(string expression, Dictionary<string, int> vars, float fallback)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return fallback;
        }

        return RPNEvaluator.RPNEvaluator.Evaluatef(expression, vars);
    }
}
