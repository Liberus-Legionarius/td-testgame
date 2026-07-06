using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelsMenuController : MonoBehaviour
{
    [SerializeField] Button level; 
    [SerializeField] Canvas canvas; 
    [SerializeField] Image darkness;
    [SerializeField] Image brightness;
    [SerializeField] Image upgrades; 
    [SerializeField] Image notes; 
    [SerializeField] GameObject[] toHide; 
    [SerializeField] TextMeshProUGUI xpText; 
    [SerializeField] TextMeshProUGUI xpLargeText;
    [SerializeField] TextMeshProUGUI xpLargeLeftText;
    [SerializeField] TextMeshProUGUI[] upgradesText;
    [SerializeField] Image xpFill; 
    [SerializeField] GameObject note;
    [SerializeField] GameObject noteList;
    [SerializeField] GameObject noteInfo;
    [SerializeField] Sprite locked;
    [SerializeField] Sprite defSpr;

    string lockedLabel;
    string lockedDesc;

    int xp; 
    public int XP
    {
        get => xp;
        set
        {
            xp = value;
            int curxp = xp;
            xpLarge = 0;
            for(int i = 1; curxp > xpToLvl;i++)
            {
                curxp -= xpToLvl;
                xpToLvl = Mathf.Clamp(250 + 150*i, 0, 1000);
                xpLarge++;
            }
            xpFill.fillAmount = (float)curxp / xpToLvl;
            xpText.text = $"{curxp}/{xpToLvl}";
            xpLargeText.text = xpLarge.ToString();
            xpLargeLeft = xpLarge;
            foreach(int i in GameController.Upgrades)
            {
                xpLargeLeft -= i;
            }
            xpLargeLeftText.text = xpLargeLeft.ToString();
        }
    }
    int xpToLvl = 250;
    int[] levels; 
    int xpLarge = 0; 
    int xpLargeLeft = 0;
    int[] upgradesLvls; 
    public int[] UpgradesLvls
    {
        get => upgradesLvls;
        set
        {
            upgradesLvls = value;
            for (int i = 0; i < 4; i++)
            {
                upgradesText[i].text = value[i].ToString();
            }
            GameController.Upgrades = UpgradesLvls;
        }
    }

    private void Awake()
    {
        GameObject.Find("Music").GetComponent<AudioSource>().time = GameController.MusicTime;
    }

    private void Start()
    {
        brightness.color = new Color(0,0,0, 1 - GameController.Brightness);
        levels = GameController.Levels;
        for(int i = 0; i < levels.Length; i++)
        {
            var btn = Instantiate(level, canvas.transform);
            LvlButtonController lvl = btn.GetComponent<LvlButtonController>();
            lvl.Init(i + 1, levels[i]);
            RectTransform rect = btn.GetComponent<RectTransform>();
            rect.anchoredPosition= new Vector2(165 + (i%10)*165, -200 - (i/10)*200);   
        }
        XP = GameController.XP;
        UpgradesLvls = GameController.Upgrades;
        switch (GameController.Language)
        {
            case 0:
                lockedLabel = "Закрито";
                lockedDesc = "Ви ще не зустрiли цього зомбi.";
                break;
            case 1:
                lockedLabel = "Locked";
                lockedDesc = "You haven't met this zombie yet.";
                break;
        }
        LoadNotes();
        GameController.LoadVolume();
    }

    void LoadNotes()
    {
        var zombies = Instantiate(note, notes.transform.GetChild(10));
        var rect = zombies.GetComponent<RectTransform>();
        {
            float aspect = rect.rect.width/ rect.rect.height;
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, notes.rectTransform.rect.height / 2);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, aspect * rect.rect.height);
        }
        zombies.transform.Translate(new Vector2(-200, 0));
        var zombiesList = Instantiate(noteList, notes.transform);
        SetNote(GameController.Enemies[0].GetComponent<ZombieController>().Sprite, zombies, zombiesList);
        zombies.GetComponent<Button>().onClick.AddListener(() => OnNoteClick(zombies.transform.parent.gameObject, zombiesList));
        Transform zombieContent = zombiesList.transform.GetChild(0).GetChild(0);
        for(int i = 0; i < GameController.Enemies.Length; i++)
        {
            var zombie = Instantiate(note, zombieContent);
            zombie.GetComponent<RectTransform>().anchoredPosition = new Vector2(150 * (i%5) + 75, -200 * (i/5) - 100);
            if (GameController.EnemiesStatus[i] == 1)
            {
                var info = GameController.Enemies[i].GetComponent<ZombieController>();
                SetNote(info.Sprite, zombie, null);
                zombie.GetComponent<Button>().onClick.AddListener(() => OnNoteInfo(info.Sprite, info.Title, info.Descr));
            }
            else
            {
                SetNote(locked, zombie, null);
                zombie.GetComponent<Button>().onClick.AddListener(() => OnNoteInfo(locked, lockedLabel, lockedDesc));
            }
        }
        zombiesList.SetActive(false);
        var towers = Instantiate(note, notes.transform.GetChild(10));
        rect = towers.GetComponent<RectTransform>();
        {
            float aspect = rect.rect.width / rect.rect.height;
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, notes.rectTransform.rect.height / 2);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, aspect * rect.rect.height);
        }
        towers.transform.Translate(new Vector2(200, 0));
        var towersList = Instantiate(noteList, notes.transform);
        SetNote(GameController.Towers[0].GetComponent<FirearmsController>().Sprite, towers, towersList);
        towers.GetComponent<Button>().onClick.AddListener(() => OnNoteClick(towers.transform.parent.gameObject, towersList));
        Transform towersContent = towersList.transform.GetChild(0).GetChild(0);
        for(int i = 0; i< GameController.Towers.Length; i++)
        {
            var tower = Instantiate(note, towersContent);
            var info = GameController.Towers[i].GetComponent<FirearmsController>();
            tower.GetComponent<RectTransform>().anchoredPosition = new Vector2(150 * (i % 5) + 75, -200 * (i/5) - 100);
            var towerInnerList = Instantiate(noteList, notes.transform);
            SetNote(info.Sprite, tower, towerInnerList);
            tower.GetComponent<Button>().onClick.AddListener(() => OnNoteClick(towersList, towerInnerList));
            var towerInnerContent = towerInnerList.transform.GetChild(0).GetChild(0);
            CreateTowerNote(towerInnerContent, info, 75, -100);
            towerInnerList.SetActive(false);
        }
        towersList.SetActive(false);
    }

    void CreateTowerNote(Transform content, FirearmsController info, int x = 0, int y = 0)
    {
        int i = 0;
        do
        {
            var tower = Instantiate(note, content);
            tower.GetComponent<RectTransform>().anchoredPosition = new Vector2(150 * i +x, y);
            SetNote(info.Sprite, tower, null);
            FirearmsController inf = info;
            tower.GetComponent<Button>().onClick.AddListener(() => OnNoteInfo(inf.Sprite, inf.Title, inf.Descr));
            if (info.Updated.Length > 1)
            {
                int j = 0;
                foreach (var item in info.Updated)
                {
                    if (item.TryGetComponent(out info))
                        CreateTowerNote(content, info, x + (i+1) * 150, y - 200 * j);
                    j++;
                }
                break;
            }
            else if (info.Updated.Length == 1)
            {
                if (!info.Updated[0].TryGetComponent(out info))
                    break;
            }
            else
            {
                break;
            }
            i++;
        } while (true);
    }

    void SetNote(Sprite sprite, GameObject note,GameObject children)
    {
        var rect = note.GetComponent<RectTransform>().rect;
        var img = note.GetComponentInChildren<SpriteController>();
        img.Sprite = sprite;
    }

    public void OnNoteClick(GameObject self, GameObject childrenList)
    {
        if(childrenList != null)
        {
            self.SetActive(false);
            childrenList.SetActive(true);
        }
    }
    public void OnNoteInfo(Sprite spr, string title, string descr)
    {
        var img = noteInfo.transform.GetChild(0).GetComponent<Image>();
        img.sprite = spr;

        FitSpriteInsideStretch(img, noteInfo.GetComponent<RectTransform>());

        noteInfo.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = title;
        noteInfo.transform.GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = descr;
    }
    void FitSpriteInsideStretch(Image img, RectTransform parent)
    {
        var rt = img.rectTransform;
        var sprite = img.sprite;
        if (sprite == null) return;

        float spriteW = sprite.rect.width;
        float spriteH = sprite.rect.height;
        float aspect = spriteW / spriteH;

        float height = rt.sizeDelta.y;

        float width = height * aspect;

        float parentWidth = parent.rect.width;

        if (width > parentWidth)
        {
            width = parentWidth;
            height = width / aspect;
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
        }

        float extraSpace = parentWidth - width;

        float left = extraSpace * 0.5f;
        float right = -left;

        Vector2 offMin = rt.offsetMin;
        Vector2 offMax = rt.offsetMax;

        offMin.x = left;
        offMax.x = right;

        rt.offsetMin = offMin;
        rt.offsetMax = offMax;
    }

    public void OnExit()
    {
        StartCoroutine(MainMenuController.Load(0, darkness));
    }

    public void OnUpgrade()
    {
        if(xpLargeLeft > 0 && GameController.Upgrades[int.Parse(EventSystem.current.currentSelectedGameObject.transform.parent.gameObject.name)] < 5)
        {
            xpLargeLeft--;
            xpLargeLeftText.text = xpLargeLeft.ToString();
            GameObject btn = EventSystem.current.currentSelectedGameObject;
            UpgradesLvls[int.Parse(btn.transform.parent.gameObject.name)]++;
            UpgradesLvls = UpgradesLvls;
        }
    }

    public void OnCancelUpgrades()
    {
        for(int i = 0; i<4;i++)
        {
            xpLargeLeft += GameController.Upgrades[i];
            GameController.Upgrades[i] = 0;
        }
        xpLargeLeftText.text = xpLargeLeft.ToString();
        UpgradesLvls = GameController.Upgrades;
    }

    public void OnReturn()
    {
        upgrades.gameObject.SetActive(false);
        notes.gameObject.SetActive(false);
        foreach (var go in toHide)
        {
            go.SetActive(true);
        }
    }

    public void OnSubmenu(GameObject submenu)
    {
        foreach (var go in toHide)
        {
            go.SetActive(false);
        }
        submenu.SetActive(true);
    }

    public void OnNotesReturn()
    {
        noteInfo.GetComponentsInChildren<Image>()[1].sprite = defSpr;
        noteInfo.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
        noteInfo.transform.GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "";

        for (int i = 10; i < notes.transform.childCount; i++)
        {
            if(notes.transform.GetChild(i).gameObject.activeSelf)
            {
                if(i == 10)
                {
                    OnReturn();
                }
                else
                {
                    if(i == 12)
                    {
                        notes.transform.GetChild(i).gameObject.SetActive(false);
                        notes.transform.GetChild(10).gameObject.SetActive(true);
                    }
                    else if(i > 12)
                    {
                        notes.transform.GetChild(i).gameObject.SetActive(false);
                        notes.transform.GetChild(12).gameObject.SetActive(true);
                    }
                    else
                    {
                        notes.transform.GetChild(i).gameObject.SetActive(false);
                        notes.transform.GetChild(i - 1).gameObject.SetActive(true);
                    }
                }
                break;
            }
        }
    }
    
}
