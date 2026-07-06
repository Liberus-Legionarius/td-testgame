using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TextController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField][Multiline] string[] texts;
    [SerializeField] TextMeshProUGUI output;
    [SerializeField] GameObject[] toShow;
    [SerializeField] GameObject[] hidenUI;
    [SerializeField] int whenShow;
    int curId = 0;
    Color[] colorsImg = new Color[0];
    Color[] colorsSpr = new Color[0];
    Color[] colorTxt = new Color[0];
    List<SpriteRenderer> sprites = new List<SpriteRenderer>();
    List<Image> images = new List<Image>();
    List<TextMeshProUGUI> txts = new List<TextMeshProUGUI> ();
    bool ending = false;
    bool hasFocus = false;

    private void Start()
    {
        foreach (var ui in hidenUI)
        {
            if (gameObject != ui)
                ui.SetActive(false);
        }
        if(toShow.Any(x => x != null))
        {
            LoadObj();
        }
       
    }
    void LoadObj()
    {
        sprites = FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, 0).ToList();
        List<Color> clr = new List<Color>();
        for (int i = 0; i < sprites.Count; i++)
        {
            clr.Add(sprites[i].color);
        }
        colorsSpr = clr.ToArray();
        images = FindObjectsByType<Image>(FindObjectsInactive.Include, 0).ToList();
        clr = new List<Color>();
        for (int i = 0; i < images.Count; i++)
        {
            clr.Add(images[i].color);
        }
        colorsImg = clr.ToArray();
        txts = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, 0).ToList();
        clr = new List<Color>();
        for (int i = 0; i < txts.Count; i++)
        {
            clr.Add(txts[i].color);
        }
        colorTxt = clr.ToArray();
    }
    public void Activate(bool f)
    {
        f = f && texts.Length > 0;
        gameObject.SetActive(f);
        if (f)
        {
            if (texts[0].Split('\n').Length > 1)
                texts = LoadLoc(texts);
            if(toShow.Any(x => x != null))
            {
                hasFocus = true;
            }
            else hasFocus = false;
                LoadObj();
            StartTyping();
        }
        else 
        {
            if(texts.Length > 0)
            {
                if (whenShow > curId)
                    ShowAll();
                if (toShow.Length > curId && toShow[curId] != null)
                {
                    ChangeColor(null, true);
                }
            }
            LevelController.GetLevel().CanSummon = true;
        }
    }
    public void Reload(string[] txts, GameObject[] show)
    {
        texts = LoadLoc(txts);
        toShow = show;
        curId = 0;
        output.text = "";
        ending = true;
    }

    string[] LoadLoc(string[] t)
    {
        List<string> txts = new List<string>();
        foreach (var txt in t)
        {
            txts.Add(txt.Split("\n")[GameController.Language]);
        }
        return txts.ToArray();
    }

    void StartTyping()
    {
        StartCoroutine(Typing(texts[curId]));
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame && !ending)
        {
            StopAllCoroutines();
            Activate(false);
        }
        else if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Next();
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        Next();
    }

    private void Next()
    {
        if (output.text == texts[curId])
        {
            output.text = "";
            curId++;
            StopAllCoroutines();
            if (curId < texts.Length)
            {
                StartTyping();
            }
            else
            {
                curId--;
                Activate(false);
            }
                
        }
        else
        {
            output.text = texts[curId];
            StopAllCoroutines();
        }
    }

    private IEnumerator Typing(string txt)
    {
        if(toShow.Length > 0)
        {
            if (hidenUI.Length > 0)
            {
                if (whenShow == curId)
                {
                    ShowAll();
                }
            }
            if (curId > 0 && toShow[curId - 1] != null)
            {
                ChangeColor(toShow[curId - 1]);
            }
            if (toShow[curId] != null)
            {
                ChangeColor();
                ChangeColor(toShow[curId], true);
            }
            else if (hasFocus)
            {
                ChangeColor(null, true);
            }
        }
        foreach (char c in txt)
        {
            output.text += c;
            yield return new WaitForSeconds(0.05f);
        }
    }

    void ShowAll()
    {
        foreach (var ui in hidenUI)
        {
            if(ui != null) 
                ui.SetActive(true);
        }
    }
    void ChangeColor(GameObject group = null, bool restore = false)
    {
        if(group == null)
        {
            for (int i = 0; i < sprites.Count; i++)
                sprites[i].color = restore? colorsSpr[i] : new Color(colorsSpr[i].r - 0.25f, colorsSpr[i].g - 0.25f, colorsSpr[i].b - 0.25f, colorsSpr[i].a);
            for (int i = 0; i < images.Count; i++)
            {
                if (images[i] == null)
                    continue;
                images[i].color = restore ? colorsImg[i] : new Color(colorsImg[i].r - 0.25f, colorsImg[i].g - 0.25f, colorsImg[i].b - 0.25f, colorsImg[i].a);
            }
            for (int i = 0; i < txts.Count; i++)
            {
                if (txts[i] == null)
                    continue;
                txts[i].color = restore ? colorTxt[i] : new Color(colorTxt[i].r - 0.25f, colorTxt[i].g - 0.25f, colorTxt[i].b - 0.25f, colorTxt[i].a);
            }
        }
        else
        {
            foreach (var item in group.GetComponentsInChildren<Image>())
            {
                if (images.IndexOf(item) == -1)
                    continue;
                item.color = restore ? colorsImg[images.IndexOf(item)] : new Color(item.color.r - 0.25f, item.color.g - 0.25f, item.color.b - 0.25f, item.color.a);
            }
            foreach (var item in group.GetComponentsInChildren<SpriteRenderer>())
            {
                if (sprites.IndexOf(item) == -1)
                    continue;
                item.color = restore ? colorsSpr[sprites.IndexOf(item)] : new Color(item.color.r - 0.25f, item.color.g - 0.25f, item.color.b - 0.25f, item.color.a);
            }
            foreach(var item in group.GetComponentsInChildren<TextMeshProUGUI>())
            {
                if (txts.IndexOf(item) == -1)
                    continue;
                item.color = restore ? colorTxt[txts.IndexOf(item)] : new Color(item.color.r - 0.25f, item.color.g - 0.25f, item.color.b - 0.25f, item.color.a);
            }
        }
    }
}
