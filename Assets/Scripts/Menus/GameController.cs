using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements.Experimental;
using UnityEngine.UI;

public static class GameController
{
    private static int[] levels = new int[] { -1, -1, -1, -1, -1 };
    private static int wins = 0;
    private static int loses = 0;
    private static int allKills = 0;
    private static int allDamage = 0;
    private static int moneySpend = 0;
    private static int[] upgrades = new int[] { 0,0,0,0 };
    private static int gametime = 0;
    private static int[] enemiesStatus = new int[] { 0,0,0,0,0,0 };
    private static GameObject[] enemies;
    private static GameObject[] towers;
    private static int[] levelsGoal;
    private static string savegame;
    private static int language;
    private static float brightness;
    private static float volume;
    private static float musicVolume;
    private static float soundVolume;
    private static float environmentVolume;
    private static int difficult;
    public static float[,] DifficultyModifiers 
    {
        get; private set;
    } =
    {
        { 0.5f, 0.75f, 2f, 0.75f, 1.5f, 2f, 1.5f },
        { 1f, 1f, 1f, 1f, 1f, 1f, 1f },
        { 2f, 1.25f, 0.5f, 1.25f, 1f, 0.9f, 0.9f }
    };
    public static int Language { get { return language; } }
    public static float Brightness { get { return brightness; }  }
    public static float Volume { get { return volume; }  }
    public static float MusicVolume { get { return musicVolume; } }
    public static float SoundVolume { get { return soundVolume; } }
    public static float EnvironmentVolume { get { return environmentVolume; } }
    public static int Difficult { get { return difficult; } }
    public static int XP
    {
        get
        {
            int xp = 0;
            foreach(int i in Levels)
            {
                if (i > 0)
                    xp += i;
                else break;
            }
            return xp;
        }
    }
    public static int[] Levels
    {
        get => levels;
        set
        {
            levels = value;
            Save();
        }
    }
    public static int Wins
    {
        get => wins;
        set
        {
            wins = value;
            Save();
        }
    }
    public static int Loses
    {
        get => loses;
        set
        {
            loses = value;
            Save();
        }
    }
    public static int AllKills
    {
        get => allKills;
        set
        {
            allKills = value;
            Save();
        }
    }
    public static int AllDamage
    {
        get => allDamage;
        set
        {
            allDamage = value;
            Save();
        }
    }
    public static int MoneySpend
    {
        get => moneySpend;
        set
        {
            moneySpend = value;
            Save();
        }
    }
    public static int[] Upgrades
    {
        get => upgrades;
        set
        {
            upgrades = value;
            Save();
        }
    }
    public static int GameTime
    {
        get => gametime;
        set
        {
            gametime = value;
            Save();
        }
    }
    public static int[] EnemiesStatus
    {
        get => enemiesStatus;
        set
        {
            enemiesStatus = value;
            Save();
        }
    }
    public static GameObject[] Enemies
    {
        get => enemies;
        set => enemies = value;
    }
    public static GameObject[] Towers
    {
        get => towers;
        set => towers = value;
    }
    public static string SaveGame
    {
        get => savegame;
        private set => savegame = value;
    }
    public static int[] LevelsGoal
    {
        get => levelsGoal;
        set => levelsGoal = value;
    }

    private static GameData data;

    private static GameSettings settings;

    static float musicTime = 0;
    public static float MusicTime { get => musicTime; set => musicTime = value; }

    public static bool Load()
    {
        var saves = Directory.GetFiles(Application.persistentDataPath).ToList();
        var ss = saves.Find(s => s.EndsWith("settings.json"));
        if(ss != null) 
            settings = JsonUtility.FromJson<GameSettings>(File.ReadAllText(ss));
        if (settings == null)
        {
            settings = new GameSettings();
            File.WriteAllText(Application.persistentDataPath + "/settings.json", JsonUtility.ToJson(settings));
        }
        if(!File.Exists(settings.lastSave))
        {
            foreach(var s in saves)
            {
                if(s.EndsWith("_savegame.json"))
                {
                    settings.lastSave = s;
                    break;
                }
            }
        }
        if (!File.Exists(settings.lastSave))
            settings.lastSave = "";
        LoadSettings();
        if (settings.lastSave != "")
        {
            LoadSave(settings.lastSave);
            return true;
        }
        else return false;
    }

