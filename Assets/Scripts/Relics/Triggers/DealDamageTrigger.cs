using UnityEngine;

public class DealDamageTrigger : RelicTrigger
{
    public DealDamageTrigger(RuntimeRelic relic) : base(relic) { }

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
        if (damage == null || damage.source != GameManager.Instance.player)
            return;

        relic.effect.Activate();
    }
}