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
        float chance = string.IsNullOrEmpty(data.effect.chance) ? 1f : float.Parse(data.effect.chance);

        if (UnityEngine.Random.value > chance)
        {
            return;
        }

        PlayerController player = GameManager.Instance.player.GetComponent<PlayerController>();
        int amount = Mathf.RoundToInt(RPNEvaluator.RPNEvaluator.Evaluatef(data.effect.amount, new Dictionary<string, int>()));

        player.hp.max_hp += amount;
        player.hp.hp += amount;
        player.healthui.SetHealth(player.hp);

        Debug.Log("[RELIC] +" + amount + " max hp");
    }
}