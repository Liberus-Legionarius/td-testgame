using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BulletMagmaController : MonoBehaviour
{
    [SerializeField] float lifeTime;
    [SerializeField] float period;
    [SerializeField] int damage;
    float timer;

    private void Awake()
    {
        timer = period;
    }
    private void FixedUpdate()
    {
        lifeTime -= Time.deltaTime;
        timer -= Time.deltaTime;
        if(timer <= 0)
        {
            foreach(var zombieObj in Physics2D.OverlapCircleAll(transform.position, GetComponent<CircleCollider2D>().radius))
            {
                if(zombieObj.gameObject.layer == 6)
                {
                    zombieObj.GetComponent<ZombieController>().HP -= damage;
                }
            }
            timer = period;
        }
        if(lifeTime < 0)
            Destroy(gameObject);
    }
}
