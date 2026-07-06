using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using static UnityEngine.GraphicsBuffer;

public class BulletExpController : BulletController
{
    [SerializeField] float expRadius;
    [SerializeField] GameObject boomEffect;
    [SerializeField] GameObject boomMark;
    [SerializeField] AudioClip boomSound;
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        if (canBeat.Contains(collision.gameObject.layer) && target == null)
        {
            LevelController.GetLevel().DoBoom(transform.position, boomEffect, expRadius*2, boomMark, boomSound);
            foreach(var zombie in Physics2D.OverlapCircleAll(transform.position, expRadius))
            {
                if (collision.gameObject.layer == zombie.gameObject.layer)
                    DamageZombie(zombie.gameObject, Mathf.RoundToInt(damage * (1 - Vector2.Distance(zombie.transform.position, transform.position)/expRadius)), 0.5f);
            }
            DamageZombie(collision.gameObject, damage);
            Destroy(gameObject);
        }
    }

    protected override void Attack()
    {
        LevelController.GetLevel().DoBoom(transform.position, boomEffect, expRadius * 2, boomMark, boomSound);
        foreach (var zombie in Physics2D.OverlapCircleAll(transform.position, expRadius))
        {
            if (target.layer == zombie.gameObject.layer)
                DamageZombie(zombie.gameObject, Mathf.RoundToInt(damage * (1 - Vector2.Distance(zombie.transform.position, transform.position) / expRadius)), 0.5f);
        }
        DamageZombie(target, damage);
        Destroy(gameObject);
    }

}
