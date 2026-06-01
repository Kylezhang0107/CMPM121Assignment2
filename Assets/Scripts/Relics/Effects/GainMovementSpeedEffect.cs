using UnityEngine;

public class GainMovementSpeedEffect : RelicEffect
{
    private readonly PlayerController.RelicData data;

    private int appliedAmount;

    public GainMovementSpeedEffect(
        PlayerController.RelicData data
    )
    {
        this.data = data;
    }

    public override void Activate()
    {
        if (active)
            return;

        base.Activate();

        PlayerController player =
            GameManager.Instance.player
            .GetComponent<PlayerController>();

        appliedAmount =
            int.Parse(
                data.effect.amount
            );

        player.speed += appliedAmount;

        Debug.Log(
            "[RELIC] +" +
            appliedAmount +
            " speed"
        );
    }

    public override void Remove()
    {
        if (!active)
            return;

        PlayerController player =
            GameManager.Instance.player
            .GetComponent<PlayerController>();

        player.speed -= appliedAmount;

        appliedAmount = 0;

        base.Remove();
    }
}