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

        if (Keyboard.current.uKey.wasPressedThisFrame)
        {
            SkillTreeManager.Instance.AddSkillPoint();

            Debug.Log("Skill Point Added. Total = " + SkillTreeManager.Instance.skillPoints);
        }

        if (!SkillTreeManager.Instance.pathChosen)
        {
            if (Keyboard.current.iKey.wasPressedThisFrame)
            {
                SkillTreeManager.Instance.ChoosePath(ElementPath.Arcane);
                Debug.Log(SkillTreeManager.Instance.GetDamageType());
            }

            if (Keyboard.current.oKey.wasPressedThisFrame)
            {
                SkillTreeManager.Instance.ChoosePath(ElementPath.Ice);
                Debug.Log(SkillTreeManager.Instance.GetDamageType());
            }

            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                SkillTreeManager.Instance.ChoosePath(ElementPath.Fire);
                Debug.Log(SkillTreeManager.Instance.GetDamageType());
            }
        }

        if (SkillTreeManager.Instance.pathChosen)
        {
            if (Keyboard.current.iKey.wasPressedThisFrame)
            {
                bool success = SkillTreeManager.Instance.UnlockSpellPower();

                Debug.Log(
                    "Arcane Power purchased = " +
                    success +
                    " | Level = " +
                    SkillTreeManager.Instance.spellPowerLevels +
                    " | Points = " +
                    SkillTreeManager.Instance.skillPoints
                );
            }

            if (Keyboard.current.oKey.wasPressedThisFrame)
            {
                Debug.Log("O pressed");
            }

            if (Keyboard.current.pKey.wasPressedThisFrame)
            {
                Debug.Log("P pressed");
            }
        }
    }
}