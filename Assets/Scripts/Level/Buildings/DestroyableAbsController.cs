using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class DestroyableAbsController : MonoBehaviour
{
    [SerializeField] protected Image hpUI; 
    [SerializeField] TextMeshProUGUI hpText; 
    [SerializeField] protected Color damaged;
    [SerializeField] protected Color healed;
    [SerializeField] AudioClip soundDamaged;
    [SerializeField] AudioClip soundDeath;
    [SerializeField] float autoDeathDMG = 10;
    AudioSource sound;
    protected SpriteRenderer sprite;
    protected Color transparent = new Color(0,0,0,0);
    private MaterialPropertyBlock propertyBlock;

    protected bool isDamaged = false;
    protected bool isHealed = false;

    Coroutine regeneration;
    float wait;
    public bool IsUndying { get; set; }

    [SerializeField] float maxhp; 
    public float MaxHP {  get => maxhp; }

    float hp; 
    public float HP
    {
        get => hp;
        set
        {
            if(!IsUndying)
            {
                if (value <= 0)
                {
                    Death();
                }
                if(hp > value)
                {
                    isDamaged = true;
                    if(sound != null)
                        sound.PlayOneShot(soundDamaged);
                    if (GameController.Upgrades[1] > 0)
                    {
                        if (regeneration != null)
                            StopCoroutine(regeneration);
                        regeneration = StartCoroutine(Regeneration(GameController.Upgrades[1]));
                    }
                }
                else if(hp < value)
                    isHealed = true;
                ExtraDamage(Mathf.RoundToInt(hp-value));
                hp = value > maxhp ? maxhp : value;
                hpUI.fillAmount = HpPercent();
                hpText.text = $"{hp}/{maxhp}";
            }
        }
    }

    protected virtual void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        propertyBlock = new MaterialPropertyBlock();
        if (GameController.Upgrades[1] > 0 && GameController.Upgrades[1] < 3)
        {
            wait = 10;
        }
        else if (GameController.Upgrades[1] > 3 && GameController.Upgrades[1] < 5)
        {
            wait = 5;
        }
        else if (GameController.Upgrades[1] == 5)
            wait = 2.5f;

        Init();
    }

    protected virtual void Start()
    {
        sound = GameObject.Find("Sound").GetComponent<AudioSource>();
    }

    protected virtual void Update()
    {
        if (isHealed)
        {
            isDamaged = false;
            StartCoroutine(ColorOff(a => isHealed = a, healed, transparent));
        }
        if (isDamaged)
        {
            isHealed = false;
            StartCoroutine(ColorOff(a => isDamaged = a, damaged, transparent));
        }
    }

    public virtual void Death()
    {
        if(gameObject != null) 
            Destroy(gameObject);
    }
    protected virtual void ExtraDamage(int gained)
    {

    }
    public float HpLost()
    {
        return maxhp-hp;
    }
    public float HpPercent()
    {
        return hp/maxhp;
    }


    protected IEnumerator ColorOff(Action<bool> change, Color color, Color newcolor)
    {
        change(false);
        SetColor(color);

        yield return new WaitForSeconds(0.4f);

        SetColor(newcolor);
    }
    protected void SetColor(Color color)
    {
        sprite.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_OverlayColor", color);
        sprite.SetPropertyBlock(propertyBlock);
    }
    public virtual void Init(float? maxhp = null, bool suicide = false)
    {
        if(maxhp != null)
            this.maxhp = maxhp.Value;
        if (GameController.Upgrades[1] > 0 && this is not ZombieController)
        {
            this.maxhp *= 1+GameController.Upgrades[1]*0.1f;
        }
        hp = this.maxhp;
        hpUI.fillAmount = HpPercent();
        hpText.text = $"{hp}/{this.maxhp}";
        hpUI.transform.parent.parent.localRotation = new Quaternion(0, 0, -gameObject.transform.parent.localRotation.z, gameObject.transform.parent.localRotation.w);
        if (suicide)
            StartCoroutine(Suicide());
    }

    IEnumerator Regeneration(int percent)
    {
        yield return new WaitForSeconds(wait);
        while (HP < maxhp)
        {
            HP += maxhp / 100 * percent;
            yield return new WaitForSeconds(1);
        }
    }
    IEnumerator Suicide()
    {
        while(true)
        {
            HP -= autoDeathDMG;
            yield return new WaitForSeconds(1);
        }
    }
}
