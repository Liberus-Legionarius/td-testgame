using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] AudioClip clicked;
    [SerializeField] AudioClip hover;
    [SerializeField] AudioClip error;
    AudioSource sound;

    protected bool isError = false;

    virtual protected void Start()
    {
        sound = GameObject.Find("Sound").GetComponent<AudioSource>();
        gameObject.GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        if (isError)
        {
            sound.PlayOneShot(error);
            isError = false;
        }
        else sound.PlayOneShot(clicked);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        sound.PlayOneShot(hover);
    }

    public void Error()
    {
        isError = true;
    }
}
