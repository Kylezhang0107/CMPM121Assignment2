using UnityEngine;
using UnityEngine.InputSystem;

public class SkillTreeTester : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        // Give skill point
        if (Keyboard.current.uKey.wasPressedThisFrame)
        {
            SkillTreeManager.Instance.AddSkillPoint();

            Debug.Log("Skill Point Added. Total = " + SkillTreeManager.Instance.skillPoints);
        }

        // Choose elemental path
        if (!SkillTreeManager.Instance.pathChosen)
        {
            if (Keyboard.current.iKey.wasPressedThisFrame)
            {
                bool success = SkillTreeManager.Instance.ChoosePath(ElementPath.Arcane);

                Debug.Log("Arcane selected = " + success);
            }

            if (Keyboard.current.oKey.wasPressedThisFrame)
            {
                bool success =
                    SkillTreeManager.Instance.ChoosePath(ElementPath.Ice);

                Debug.Log("Ice selected = " + success);
            }

            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                bool success = SkillTreeManager.Instance.ChoosePath(ElementPath.Fire);

                Debug.Log("Fire selected = " + success);
            }

            return;
        }

        // Arcane upgrades
        if (SkillTreeManager.Instance.currentPath == ElementPath.Arcane)
        {
            if (Keyboard.current.iKey.wasPressedThisFrame)
            {
                bool success = SkillTreeManager.Instance.UnlockSpellPower();

                Debug.Log("Spell Power = " + success + " | Level = " + SkillTreeManager.Instance.spellPowerLevels + " | Points = " + SkillTreeManager.Instance.skillPoints);
            }

            if (Keyboard.current.oKey.wasPressedThisFrame)
            {
                bool success = SkillTreeManager.Instance.UnlockSpellSpeed();

                Debug.Log("Spell Speed = " + success + " | Level = " + SkillTreeManager.Instance.spellSpeedLevels + " | Points = " + SkillTreeManager.Instance.skillPoints);
            }

            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                bool success = SkillTreeManager.Instance.UnlockHealChance();

                Debug.Log(
                    "Heal Chance = " +
                    success +
                    " | Level = " +
                    SkillTreeManager.Instance.healChanceLevels +
                    " | Points = " +
                    SkillTreeManager.Instance.skillPoints
                );
            }
        }

        // Ice upgrades
        else if (SkillTreeManager.Instance.currentPath == ElementPath.Ice)
        {
            if (Keyboard.current.iKey.wasPressedThisFrame)
            {
                Debug.Log("Ice Upgrade 1");
            }

            if (Keyboard.current.oKey.wasPressedThisFrame)
            {
                Debug.Log("Ice Upgrade 2");
            }

            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                Debug.Log("Ice Upgrade 3");
            }
        }

        // Fire upgrades
        else if (SkillTreeManager.Instance.currentPath == ElementPath.Fire)
        {
            if (Keyboard.current.iKey.wasPressedThisFrame)
            {
                Debug.Log("Fire Upgrade 1");
            }

            if (Keyboard.current.oKey.wasPressedThisFrame)
            {
                Debug.Log("Fire Upgrade 2");
            }

            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                Debug.Log("Fire Upgrade 3");
            }
        }
    }
}