using UnityEngine;
using System.Collections.Generic;

public class GainSpellPowerEffect : RelicEffect
{
    private readonly PlayerController.RelicData data;
    private int appliedAmount;

    public GainSpellPowerEffect(PlayerController.RelicData data)
    {
        this.data = data;
    }

    public override void Activate()
    {
        PlayerController player =
            GameManager.Instance.player.GetComponent<PlayerController>();

        int amount = Mathf.RoundToInt(
            RPNEvaluator.RPNEvaluator.Evaluatef(
                data.effect.amount,
                new Dictionary<string, int>()
                {
                    { "wave", GameManager.Instance.currentWave }
                }
            )
        );

        appliedAmount += amount;

        player.AddSpellPowerBonus(amount);

        Debug.Log("[RELIC] +" + amount + " spell power");
    }

    public override void Remove()
    {
        base.Remove();

        PlayerController player =
            GameManager.Instance.player.GetComponent<PlayerController>();

        player.AddSpellPowerBonus(-appliedAmount);

        Debug.Log("[RELIC REMOVED] -" + appliedAmount + " spell power");

        appliedAmount = 0;
    }
}