    public static void LoadSave(string path)
    {
        SaveGame = path;
        settings.lastSave = SaveGame;
        SaveSettings();
        if (File.ReadAllText(path).Length > 10)
        {
            string saving = File.ReadAllText(SaveGame);
            data = JsonUtility.FromJson<GameData>(saving);
            levels = data.LevelsData;
            wins = data.Wins;
            loses = data.Loses;
            allKills = data.AllKills;
            allDamage = data.AllDamage;
            moneySpend = data.MoneySpend;
            upgrades = data.UpgradesData;
            enemiesStatus = data.EnemiesStatus;
            gametime = data.GameTime;
            if(levels.Length < levelsGoal.Length)
            {
                var l = new int[levelsGoal.Length - levels.Length];
                for(int i = 0; i < l.Length; i++)
                {
                    l[i] = -1;
                }
                if (levels[^1] > 0)
                    l[0] = 0;
                levels = levels.Concat(l).ToArray();
            }
            
        }
        else
        {
            CreateSave(path);
        }
    }

    public static void Save()
    {
        data = new GameData(Levels, Wins, Loses, AllKills, AllDamage, MoneySpend, Upgrades, EnemiesStatus, gametime);
        File.WriteAllText(SaveGame, JsonUtility.ToJson(data, true));
    }

    public static string[] GetSaves()
    {
        var saves = Directory.GetFiles(Application.persistentDataPath).ToList();
        saves = saves.Select(inf => new FileInfo(inf)).OrderBy(inf => inf.LastWriteTime).Select(inf => inf.FullName).ToList();
        for(int i = saves.Count-1; i >= 0; i--) 
        {
            if (!saves[i].Contains("savegame.json"))
            {
                saves.Remove(saves[i]);
            }
        }
        if(saves.Contains(SaveGame))
        {
            saves.Remove(SaveGame);
            saves.Add(SaveGame);
        }
        return saves.ToArray();
    }
    public static string GetLastLevel(string path)
    {
        int[] lvls = JsonUtility.FromJson<GameData>(File.ReadAllText(path)).LevelsData;
        for (int i = 0; i < lvls.Length; i++)
        {
            if(lvls[i] < 0)
            {
                return $"{i}";
            }
        }
        return Levels.Length.ToString();
    }
    public static int GetLastLevel()
    {
        for(int i = 0; i < Levels.Length; i++)
        {
            if (Levels[i] < 0)
            {
                return i-1;
            }
        }
        return levels.Length;
    }
    public static string GetTime(string path)
    {
        GameData gd = JsonUtility.FromJson<GameData>(File.ReadAllText(path));
        TimeSpan time = TimeSpan.FromSeconds(gd.GameTime);
        return $"{time.Hours.ToString("D2")}:{time.Minutes.ToString("D2")}:{time.Seconds.ToString("D2")}";
    }
    public static void CreateSave(string path)
    {
        if (!File.Exists(path))
        {
            File.WriteAllText(path, JsonUtility.ToJson(new GameData(), true));
            LoadSave(path);
        }
    }
    private static void LoadSettings()
    {
        language = settings.lang;
        brightness = settings.brightness;
        volume = settings.commonVolume;
        musicVolume = settings.musicVolume;
        soundVolume = settings.soundVolume;
        environmentVolume = settings.environmentVolume;
        difficult = settings.difficult;
        SaveGame = settings.lastSave;
        LoadVolume();
    }
    public static void SaveSettings(GameSettings stt)
    {
        settings = stt;
        File.WriteAllText(Application.persistentDataPath + "/settings.json", JsonUtility.ToJson(settings));
        LoadSettings();
    }
    public static void SaveSettings(AudioType type, float value)
    {
        switch(type)
        {
            case AudioType.Global: settings.commonVolume = value; break;
            case AudioType.Music: settings.musicVolume = value; break;
            case AudioType.Sound: settings.soundVolume = value; break;
            case AudioType.Environment: settings.environmentVolume = value; break;
        }
        SaveSettings(settings);
    }
    public static void SaveSettings()
    {
        File.WriteAllText(Application.persistentDataPath + "/settings.json", JsonUtility.ToJson(settings));
    }
    public static void SetVolume(AudioSource audio, AudioType type)
    {
        switch (type)
        {
            case AudioType.Global: audio.volume = volume; break;
            case AudioType.Music: audio.volume = musicVolume * volume; break;
            case AudioType.Sound: audio.volume = soundVolume * volume; break;
            case AudioType.Environment: audio.volume = environmentVolume * volume; break;
            case AudioType.Firing: audio.volume = soundVolume * volume * 0.5f; break;
        }
    }
    public static void LoadVolume()
    {
        foreach (var music in GameObject.FindGameObjectsWithTag("Music").Select(x => x.GetComponent<AudioSource>()))
        {
            SetVolume(music, AudioType.Music);
        }
        foreach (var sound in GameObject.FindGameObjectsWithTag("Sound").Select(x => x.GetComponent<AudioSource>()))
        {
            SetVolume(sound, AudioType.Sound);
        }
        foreach (var environment in GameObject.FindGameObjectsWithTag("Environment").Select(x => x.GetComponent<AudioSource>()))
        {
            SetVolume(environment, AudioType.Environment);
        }
    }
}

