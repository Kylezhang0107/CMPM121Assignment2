using System.Collections;
using UnityEngine;

public class DoublerSpell : ModifierSpell
{
    private readonly float delaySeconds;

    public DoublerSpell(SpellCaster owner, Spell inner, float delaySeconds)
        : base(owner, inner)
    {
        this.delaySeconds = delaySeconds;
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        yield return inner.Cast(where, target, team);
        if (delaySeconds > 0f)
        {
            yield return new WaitForSeconds(delaySeconds);
        }
        yield return inner.Cast(where, target, team);
    }
}
