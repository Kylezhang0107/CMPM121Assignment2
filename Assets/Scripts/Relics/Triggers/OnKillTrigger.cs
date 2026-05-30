using UnityEngine;

public class OnKillTrigger : RelicTrigger
{
    public OnKillTrigger(RuntimeRelic relic) : base(relic) { }

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
        if (relic.IsTemporary())
            RelicManager.Instance.ActivateTemporary(relic);
        else
            relic.effect.Activate();
    }
}