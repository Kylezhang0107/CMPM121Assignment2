using System.Collections;
using UnityEngine;

// Custom behavior modifier: adds a small side burst to each cast.
public class SidecarSpell : ModifierSpell
{
    private readonly int extraProjectiles;
    private readonly float spreadDegrees;

    public SidecarSpell(SpellCaster owner, Spell inner, int extraProjectiles, float spreadDegrees)
        : base(owner, inner)
    {
        this.extraProjectiles = Mathf.Max(1, extraProjectiles);
        this.spreadDegrees = Mathf.Max(1f, spreadDegrees);
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        Vector3 baseDirection = target - where;
        if (baseDirection.sqrMagnitude < 0.0001f)
        {
            baseDirection = Vector3.right;
        }

        yield return inner.Cast(where, target, team);

        for (int i = 0; i < extraProjectiles; i++)
        {
            float t = extraProjectiles == 1 ? 0f : (i / (float)(extraProjectiles - 1));
            float angle = Mathf.Lerp(-spreadDegrees * 0.5f, spreadDegrees * 0.5f, t);
            Vector3 sideDirection = Quaternion.Euler(0f, 0f, angle) * baseDirection;
            yield return inner.Cast(where, where + sideDirection, team);
        }
    }
}
