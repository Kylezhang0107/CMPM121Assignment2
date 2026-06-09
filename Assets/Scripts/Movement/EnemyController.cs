using UnityEngine;

public class EnemyController : MonoBehaviour
{

    public Transform target;
    public int speed;
    public Hittable hp;
    public HealthBar healthui;
    public bool dead;
    public int attackDamage = 5;
    private bool isPhasing = false;
    private float phaseTimer = 0f;
    private Vector3 phaseDirection;
    private int scaryPhase = 0;
    private float scaryTimer = 0f;
    private int dashPhase = 0;
    private float dashTimer = 0f;
    private Vector3 dashDirection;
    public string movementType = "chase";
    public string summonEnemy;
    public bool summonCheckedThisIdle;
    public string enemyType;
    public Damage.Type attackType = Damage.Type.PHYSICAL;

    public float last_attack;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        target = GameManager.Instance.player.transform;
        hp.OnDeath += Die;
        healthui.SetHealth(hp);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = target.position - transform.position;

        if (direction.magnitude < 2f)
        {
            DoAttack();
            return;
        }

        Unit unit = GetComponent<Unit>();

        switch (movementType.ToLower())
        {
            case "circle":
                MovementTypes.Circle(unit, direction, speed);
                break;

            case "dash":
                MovementTypes.Dash(unit, direction, speed, ref dashPhase, ref dashTimer, ref dashDirection);
                break;
            
            case "zigzag":
                MovementTypes.ZigZag(unit, direction, speed);
                break;

            case "phase":
                MovementTypes.Phase(unit, transform, GetComponent<SpriteRenderer>(), GetComponent<Collider2D>(), this, ref isPhasing, ref phaseTimer, ref phaseDirection, speed);
                break;

            case "scary":
                MovementTypes.Scary(unit, direction, speed, transform, GameManager.Instance.player.transform, GetComponent<SpriteRenderer>(), ref scaryPhase, ref scaryTimer, ref phaseDirection, ref isPhasing);
                break;

            default:
                MovementTypes.Chase(unit, direction, speed);
                break;
        }
    }

            
    void DoAttack()
    {
        if (last_attack + 2 < Time.time)
        {
            last_attack = Time.time;
            target.gameObject.GetComponent<PlayerController>().hp.Damage(new Damage(attackDamage, attackType));
            //Debug.Log(gameObject.name + " dealt " + attackDamage + " " + attackType + " damage");
        }
    }


    void Die()
    {
        if (!dead)
        {
            dead = true;
            if (enemyType == "leprachaun")
            {
                SkillTreeManager.Instance.AddSkillPoint();
            }
            GameManager.Instance.RemoveEnemy(gameObject);
            Destroy(gameObject);
        }
    }
}
