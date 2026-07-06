using System.Collections;
using UnityEngine;

public class MoveEntityController : DestroyableAbsController
{
    [SerializeField] protected int speed;
    [SerializeField] bool scripting = true;
    Transform[] path;
    int[] angles;
    Rigidbody2D rb;
    protected float timer = 0;
    int curPos = 2;
    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        if (path.Length > 1)
        {
            var v = path[2].position - path[1].position;
            float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
            rb.SetRotation(angle);
        }

    }

    protected override void Update()
    {
        base.Update();
        if (scripting)
        {
            timer += Time.deltaTime;
            if (curPos < path.Length)
            {
                rb.linearVelocity = transform.right * speed;
                if (Vector2.Distance(path[curPos].position, transform.position) <= 15)
                {
                    StartCoroutine(Rotate(angles[curPos - 1]));
                    curPos++;
                }
            }
        }
        else rb.linearVelocity = transform.right * speed;
    }

    public virtual void Init(Transform[] path, int[] angles, Vector2 pos)
    {
        this.path = path;
        this.angles = angles;
        transform.localPosition = pos;
    }

    IEnumerator Rotate(int angle)
    {
        int inc = speed / 10;
        if (rb.rotation > angle)
        {
            inc *= -1;
        }
        while (Mathf.Abs(rb.rotation - angle) > 2)
        {
            rb.MoveRotation(rb.rotation + inc);
            yield return new WaitForFixedUpdate();
        }
        rb.SetRotation(angle);
    }
}
