using UnityEngine;

public class TakeDamageTrigger : RelicTrigger
{
    public TakeDamageTrigger(RelicEffect effect) : base(effect)
    {
    }

    public override void Register()
    {
        EventBus.Instance.OnDamage += OnDamage;
    }

    public override void Unregister()
    {
        EventBus.Instance.OnDamage -= OnDamage;
    }

    private void OnDamage(Vector3 where, Damage damage, Hittable target)
    {
        if (target != null && target.owner == GameManager.Instance.player)
        {
            effect.Activate();
        }
    }
}