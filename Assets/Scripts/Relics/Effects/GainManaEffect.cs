using UnityEngine;
using System.Collections.Generic;

public class GainManaEffect : RelicEffect
{
    private readonly PlayerController.RelicData data;

    public GainManaEffect(PlayerController.RelicData data)
    {
        this.data = data;
    }

    public override void Activate()
    {
        PlayerController player = GameManager.Instance.player.GetComponent<PlayerController>();

        int amount = Mathf.RoundToInt(RPNEvaluator.RPNEvaluator.Evaluatef(data.effect.amount, 
        new Dictionary<string, int>(){{"wave", GameManager.Instance.currentWave}}));

        player.spellcaster.mana = Mathf.Min(player.spellcaster.max_mana, player.spellcaster.mana + amount);
        Debug.Log("[RELIC] +" + amount +" mana");
    }
}