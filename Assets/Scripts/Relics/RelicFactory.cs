using UnityEngine;

public static class RelicFactory
{
    public static RuntimeRelic Create(PlayerController.RelicData data)
    {
        RuntimeRelic relic = new RuntimeRelic(data, null, null);

        RelicEffect effect = CreateEffect(data);
        RelicTrigger trigger = CreateTrigger(data, relic, effect);

        relic.effect = effect;
        relic.trigger = trigger;

        return relic;
    }

    private static RelicEffect CreateEffect(PlayerController.RelicData data)
    {
        switch (data.effect.type)
        {
            case "gain-mana":
                return new GainManaEffect(data);

            case "gain-spellpower":
                return new GainSpellPowerEffect(data);

            case "gain-maxhp":
                return new GainMaxHpEffect(data);

            case "gain-heal":
                return new GainHealEffect(data);

            default:
                Debug.LogError("Unknown relic effect: " + data.effect.type);
                return null;
        }
    }

    private static RelicTrigger CreateTrigger(PlayerController.RelicData data, RuntimeRelic relic, RelicEffect effect)
    {
        switch (data.trigger.type)
        {
            case "take-damage":
                return new TakeDamageTrigger(relic);

            case "on-kill":
                return new OnKillTrigger(relic);

            case "wave-complete":
                return new WaveCompleteTrigger(relic);

            case "stand-still":
                return new StandStillTrigger(relic);

            case "deal-damage":
                return new DealDamageTrigger(relic);

            case "distance-moved":
                return new DistanceMovedTrigger(relic);

            default:
                Debug.LogError("Unknown relic trigger: " + data.trigger.type);
                return null;
        }
    }
}