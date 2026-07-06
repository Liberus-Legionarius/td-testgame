using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SupportHelliController : MonoBehaviour
{
    [SerializeField] int speed;
    [SerializeField] float reload;
    [SerializeField] int damage;
    [SerializeField] GameObject bulletParal;
    [SerializeField] GameObject bulletCons;
    [SerializeField] Transform[] gunsParal;
    [SerializeField] Transform[] gunsCons;
    [SerializeField] GameObject[] updates;
    [SerializeField] GameObject healing;
    [SerializeField] GameObject attacking;
    int baseDamage;
    int mode = 0;
    GameObject castle;
    LayerMask mask;
    AudioSource sound;
    
    public GameObject[] Updates { get { return updates; } }
    GameObject goal;
    Rigidbody2D rb;
    int[] ls;
    Coroutine use;
    int curGun = 0;
    float angle = 0;

    float timer = 0;

    private void Awake()
    {
        if (Updates[GameController.Upgrades[3]] != gameObject)
        {
            Instantiate(Updates[GameController.Upgrades[3]], transform.parent);
            Destroy(gameObject);
        }
        rb = GetComponent<Rigidbody2D>();
        sound = GetComponent<AudioSource>();
        GameController.SetVolume(sound, AudioType.Sound);
    }

    private void Start()
    {
        castle = GameObject.Find("Крепость");
        baseDamage = damage;
        mask = (1 << 6) | (1 << 10);
        ls = new int[] { 6, 10 };
    }

    private void FixedUpdate()
    {
        timer += Time.deltaTime;
        if (goal == null)
        {
            rb.linearVelocity = Vector2.zero;
            GetGoal();
        }
        else
        {
            if (Vector2.Distance(transform.position, goal.transform.position) >= 50)
            {
                MoveToGoal();
            }
            else
            {
                Vector2 offset = transform.position - goal.transform.position;
                angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
                MoveAround();
            }
            if (ExtraIf() || timer < reload)
            {
                return;
            }
            timer = 0;
            var blt = Instantiate(bulletCons, gunsCons[curGun]).GetComponent<BulletController>();
            blt.transform.localPosition = Vector2.zero;
            blt.transform.SetParent(transform.parent, true);
            Vector2 g = goal.transform.position - blt.transform.position;
            float anglelocal = Mathf.Atan2(g.y, g.x) * Mathf.Rad2Deg;
            blt.Init(damage, anglelocal, ls, goal, true);
            foreach (var gun in gunsParal)
            {
                blt = Instantiate(bulletParal, gun).GetComponent<BulletController>();
                blt.Init(damage, anglelocal, ls, goal, true);
                blt.transform.SetParent(transform.parent, true);
            }
            if (curGun == gunsCons.Length - 1)
            {
                curGun = 0;
            }
            else curGun++;
        }
    }
    

    void MoveToGoal()
    {
        Vector2 g = goal.transform.position - transform.position;
        float angle = Mathf.Atan2(g.y, g.x) * Mathf.Rad2Deg;
        rb.rotation = angle - 90;
        rb.linearVelocity = transform.up * speed;
    }
    void MoveAround()
    {
        Vector2 g = goal.transform.position - transform.position;
        float anglel = Mathf.Atan2(g.y, g.x) * Mathf.Rad2Deg;
        rb.rotation = anglel - 90;
        angle += speed * Time.deltaTime;
        float rad = angle * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * 50;
        transform.position = (Vector2)goal.transform.position + offset;
    }
   
    void GetGoal()
    {
        GameObject go = gameObject;
        if (mode == 0)
        {
            float lenght = 9999;
            foreach (var enemy in Physics2D.OverlapCircleAll(transform.position, 9999, mask).Select(e => e.gameObject))
            {
                if (enemy != null)
                {
                    float l = Vector2.Distance(castle.transform.position, enemy.transform.position);
                    if (lenght > l)
                    {
                        lenght = l;
                        go = enemy;
                    }
                }
            }
        }
        else if (mode == 1)
        {
            float percent = 1;
            foreach (var ally in Physics2D.OverlapCircleAll(transform.position, 9999, mask).Select(e => e.gameObject.GetComponent<DestroyableAbsController>()))
            {
                if (ally != null)
                {
                    float p = ally.HpPercent();
                    if (percent > p)
                    {
                        percent = p;
                        go = ally.gameObject;
                    }
                }
            }
        }
        if (go != gameObject)
            goal = go;
    }
    public void OnModeChange()
    {
        if (mode == 0)
        {
            mode = 1;
            mask = (1 << 8);
            ls = new int[] { 8 };
            damage = Mathf.RoundToInt(baseDamage * 0.5f);
            bulletCons = healing;
            bulletParal = healing;
        }
        else
        {
            mode = 0;
            mask = (1 << 6) | (1 << 10);
            ls = new int[] { 6, 10 };
            damage = baseDamage;
            bulletCons = attacking;
            bulletParal = attacking;
        }
        goal = null;
        if(use != null)
            StopCoroutine(use);
        use = null;
    }
    bool ExtraIf()
    {
        if (mode == 1 && goal.GetComponent<DestroyableAbsController>().HpLost() < 1)
            return true;
        if (Vector2.Distance(goal.transform.position, transform.position) > GetComponent<CircleCollider2D>().radius)
            return true;
        return false;
    }

    
}
