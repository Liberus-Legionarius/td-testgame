using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class LevelController : MonoBehaviour
{
    [SerializeField] int startMoney; 
    [SerializeField] GameObject[] paths; 
    [SerializeField] GameObject[] flyingPaths;
    [SerializeField] string[] angles; 
    [SerializeField] List<int> enemies; 
    [SerializeField] List<int> enemiesWave; 
    [SerializeField] List<int> enemiesFlying; 
    [SerializeField] int flyingChance = 0;
    [SerializeField] int hordeSize;
    [SerializeField] int killsToWave; 
    [SerializeField] int killsToWaveInc;
    [SerializeField] protected float[] delay;
    [SerializeField] protected bool nowDay;
    [SerializeField] int maxTower = 5;
    [SerializeField] protected UILevelController ui;
    [SerializeField] protected AudioSource music;
    [SerializeField] AudioSource environment;
    [SerializeField] AudioSource sound;
    [SerializeField] AudioClip hordeSummoning;
    bool goodbying = false;

    protected Coroutine horde;
    public int AllDamage { get; set; }
    public int AllKills { get; set; }
    public float AllMoneyGained { get; set; }
    public float AllMoneySpend { get; set; }

    int[][] realAngles;
    Transform[][] pathPoints;
    Transform[][] flyingPoints;
    float timer = 0; 
    bool continues = true; 
    protected bool canSummon = true; 
    public bool CanSummon { get { return canSummon; } set { canSummon = value; music.Play(); } }

    CastleController castle;

    float bonusMoney = 0;

    private float money = 0; 
    public float Money
    {
        get => money;
        set
        {
            if (money < value)
                AllMoneyGained += value - money;
            else if(money > value)
                AllMoneySpend += money - value;
                money = value > 0 ? value : 0;
            ui.Money = money;

        }
    }

    private int xp = 0; 
    public int XP
    {
        get => xp;
        set
        {
            if (value > 0)
                xp += Mathf.RoundToInt(value * castle.HpPercent());
            else xp = Mathf.RoundToInt(Mathf.Clamp(xp + value, 0, float.PositiveInfinity));
            ui.XP = xp;
        }
    }

    

    private int count = 0; 
    public int Count
    {
        get => count;
        set
        {
            count = value;
            ui.FillAmount = (float)value / (WaveCount == delay.Length ? hordeSize : killsToWave);
        }
    }

    private int waveCount = 0; 
    public int WaveCount
    {
        get => waveCount;
        set
        {
            waveCount = value;
            ui.Wave = value == 0 ? (GameController.Language == 0? "Наближається Орда" : GameController.Language == 1? "Horde is Approaching" : "") : $"{( GameController.Language == 0? $"Орда {value} з" : GameController.Language == 1? $"Horde {value} of" : $"")} {delay.Length}";
            Count = 0;
        }
    }

    List<GameObject> zombies = new List<GameObject>(); 
    public List<GameObject> Zombies { get { return zombies; } }

    protected virtual void Start()
    {
        for (int i = 0; i < delay.Length; i++)
        {
            delay[i] = delay[i] * GameController.DifficultyModifiers[GameController.Difficult, 6];
        }
        List<int> enemiesAll = enemies.Concat(enemiesFlying).Concat(enemiesWave).ToList();
        foreach(int e in enemiesAll)
        {
            GameController.EnemiesStatus[e] = 1;
        }
        GameController.EnemiesStatus[5] = 1;
        var background = GetComponent<SpriteRenderer>();
        Camera cam = Camera.main;
        cam.orthographicSize = Mathf.Max(background.bounds.size.x/ (2 * cam.aspect), background.bounds.size.y/2);
        canSummon = ui.GreatingsEnded();
        ui.SetTowers(GameController.Towers, maxTower);
        Money = startMoney;
        SetPaths(ref pathPoints, paths);
        SetPaths(ref flyingPoints, flyingPaths);
        SetAngles(ref realAngles, angles, paths.Length);
        WaveCount = 0;
        
        bonusMoney = GameController.Upgrades[0] * 2.5f;
        if (GameController.Upgrades[0] > 0)
        {
            ui.SetExtraMoney(bonusMoney);
            StartCoroutine(ExtraMoney());
        }
        castle = CastleController.GetCastle();

        GameController.LoadVolume();
    }

    protected void SetAngles(ref int[][] real, string[] anglesStr, int length)
    {
        real = new int[length][];
        for (int i = 0; i < anglesStr.Length; i++)
        {
            real[i] = anglesStr[i].Split(';').Select(x => int.Parse(x)).ToArray();
        }
    }
    protected void SetPaths(ref Transform[][] real, GameObject[] paths)
    {
        real = new Transform[paths.Length][];
        for (int i = 0; i < paths.Length; i++)
        {
            Transform[] pos = paths[i].GetComponentsInChildren<Transform>();
            real[i] = pos;
        }
    }
    void FixedUpdate()
    {
        timer += Time.deltaTime;
        if (Count < killsToWave && WaveCount < delay.Length && canSummon) 
            Summon();
        else if (delay.Length > WaveCount && canSummon)
        {
            WaveCount++;
            killsToWave += WaveCount == delay.Length ? -killsToWave : killsToWaveInc;
            SummonWave();
        }
        else if (zombies.Count == 0 && delay.Length == WaveCount && horde == null && continues && !goodbying) 
        {
            goodbying = ui.GoodBye();
            if (!goodbying)
                continues = false;
        }
    }

    public bool IsWin()
    {
        if(zombies.Count == 0) return true;
        else return false;
    }

    private void Summon()
    {
        if (timer >= delay[WaveCount] + UnityEngine.Random.Range(-(delay[WaveCount] / 4), delay[WaveCount] / 4))
        {
            timer = 0;
            if(UnityEngine.Random.Range(0,101) < flyingChance)
            {
                Summoning(enemiesFlying, true);
            }
            else Summoning(enemies);
            Count++;
        }
    }

    void SummonWave()
    {
        horde = StartCoroutine(HordeSummoning());
    }
    protected IEnumerator HordeSummoning(bool repeat = false)
    {
        sound.PlayOneShot(hordeSummoning);
        do
        {
            if(flyingChance < 100)
                Summoning(new List<int> { 5 });
            for (int i = 1; i < hordeSize; i++)
            {
                if (UnityEngine.Random.Range(0, 101) < flyingChance)
                {
                    Summoning(enemiesFlying, true);
                }
                else Summoning(enemiesWave);
                yield return new WaitForSeconds(delay[WaveCount - 1] / 5);
            }
            hordeSize += hordeSize / 4;
            yield return new WaitForSeconds(2.5f);
            if(repeat)
                WaveCount++;
        } while (repeat && WaveCount < delay.Length);
        yield return new WaitForSeconds(7.5f);
        horde = null;
        ui.Wave = waveCount < delay.Length ? (GameController.Language == 0 ? "Наближається Орда" : GameController.Language == 1 ? "Horde is Approaching" : "") : (GameController.Language == 0 ? "Остання Орда!" : GameController.Language == 1 ? "The Last Horde!" : "");
    }

    protected virtual void Summoning(List<int> enemies, bool isFly = false)
    {
        System.Random rand = new System.Random();
        var pp = isFly ? flyingPoints : pathPoints;
        if (isFly)
        {
            for (int i = 0; i < pp.Length; i++)
            {
                pp[i] = new Transform[] { pp[i][0], pp[i][1], GetGoal() };
            }
        }
        int way = rand.Next(pp.Length);
        var zombie = Instantiate(GameController.Enemies[enemies[rand.Next(enemies.Count)]], pp[way][1]);
        ZombieController controller = zombie.GetComponent<ZombieController>();
        controller.Init(pp[way], isFly ? new int[] { 0, 0 } : realAngles[way], GetRandomizedLocation(controller.gameObject.transform, 8 / paths[0].transform.localScale.x), nowDay);
        zombies.Add(zombie);
    }


    public static Vector2 GetRandomizedLocation(Transform trans, float randomize = 6)
    {
        return new Vector2(trans.localPosition.x + UnityEngine.Random.value * (UnityEngine.Random.value > 0.5f ? randomize : -randomize), trans.localPosition.y + UnityEngine.Random.value * (UnityEngine.Random.value > 0.5f ? randomize : -randomize));
    }

    

    public GameObject PlaceWeapon(Transform cell, GameObject selected = null)
    {
        if (selected == null)
            selected = ui.SelectedWeapon;
        Money -= selected.GetComponentInChildren<FirearmsController>().Cost * (1 - GameController.Upgrades[2] * 0.05f);
        GameController.MoneySpend += selected.GetComponentInChildren<FirearmsController>().Cost;
        if (selected == ui.SelectedWeapon)
        {
            ui.UnCheckWeap();
        }
        ui.SelectedWeapon = null;
        return Instantiate(selected, cell);
    }

    
    public void DoBoom(Vector2 pos, GameObject effect, float size, GameObject mark, AudioClip sound)
    {
        this.sound.PlayOneShot(sound);
        var b = Instantiate(effect);
        b.transform.position = pos;
        b.transform.localScale = new Vector2(size, size);
        if(mark != null)
        {
            var b1 = Instantiate(mark, transform);
            b1.transform.position = pos;
        }

    }
    public static LevelController GetLevel()
    {
        return GameObject.Find("Level").GetComponent<LevelController>();
    }

    public AudioSource GetSound()
    {
        return sound;
    }

    public UILevelController GetUI()
    {
        return ui;
    }
    public static Transform FindGoal(List<Transform> goals, CastleController castle, int[] canBeat)
    {

        Transform nextGoal = null;
        Vector2 toDef = castle.transform.position;
        float l = 99999999;
        Transform g = null;
        if (canBeat.Contains(10) && goals.Any(x => x.gameObject.layer == 10))
        {
            foreach (var goal in goals)
            {
                if (goal != null && goal.gameObject.layer == 10)
                {
                    if (Vector2.Distance(goal.position, toDef) < l)
                    {
                        l = Vector2.Distance(goal.position, toDef);
                        g = goal;
                    }
                }
            }
        }
        else
        {
            foreach (var goal in goals)
            {
                if (goal != null)
                {
                    if (Vector2.Distance(goal.position, toDef) < l)
                    {
                        l = Vector2.Distance(goal.position, toDef);
                        g = goal;
                    }
                }
            }
        }
        nextGoal = g;
        return nextGoal;
    }
    public GameObject DangerestEnemy()
    {
        GameObject zombie = null;

        float hp = -1;
        foreach(var zomb in zombies.Select(x => x.GetComponent<DestroyableAbsController>()))
        {
            if(hp < zomb.HP)
            {
                hp = zomb.HP;
                zombie = zomb.gameObject;
            }
        }
        return zombie;
    }
    public virtual Transform GetGoal()
    {
        return castle.transform;
    }
    IEnumerator ExtraMoney()
    {
        while(true)
        {
            if(ui.GreatingsEnded())
            {
                if (GameController.Upgrades[0] == 5)
                {
                    float f = UnityEngine.Random.value;
                    if (f <= 0.12f)
                    {
                        Money += 50;
                        ui.SetExtraMoney(50);
                    }
                    else if (f <= 0.5f)
                    {
                        Money += 25;
                        ui.SetExtraMoney(25);
                    }
                    else
                    {
                        Money += bonusMoney;
                        ui.SetExtraMoney(bonusMoney);
                    }
                }
                else Money += bonusMoney;
            }
            if(ui.LvlEnded)
                break;
            yield return new WaitForSeconds(1);
        }
    }
}
