using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class BaseFirearms : MonoBehaviour
{
    [SerializeField] protected int damage; 
    [SerializeField] protected GameObject[] bullet;
    [SerializeField] protected int[] bulletChance;
    [SerializeField] protected float[] corrections = new float[] { 0 };
    [SerializeField] protected float[] angleCorrections = new float[] { 0 };
    [SerializeField] protected Transform[] gun;
    [SerializeField] protected bool isDynamic = true;
    [SerializeField] protected int[] canBeat;
    protected AudioSource sound;
    public bool IsDynamic { get { return isDynamic; } }
    protected float timer = 0; 
    public float Timer { get { return timer; } }
    [SerializeField] protected float reload; 
    public float Reload { get { return reload; } }
    protected List<Transform> goals = new List<Transform>(); 
    protected Transform curGoal; 

    private void Start()
    {
        Extra();
        reload = reload * GameController.DifficultyModifiers[GameController.Difficult, 4];
        damage = Mathf.RoundToInt(damage * GameController.DifficultyModifiers[GameController.Difficult, 5]);
        if (GameController.Upgrades[0] == 5 && GameController.Upgrades[2] == 5)
        {
            damage = Mathf.RoundToInt(damage * 0.75f);
            reload /= 2;
        }
        sound = GetComponent<AudioSource>();
        GameController.SetVolume(sound, AudioType.Firing);
    }
    protected virtual void Extra() { }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (canBeat.Contains(collision.gameObject.layer))
        {
            goals.Add(collision.gameObject.transform);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (canBeat.Contains(collision.gameObject.layer))
        {
            goals.Remove(collision.gameObject.transform);
            if (goals.Count > 0)
                curGoal = goals[0];
        }
    }


    protected virtual void FixedUpdate()
    {
        timer += Time.deltaTime;
        if (isDynamic)
        {
            if (curGoal == null) 
            {
                if (goals.Count > 0)
                    curGoal = goals[0];
            }
            if (curGoal != null) 
            {
                for (int i = 0; i < gun.Length; i++)
                {
                    float angle = RotateObj(curGoal.position, gun[i], -90);
                    if (timer >= reload)
                    {
                        if (i == gun.Length - 1)
                            timer = 0;
                        Fire(angle, Vector2.zero);
                    }
                }

            }
        }
    }

    public static float RotateObj(Vector3 pos, Transform obj, float correction = 0, bool apply = true)
    {
        Vector2 goal = pos - obj.position;
        float angle = Mathf.Atan2(goal.y, goal.x) * Mathf.Rad2Deg;
        if(apply) 
            obj.rotation = Quaternion.Euler(0, 0, angle + correction);
        return angle;
    }
    public virtual void Fire(float angle, Vector2 pos, GameObject altBullet = null)
    {
        for (int i = 0; i < corrections.Length; i++)
        {
            GameObject blt = Instantiate(altBullet == null ? BulletSelect() : altBullet, transform);
            blt.transform.Translate(new Vector2(corrections[i], 0));
            if (pos != Vector2.zero)
            {
                blt.transform.Translate(pos + new Vector2(UnityEngine.Random.Range(-5, 5), UnityEngine.Random.Range(-5, 5)));
            }

            BulletController bltc = blt.GetComponent<BulletController>();
            bltc.Init(damage * (altBullet == null ? 1 : 100), angle + angleCorrections[i], canBeat);

            sound.Play();
        }
    }
    protected GameObject BulletSelect()
    {
        if (bullet.Length == 1)
            return bullet[0];


        float rnd = UnityEngine.Random.Range(0, 101);
        GameObject b = null;
        for (int i = 0; i < bulletChance.Length; i++)
        {
            if (bulletChance[i] >= rnd)
            {
                b = bullet[i];
                break;
            }
        }
        if (b == null)
            b = bullet[^1];
        return b;
    }
}
