using UnityEngine;

public class RuntimeRelic
{
    public PlayerController.RelicData data;

    public RelicTrigger trigger;
    public RelicEffect effect;

    public RuntimeRelic(PlayerController.RelicData data, RelicTrigger trigger, RelicEffect effect)
    {
        this.data = data;
        this.trigger = trigger;
        this.effect = effect;
    }

    public void Register()
    {
        trigger?.Register();
    }

    public void Unregister()
    {
        trigger?.Unregister();
    }
}