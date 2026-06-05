using UnityEngine;

public class HealOnDamageSkill : MonoBehaviour
{
    private void Awake()
    {
        EventBus.Instance.OnDamage += OnDamage;
    }

    private void OnDestroy()
    {
        EventBus.Instance.OnDamage -= OnDamage;
    }

    private void OnDamage(Vector3 where, Damage damage, Hittable target)
    {
        GameObject player = GameManager.Instance.player;

        if (target == null || target.owner != player)
        {
            return;
        }

        float chance = SkillTreeManager.Instance.GetHealChance();

        if (Random.value > chance)
        {
            return;
        }

        PlayerController pc = player.GetComponent<PlayerController>();

        pc.hp.hp = Mathf.Min(pc.hp.max_hp, pc.hp.hp + 25);
        pc.healthui.SetHealth(pc.hp);
    }
}