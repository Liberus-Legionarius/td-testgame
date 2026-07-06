using System.Collections;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.Rendering.DebugUI;

public class ZombieController : MoveEntityController
{
    [SerializeField] int xp;
    [SerializeField] int money; 
    [SerializeField] int damage; 
    [SerializeField] bool armored; 
    [SerializeField] float delay;
    [SerializeField] float defense = 1;
    public float Defense { get { return defense; } }
    [SerializeField] GameObject hpImg;
    [SerializeField] Color frozen;
    [SerializeField] Color frozenDmg;
    [SerializeField] Color frozenHld;
    [SerializeField] Color firing;
    [SerializeField] Color firingDmg;
    [SerializeField] GameObject iced;
    [SerializeField] bool hasDayDamage = false;
    public bool HasDayDamage { get { return hasDayDamage; } }
    [SerializeField] int dayDamage;
    [SerializeField] float dayDamageDelay;
    [SerializeField][Multiline] string[] descr;
    public string Descr { get => string.Format(descr[GameController.Language], new string[] { $"Здоров'я: {MaxHP}\nУрон: {damage}\nШвидкiсть: {GetSpeed()}\n\nОчки досвiду: {xp}\nМонети: {money}", $"HP: {MaxHP}\nDamage: {damage}\nSpeed: {GetSpeed()}\n\nXP: {xp}\nMoney: {money}", $"Здоровье: {MaxHP}\nУрон: {damage}\nСкорость: {GetSpeed()}\n\nОчки опыта: {xp}\nМонеты: {money}" }[GameController.Language]); }
    string GetSpeed()
    {
        if(speed < 15)
        {
            return new string[] { "Повiльна", "Slow" }[GameController.Language];
        }
        else if(speed < 20)
        {
            return new string[] { "Сповiльнена", "Semi-Slow" }[GameController.Language];
        }
        else if( speed < 22)
        {
            return new string[] { "Нормальна", "Normal" }[GameController.Language];
        }
        else if(speed < 26)
        {
            return new string[] { "Прискорена", "Semi-Fast" }[GameController.Language];
        }
        else
        {
            return new string[] { "Швидка", "Fast" }[GameController.Language];
        }
    }
    [SerializeField] string[] title;
    public string Title { get { return title[GameController.Language]; } }
    bool underRadiation = false;
    public bool UnderRadiation { get => underRadiation; set => underRadiation = value; }
    float radiationTimer = 0;
    bool isDay;
    float dayTimer;
    Coroutine fire = null;
    int baseSpeed;
    float firingTime = 0;
    public Sprite Sprite
    {
        get => gameObject.GetComponent<SpriteRenderer>().sprite;
    }
    public float FiringTime
    {
        get { return firingTime; }
        set
        {
            if (value <= 0)
            {
                firingTime = 0;
                curFireDmg = 0;
            }
            else
            {
                firingTime = value;
            }
        }
    }
    float curFireDmg = 0;
    float frozenProgress = 0;
    public float Frozen
    {
        get => frozenProgress;
        set
        {
            if (value <= 0)
            {
                var ice = Instantiate(iced, transform.parent);
                ice.transform.position = transform.position;
                ice.transform.rotation = transform.rotation;
                var a = ice.GetComponent<DestroyableAbsController>();
                a.Init(MaxHP, true);
                a.HP -= HpLost();
                Death();
            }
            else if (value >= MaxHP)
            {
                frozenProgress = MaxHP;
                SetColor(transparent);
            }
            else
            {
                frozenProgress = value;
            }
            speed = Mathf.RoundToInt(baseSpeed * (frozenProgress / MaxHP));
        }
    }
    public bool Armored { get { return armored; } }

    private DestroyableAbsController goal; 
    private LevelController level;