public class GameData
{
    public int[] LevelsData;
    public int Wins;
    public int Loses;
    public int AllKills;
    public int AllDamage;
    public int MoneySpend;
    public int[] UpgradesData;
    public int[] EnemiesStatus;
    public int GameTime;

    public GameData()
    {
        LevelsData = new int[] { 0, -1, -1, -1, -1};
        Wins = 0;
        Loses = 0;
        AllKills = 0;
        AllDamage = 0;
        MoneySpend = 0;
        UpgradesData = new int[] { 0, 0, 0, 0 };
        EnemiesStatus = new int[] { 0, 0, 0, 0, 0, 0 };
        GameTime = 0;
    }
    public GameData(int[] levelsData, int wins, int loses, int allKills, int allDamage, int moneySpend, int[] upgradesData, int[] enemiesStatus, int gametime)
    {
        LevelsData = levelsData;
        Wins = wins;
        Loses = loses;
        AllKills = allKills;
        AllDamage = allDamage;
        MoneySpend = moneySpend;
        UpgradesData = upgradesData;
        EnemiesStatus = enemiesStatus;
        GameTime = gametime;
    }
}

public class GameSettings
{
    public int lang;
    public float brightness;
    public float commonVolume;
    public float musicVolume;
    public float soundVolume;
    public float environmentVolume;
    public int difficult;
    public string lastSave;

    public GameSettings()
    {
        lang = 0;
        brightness = 1;
        commonVolume = 1;
        musicVolume = 1;
        soundVolume = 1;
        environmentVolume = 1;
        difficult = 1;
        lastSave = "";
    }
    public GameSettings(int lang, float brightness, float commonVolume, float musicVolume, float soundVolume, float environmentVolume, int difficult, string lastSave)
    {
        this.lang = lang;
        this.brightness = brightness;
        this.commonVolume = commonVolume;
        this.musicVolume = musicVolume;
        this.soundVolume = soundVolume;
        this.environmentVolume = environmentVolume;
        this.difficult = difficult;
        this.lastSave = lastSave;
    }

    
}

public enum AudioType { Sound, Music, Global, Environment, Firing };    