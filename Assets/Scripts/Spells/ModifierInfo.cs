public class ModifierInfo
{
    public string id;
    public ModifierType type;
    public ModifierInfo(string id, ModifierType type)
    {
        this.id = id;
        this.type = type;
    }
}

public enum ModifierType
{
    Property, // e.g. damage_amp, efficiency, overclock
    Behavior  // e.g. doubler, splitter, sidecar
}
