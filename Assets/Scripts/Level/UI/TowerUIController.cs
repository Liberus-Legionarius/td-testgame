using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TowerUIController : UIController
{
    Button[] buttons; 
    public Button[] Buttons { get { return buttons; } }
    [SerializeField] GameObject upgradeIcon;
    [SerializeField] bool isWillBeTower = true;
    [SerializeField] bool isInteractable = false;
    [SerializeField] int timeToUse = -1;
    FirearmsController tower;
    LevelController level;
    DestroyableAbsController hp;
    TextMeshProUGUI timerObj;
    float timer = 0;

    void Awake()
    {
        buttons = new Button[clickShow.Length];
        for(int i = 0; i < clickShow.Length; i++)
        {
            buttons[i] = clickShow[i].GetComponent<Button>();
        }
        tower = transform.parent.parent.GetComponent<FirearmsController>();
        level = GameObject.Find("Level").GetComponent<LevelController>();
        hp = tower.GetComponent<DestroyableAbsController>();
        if (buttons.Length < 1 || buttons[0] == null)
            buttons = GetComponentsInChildren<Button>(includeInactive: true);
        if (tower.Updated == null)
            buttons[0].interactable = false;
        foreach (var go in hoverShow)
        {
            if (go.name == "Timer")
            {
                timerObj = go.GetComponent<TextMeshProUGUI>();
            }
            else if(go.name == "Radius")
            {
                float r = tower.GetComponent<CircleCollider2D>().radius * 2;
                go.transform.localScale = new Vector2(r, r);
            }
        }
        transform.parent.rotation = Quaternion.identity;
        if(tower.Updated.Length > 0 && isWillBeTower)
            buttons[0].GetComponentInChildren<TextMeshProUGUI>().text = tower.Updated[0].GetComponent<FirearmsController>().Cost.ToString();
    }
    private void FixedUpdate()
    {
        if (isWillBeTower)
        {
            if ((hp.HpLost() > 0 && level.Money >= tower.Cost * (1 - hp.HpPercent())) || (tower.Updated != null && tower.Updated.Length > 0 && tower.Updated[0] != null && level.Money >= tower.Updated[0].GetComponent<FirearmsController>().Cost))
                buttons[0].interactable = true;
            else buttons[0].interactable = false;
        }
        else
        {
            if (hp.HpLost() > 0 || tower.transform.parent.GetComponentInChildren<CellController>().CanChange)
                buttons[0].interactable = true;
            else buttons[0].interactable = false;
        }
        if (isInteractable)
        {
            if (timeToUse != -1)
            {
                timer += Time.deltaTime;
                SetTime();
                if (timer >= timeToUse && hp.HP > hp.MaxHP / 2)
                {
                    buttons[2].interactable = true;
                }
                else buttons[2].interactable = false;
            }
        }
        if (hp.HpLost() > 0)
            buttons[0].GetComponentInChildren<TextMeshProUGUI>().text = (tower.Cost * (1 - hp.HpPercent())).ToString();
        else if(tower.Updated != null)
            buttons[0].GetComponentInChildren<TextMeshProUGUI>().text = tower.Updated[0].GetComponent<FirearmsController>().Cost.ToString();
        else buttons[0].GetComponentInChildren<TextMeshProUGUI>().text = "";

    }
    void SetTime()
    {
        timerObj.text = Mathf.RoundToInt(timer > timeToUse ? timeToUse : timer).ToString();
    }
    public void OnDelete()
    {
        level.Money += Mathf.RoundToInt(tower.Cost / 2 * (hp.HP / hp.MaxHP));
        tower.transform.parent.GetComponentInChildren<CellController>().Weapon = null;
        Destroy(tower.gameObject);
    }
    public void OnUpdate()
    {
        if(hp.HpLost() > 0)
        {
            hp.HP = hp.MaxHP;
            level.Money -= Mathf.RoundToInt(tower.Cost * (1 - hp.HpPercent()));
        }
        else
        {
            if (tower.Updated.Length > 1)
            {
                float dist = -(float)tower.Updated.Length / 2 * 30f;
                for (int i = 0; i < tower.Updated.Length; i++)
                {
                    var go = Instantiate(upgradeIcon, transform.parent);
                    go.transform.localPosition = new Vector2(dist, 10);
                    var c = go.GetComponent<WeaponIconUpgradeController>();
                    c.Init(tower.Updated[i], i);
                    dist += 60;
                }
            }
            else
            {
                Updating(0);
            }
        }
    }
    public void OnActive()
    {
        timer = 0;
        tower.OnActivate();
        buttons[2].interactable = false;
    }
    public void Updating(int i)
    {
        var cell = tower.transform.parent.GetComponentInChildren<CellController>();
        if (isWillBeTower)
        {
            cell.Weapon = level.PlaceWeapon(tower.transform.parent, tower.Updated[i]);
            Destroy(transform.parent.parent.gameObject);
        }
        else cell.SelfUpdate(tower.Updated[i]);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if(transform.parent.GetComponentsInChildren<WeaponIconUpgradeController>().Length > 0)
        {
            foreach(var icon in transform.parent.GetComponentsInChildren<WeaponIconUpgradeController>().Select(x => x.gameObject))
            {
                Destroy(icon);
            }
        }
    }
}
