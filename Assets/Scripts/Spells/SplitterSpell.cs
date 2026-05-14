using System.Collections;
using UnityEngine;

public class SplitterSpell : ModifierSpell
{
    private readonly float splitAngleDegrees;

    public float SplitAngleDegrees => splitAngleDegrees;

    public SplitterSpell(SpellCaster owner, Spell inner, float splitAngleDegrees)
        : base(owner, inner)
    {
        this.splitAngleDegrees = splitAngleDegrees;
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        Vector3 baseDirection = target - where;
        if (baseDirection.sqrMagnitude < 0.0001f)
        {
            baseDirection = Vector3.right;
        }

        float randomA = Random.Range(-splitAngleDegrees, splitAngleDegrees);
        float randomB = Random.Range(-splitAngleDegrees, splitAngleDegrees);
        Vector3 dirA = Quaternion.Euler(0f, 0f, randomA) * baseDirection;
        Vector3 dirB = Quaternion.Euler(0f, 0f, randomB) * baseDirection;

        yield return inner.Cast(where, where + dirA, team);
        yield return inner.Cast(where, where + dirB, team);
    }
}
