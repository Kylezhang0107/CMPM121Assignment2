using UnityEngine;

public class OffCooldownTrigger : RelicTrigger
{
    public OffCooldownTrigger(RuntimeRelic relic) : base(relic)
    {
    }

    public override void Register()
    {
        EventBus.Instance.OnSpellCooldownEnded += OnCooldownEnded;
    }

    public override void Unregister()
    {
        EventBus.Instance.OnSpellCooldownEnded -= OnCooldownEnded;
    }

    private void OnCooldownEnded(Spell spell)
    {
        RelicManager.Instance.RemoveUntil("coolup");
    }
}