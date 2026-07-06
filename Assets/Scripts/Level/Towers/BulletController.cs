using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    [SerializeField] float speed = 100;
    [SerializeField] protected float penetration = 0.5f;
    [SerializeField] float fleshBonus = 1;
    [SerializeField] bool isFrozen = false;
    [SerializeField] int frozenEffect = 0;
    [SerializeField] float lengthDmg;
    [SerializeField] int fireDamage;
    [SerializeField] float period;
    [SerializeField] bool isUran;
    protected int[] canBeat = new int[] { 6 };
    protected int damage; 
    Rigidbody2D rb; 
    float timer = 0; 
    protected GameObject target;
    bool hasTarget = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void FixedUpdate()
    {
        timer += Time.deltaTime;
        rb.linearVelocity = transform.up * speed;
        if (timer > 5 && speed > 0 && target == null && !hasTarget)
        {
            Destroy(gameObject);
        }
        else if(target == null && hasTarget)
        {
            target = LevelController.GetLevel().DangerestEnemy();
            if(target == null)
                Destroy(gameObject);
        }
        if (target != null)
        {
            BaseFirearms.RotateObj(target.transform.position, transform, -90);
            if (Vector2.Distance(target.transform.position, transform.position) <= 5)
            {
                Attack();
            }
        }
    }

    protected virtual void Attack()
    {
        DamageZombie(target, damage);
        Destroy(gameObject);
    }

    public void Init(int dmg, float angle, int[] beat, GameObject t = null, bool oneTimeAim = false, int spd = -1)
    {
        rb.rotation = angle - 90;
        damage = dmg;
        canBeat = beat;
        target = t;
        if(target != null && !oneTimeAim)
            hasTarget = true;
        else if(target != null && oneTimeAim)
        {
            BaseFirearms.RotateObj(target.transform.position, transform, -90);
            target = null;
        }
        if (spd != -1)
            speed = spd;
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (canBeat.Contains(collision.gameObject.layer) && gameObject.layer != 13 && target == null)
        {
            DamageZombie(collision.gameObject, damage);
            Destroy(gameObject);
        }
        else if(collision.gameObject.layer == 8 && gameObject.layer == 13)
        {
            collision.gameObject.GetComponent<DestroyableAbsController>().HP += damage;
        }
        else if(target != null && collision.transform == target)
        {
            DamageZombie(collision.gameObject, damage);
            Destroy(gameObject);
        }
    }

    protected virtual void DamageZombie(GameObject zombieObj, int dmg, float penCoef = 1)
    {
        var zombie = zombieObj.GetComponent<ZombieController>();
        zombie.HP -= Mathf.RoundToInt(dmg * zombie.Defense * (zombie.Armored ? penetration * penCoef : fleshBonus) * (zombie.HasDayDamage && fireDamage > 0 ? 2 : 1));
        if (isFrozen)
        {
            zombie.Frozen -= frozenEffect;
        }
        if(fireDamage > 0)
        {
            zombie.Firing(lengthDmg, fireDamage, period);
        }
        if(isUran)
        {
            zombie.UnderRadiation = isUran;
        }
    }
}
