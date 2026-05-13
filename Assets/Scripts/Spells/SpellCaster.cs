using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpellCaster 
{
    public const int MaxEquippedSpells = 4;

    public int mana;
    public int max_mana;
    public int mana_reg;
    public int spellPower;
    public Hittable.Team team;
    public List<Spell> spells;
    public int activeSpellIndex;

    // potential spell to be accepted
    public Spell pendingSpell;

    public IEnumerator ManaRegeneration()
    {
        while (true)
        {
            mana += mana_reg;
            mana = Mathf.Min(mana, max_mana);
            yield return new WaitForSeconds(1);
        }
    }

    public SpellCaster(int mana, int mana_reg, Hittable.Team team)
    {
        this.mana = mana;
        this.max_mana = mana;
        this.mana_reg = mana_reg;
        this.spellPower = 0;
        this.team = team;

        spells = new List<Spell>();
        activeSpellIndex = 0;

        Spell starter = new SpellBuilder().BuildSpecific(this, "arcane_bolt", spellPower, Mathf.Max(1, GameManager.Instance.currentWave));
        spells.Add(starter);
    }

    public int SpellCount => spells.Count;

    public Spell ActiveSpell
    {
        get
        {
            if (spells.Count == 0)
            {
                return null;
            }

            activeSpellIndex = Mathf.Clamp(activeSpellIndex, 0, spells.Count - 1);
            return spells[activeSpellIndex];
        }
    }

    public Spell GetSpellAt(int index)
    {
        if (index < 0 || index >= spells.Count)
        {
            return null;
        }

        return spells[index];
    }

    public void SetActiveSpellIndex(int index)
    {
        if (spells.Count == 0)
        {
            activeSpellIndex = 0;
            return;
        }

        activeSpellIndex = Mathf.Clamp(index, 0, spells.Count - 1);
    }

    public void SelectNextSpell()
    {
        if (spells.Count == 0)
        {
            return;
        }

        activeSpellIndex = (activeSpellIndex + 1) % spells.Count;
    }

    public bool AcceptPendingSpell()
    {
        if (pendingSpell == null)
        {
            return false;
        }

        if (spells.Count >= MaxEquippedSpells)
        {
            return false;
        }

        spells.Add(pendingSpell);
        activeSpellIndex = spells.Count - 1;
        pendingSpell = null;
        return true;
    }

    public bool ReplaceSpellAt(int index)
    {
        if (pendingSpell == null)
        {
            return false;
        }

        if (index < 0 || index >= spells.Count)
        {
            return false;
        }

        spells[index] = pendingSpell;
        activeSpellIndex = index;
        pendingSpell = null;
        return true;
    }

    public virtual IEnumerator Cast(Vector3 where, Vector3 target)
    {
        Spell spell = ActiveSpell;
        if (spell == null)
        {
            yield break;
        }

        if (mana >= spell.GetManaCost()
            && spell.IsReady())
        {
            mana -= spell.GetManaCost();
            spell.last_cast = Time.time;

            yield return spell.Cast(
                where,
                target,
                team
            );
        }

        yield break;
    }

}
