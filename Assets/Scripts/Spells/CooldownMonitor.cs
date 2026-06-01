using UnityEngine;

public class CooldownMonitor : MonoBehaviour
{
    private bool wasOnCooldown;

    private void Update()
    {
        if (GameManager.Instance?.player == null) return;

        PlayerController player = GameManager.Instance.player.GetComponent<PlayerController>();

        if (player?.spellcaster == null) return;

        Spell spell = player.spellcaster.ActiveSpell;

        if (spell == null) return;

        bool isOnCooldown = !spell.IsReady();

        if (isOnCooldown && !wasOnCooldown)
        {
            EventBus.Instance.SpellCooldownStarted(spell);
        }
        else if (!isOnCooldown && wasOnCooldown)
        {
            EventBus.Instance.SpellCooldownEnded(spell);
        }

        wasOnCooldown = isOnCooldown;
    }
}