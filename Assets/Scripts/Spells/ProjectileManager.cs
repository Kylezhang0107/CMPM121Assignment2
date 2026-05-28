using UnityEngine;
using System;

public class ProjectileManager : MonoBehaviour
{
    public GameObject[] projectiles;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.projectileManager = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateProjectile(int which, string trajectory, Vector3 where, Vector3 direction, float speed, Action<Hittable, Vector3> onHit, bool piercing = false, Hittable.Team ownerTeam = Hittable.Team.PLAYER, float lifetime = -1f)
    {
        GameObject new_projectile = Instantiate(projectiles[which], where + direction.normalized*1.1f, Quaternion.Euler(0,0,Mathf.Atan2(direction.y, direction.x)*Mathf.Rad2Deg));
        ProjectileController controller = new_projectile.GetComponent<ProjectileController>();
        controller.movement = MakeMovement(trajectory, speed);
        controller.OnHit += onHit;
        controller.piercing = piercing;
        controller.ownerTeam = ownerTeam;
        if (lifetime > 0f)
        {
            controller.SetLifetime(lifetime);
        }
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
