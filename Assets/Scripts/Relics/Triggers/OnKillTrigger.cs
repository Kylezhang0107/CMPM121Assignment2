using UnityEngine;

public class OnKillTrigger : RelicTrigger
{
    public OnKillTrigger(RelicEffect effect) : base(effect)
    {
    }

    public override void Register()
    {
        EventBus.Instance.OnEnemyKilled += OnKill;
    }

    public override void Unregister()
    {
        EventBus.Instance.OnEnemyKilled -= OnKill;
    }

    private void OnKill(GameObject enemy)
    {
        effect.Activate();
    }
}