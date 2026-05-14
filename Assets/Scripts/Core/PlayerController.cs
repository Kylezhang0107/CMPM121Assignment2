using UnityEngine;
using UnityEngine.InputSystem;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public Hittable hp;
    public HealthBar healthui;
    public ManaBar manaui;

    public SpellCaster spellcaster;
    public SpellUI spellui;
    public SpellUIContainer spellUIContainer;

    public int speed;

    public Unit unit;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        unit = GetComponent<Unit>();
        GameManager.Instance.player = gameObject;
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
