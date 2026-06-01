using UnityEngine;

public class DistanceMovedTrigger : RelicTrigger
{
    private float requiredDistance;
    private float currentDistance;

    public DistanceMovedTrigger(RuntimeRelic relic)
        : base(relic)
    {
        float.TryParse(
            relic.data.trigger.amount,
            out requiredDistance
        );

        if (requiredDistance <= 0)
        {
            requiredDistance = 50f;
        }
    }

    public override void Register()
    {
        currentDistance = 0f;

        EventBus.Instance.OnDistanceMoved += OnDistanceMoved;
    }

    public override void Unregister()
    {
        EventBus.Instance.OnDistanceMoved -= OnDistanceMoved;
    }

    private void OnDistanceMoved(float distance)
    {
        currentDistance += distance;

        if (currentDistance < requiredDistance)
        {
            return;
        }

        currentDistance -= requiredDistance;

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