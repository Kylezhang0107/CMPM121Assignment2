using UnityEngine;
using System;

public class EventBus
{
    private static EventBus theInstance;

    public static EventBus Instance
    {
        get
        {
            if (theInstance == null)
            {
                theInstance = new EventBus();
            }

            return theInstance;
        }
    }

    // =========================
    // DAMAGE
    // =========================

    public event Action<Vector3, Damage, Hittable> OnDamage;

    public void DoDamage(
        Vector3 where,
        Damage dmg,
        Hittable target
    )
    {
        OnDamage?.Invoke(where, dmg, target);
    }

    // =========================
    // ENEMY KILLED
    // =========================

    public event Action<GameObject> OnEnemyKilled;

    public void EnemyKilled(GameObject enemy)
    {
        OnEnemyKilled?.Invoke(enemy);
    }

    // =========================
    // SPELL CAST
    // =========================

    public event Action<Spell> OnSpellCast;

    public void SpellCast(Spell spell)
    {
        OnSpellCast?.Invoke(spell);
    }

    public event Action<Spell> OnSpellCooldownStarted;
    public event Action<Spell> OnSpellCooldownEnded;
    public void SpellCooldownStarted(Spell spell)
    {
        OnSpellCooldownStarted?.Invoke(spell);
    }

    public void SpellCooldownEnded(Spell spell)
    {
        OnSpellCooldownEnded?.Invoke(spell);
    }
        

    // =========================
    // PLAYER MOVE
    // =========================

    public event Action<Vector2> OnPlayerMove;

    public void PlayerMove(Vector2 movement)
    {
        OnPlayerMove?.Invoke(movement);
    }

    public event Action<float> OnDistanceMoved;

    public void DistanceMoved(float distance)
    {
        OnDistanceMoved?.Invoke(distance);
    }

    // =========================
    // WAVE COMPLETE
    // =========================

    public event Action<int> OnWaveComplete;
    
    public void WaveComplete(int wave)
    {
        OnWaveComplete?.Invoke(wave);
    }
}
