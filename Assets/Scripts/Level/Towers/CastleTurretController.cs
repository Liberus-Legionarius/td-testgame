using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CastleTurretController : BaseFirearms
{
    [SerializeField] SpriteRenderer radius;
    protected override void Extra()
    {
        float r = GetComponent<CircleCollider2D>().radius*2;
        radius.transform.localScale = new Vector2(r, r);
        gun = new Transform[] { transform };
    }

    public void Init(int damage, float reload)
    {
        this.damage = damage;
        this.reload = reload;
    }
}
