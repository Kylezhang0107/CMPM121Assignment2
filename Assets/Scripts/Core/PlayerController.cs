using UnityEngine;
using UnityEngine.InputSystem;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;

public class PlayerController : MonoBehaviour
{
    [Serializable]
    public class RelicTriggerData
    {
        public string description;
        public string type;
        public string amount;
    }

    [Serializable]
    public class RelicEffectData
    {
        public string description;
        public string type;
        public string amount;
        public string until;
    }

    [Serializable]
    public class RelicData
    {
        public string name;
        public int sprite;
        public RelicTriggerData trigger;
        public RelicEffectData effect;

        public string GetDescription()
        {
            string triggerText = trigger != null ? trigger.description : string.Empty;
            string effectText = effect != null ? effect.description : string.Empty;
            return $"{triggerText} {effectText}".Trim();
        }
    }

    public Hittable hp;
    public HealthBar healthui;
    public ManaBar manaui;

    public SpellCaster spellcaster;
    public SpellUI spellui;
    public SpellUIContainer spellUIContainer;

    public int speed;

    public Unit unit;

    private readonly List<RelicData> relicCatalog = new List<RelicData>();
    private readonly List<RelicData> grantedRelics = new List<RelicData>();
    private readonly HashSet<string> ownedRelicNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public event Action<RelicData> OnRelicGranted;

