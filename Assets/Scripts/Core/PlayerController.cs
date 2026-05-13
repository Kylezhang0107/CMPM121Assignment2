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
        spellcaster = new SpellCaster(125, 8, Hittable.Team.PLAYER);
        StartCoroutine(spellcaster.ManaRegeneration());
        
        hp = new Hittable(100, Hittable.Team.PLAYER, gameObject);
        hp.OnDeath += Die;
        hp.team = Hittable.Team.PLAYER;

        // tell UI elements what to show
        healthui.SetHealth(hp);
        manaui.SetSpellCaster(spellcaster);

        if (spellUIContainer == null)
        {
            spellUIContainer = FindFirstObjectByType<SpellUIContainer>();
        }

        if (spellUIContainer != null)
        {
            spellUIContainer.SetSpellCaster(spellcaster);
        }
        else if (spellui != null)
        {
            spellui.SetSpell(spellcaster.ActiveSpell);
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
