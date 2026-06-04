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
}