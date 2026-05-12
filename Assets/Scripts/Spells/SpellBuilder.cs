using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public class SpellBuilder
{
    private Dictionary<string, SpellData> spells;

    public SpellBuilder()
    {
        spells = SpellJsonLoader.LoadSpells();
    }

    // builds a SPECIFIC spell
    public Spell BuildSpecific(
        SpellCaster owner,
        SpellData data,
        int power = 0
    )
    {
        Spell spell = new Spell(owner);

        Dictionary<string, int> vars =
            new Dictionary<string, int>()
            {
                { "power", power }
            };

        spell.spellName = data.name;
        spell.description = data.description ?? "";

        spell.manaCost =
            RPNEvaluator.RPNEvaluator.Evaluate(
                data.mana_cost,
                vars
            );

        spell.cooldown =
            RPNEvaluator.RPNEvaluator.Evaluate(
                data.cooldown,
                vars
            );

        spell.damageAmount =
            RPNEvaluator.RPNEvaluator.Evaluate(
                data.damage.amount,
                vars
            );

        spell.projectileTrajectory =
            data.projectile.trajectory;

        spell.projectileSpeed =
            RPNEvaluator.RPNEvaluator.Evaluate(
                data.projectile.speed,
                vars
            );

        spell.projectileSprite =
            data.projectile.sprite;

        spell.icon = data.icon;

        return spell;
    }

    // builds RANDOM spell reward
    public Spell BuildRandom(
        SpellCaster owner,
        int power = 0
    )
    {
        List<string> validSpells =
            new List<string>()
            {
                "magic_missile",
                "arcane_blast",
                "arcane_spray"
            };

        string chosen =
            validSpells[
                Random.Range(0, validSpells.Count)
            ];

        return BuildSpecific(
            owner,
            spells[chosen],
            power
        );
    }
}
