using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;


public class SpellBuilder
{
    private SpellData data;

    // constructor stores spell definition
    public SpellBuilder(SpellData spellData)
    {
        data = spellData;
    }

    // builds configured runtime Spell
    public Spell Build(SpellCaster owner, int power = 0)
    {
        Spell spell = new Spell(owner);

        Dictionary<string, int> vars =
            new Dictionary<string, int>()
            {
                { "power", power }
            };

        // basic info
        spell.spellName = data.name;

        // evaluate RPN expressions
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

        // projectile settings
        spell.projectileTrajectory =
            data.projectile.trajectory;

        spell.projectileSpeed =
            RPNEvaluator.RPNEvaluator.Evaluate(
                data.projectile.speed,
                vars
            );

        spell.projectileSprite =
            data.projectile.sprite;

        // icon
        spell.icon = data.icon;

        return spell;
    }
}
