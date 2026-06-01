using UnityEngine;

public class OnCooldownTrigger : RelicTrigger
{
    public OnCooldownTrigger(RuntimeRelic relic) : base(relic)
    {
    }

    public override void Register()
    {
        EventBus.Instance.OnSpellCooldownStarted += OnCooldownStarted;
    }

    public override void Unregister()
    {
        EventBus.Instance.OnSpellCooldownStarted -= OnCooldownStarted;
    }

    private void OnCooldownStarted(Spell spell)
    {
        if (relic.IsTemporary())
        {
            RelicManager.Instance.ActivateTemporary(relic);
        }
        else
        {
            relic.effect.Activate();
        }
    }
}