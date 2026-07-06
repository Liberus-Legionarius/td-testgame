using System.Collections;
using System.IO;
using UnityEngine;

public class ConvoyController : MoveEntityController
{
    AudioSource soundConvoy;
    protected override void Awake()
    {
        base.Awake();
        soundConvoy = GetComponent<AudioSource>();
        GameController.SetVolume(soundConvoy, AudioType.Sound);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject == CastleController.GetCastle().gameObject)
        {
            LevelController lvl = LevelController.GetLevel();
            if(lvl is Level5Controller level)
            {
                level.Finish();
                level.Succes = true;
                Destroy(gameObject);
            }
        }
    }
    private void OnDestroy()
    {
        LevelController lvl = LevelController.GetLevel();
        if (lvl is Level5Controller level)
        {
            if(!level.Succes)
            {
                level.GetUI().WinLose();
            }
        }
    }
}
