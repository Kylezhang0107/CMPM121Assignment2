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
        public string chance;
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

    [Serializable]
    public class CharacterClassData
    {
        public int sprite;
        public string health;
        public string mana;
        public string mana_regeneration;
        public string spellpower;
        public string speed;
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
    private readonly Dictionary<string, CharacterClassData> characterClasses = new Dictionary<string, CharacterClassData>(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> ownedRelicNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public event Action<RelicData> OnRelicGranted;
    private string selectedCharacterClass = "mage";

    // bonus fields
    private int relicBonusSpellPower = 0;
    private float stationaryTimer = 0f;
    private Vector3 previousPosition;

    // active relic tracker
    private readonly List<RelicData> activeTemporaryRelics = new List<RelicData>();

    public int RelicCount => grantedRelics.Count;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        unit = GetComponent<Unit>();
        previousPosition = transform.position;
        GameManager.Instance.player = gameObject;
        LoadCharacterClasses();
        LoadRelicCatalog();
        GrantRelicByName("Swift Steel");
        EventBus.Instance.OnDamage += OnDamageEvent;
        EventBus.Instance.OnEnemyKilled += OnEnemyKilledEvent;
        EventBus.Instance.OnSpellCast += OnSpellCastEvent;
        EventBus.Instance.OnPlayerMove += OnPlayerMoveEvent;
        EventBus.Instance.OnWaveComplete += OnWaveCompleteEvent;
    }

    private void OnDestroy()
    {
        EventBus.Instance.OnDamage -= OnDamageEvent;
        EventBus.Instance.OnEnemyKilled -= OnEnemyKilledEvent;
        EventBus.Instance.OnSpellCast -= OnSpellCastEvent;
        EventBus.Instance.OnPlayerMove -= OnPlayerMoveEvent;
        EventBus.Instance.OnWaveComplete -= OnWaveCompleteEvent;
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

    private void LoadCharacterClasses()
    {
        characterClasses.Clear();

        TextAsset classFile = Resources.Load<TextAsset>("classes");
        if (classFile == null)
        {
            Debug.LogError("Could not find Assets/Resources/classes.json");
            return;
        }

        try
        {
            JObject root = JObject.Parse(classFile.text);
            foreach (JProperty property in root.Properties())
            {
                CharacterClassData classData = property.Value.ToObject<CharacterClassData>();
                if (classData != null)
                {
                    characterClasses[property.Name] = classData;
                }
            }

            if (!characterClasses.ContainsKey(selectedCharacterClass) && characterClasses.Count > 0)
            {
                selectedCharacterClass = characterClasses.Keys.First();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to parse classes.json: {e.Message}");
        }
    }

    public void SetCharacterClass(string className)
    {
        if (string.IsNullOrWhiteSpace(className))
        {
            return;
        }

        if (!characterClasses.ContainsKey(className))
        {
            Debug.LogWarning($"Unknown character class '{className}', keeping {selectedCharacterClass}.");
            return;
        }

        selectedCharacterClass = className;
    }

    private int EvaluateClassStat(string expression, int wave, int fallback)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return fallback;
        }

        Dictionary<string, int> vars = new Dictionary<string, int>()
        {
            { "wave", wave }
        };

        return Mathf.RoundToInt(RPNEvaluator.RPNEvaluator.Evaluatef(expression, vars));
    }

    public void RefreshRelicStats()
    {
        if (spellcaster != null)
        {
            spellcaster.RebuildSpells();
        }
    }

    public void AddSpellPowerBonus(int amount)
    {
        relicBonusSpellPower += amount;

        int basePower = 10;
        if (characterClasses.TryGetValue(selectedCharacterClass, out CharacterClassData classData))
        {
            basePower = EvaluateClassStat(classData.spellpower, GameManager.Instance.currentWave, 10);
        }

        spellcaster.spellPower = basePower + relicBonusSpellPower;
        spellcaster.RebuildSpells();
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

            RuntimeRelic runtimeRelic = RelicFactory.Create(relic);
            RelicManager.Instance.AddRelic(runtimeRelic);

            OnRelicGranted?.Invoke(relic);
            Debug.Log("Relic acquired: " + relic.name);
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
        RelicData relic = GetRelicAt(index);

        if (relic == null)
        {
            return false;
        }

        return activeTemporaryRelics.Contains(relic);
    }

    public void TriggerRelics(
        string triggerType,
        Damage damage = null,
        Hittable target = null
    )
    {
        foreach (RelicData relic in grantedRelics)
        {
            if (relic == null)
            {
                continue;
            }

            if (!string.Equals(
                relic.trigger?.type,
                triggerType,
                StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            ApplyRelicEffect(relic);
        }
    }

    private void ApplyRelicEffect(RelicData relic)
    {
        if (relic == null || relic.effect == null)
        {
            return;
        }

        string effectType =
            relic.effect.type?.ToLower();

        switch (effectType)
        {
            case "gain-mana":
            {
                int manaGain =
                    Mathf.RoundToInt(
                        RPNEvaluator.RPNEvaluator.Evaluatef(
                            relic.effect.amount ?? "0",
                            new Dictionary<string, int>()
                            {
                                { "wave", GameManager.Instance.currentWave }
                            }
                        )
                    );

                int oldMana = spellcaster.mana;

                spellcaster.mana =
                    Mathf.Min(
                        spellcaster.max_mana,
                        spellcaster.mana + manaGain
                    );

                Debug.Log(
                    "[RELIC ACTIVATED]" +
                    relic.name +
                    " restored " +
                    (spellcaster.mana - oldMana) +
                    " mana. " +
                    "Current Mana: " +
                    spellcaster.mana +
                    "/" +
                    spellcaster.max_mana
                );

                break;
            }

            case "gain-maxhp":
            {
                int prevMax_hp = hp.max_hp;

                int amount = Mathf.RoundToInt(
                        RPNEvaluator.RPNEvaluator.Evaluatef(
                            relic.effect.amount ?? "0",
                            new Dictionary<string, int>()
                        )
                );

                hp.max_hp += amount;
                hp.hp += amount;

                healthui.SetHealth(hp);

                Debug.Log(
                    "[RELIC ACTIVATED] " +
                    relic.name +
                    " grant +" +
                    amount +
                    " max HP."
                );

                Debug.Log(
                    "[PLAYER HP] Previous Max HP = " +
                    prevMax_hp +
                    " and Current Max HP = " +
                    hp.max_hp
                );

                break;
            }

            case "gain-spellpower":
            {
                if (activeTemporaryRelics.Contains(relic))
                {
                    return;
                }

                int amount =
                    Mathf.RoundToInt(
                        RPNEvaluator.RPNEvaluator.Evaluatef(
                            relic.effect.amount ?? "0",
                            new Dictionary<string, int>()
                            {
                                { "wave", GameManager.Instance.currentWave }
                            }
                        )
                    );

                Debug.Log(
                    "[RELIC ACTIVATED] " +
                    relic.name +
                    " | +" +
                    amount +
                    " spellpower"
                );

                relicBonusSpellPower += amount;

                spellcaster.spellPower += amount;

                spellcaster.RebuildSpells();

                Debug.Log(
                    "[SPELLPOWER] Current spellpower = " +
                    spellcaster.spellPower
                );

                if (spellcaster.ActiveSpell != null)
                {
                    Debug.Log(
                        "[SPELL DAMAGE] " +
                        spellcaster.ActiveSpell.GetName() +
                        " = " +
                        spellcaster.ActiveSpell.GetDamage()
                    );
                }

                // temporary relics
                if (!string.IsNullOrEmpty(relic.effect.until))
                {
                    activeTemporaryRelics.Add(relic);
                }

                break;
            }
        }
    }

    private void OnDamageEvent(Vector3 where, Damage damage, Hittable target)
    {
        if (spellcaster == null)
        {
            return;
        }

        // player took damage
        if (target != null && target.owner == gameObject)
        {
            TriggerRelics(
                "take-damage",
                damage,
                target
            );
        }

        // player dealt damage
        if (damage != null && damage.source == gameObject)
        {
            TriggerRelics("deal-damage");
        }
    }

    private void OnEnemyKilledEvent(GameObject enemy)
    {
        TriggerRelics("on-kill");
    }

    private void OnSpellCastEvent(Spell spell)
    {
        RemoveTemporaryRelics("cast-spell");
    }

    private void OnPlayerMoveEvent(Vector2 movement)
    {
        if (movement.sqrMagnitude > 0.01f)
        {
            RemoveTemporaryRelics("move");
        }
    }

    private void OnWaveCompleteEvent(int wave)
    {
        TriggerRelics("wave-complete");
    }

    public void StartLevel()
    {
        ApplyWaveScaling(1);
    }

    public void ApplyWaveScaling(int wave)
    {
        if (!characterClasses.TryGetValue(selectedCharacterClass, out CharacterClassData classData))
        {
            Debug.LogWarning($"Class '{selectedCharacterClass}' not loaded, falling back to mage formulas.");
            classData = new CharacterClassData
            {
                health = "95 wave 5 * +",
                mana = "90 wave 10 * +",
                mana_regeneration = "10 wave +",
                spellpower = "wave 10 *",
                speed = "5"
            };
        }

        // evaluate scaled stats according to selected class definitions.
        int maxHP = EvaluateClassStat(classData.health, wave, 100);
        int mana = EvaluateClassStat(classData.mana, wave, 100);
        int manaRegen = EvaluateClassStat(classData.mana_regeneration, wave, 10);
        int power = EvaluateClassStat(classData.spellpower, wave, 10);
        int moveSpeed = EvaluateClassStat(classData.speed, wave, 5);

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
            // hp.hp = hp.max_hp;
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
            spellcaster.spellPower = power + relicBonusSpellPower;

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
        HandleStandStillRelics();

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

    private void HandleStandStillRelics()
    {
        if (unit == null)
        {
            return;
        }

        if (unit.movement.sqrMagnitude < 0.01f)
        {
            stationaryTimer += Time.deltaTime;
        }
        else
        {
            stationaryTimer = 0f;

            RemoveTemporaryRelics("move");
        }

        foreach (RelicData relic in grantedRelics)
        {
            if (relic == null)
            {
                continue;
            }

            if (!string.Equals(
                relic.trigger?.type,
                "stand-still",
                StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            float required =
                float.Parse(relic.trigger.amount);

            if (stationaryTimer >= required
                && !activeTemporaryRelics.Contains(relic))
            {
                ApplyRelicEffect(relic);
            }
        }
    }

    private void RemoveTemporaryRelics(string untilType)
    {
        for (int i = activeTemporaryRelics.Count - 1; i >= 0; i--)
        {
            RelicData relic = activeTemporaryRelics[i];

            if (!string.Equals(
                relic.effect.until,
                untilType,
                StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (relic.effect.type == "gain-spellpower")
            {
                int amount =
                    Mathf.RoundToInt(
                        RPNEvaluator.RPNEvaluator.Evaluatef(
                            relic.effect.amount ?? "0",
                            new Dictionary<string, int>()
                            {
                                { "wave", GameManager.Instance.currentWave }
                            }
                        )
                    );

                relicBonusSpellPower -= amount;

                relicBonusSpellPower = Mathf.Max(0, relicBonusSpellPower);

                int basePower = 10;
                if (characterClasses.TryGetValue(selectedCharacterClass, out CharacterClassData classData))
                {
                    basePower = EvaluateClassStat(classData.spellpower, GameManager.Instance.currentWave, 10);
                }

                spellcaster.spellPower = basePower + relicBonusSpellPower;

                spellcaster.RebuildSpells();
            }

            activeTemporaryRelics.RemoveAt(i);
        }
    }

    void OnAttack(InputValue value)
    {
        if (GameManager.Instance.state == GameManager.GameState.PREGAME || GameManager.Instance.state == GameManager.GameState.GAMEOVER) return;
        Vector2 mouseScreen = Mouse.current.position.value;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0;
        StartCoroutine(spellcaster.Cast(transform.position, mouseWorld));
        EventBus.Instance.SpellCast(
            spellcaster.ActiveSpell
        );
        // RemoveTemporaryRelics("cast-spell");
    }

    void OnMove(InputValue value)
    {
        if (GameManager.Instance.state == GameManager.GameState.PREGAME || GameManager.Instance.state == GameManager.GameState.GAMEOVER) return;
        unit.movement = value.Get<Vector2>()*speed;
        EventBus.Instance.PlayerMove(
            unit.movement
        );
    }

    void Die()
    {
        Debug.Log("You Lost!");
        GameManager.Instance.state = GameManager.GameState.GAMEOVER;
    }

    // relic tester
    public void GrantRelicByName(string relicName)
    {
        RelicData relic = relicCatalog.Find(r => r.name == relicName);
        if (relic != null)
        {
            GrantRelic(relic);
        }
    }
}
