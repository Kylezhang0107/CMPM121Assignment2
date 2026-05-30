using System.Collections.Generic;
using UnityEngine;

public class RelicManager : MonoBehaviour
{
    public static RelicManager Instance;

    private readonly List<RuntimeRelic> relics = new();
    private readonly List<RuntimeRelic> activeTemporary = new();
    private bool castJustHappened;

    private void Awake()
    {
        Instance = this;
        EventBus.Instance.OnDamage += OnDamage;
        EventBus.Instance.OnSpellCast += OnSpellCast;
        EventBus.Instance.OnPlayerMove += OnPlayerMove;
    }

    private void OnDestroy()
    {   
        EventBus.Instance.OnDamage -= OnDamage;
        EventBus.Instance.OnSpellCast -= OnSpellCast;
        EventBus.Instance.OnPlayerMove -= OnPlayerMove;
    }

    public void AddRelic(RuntimeRelic relic)
    {
        relics.Add(relic);
        relic.Register();
    }


    public void ActivateTemporary(RuntimeRelic relic)
    {
        if (!relic.IsTemporary())
            return;

        relic.ActivateTemporary();

        if (!activeTemporary.Contains(relic))
            activeTemporary.Add(relic);
    }


    public void RemoveUntil(string untilType)
    {
        for (int i = activeTemporary.Count - 1; i >= 0; i--)
        {
            RuntimeRelic relic = activeTemporary[i];

            if (relic.UntilType() != untilType)
                continue;

            relic.DeactivateTemporary();
            activeTemporary.RemoveAt(i);
        }
    }

    private void OnSpellCast(Spell spell)
    {
        castJustHappened = true;
    }

    private void OnDamage(Vector3 where, Damage damage, Hittable target)
    {

        if (!castJustHappened)
            return;

        RemoveUntil("cast-spell");
        castJustHappened = false;
    }

    private void OnPlayerMove(Vector2 movement)
    {
        if (movement.sqrMagnitude > 0.01f)
        {
            RemoveUntil("move");
        }
    }

    public void ClearRelics()
    {
        foreach (RuntimeRelic relic in relics)
        {
            relic.Unregister();
        }

        relics.Clear();
        activeTemporary.Clear();
    }
}