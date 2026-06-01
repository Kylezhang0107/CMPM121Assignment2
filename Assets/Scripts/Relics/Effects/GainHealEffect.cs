using UnityEngine;
using System.Collections.Generic;

public class GainHealEffect : RelicEffect
{
    private readonly PlayerController.RelicData data;

    public GainHealEffect(PlayerController.RelicData data)
    {
        this.data = data;
    }

    public override void Activate()
    {
        float chance = string.IsNullOrEmpty(data.effect.chance) ? 1f : float.Parse(data.effect.chance);
        if (Random.value > chance)
        {
            return;
        }

        PlayerController player = GameManager.Instance.player.GetComponent<PlayerController>();

        int amount = Mathf.RoundToInt(RPNEvaluator.RPNEvaluator.Evaluatef(data.effect.amount, new Dictionary<string, int>()));
        int oldHp = player.hp.hp;

        player.hp.hp = Mathf.Min(player.hp.max_hp, player.hp.hp + amount);
        player.healthui.SetHealth(player.hp);

        Debug.Log("[RELIC] healed " + (player.hp.hp - oldHp) + " HP");
    }
}