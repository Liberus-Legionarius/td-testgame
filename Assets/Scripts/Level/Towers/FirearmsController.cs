using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class FirearmsController : BaseFirearms
{
    CastleController castle; 
    [SerializeField] Sprite sprite; 
    public Sprite Sprite { get { return sprite; } }
    [SerializeField] int cost; 
    public int Cost { get { return cost; } }
    [SerializeField] GameObject[] updated = new GameObject[] { };
    [SerializeField] int staticMode = 0;
    public int StaticMode { get { return staticMode; } }
    bool isSelected = false;
    DestroyableAbsController hp;
    public GameObject[] Updated { get { return updated; } }
    [SerializeField] GameObject specialBullet;
    [SerializeField] string[] title;
    [SerializeField] [Multiline] string[] descr;
    public string Descr { get => string.Format(descr[GameController.Language], string.Format(new string[] { "Броня: {0}\nУрон: {1}\nЧас перезаряджання: {2}\nРадiус: {3}\nЦiна: {4}", "Armor: {0}\nDamage: {1}\nReload: {2}\nRadius: {3}\nPrice: {4}", "Броня: {0}\nУрон: {1}\nВремя перезарядки: {2}\nРадиус: {3}\nЦена: {4}" }[GameController.Language], GetComponent<DestroyableAbsController>().MaxHP, damage, reload, GetComponent<CircleCollider2D>().radius, cost)); }
    public string Title { get => title[GameController.Language]; }

    protected override void Extra()
    {
        castle = CastleController.GetCastle();
        hp = GetComponent<DestroyableAbsController>();
        if (GameController.Upgrades[1] == 5 && GameController.Upgrades[3] == 5)
            damage += Mathf.RoundToInt(damage * 0.25f);
    }

    private void Update()
    {
        if (isDynamic)
        {
            if (goals.Count > 0)
            {
                curGoal = LevelController.FindGoal(goals, castle, canBeat);
            }
        }
        if (isSelected && Mouse.current.leftButton.isPressed)
        {
            StaticFeatures(GameObject.Find("Main Camera").GetComponent<Camera>().ScreenToWorldPoint(Mouse.current.position.ReadValue()));
        }

    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if(staticMode == 1)
        {
            if(timer >= reload)
            {
                timer = 0;
                StaticFeatures(transform.position);
            }
        }
    }

    public override void Fire(float angle, Vector2 pos, GameObject altBullet = null)
    {
        base.Fire(angle, pos);
        StartCoroutine(Firing());
    }

    private IEnumerator Firing()
    {
        foreach (var g in gun)
        {
            g.Translate(Vector2.down);
        }
        for (int i = 0; i < 10; i++)
        {
            foreach (var g in gun)
            {
                g.Translate(new Vector2(0, 0.1f));
            }
            yield return new WaitForSeconds(reload / 15f);
        }
        foreach (var g in gun)
        {
            g.localPosition = Vector3.zero;
        }
    }

    public void OnActivate()
    {
        if (staticMode < 2)
        {
            isSelected = !isSelected;
        }
        else StaticFeatures(transform.position);
    }
    public void StaticFeatures(Vector2 pos)
    {
        isSelected = false;
        switch (staticMode)
        {
            case 0:
                foreach (var g in gun)
                {
                    float angle = RotateObj(pos, g, -90);
                    Fire(angle, Vector2.zero, specialBullet);
                }
                break;
            case 1:
                sound.Play();
                foreach (var zombieObj in Physics2D.OverlapCircleAll(pos, GetComponent<CircleCollider2D>().radius))
                {
                    if (zombieObj.gameObject.layer == 6)
                    {
                        zombieObj.GetComponent<ZombieController>().HP -= damage;
                    }
                }
                break;
            case 2:
                foreach(var zombie in LevelController.GetLevel().Zombies.Select(z => z.GetComponent<ZombieController>()))
                {
                    float freeze = zombie.MaxHP > 500 ? 250 : zombie.MaxHP/2;
                    zombie.Freeze(5);
                }
                break;
        }
    }


}
