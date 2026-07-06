using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class Level5Controller : LevelController
{
    [SerializeField] GameObject convoy;
    [SerializeField] GameObject[] convoyPaths;
    [SerializeField] string[] convoyAngles;
    [SerializeField] AudioResource final;

    Transform[][] convoyPoints;
    int[][] realConvoyAngles;
    public bool Succes { get; set; } = false;
    bool started = false;
    Transform convoyTransform;
    

    protected override void Start()
    {
        base.Start();
        SetPaths(ref convoyPoints, convoyPaths);
        SetAngles(ref realConvoyAngles, convoyAngles, convoyPaths.Length);
    }
    void Update()
    {
        if (canSummon && !started)
        {
            started = true;
            int path = Random.Range(0, convoyPoints.Length);
            var c = Instantiate(convoy, convoyPoints[path][0]);
            var ctrl = c.GetComponent<ConvoyController>();
            ctrl.Init(convoyPoints[path], realConvoyAngles[path], convoyPoints[path][1].position);
            convoyTransform = c.transform;
        }
    }


    protected override void Summoning(List<int> enemies, bool isFly = false)
    {
        base.Summoning(enemies, isFly);
        if(Random.Range(0, 101) < 25 && ui.TimerReal > 5)
        {
            int way = Random.Range(0,convoyPoints.Length);
            var zombie = Instantiate(GameController.Enemies[enemies[Random.Range(0,enemies.Count)]], convoyPoints[way][1]);
            ZombieController controller = zombie.GetComponent<ZombieController>();
            controller.Init(convoyPoints[way], isFly ? new int[] { 0, 0 } : realConvoyAngles[way], GetRandomizedLocation(controller.gameObject.transform), nowDay);
            Zombies.Add(zombie);
        }
    }

    public void Finish()
    {
        canSummon = false;
        WaveCount = 1;
        music.Stop();
        music.resource = final;
        music.Play();
        if(horde != null)
        {
            StopCoroutine(horde);
        }
        horde = StartCoroutine(HordeSummoning(true));
    }
    public override Transform GetGoal()
    {
        return convoyTransform == null? CastleController.GetCastle().transform : convoyTransform;
    }
}