    protected override void Awake()
    {
        level = LevelController.GetLevel();
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
        speed = Mathf.RoundToInt(speed * GameController.DifficultyModifiers[GameController.Difficult, 3]);
        baseSpeed = speed;
        frozenProgress = MaxHP;
        xp = Mathf.RoundToInt(xp * GameController.DifficultyModifiers[GameController.Difficult, 0]);
        damage = Mathf.RoundToInt(damage * GameController.DifficultyModifiers[GameController.Difficult, 1]);
    }
    void FixedUpdate()
    {
        if (goal != null && goal.HP > 0 && timer >= delay && delay > 0) 
        {
            if (goal is CastleController)
            {
                level.XP = Mathf.FloorToInt((float)-damage / 4);
            }
            goal.HP -= damage;
            timer = 0;
        }
        else if (delay <= 0)
        {
            BaseFirearms.RotateObj(LevelController.GetLevel().GetGoal().position, transform);
        }
        if (Frozen < MaxHP)
        {
            Frozen += Time.deltaTime;
        }
        if (isDay && hasDayDamage)
        {
            dayTimer += Time.deltaTime;
            if (dayTimer >= dayDamageDelay)
            {
                dayTimer = 0;
                HP -= dayDamage;
            }
        }
        if (underRadiation)
        {
            radiationTimer += Time.deltaTime;
            if (radiationTimer >= 5)
            {
                HP -= 15;
                radiationTimer = 0;
            }
        }
    }
    protected override void Update()
    {
        base.Update();
        if (isHealed)
        {
            isDamaged = false;
            if (frozenProgress < MaxHP)
            {
                StartCoroutine(ColorOff(a => isHealed = a, frozenHld, frozen));
            }
            else StartCoroutine(ColorOff(a => isHealed = a, frozenHld, transparent));
        }
        if (isDamaged)
        {
            isHealed = false;
            if (frozenProgress < MaxHP)
            {
                StartCoroutine(ColorOff(a => isDamaged = a, frozenDmg, frozen));
            }
            else if (FiringTime > 0)
            {
                StartCoroutine(ColorOff(a => isDamaged = a, firingDmg, firing));
            }
            else StartCoroutine(ColorOff(a => isDamaged = a, damaged, transparent));

        }
    }
    public void Init(Transform[] path, int[] angles, Vector2 pos, bool isday)
    {
        base.Init(path, angles, pos);
        isDay = isday;
        if (hasDayDamage && isDay)
            dayTimer = 0;
    }

    protected override void ExtraDamage(int gained)
    {
        if (level != null)
            level.AllDamage += gained;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 8 )
        {
            goal = collision.gameObject.GetComponent<DestroyableAbsController>() != null ? collision.gameObject.GetComponent<DestroyableAbsController>() : goal;
            if (gameObject.layer == 10 && goal != null)
            {
                goal.HP -= damage;
                if(level.Zombies.Contains(gameObject))
                    level.Zombies.Remove(gameObject);
                GameController.AllDamage += Mathf.RoundToInt(HpLost());
                Destroy(gameObject);
            }
        }
    }

    public override void Death()
    {
        try
        {
            level.Money += money;
            level.XP = xp;
            level.Zombies.Remove(gameObject);
            level.AllKills++;
        }
        catch
        {
            level = LevelController.GetLevel();
            level.Money += money;
            level.XP = xp;
            level.Zombies.Remove(gameObject);
            level.AllKills++;
        }
        base.Death();
    }
    public void Firing(float time, float dmg, float delay)
    {
        FiringTime += time;
        curFireDmg = dmg;
        if (fire == null)
            fire = StartCoroutine(Firing(delay));
    }

    IEnumerator Firing(float delay)
    {
        while (FiringTime > 0)
        {
            HP -= curFireDmg * (hasDayDamage ? 2 : 1);
            FiringTime -= delay;
            yield return new WaitForSeconds(delay);
        }
        SetColor(transparent);
        fire = null;
    }
    public void Freeze(float time)
    {
        StartCoroutine(Freezing(time));
    }
    IEnumerator Freezing(float time)
    {
        speed = 0;
        yield return new WaitForSeconds(time);
        speed = baseSpeed;
    }
}
