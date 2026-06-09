using UnityEngine;
using System;

public class ProjectileManager : MonoBehaviour
{
    public GameObject[] projectiles;
    public GameObject spellLightPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.projectileManager = this;
        spellLightPrefab = Resources.Load<GameObject>("SpellLight");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateProjectile(int which, string trajectory, Vector3 where, Vector3 direction, float speed, Action<Hittable, Vector3> onHit, bool piercing = false, Hittable.Team ownerTeam = Hittable.Team.PLAYER, Color? tint = null)
    {
        GameObject new_projectile = Instantiate(projectiles[which], where + direction.normalized * 1.1f, Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg));
        ProjectileController controller = new_projectile.GetComponent<ProjectileController>();

        if (spellLightPrefab != null)
        {
            Debug.Log("Creating spell light");
            Instantiate(spellLightPrefab, new_projectile.transform);
        }
        else
        {
            Debug.LogError("spellLightPrefab is NULL");
        }

        if (tint.HasValue)
        {
            SpriteRenderer sr = new_projectile.GetComponent<SpriteRenderer>();

            if (sr != null)
            {
                sr.color = tint.Value;
            }
        }

        controller.movement = MakeMovement(trajectory, speed);
        controller.OnHit += onHit;
        controller.piercing = piercing;
        controller.ownerTeam = ownerTeam;
    }

    public void CreateProjectile(int which, string trajectory, Vector3 where, Vector3 direction, float speed, Action<Hittable, Vector3> onHit, float lifetime, bool piercing = false, Hittable.Team ownerTeam = Hittable.Team.PLAYER, Color? tint = null)
    {
        GameObject new_projectile = Instantiate(projectiles[which], where + direction.normalized * 1.1f, Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg));
        ProjectileController controller = new_projectile.GetComponent<ProjectileController>();

        if (spellLightPrefab != null)
        {
            Debug.Log("Creating spell light");
            Instantiate(spellLightPrefab, new_projectile.transform);
        }
        else
        {
            Debug.LogError("spellLightPrefab is NULL");
        }

        if (tint.HasValue)
        {
            SpriteRenderer sr = new_projectile.GetComponent<SpriteRenderer>();

            if (sr != null)
            {
                sr.color = tint.Value;
            }
        }

        controller.movement = MakeMovement(trajectory, speed);
        controller.OnHit += onHit;
        controller.piercing = piercing;
        controller.ownerTeam = ownerTeam;
        controller.SetLifetime(lifetime);
    }

    public ProjectileMovement MakeMovement(string name, float speed)
    {
        if (name == "straight")
        {
            return new StraightProjectileMovement(speed);
        }
        if (name == "homing")
        {
            return new HomingProjectileMovement(speed);
        }
        if (name == "spiraling")
        {
            return new SpiralingProjectileMovement(speed);
        }
        return null;
    }

}
