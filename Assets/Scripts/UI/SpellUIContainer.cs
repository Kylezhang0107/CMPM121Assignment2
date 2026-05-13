using UnityEngine;
using UnityEngine.UI;

public class SpellUIContainer : MonoBehaviour
{
    public GameObject[] spellUIs;
    public PlayerController player;
    private SpellCaster caster;

    void Start()
    {
        for (int i = 0; i < spellUIs.Length; ++i)
        {
            SpellUI ui = spellUIs[i].GetComponent<SpellUI>();
            if (ui != null && ui.dropbutton != null)
            {
                Button dropButton = ui.dropbutton.GetComponent<Button>();
                if (dropButton != null)
                {
                    int slot = i;
                    dropButton.onClick.AddListener(() => OnDropButtonPressed(slot));
                }
            }
        }

        Refresh();
    }

    public void SetSpellCaster(SpellCaster caster)
    {
        this.caster = caster;
        Refresh();
    }

    void Update()
    {
        if (caster == null && player != null)
        {
            caster = player.spellcaster;
        }

        Refresh();
    }

    public void OnDropButtonPressed(int slotIndex)
    {
        if (caster == null)
        {
            return;
        }

        if (caster.pendingSpell != null && caster.ReplaceSpellAt(slotIndex))
        {
            if (RewardScreenManager.Instance != null && RewardScreenManager.Instance.rewardUI != null)
            {
                RewardScreenManager.Instance.rewardUI.SetActive(false);
            }
        }
    }

    private void Refresh()
    {
        for (int i = 0; i < spellUIs.Length; ++i)
        {
            SpellUI ui = spellUIs[i].GetComponent<SpellUI>();
            if (ui == null)
            {
                continue;
            }

            Spell spell = caster != null ? caster.GetSpellAt(i) : null;
            bool hasSpell = spell != null;

            spellUIs[i].SetActive(hasSpell);
            if (!hasSpell)
            {
                continue;
            }

            ui.SetSpell(spell);
            ui.SetHighlighted(caster != null && caster.activeSpellIndex == i);

            if (ui.dropbutton != null)
            {
                bool canReplace = caster != null
                    && caster.pendingSpell != null
                    && caster.SpellCount >= SpellCaster.MaxEquippedSpells
                    && GameManager.Instance.state == GameManager.GameState.WAVEEND;
                ui.dropbutton.SetActive(canReplace);
            }
        }
    }

}
