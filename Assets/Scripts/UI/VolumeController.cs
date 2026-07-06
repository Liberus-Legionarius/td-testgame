using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    [SerializeField] Slider[] volumes;
    [SerializeField] AudioClip[] effects;
    AudioSource audioS;
    private void Awake()
    {
        audioS = GetComponent<AudioSource>();
    }
    public void OnVolumeChange(int i)
    {
        GameController.SaveSettings(i == 0 ? AudioType.Global : i == 1 ? AudioType.Music : i == 2 ? AudioType.Sound : AudioType.Environment, volumes[i].value);
        if (i > 0 && audioS !=null)
        {
            audioS.volume = volumes[i].value * volumes[0].value;
            audioS.resource = effects[i];
            audioS.PlayOneShot(effects[i]);
        }
    }
}
