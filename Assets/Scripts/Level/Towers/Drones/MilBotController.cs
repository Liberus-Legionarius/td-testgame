using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class MilBotController : DestroyableAbsController
{
    [SerializeField] int[] canBeat = new int[] { 6 };
    [SerializeField] int speed;
    [SerializeField] int damage;
    [SerializeField] float reload;
    [SerializeField] bool isGunner;
    [SerializeField] GameObject updated;
    public GameObject Updated {  get { return updated; } }
    Rigidbody2D rb;
    Transform curGoal;
    List<Transform> goals = new List<Transform>();
    CastleController castle;
    float timer = 0;
    override protected void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        castle = CastleController.GetCastle();
    }

    private void FixedUpdate()
    {
        timer += Time.deltaTime;
        if (goals.Count > 0)
        {
            curGoal = LevelController.FindGoal(goals, castle, canBeat);
        }
        if (curGoal != null)
        {
            BaseFirearms.RotateObj(curGoal.position, transform, -90);
            if((isGunner && Vector2.Distance(transform.position, curGoal.position) > 50) || !isGunner)
            {
                rb.linearVelocity = transform.up * speed;
            }
            else if(isGunner && Vector2.Distance(transform.position, curGoal.position) < 50)
            {
                rb.linearVelocity = transform.up * -speed;
            }
        }
        else rb.linearVelocity = Vector2.zero;
    }

    void InitTurret()
    {
        var tur = gameObject.GetComponentsInChildren<CastleTurretController>();
        if (tur.Length > 0)
        {
            foreach(var t in tur)
            {
                t.Init(damage, reload);
            }
        }
    }
    public override void Init(float? maxhp = null, bool suicide = false)
    {
        base.Init(MaxHP + 25 * GameController.Upgrades[3]);
        damage += 5 * GameController.Upgrades[3];
        reload -= 0.05f * GameController.Upgrades[3];
        speed += 2 * GameController.Upgrades[3];
        InitTurret();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(canBeat.Contains(collision.gameObject.layer) && !goals.Contains(collision.transform))
        {
            goals.Add(collision.transform);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (canBeat.Contains(collision.gameObject.layer) && goals.Contains(collision.transform))
        {
            goals.Remove(collision.transform);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(canBeat.Contains(collision.gameObject.layer) && goals.Contains(collision.transform))
        {
            curGoal = collision.transform;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (canBeat.Contains(collision.gameObject.layer) && goals.Contains(collision.transform))
        {
            var zomb = collision.gameObject.GetComponent<ZombieController>();
            if(timer >= reload)
            {
                timer = 0;
                zomb.HP -= damage;
                HP -= damage/2;
            }
        }
    }
}
