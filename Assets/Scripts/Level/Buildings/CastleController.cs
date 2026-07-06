using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CastleController : DestroyableAbsController
{
    [SerializeField] LevelController level;
    [SerializeField] GameObject kamikadze;
    [SerializeField] GameObject militiaBot;
    [SerializeField] GameObject helicopter;
    [SerializeField] Transform[] summonPoint;
    [SerializeField] Button[] activeSkills;
    [SerializeField] GameObject[] turretsUpd;
    [SerializeField] Transform[] turretsCells;
    GameObject inf;
    int quantityModifier=  1;
    float discont;
    Coroutine damagedCor;

    protected override void Start()
    {
        base.Start();
        GetComponentInChildren<Canvas>().transform.rotation = Quaternion.identity;
        discont = 1-GameController.Upgrades[2] * 0.05f;
        int hpb = 0;
        foreach(int i in GameController.Upgrades)
        {
            hpb += i * 50;
        }
        Init(MaxHP * GameController.DifficultyModifiers[GameController.Difficult, 2] + hpb);
        switch(GameController.Upgrades[3])
        {
            case 0:
            case 1:
            case 2:
                quantityModifier = 1;
                break;
            case 3:
            case 4:
                quantityModifier = 2;
                break;
            case 5:
                quantityModifier = 3;
                break;
        }
        foreach(var tur in turretsCells)
        {
            Instantiate(turretsUpd[GameController.Upgrades[3]], tur);
        }
        if (GameController.Upgrades[3] == 5)
        {
            militiaBot = militiaBot.GetComponent<MilBotController>().Updated;
        }
        inf = GetComponentInChildren<UIController>().HoverShow;
        activeSkills[0].GetComponentInChildren<TextMeshProUGUI>().text = (150 * (1 + GameController.Upgrades[3]) / 2 * discont).ToString();
        activeSkills[1].GetComponentInChildren<TextMeshProUGUI>().text = (250 * (1 + GameController.Upgrades[3]) / 2 * discont).ToString();
        activeSkills[2].GetComponentInChildren<TextMeshProUGUI>().text = (50 * (1 + GameController.Upgrades[3]) / 2 * discont).ToString();
    }

    protected override void ExtraDamage(int gained)
    {
        if(damagedCor != null)
            StopCoroutine(damagedCor);
        damagedCor = StartCoroutine(ShowInfTemp());
    }

    IEnumerator ShowInfTemp()
    {
        inf.SetActive(true);
        yield return new WaitForSeconds(2);
        inf.SetActive(false);
    }

    public override void Death()
    {
        level.GetUI().WinLose();
    }

    private void FixedUpdate()
    {
        if(level.Money >= 250 * (1 + GameController.Upgrades[3]) / 2 * discont)
        {
            activeSkills[0].interactable = true;
            activeSkills[1].interactable = true;
            activeSkills[2].interactable = true;
        }
        else
        {
            activeSkills[1].interactable = false;
            if (level.Money >= 150 * (1 + GameController.Upgrades[3]) / 2 * discont)
            {
                activeSkills[0].interactable = true;
                activeSkills[2].interactable = true;
            }
            else if(level.Money >= 50 * (1 + GameController.Upgrades[3])/2 * discont)
            {
                activeSkills[0].interactable = false;
                activeSkills[2].interactable = true;
            }
            else
            {
                activeSkills[0].interactable = false;
                activeSkills[2].interactable = false;
            }
        }
        if (activeSkills[2].interactable && level.Zombies.Count < 1)
            activeSkills[2].interactable = false;
        if (activeSkills[1].interactable && FindObjectsByType<GameObject>(FindObjectsSortMode.None).Any(x => x.TryGetComponent<SupportHelliController>(out var a)))
            activeSkills[1].interactable = false;
        if (activeSkills[0].interactable && FindObjectsByType<GameObject>(FindObjectsSortMode.None).Any(x => x.TryGetComponent<MilBotController>(out var a)))
            activeSkills[0].interactable = false;
    }

    public void OnSummonMilitia()
    {
        level.Money -= 150 * (1 + GameController.Upgrades[3]) / 2 * discont;
        for(int i = 0; i < summonPoint.Length; i++)
        {
            for (int j = 0; j < quantityModifier; j++)
            {
                var milbot = Instantiate(militiaBot, summonPoint[i]);
                milbot.transform.localPosition = Vector2.zero;
                milbot.transform.localPosition = LevelController.GetRandomizedLocation(milbot.transform, 10);
                var c = milbot.GetComponent<MilBotController>();
                c.Init();
            }
        }
    }
    public void OnSummonHelicopter()
    {
        level.Money -= 250 * (1 + GameController.Upgrades[3]) / 2 * discont;
        var heli = Instantiate(helicopter, summonPoint[0]);
            heli.transform.localPosition = Vector2.zero;
        
    }
    public void OnSummonDrones()
    {
        level.Money -= 50 * (1 + GameController.Upgrades[3]) / 2 * discont;
        for(int i = 0; i < summonPoint.Length * quantityModifier;  i++)
        {
            var drone = Instantiate(kamikadze, summonPoint[i / quantityModifier]);
            drone.transform.localPosition = Vector2.zero;
            var inf = drone.GetComponent<BulletExpController>();
            inf.Init(100 + 15 * GameController.Upgrades[3], 0, new int[] { 6, 10 }, level.DangerestEnemy(), false, 60 + 10 * GameController.Upgrades[3]);
        }
    }
    public static CastleController GetCastle()
    {
        return LevelController.GetLevel().gameObject.GetComponentInChildren<CastleController>();
    }
}