using UnityEngine;

public class Damage 
{
    public int amount;
    public enum Type
    {
        PHYSICAL, ARCANE, NATURE, FIRE, ICE, DARK, LIGHT
    }
    public Type type;

    public GameObject source;
    public Damage(int amount, Type type, GameObject source = null)
    {
        this.amount = amount;
        this.type = type;
        this.source = source;
    }

    public static Type TypeFromString(string type)
    {
        string t = type.ToLower();
        if (t == "arcane") return Type.ARCANE;
        if (t == "nature") return Type.NATURE;
        if (t == "fire") return Type.FIRE;
        if (t == "ice") return Type.ICE;
        if (t == "dark") return Type.DARK;
        if (t == "light") return Type.LIGHT;
        return Type.PHYSICAL;
    }
}