    public int RelicCount => grantedRelics.Count;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        unit = GetComponent<Unit>();
        GameManager.Instance.player = gameObject;
        LoadRelicCatalog();
        EventBus.Instance.OnDamage += OnDamageEvent;
    }

    private void OnDestroy()
    {
        EventBus.Instance.OnDamage -= OnDamageEvent;
    }

    private void LoadRelicCatalog()
    {
        relicCatalog.Clear();

        TextAsset relicFile = Resources.Load<TextAsset>("relics");
        if (relicFile == null)
        {
            Debug.LogError("Could not find Assets/Resources/relics.json");
            return;
        }

        try
        {
            List<RelicData> parsed = JsonConvert.DeserializeObject<List<RelicData>>(relicFile.text);
            if (parsed != null)
            {
                relicCatalog.AddRange(parsed.Where(r => r != null && !string.IsNullOrWhiteSpace(r.name)));
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to parse relics.json: {e.Message}");
        }
    }

    public bool TryGetNextRelic(out RelicData relic)
    {
        relic = null;

        // First relic pass: prioritize the assignment's Green Gem trigger/effect.
        relic = relicCatalog.FirstOrDefault(r =>
            !ownedRelicNames.Contains(r.name) &&
            string.Equals(r.name, "Green Gem", StringComparison.OrdinalIgnoreCase));

        return relic != null;
    }

    public List<RelicData> GetRelicChoices(int count)
    {
        List<RelicData> available = relicCatalog
            .Where(r => r != null && !ownedRelicNames.Contains(r.name))
            .ToList();

        List<RelicData> choices = new List<RelicData>();
        if (count <= 0 || available.Count == 0)
        {
            return choices;
        }

        while (choices.Count < count && available.Count > 0)
        {
            int pick = UnityEngine.Random.Range(0, available.Count);
            choices.Add(available[pick]);
            available.RemoveAt(pick);
        }

        return choices;
    }

    public void GrantRelic(RelicData relic)
    {
        if (relic == null || string.IsNullOrWhiteSpace(relic.name))
        {
            return;
        }

        if (ownedRelicNames.Add(relic.name))
        {
            grantedRelics.Add(relic);
            OnRelicGranted?.Invoke(relic);
            Debug.Log($"Relic acquired: {relic.name}");
        }
    }

    public RelicData GetRelicAt(int index)
    {
        if (index < 0 || index >= grantedRelics.Count)
        {
            return null;
        }

        return grantedRelics[index];
    }

    public string GetRelicLabelAt(int index)
    {
        RelicData relic = GetRelicAt(index);
        if (relic == null)
        {
            return string.Empty;
        }

        return relic.GetDescription();
    }

    public bool IsRelicActiveAt(int index)
    {
        // Current implemented relic effects are passive/always-on while owned.
        return GetRelicAt(index) != null;
    }

    private void OnDamageEvent(Vector3 where, Damage damage, Hittable target)
    {
        if (target == null || target.owner != gameObject || spellcaster == null)
        {
            return;
        }

        foreach (RelicData relic in relicCatalog)
        {
            if (relic == null || !ownedRelicNames.Contains(relic.name))
            {
                continue;
            }

            if (!string.Equals(relic.trigger?.type, "take-damage", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!string.Equals(relic.effect?.type, "gain-mana", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!int.TryParse(relic.effect.amount, out int manaGain))
            {
                continue;
            }

            spellcaster.mana = Mathf.Min(spellcaster.max_mana, spellcaster.mana + manaGain);
        }
    }

    public void StartLevel()
    {
        ApplyWaveScaling(1);
    }

    public void ApplyWaveScaling(int wave)
    {
        Dictionary<string, int> vars =
            new Dictionary<string, int>()
            {
                { "wave", wave }
            };

        // evaluate scaled stats
        int maxHP = Mathf.RoundToInt(
            RPNEvaluator.RPNEvaluator.Evaluatef(
                "95 wave 5 * +",
                vars
            )
        );

        int mana = Mathf.RoundToInt(
            RPNEvaluator.RPNEvaluator.Evaluatef(
                "90 wave 10 * +",
                vars
            )
        );

        int manaRegen = Mathf.RoundToInt(
            RPNEvaluator.RPNEvaluator.Evaluatef(
                "10 wave +",
                vars
            )
        );

        int power = Mathf.RoundToInt(
            RPNEvaluator.RPNEvaluator.Evaluatef(
                "wave 10 *",
                vars
            )
        );

        int moveSpeed = Mathf.RoundToInt(
            RPNEvaluator.RPNEvaluator.Evaluatef(
                "5",
                vars
            )
        );

        // movement speed
        speed = moveSpeed;

        // HP setup
        if (hp == null)
        {
            hp = new Hittable(
                maxHP,
                Hittable.Team.PLAYER,
                gameObject
            );

            hp.OnDeath += Die;
        }
        else
        {
            // preserves health percentage
            hp.SetMaxHP(maxHP);

            // optional full heal each wave:
            hp.hp = hp.max_hp;
        }

        // spellcaster setup
        if (spellcaster == null)
        {
            spellcaster =
                new SpellCaster(
                    mana,
                    manaRegen,
                    power,
                    Hittable.Team.PLAYER
                );

            StartCoroutine(
                spellcaster.ManaRegeneration()
            );
        }
        else
        {
            spellcaster.max_mana = mana;
            spellcaster.mana = mana;
            spellcaster.mana_reg = manaRegen;
            spellcaster.spellPower = power;

            // IMPORTANT:
            // rebuild spells using new power
            spellcaster.RebuildSpells();
        }

        // refresh UI
        healthui.SetHealth(hp);
        manaui.SetSpellCaster(spellcaster);

        if (spellUIContainer == null)
        {
            spellUIContainer =
                FindFirstObjectByType<SpellUIContainer>();
        }

        if (spellUIContainer != null)
        {
            spellUIContainer.SetSpellCaster(spellcaster);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (spellcaster == null || Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            spellcaster.SelectNextSpell();
        }

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            spellcaster.SetActiveSpellIndex(0);
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            spellcaster.SetActiveSpellIndex(1);
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            spellcaster.SetActiveSpellIndex(2);
        }

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            spellcaster.SetActiveSpellIndex(3);
        }
    }

    void OnAttack(InputValue value)
    {
        if (GameManager.Instance.state == GameManager.GameState.PREGAME || GameManager.Instance.state == GameManager.GameState.GAMEOVER) return;
        Vector2 mouseScreen = Mouse.current.position.value;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0;
        StartCoroutine(spellcaster.Cast(transform.position, mouseWorld));
    }

    void OnMove(InputValue value)
    {
        if (GameManager.Instance.state == GameManager.GameState.PREGAME || GameManager.Instance.state == GameManager.GameState.GAMEOVER) return;
        unit.movement = value.Get<Vector2>()*speed;
    }

    void Die()
    {
        Debug.Log("You Lost!");
        GameManager.Instance.state = GameManager.GameState.GAMEOVER;
    }
}
