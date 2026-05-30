using UnityEngine;

public static class RelicFactory
{
    public static RuntimeRelic Create(PlayerController.RelicData data)
    {
        RelicEffect effect = CreateEffect(data);
        RelicTrigger trigger = CreateTrigger(data, effect);

        return new RuntimeRelic(data, trigger, effect);
    }

    private static RelicEffect CreateEffect(PlayerController.RelicData data)
    {
        switch (data.effect.type)
        {
            case "gain-mana":
                return new GainManaEffect(data);

            // case "gain-spellpower":
            //     return new GainSpellPowerEffect(data);

            case "gain-maxhp":
                return new GainMaxHpEffect(data);

            default:
                Debug.LogError("Unknown relic effect: " + data.effect.type);
                return null;
        }
    }

    private static RelicTrigger CreateTrigger(PlayerController.RelicData data, RelicEffect effect)
    {
        switch (data.trigger.type)
        {
            case "take-damage":
                return new TakeDamageTrigger(effect);

            case "on-kill":
                return new OnKillTrigger(effect);

            case "wave-complete":
                return new WaveCompleteTrigger(effect);

            default:
                Debug.LogError("Unknown relic trigger: " + data.trigger.type);
                return null;
        }
    }
}