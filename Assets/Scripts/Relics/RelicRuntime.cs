using UnityEngine;

public class RuntimeRelic
{
    public PlayerController.RelicData data;
    public RelicTrigger trigger;
    public RelicEffect effect;

    private bool isActiveTemporary;

    public RuntimeRelic(PlayerController.RelicData data, RelicTrigger trigger, RelicEffect effect)
    {
        this.data = data;
        this.effect = effect;
        this.trigger = trigger;
    }

    public void Register()
    {
        trigger?.Register();
    }

    public void Unregister()
    {
        trigger?.Unregister();
        effect?.Remove();
    }

    public void ActivateTemporary()
    {
        if (effect == null)
            return;

        if (isActiveTemporary)
            return;

        isActiveTemporary = true;
        effect.Activate();
    }

    public void DeactivateTemporary()
    {
        if (effect == null || !isActiveTemporary)
            return;

        isActiveTemporary = false;

        effect.Remove();
        trigger?.OnEffectRemoved(); 

    }

    public bool IsTemporary()
    {
        return !string.IsNullOrEmpty(data.effect.until);
    }

    public string UntilType()
    {
        return data.effect.until;
    }
}