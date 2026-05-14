using System.Collections.Generic;

public static class SpellExtensions
{
    // Returns the innermost base spell (not a ModifierSpell)
    public static Spell GetBaseSpell(this Spell spell)
    {
        Spell current = spell;
        while (current is ModifierSpell mod)
        {
            current = mod.GetInner();
        }
        return current;
    }

    // Returns behavior modifiers from outer to inner
    public static List<BehaviorModifier> GetBehaviorChain(this Spell spell)
    {
        var chain = new List<BehaviorModifier>();
        Spell current = spell;
        while (current is ModifierSpell mod)
        {
            if (mod is SidecarSpell sidecar)
            {
                chain.Add(new BehaviorModifier 
                { 
                    Type = "sidecar", 
                    ExtraProjectiles = sidecar.ExtraProjectiles,
                    SpreadDegrees = sidecar.SpreadDegrees
                });
            }
            else if (mod is DoublerSpell doubler)
            {
                chain.Add(new BehaviorModifier 
                { 
                    Type = "doubler", 
                    DelaySeconds = doubler.DelaySeconds
                });
            }
            else if (mod is SplitterSpell splitter)
            {
                chain.Add(new BehaviorModifier 
                { 
                    Type = "splitter", 
                    SplitAngleDegrees = splitter.SplitAngleDegrees
                });
            }
            current = mod.GetInner();
        }
        chain.Reverse();
        return chain;
    }
}

public class BehaviorModifier
{
    public string Type;
    public float DelaySeconds;
    public float SplitAngleDegrees;
    public int ExtraProjectiles;
    public float SpreadDegrees;
}
