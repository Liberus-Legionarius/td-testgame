using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UILevelController : MonoBehaviour
{

    [SerializeField] GameObject cardWeapPanel;
    [SerializeField] TextMeshProUGUI time;
    [SerializeField] Image brightness;
    [SerializeField] GameObject card;
    [SerializeField] GameObject pause; 
    [SerializeField] TextMeshProUGUI xpText; 
    [SerializeField] TextMeshProUGUI recordText;
    [SerializeField] TextMeshProUGUI moneyText;
    [SerializeField] GameObject winLoseMenu; 
    [SerializeField] TextMeshProUGUI[] winLose; 
    [SerializeField] TextMeshProUGUI wave; 
    [SerializeField] Image kills;
    [SerializeField] TextController greatings; 
    [SerializeField][Multiline] string[] goodbye = new string[0];
    [SerializeField] GameObject[] goodbyeShow = new GameObject[0];
    [SerializeField] Image stars;
    [SerializeField] GameObject zatychka;
    [SerializeField] AudioSource music;
    [SerializeField] AudioSource sound;
    [SerializeField] AudioClip winSound;
    [SerializeField] AudioClip loseSound;
    [SerializeField] GameObject settings;
    List<WeaponIconController> cardsWeap = new List<WeaponIconController>(); 
    List<WeaponIconController> cardsDef = new List<WeaponIconController>();
    int levelId;
    bool goodbyed = false;

    private GameObject selectedWeapon; 
    public GameObject SelectedWeapon
    {
        get => selectedWeapon;
        set
        {
            if (value is null)
            {
                selectedWeapon = null;
                foreach (var c in cardsWeap)
                {
                    c.Active(false);
                }
            }
            else
            {
                selectedWeapon = value;
                foreach (var c in cardsWeap)
                {
                    if (c.Weapon != value)
                    {
                        c.Active(false);
                    }
                }
            }
        }
    }
    private GameObject selectedDefense;
    public GameObject SelectedDefense
    {
        get => selectedDefense;
        set
        {
            if (value is null)
            {
                selectedDefense = null;
                foreach (var c in cardsDef)
                {
                    c.Active(false);
                }
            }
            else
            {
                selectedDefense = value;
                foreach (var c in cardsDef)
                {
                    if (c.Weapon != value)
                    {
                        c.Active(false);
                    }
                }
            }
        }
    }


    float timerReal = 0;
    public float TimerReal
    {
        get => timerReal;
        private set
        {
            timerReal = value;
            var t = TimeSpan.FromSeconds(value);
            time.text = $"{t.Minutes.ToString("D2")}:{t.Seconds.ToString("D2")}";
        }
    }

    public float Money
    {
        get => float.Parse(moneyText.text);
        set => moneyText.text = Mathf.RoundToInt(value).ToString();
    }
    public float XP
    {
        get => float.Parse(xpText.text.Remove(xpText.text.IndexOf(" ")));
        set => xpText.text = Mathf.RoundToInt(value).ToString() + " XP";
    }

    public float FillAmount
    {
        get => kills.fillAmount;
        set => kills.fillAmount = value;
    }

    public string Wave
    {
        get => wave.text;
        set => wave.text = value;
    }

    public bool LvlEnded
    {
        get => winLoseMenu.activeSelf;
    }

    private void Start()
    {
        levelId = SceneManager.GetActiveScene().buildIndex - 2;
        brightness.color = new Color(0, 0, 0, 1 - GameController.Brightness);
        winLose = winLoseMenu.GetComponentsInChildren<TextMeshProUGUI>();
        if (GameController.Levels[levelId] > 0)
        {
            recordText.text = $"{(GameController.Language == 0 ? "Найкращий результат" : GameController.Language == 1 ? "The Best" : "")}: {GameController.Levels[levelId]} XP";
        }
        else
        {
            Destroy(recordText.gameObject);
            xpText.gameObject.transform.position = new Vector2(xpText.gameObject.transform.position.x, xpText.gameObject.transform.position.y + 25);
            moneyText.gameObject.transform.position = new Vector2(moneyText.gameObject.transform.position.x, moneyText.gameObject.transform.position.y + 25);
        }
        if(greatings != null)
        {
            greatings.Activate(true);
            zatychka.SetActive(true);
        }
        var v = settings.GetComponentsInChildren<Slider>();
        v[0].value = GameController.Volume;
        v[1].value = GameController.MusicVolume;
        v[2].value = GameController.SoundVolume;
        v[3].value = GameController.EnvironmentVolume;
    }

    private void Update()
    {
        if ((Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame) && GreatingsEnded())
        {
            if (settings.activeSelf)
            {
                settings.SetActive(false);
            }
            else OnPause();
        }
        
    }
    private void FixedUpdate()
    {
        if(GreatingsEnded() && !pause.activeSelf && !winLoseMenu.activeSelf)
        {
            TimerReal += Time.deltaTime * (1 / Time.timeScale);
        }
        if(goodbyed && GreatingsEnded())
        {
            goodbyed = false;
            WinLose();
        }
    }

    public bool GreatingsEnded()
    {
        if (!greatings.gameObject.activeSelf)
        {
            if(!winLoseMenu.activeSelf)
                zatychka.SetActive(false);
            return true;
        }
        else return false;
    }

    public GameObject[] SetTowers(GameObject[] from, int max = -1)
    {
        var utilities = from;
        max = max < 0 ? from.Length : max;
        for (int i = 0; i < max; i++)
        {
            WeaponIconController icon = Instantiate(card, cardWeapPanel.transform).GetComponent<WeaponIconController>();
            icon.transform.Translate(100 * i, 25, 0);
            cardsWeap.Add(icon);
            icon.Weapon = utilities[i];
        }
        return utilities;
    }

    public void SetExtraMoney(float bonus)
    {
        moneyText.GetComponentsInChildren<TextMeshProUGUI>()[1].text = $"{bonus} +";
    }

    public void UnCheckWeap()
    {
        foreach (var c in cardsWeap)
        {
            c.Active(false);
        }
    }

    public bool GoodBye()
    {
        if (goodbye.Length > 0)
        {
            greatings.Reload(goodbye, goodbyeShow);
            greatings.Activate(true);
            goodbyed = true;
            return true;
        }
        else WinLose();
        return false;
    }
    public void WinLose()
    {
        Destroy(GameObject.Find("PauseBtn"));
        music.Stop();
        zatychka.SetActive(true);
        winLoseMenu.SetActive(true);
        GameObject next = winLoseMenu.GetComponentsInChildren<Button>()[2].gameObject;
        LevelController level = LevelController.GetLevel();
        if (level.IsWin())
        {
            sound.PlayOneShot(winSound);
            stars.fillAmount = XP / GameController.LevelsGoal[levelId];

            winLose[0].text = $"{(GameController.Language == 0 ? "Успiх!" : GameController.Language == 1 ? "You Won!" : "Успех!")}";
            winLose[1].text = $"{XP} XP";
            winLose[2].text = $"{(GameController.Language == 0 ? "Шкоди завдано" : GameController.Language == 1 ? "Damage Done" : "Урона нанесено")}: {level.AllDamage}";
            winLose[3].text = $"{(GameController.Language == 0 ? "Зомбi вбито" : GameController.Language == 1 ? "Zombies Killed" : "Зомби убито")}: {level.AllKills}";
            winLose[4].text = $"{(GameController.Language == 0 ? "Грошей отримано" : GameController.Language == 1 ? "Money Gained" : "Денег получено")}: {level.AllMoneyGained}";
            winLose[5].text = $"{(GameController.Language == 0 ? "Грошей витрачено" : GameController.Language == 1 ? "Money Spend" : "Денег потрачено")}: {level.AllMoneySpend}";
            if (levelId == 4)
                next.SetActive(false);
            GameController.Wins++;
            GameController.Levels[levelId] = Mathf.RoundToInt(XP > GameController.Levels[levelId] ? XP : GameController.Levels[levelId]);
            if(levelId + 1 < GameController.Levels.Length)
                GameController.Levels[levelId + 1] = GameController.Levels[levelId + 1] > 0 ? GameController.Levels[levelId + 1] : 0;
        }
        else
        {
            sound.PlayOneShot(loseSound);
            winLose[0].text = $"{(GameController.Language == 0 ? "Зомбі з'їли ваші мізки" : GameController.Language == 1 ? "Zombies ate your brains" : "Зомби съели ваши мозги")}...";
            winLose[1].text = $"{XP} XP";
            winLose[3].text = $"{(GameController.Language == 0 ? "Яка досада" : GameController.Language == 1 ? "That a shame" : "Какая досада")}...";
            Time.timeScale = 0;
            next.SetActive(false);
            GameController.Loses++;
        }
        GameController.AllKills += level.AllKills;
        GameController.AllDamage += level.AllDamage;
        GameController.GameTime += Mathf.RoundToInt(timerReal);
    }
    public void OnContinue()
    {
        Time.timeScale = 1;
        settings.SetActive(false);
        pause.SetActive(false);
    }
    public void OnRestart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(levelId + 2);
    }
    public void OnExit()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(1);
    }
    public void OnNext()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(levelId + 3);
    }

    public void SpeedOn(int x)
    {
        if (Time.timeScale != 0)
            Time.timeScale = x;
    }

    public void OnSettings()
    {
        if(settings.activeSelf)
        {
            settings.SetActive(false);
        }
        else settings.SetActive(true);
    }

    public void OnPause()
    {
        if (pause.activeSelf)
        {
            OnContinue();
            zatychka.SetActive(false);
        }
        else if (!greatings.gameObject.activeSelf)
        {
            Time.timeScale = 0;
            pause.SetActive(true);
            zatychka.SetActive(true);
        }
    }
}
