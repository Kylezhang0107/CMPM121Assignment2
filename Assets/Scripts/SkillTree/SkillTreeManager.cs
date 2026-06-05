using UnityEngine;
using UnityEngine.InputSystem;
using System;

public enum ElementPath
{
    Arcane,
    Ice,
    Fire
}

public class SkillTreeManager
{
    private static SkillTreeManager instance;

    public static SkillTreeManager Instance => instance ??= new SkillTreeManager();

    public event Action OnSkillsChanged;

    public int skillPoints;

    public ElementPath currentPath = ElementPath.Arcane;

    public bool pathChosen = false;

    // Arcane
    public int spellPowerLevels;
    public int spellSpeedLevels;
    public int healChanceLevels;

    // Ice
    public int manaLevels;
    public int freezeDurationLevels;
    public int freezePotencyLevels;

    // Fire
    public int moveSpeedLevels;
    public int burnDurationLevels;
    public int burnDamageLevels;

    public void AddSkillPoint()
    {
        skillPoints++;
    }

    public bool SpendSkillPoint()
    {
        if (skillPoints <= 0)
        {
            return false;
        }

        skillPoints--;
        return true;
    }

    public bool ChoosePath(ElementPath path)
    {
        if (pathChosen)
        {
            return false;
        }

        if (!SpendSkillPoint())
        {
            return false;
        }

        currentPath = path;
        pathChosen = true;

        return true;
    }

    private void NotifyChanged()
    {
        OnSkillsChanged?.Invoke();
    }

    public Damage.Type GetDamageType()
    {
        switch (currentPath)
        {
            case ElementPath.Fire:
                return Damage.Type.FIRE;

            case ElementPath.Ice:
                return Damage.Type.ICE;

            default:
                return Damage.Type.ARCANE;
        }
    }

    public int GetSpellPowerBonus()
        {
            return spellPowerLevels * 10;
        }

    public float GetSpellSpeedMultiplier()
    {
        return 1f + (spellSpeedLevels * 0.05f);
    }

    public float GetHealChance()
    {
        return healChanceLevels * 0.05f;
    }

    public int GetManaBonus()
    {
        return manaLevels * 25;
    }

    public int GetMoveSpeedBonus()
    {
        return moveSpeedLevels * 1;
    }

    public float GetFreezeDurationBonus()
    {
        return freezeDurationLevels * 0.5f;
    }

    public int GetBurnDamageBonus()
    {
        return burnDamageLevels * 5;
    }

    public bool UnlockSpellPower()
    {
        if (currentPath != ElementPath.Arcane)
        {
            return false;
        }

        if (skillPoints <= 0)
        {
            return false;
        }

        skillPoints--;
        spellPowerLevels++;
        Debug.Log(
        "Levels=" + spellPowerLevels +
        " Bonus=" + GetSpellPowerBonus()
    );

        Debug.Log("Purchased Arcane Power. Level = " + spellPowerLevels);

        NotifyChanged();

        return true;
    }

    public bool UnlockSpellSpeed()
    {
        if (currentPath != ElementPath.Arcane)
        {
            return false;
        }

        if (!SpendSkillPoint())
        {
            return false;
        }

        spellSpeedLevels++;

        Debug.Log("Purchased Spell Speed. Level = " + spellSpeedLevels);

        NotifyChanged();

        return true;
    }

    public bool UnlockHealChance()
    {
        if (currentPath != ElementPath.Arcane)
        {
            return false;
        }

        if (!SpendSkillPoint())
        {
            return false;
        }

        healChanceLevels++;

        Debug.Log("Purchased Heal Chance. Level = " + healChanceLevels);

        NotifyChanged();

        return true;
    }
}