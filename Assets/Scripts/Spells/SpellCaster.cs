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

    public SpellCaster(int mana, int mana_reg, int spellPower, Hittable.Team team)
    {
        this.mana = mana;
        this.max_mana = mana;
        this.mana_reg = mana_reg;
        this.spellPower = spellPower;
        this.team = team;

        spells = new List<Spell>();
        activeSpellIndex = 0;

        Spell starter = new SpellBuilder().BuildSpecific(this, "arcane_bolt", spellPower, Mathf.Max(1, GameManager.Instance.currentWave));
        starter.lastProgressionPower = spellPower;
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

        // Initialize progression tracking for new spell
        pendingSpell.GetBaseSpell().lastProgressionPower = spellPower;
        
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

        // Initialize progression tracking for new spell
        pendingSpell.GetBaseSpell().lastProgressionPower = spellPower;
        
        spells[index] = pendingSpell;
        activeSpellIndex = index;
        pendingSpell = null;
        return true;
    }

    public void RebuildSpells()
    {
        SpellBuilder builder = new SpellBuilder();

        for (int i = 0; i < spells.Count; i++)
        {
            Spell currentSpell = spells[i];
            if (currentSpell == null)
            {
                Debug.LogError("Null spell in list.");
                continue;
            }

            // Get base spell ID (even if wrapped in modifiers)
            Spell baseSpell = currentSpell.GetBaseSpell();
            string baseSpellId = baseSpell.spellId;
            
            if (string.IsNullOrEmpty(baseSpellId))
            {
                Debug.LogError("Spell missing spellId: " + currentSpell.spellName);
                continue;
            }

            int oldPower = baseSpell.lastProgressionPower;
            int newPower = spellPower;
            
            // If first progression or no prior tracking, initialize from power 0
            if (oldPower == 0 && spellPower > 0)
                oldPower = 0;

            // Only apply gains if power actually increased
            if (newPower > oldPower)
            {
                // Get pure base spell properties at OLD power
                builder.GetBaseSpellProperties(
                    baseSpellId, 
                    oldPower, 
                    1, 
                    out int oldManaCost, 
                    out int oldDamage, 
                    out int oldSecondaryDamage,
                    out float oldCooldown,
                    out float oldProjectileSpeed,
                    out float oldSecondaryProjectileSpeed
                );

                // Get pure base spell properties at NEW power
                builder.GetBaseSpellProperties(
                    baseSpellId, 
                    newPower, 
                    1, 
                    out int newManaCost, 
                    out int newDamage, 
                    out int newSecondaryDamage,
                    out float newCooldown,
                    out float newProjectileSpeed,
                    out float newSecondaryProjectileSpeed
                );

                // Calculate additive gains
                int manaCostGain = newManaCost - oldManaCost;
                int damageGain = newDamage - oldDamage;
                int secondaryDamageGain = newSecondaryDamage - oldSecondaryDamage;
                float cooldownGain = newCooldown - oldCooldown;
                float projectileSpeedGain = newProjectileSpeed - oldProjectileSpeed;
                float secondaryProjectileSpeedGain = newSecondaryProjectileSpeed - oldSecondaryProjectileSpeed;

                // Apply gains directly to CURRENT spell (preserves all modifiers)
                currentSpell.manaCost = Mathf.Max(0, currentSpell.manaCost + manaCostGain);
                currentSpell.damageAmount = Mathf.Max(1, currentSpell.damageAmount + damageGain);
                if (currentSpell.secondaryDamageAmount > 0)
                {
                    currentSpell.secondaryDamageAmount = Mathf.Max(1, currentSpell.secondaryDamageAmount + secondaryDamageGain);
                }
                currentSpell.cooldown = Mathf.Max(0.05f, currentSpell.cooldown + cooldownGain);
                currentSpell.projectileSpeed = Mathf.Max(0.1f, currentSpell.projectileSpeed + projectileSpeedGain);
                if (currentSpell.secondaryProjectileSpeed > 0f)
                {
                    currentSpell.secondaryProjectileSpeed = Mathf.Max(0.1f, currentSpell.secondaryProjectileSpeed + secondaryProjectileSpeedGain);
                }

                // Update base spell's lastProgressionPower to prevent duplicate gains
                baseSpell.lastProgressionPower = newPower;
            }

            spells[i] = currentSpell;
        }
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
