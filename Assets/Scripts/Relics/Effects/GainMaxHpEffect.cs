using UnityEngine;
using System.Collections.Generic;

public class GainMaxHpEffect : RelicEffect
{
    private readonly PlayerController.RelicData data;

    public GainMaxHpEffect(PlayerController.RelicData data)
    {
        this.data = data;
    }

    public override void Activate()
    {
        PlayerController player = GameManager.Instance.player.GetComponent<PlayerController>();
        int amount = Mathf.RoundToInt(RPNEvaluator.RPNEvaluator.Evaluatef(data.effect.amount,new Dictionary<string, int>()));

        player.hp.max_hp += amount;
        player.hp.hp += amount;
        player.healthui.SetHealth(player.hp);

        Debug.Log("[RELIC] +" + amount + " max hp");
    }
}