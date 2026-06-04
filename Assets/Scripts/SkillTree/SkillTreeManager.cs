public enum ElementPath
{
    Arcane,
    Ice,
    Fire
}

public class SkillTreeManager
{
    private static SkillTreeManager instance;
    public static SkillTreeManager Instance =>
        instance ??= new SkillTreeManager();

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
}