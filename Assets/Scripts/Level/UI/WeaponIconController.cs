using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponIconController : SpriteController
{
    [SerializeField] TextMeshProUGUI text;

    private int cost; 
    private LevelController level;
    UILevelController ui;
    private Image sprite;
    private bool isActive = false;

    private GameObject weapon;
    public GameObject Weapon
    {
        get => weapon;
        set
        {
            weapon = value;

            FirearmsController tower = value.GetComponentInChildren<FirearmsController>();
            Sprite = tower.Sprite;
            cost = Mathf.RoundToInt(tower.Cost * (1 - GameController.Upgrades[2] * 0.05f));

            text.text = cost.ToString();
        }
    }

    override protected void Start()
    {
        base.Start();
        level = LevelController.GetLevel();
        ui = level.GetUI();
        sprite = gameObject.GetComponent<Image>();
    }

    private void Update()
    {
        if (level.Money < cost) 
        {
            text.color = Color.gray;
        }
        else text.color = Color.white;
    }

    public void OnSelect()
    {
        if (isActive)
        {
            Active(false);
        }
        else if (level.Money >= cost)
        {
            Active(true);
            ui.SelectedWeapon = weapon;
        }
        else ui.SelectedWeapon = null;
    }

    public void Active(bool active)
    {
        if (!active)
        {
            sprite.color = Color.white;
            isActive = false;
        }
        else
        {
            sprite.color = Color.red;
            isActive = true;
        }
    }
}
