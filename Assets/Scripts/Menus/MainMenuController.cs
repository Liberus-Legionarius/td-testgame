using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using System.IO;
using Unity.VisualScripting;
using System;
using System.Linq;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class MainMenuController : MonoBehaviour
{

    [SerializeField] Image darkness; 
    [SerializeField] Image brightness;
    [SerializeField] Canvas infoCanvas;
    [SerializeField] GameObject[] submenus;
    [SerializeField] GameObject yesNo; 
    [SerializeField] GameObject[] towers; 
    [SerializeField] GameObject[] enemies; 
    [SerializeField] GameObject save; 
    [SerializeField] Transform whereSaves;
    [SerializeField] Button[] buttons;
    [SerializeField] GameObject saveCreate;
    [SerializeField] int[] levelsGoal;
    [SerializeField] LocalizedString[] d;
    [SerializeField] LocalizedString[] approveDesc;
    TextMeshProUGUI[] statsText;
    YesNoController yesNoMenu; 
    bool newGame = false;

    private void Awake()
    {
        GameObject.Find("Music").GetComponent<AudioSource>().time = GameController.MusicTime;
    }

    private void Start()
    {
        GameController.Towers = towers;
        GameController.Enemies = enemies;
        GameController.LevelsGoal = levelsGoal;
        statsText = submenus[1].GetComponentsInChildren<TextMeshProUGUI>();
        Prepare();
        LoadSettings();
        GameController.LoadVolume();
    }

    void Prepare(string path = "")
    {
        if(path != "")
        {
            GameController.LoadSave(path);
            LoadSaves(GameController.GetSaves());
        }
        else
        {
            SetUpContinue();
            LoadSaves(GameController.GetSaves());
        }
        LoadStats();
    }

    void SetUpContinue()
    {
        if (GameController.Load())
        {
            buttons[0].interactable = true;
        }
        else
        {
            buttons[0].interactable = false;
        }
    }


    void LoadSettings()
    {
        var dropdowns = submenus[2].GetComponentsInChildren<TMP_Dropdown>();
        dropdowns[0].value = GameController.Language;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[dropdowns[0].value];
        dropdowns[1].value = GameController.Difficult;
        for (int i = 0; i < dropdowns[1].options.Count; i++)
        {
            dropdowns[1].options[i].text = d[i].GetLocalizedString();
        }
        dropdowns[1].captionText.text = d[dropdowns[1].value].GetLocalizedString();
        var sliders = submenus[2].GetComponentsInChildren<Slider>();
        sliders[0].value = GameController.Brightness;
        brightness.color = new Color(0, 0, 0, 1 - GameController.Brightness);
        sliders[1].value = GameController.Volume;
        sliders[2].value = GameController.MusicVolume;
        sliders[3].value = GameController.SoundVolume;
        sliders[4].value = GameController.EnvironmentVolume;
    }
    private void LoadStats()
    {
        statsText[1].text = GameController.Wins.ToString();
        statsText[3].text = GameController.Loses.ToString();
        if (GameController.Wins == 0)
            statsText[5].text = "0.00";
        else statsText[5].text = ((float)GameController.Wins / (GameController.Wins + GameController.Loses)).ToString("F2");
        statsText[7].text = GameController.AllKills.ToString();
        statsText[9].text = GameController.AllDamage.ToString();
        statsText[11].text = GameController.MoneySpend.ToString();
        int xp = 0;
        float n = 0;

        for(int i = 0; i < GameController.Levels.Length; i++)
        {
            if (GameController.Levels[i] > 0)
            {
                xp += GameController.Levels[i];
                n += Mathf.Clamp01((float)GameController.Levels[i] / levelsGoal[i]);
            }
            else break;
        }
        statsText[13].text = xp.ToString();
        statsText[15].text = $"{(float)(n / GameController.Levels.Length) * 100 :F2}%";
    }

    private void LoadSaves(string[] saves)
    {
        Transform[] children = new Transform[whereSaves.childCount];
        for(int i = whereSaves.childCount - 1; i >= 0; i--)
        {
            Destroy(whereSaves.GetChild(i).gameObject);
        }
        for(int i = saves.Length - 1; i >= 0; i--)
        {
            var savegame = Instantiate(save, whereSaves);
            if(i == saves.Length-1)
                savegame.GetComponent<Image>().color = Color.lightGreen;
            TextMeshProUGUI[] info = savegame.GetComponentsInChildren<TextMeshProUGUI>();
            info[0].text = File.GetCreationTime(saves[i]).ToString();
            info[1].text = GameController.GetLastLevel(saves[i]);
            info[3].text = GameController.GetTime(saves[i]);
            info[4].text = Path.GetFileName(saves[i]).Replace("_savegame.json", "");
            Button[] buttons = savegame.GetComponentsInChildren<Button>();
            string p = saves[i];
            buttons[1].onClick.AddListener(() => OnErase(p));
            buttons[0].onClick.AddListener(() => OnLoadSave(p));
            var rect = savegame.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, -rect.rect.height * 1.5f * (saves.Length - 1 - i));
        }
    }

    public void OnContinue()
    {
        StartCoroutine(Load(1, darkness));
    }

    public void OnSubMenu(GameObject button)
    {
        GameObject toShow;
        string txt = button.name;
        switch(txt)
        {
            case "SaveButton":
                toShow = submenus[0];
                break;
            case "StatsButton":
                toShow = submenus[1];
                break;
            case "SettingsButton":
                toShow = submenus[2];
                break;
            default: toShow = submenus[0]; break;
        }
        foreach(var btn in buttons)
            btn.interactable = true;
        button.GetComponent<Button>().interactable = false;
        SetUpContinue();
        for (int i = 0; i < infoCanvas.transform.childCount; i++)
        {
            infoCanvas.transform.GetChild(i).gameObject.SetActive(false);
        }
        toShow.SetActive(true);
    }

    public void OnErase(string path)
    {
        yesNoMenu = Instantiate(yesNo, darkness.transform.parent).GetComponent<YesNoController>();
        yesNoMenu.Init(approveDesc[1].GetLocalizedString(), () => EraseSave(path));
    }

    private void EraseSave(string path)
    {
        File.Delete(path);
        Prepare();
        Destroy(yesNoMenu.gameObject);
    }

    void OnLoadSave(string path)
    {
        if (File.Exists(path))
        {
            Prepare(path);
        }
        else Prepare();
    }

    public void OnExit()
    {
        yesNoMenu = Instantiate(yesNo, darkness.transform.parent).GetComponent<YesNoController>();
        yesNoMenu.Init(approveDesc[0].GetLocalizedString(), Application.Quit);
    }

    public void OnCreate(bool f)
    {
        newGame = f;
        saveCreate.SetActive(true);
    }

    public void OnCreateAccept(ButtonController btn)
    {
        var pathText = saveCreate.GetComponentInChildren<TMP_InputField>();
        if(pathText.text.Length < 1 || pathText.text.Length > 10 || File.Exists(Application.persistentDataPath + @"\" + pathText.text + "_savegame.json") || pathText.text.Any(x => new char[] { '/', '\\', ':', '*', '?', '<', '>', '|' }.Contains(x)))
        {
            btn.Error();
            return;
        }
        string path = pathText.text;
        pathText.text = "";
        SaveCreate(path);
        GameController.LoadSave(Application.persistentDataPath + @"\" + path + "_savegame.json");
        if(newGame)
            OnContinue();
    }
    void SaveCreate(string path)
    {
        GameController.CreateSave(Application.persistentDataPath + @"\" + path + "_savegame.json");
        saveCreate.SetActive(false);
        Prepare();
    }
    public void OnCreationCancel()
    {
        var pathText = saveCreate.GetComponentInChildren<TMP_InputField>();
        pathText.text = "";
        saveCreate.SetActive(false);
        Prepare();
    }


    public void OnSaveSettings()
    {
        var dropdowns = submenus[2].GetComponentsInChildren<TMP_Dropdown>();
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[dropdowns[0].value];
        for(int i = 0; i < dropdowns[1].options.Count; i++)
        {
            dropdowns[1].options[i].text = d[i].GetLocalizedString();
        }
        dropdowns[1].captionText.text = d[dropdowns[1].value].GetLocalizedString();
        var sliders = submenus[2].GetComponentsInChildren<Slider>();
        GameController.SaveSettings(new GameSettings(dropdowns[0].value, sliders[0].value, sliders[1].value, sliders[2].value,sliders[3].value, sliders[4].value, dropdowns[1].value, GameController.SaveGame));
        brightness.color = new Color(0, 0, 0, 1 - GameController.Brightness);
        GameController.LoadVolume();
    }
    public static IEnumerator Load(int n, Image darkness)
    {
        darkness.gameObject.SetActive(true);
        for (float i = 0; i < 1; i += 0.01f)
        {
            darkness.color = new Color(0, 0, 0, i);
            yield return new WaitForSeconds(0.01f);
        }
        GameController.MusicTime = GameObject.Find("Music").GetComponent<AudioSource>().time;
        SceneManager.LoadScene(n);
    }

}
