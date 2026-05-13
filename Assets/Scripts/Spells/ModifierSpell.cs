using System.Collections;
using UnityEngine;

public class ModifierSpell : Spell
{
    protected readonly Spell inner;

    public ModifierSpell(SpellCaster owner, Spell inner)
        : base(owner)
    {
        this.inner = inner;
    }

    public override int GetIcon()
    {
        return inner.GetIcon();
    }

    public override string GetName()
    {
        return inner.GetName();
    }

    public override string GetDescription()
    {
        return inner.GetDescription();
    }

    public override int GetManaCost()
    {
        return inner.GetManaCost();
    }

    public override int GetDamage()
    {
        return inner.GetDamage();
    }

    public override float GetCooldown()
    {
        return inner.GetCooldown();
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        yield return inner.Cast(where, target, team);
    }

    public override void ApplyModifier(SpellData modifierData, int power = 0, int wave = 1)
    {
        inner.ApplyModifier(modifierData, power, wave);
    }
}
