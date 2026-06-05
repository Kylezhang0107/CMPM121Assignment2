using UnityEngine;
using System;

public enum ElementPath
{
    Arcane,
    Ice,
    Fire
}

public class SkillTreeManager
{
    private const int SkillPointWaveInterval = 2;

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

    private SkillTreeManager()
    {
        EventBus.Instance.OnWaveComplete += HandleWaveComplete;
    }

    private void HandleWaveComplete(int wave)
    {
        if (wave > 0 && wave % SkillPointWaveInterval == 0)
        {
            AddSkillPoint();
            Debug.Log("Skill point awarded for wave " + wave + ". Total = " + skillPoints);
        }
    }

    public void AddSkillPoint()
    {
        skillPoints++;
        NotifyChanged();
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

        NotifyChanged();

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

        public float GetFreezeDuration()
    {
        return 3f + (freezeDurationLevels * 2f);
    }

    public float GetFreezeSlowAmount()
    {
        return 0.20f + (freezePotencyLevels * 0.10f);
    }

    public int GetManaBonus()
    {
        return manaLevels * 25;
    }

    public float GetBurnDuration()
    {
        return 3f + (burnDurationLevels * 2f);
    }

    public int GetBurnTickDamage()
    {
        return 3 + (burnDamageLevels * 3);
    }

    public float GetMoveSpeedBonusMultiplier()
    {
        return 1f + (moveSpeedLevels * 0.25f);
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

    public bool UnlockMana()
    {
        if (currentPath != ElementPath.Ice)
            return false;

        if (!SpendSkillPoint())
            return false;

        manaLevels++;

        NotifyChanged();

        return true;
    }

    public bool UnlockFreezeDuration()
    {
        if (currentPath != ElementPath.Ice)
            return false;

        if (!SpendSkillPoint())
            return false;

        freezeDurationLevels++;

        NotifyChanged();

        return true;
    }

    public bool UnlockFreezePotency()
    {
        if (currentPath != ElementPath.Ice)
            return false;

        if (!SpendSkillPoint())
            return false;

        freezePotencyLevels++;

        NotifyChanged();

        return true;
    }

    public bool UnlockMoveSpeed()
    {
        if (currentPath != ElementPath.Fire)
            return false;

        if (!SpendSkillPoint())
            return false;

        moveSpeedLevels++;

        NotifyChanged();

        return true;
    }

    public bool UnlockBurnDuration()
    {
        if (currentPath != ElementPath.Fire)
            return false;

        if (!SpendSkillPoint())
            return false;

        burnDurationLevels++;

        NotifyChanged();

        return true;
    }

    public bool UnlockBurnDamage()
    {
        if (currentPath != ElementPath.Fire)
            return false;

        if (!SpendSkillPoint())
            return false;

        burnDamageLevels++;

        NotifyChanged();

        return true;
    }
}