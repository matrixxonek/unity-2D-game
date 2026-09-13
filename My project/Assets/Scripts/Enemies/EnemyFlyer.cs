using UnityEngine;

/// Mucha: unosi się przy suficie, a gdy gracz podejdzie — telegrafuje i nurkuje.
/// Wersja nieagresywna tylko wisi w miejscu — jako cel do pogo.
[RequireComponent(typeof(Rigidbody2D), typeof(Health))]
public class EnemyFlyer : MonoBehaviour
{
    [Header("Zachowanie")]
    public bool aggressive = true;
    public float hoverAmplitude = 0.35f;
    public float hoverSpeed = 2.5f;
    public float detectRange = 6.5f;
    public float diveSpeed = 12f;
    public float telegraphTime = 0.45f;
    public float diveCooldown = 2.2f;

    [Header("Wizualizacja")]
    public SpriteRenderer sprite;

    enum State { Hover, Telegraph, Dive, Return }
    State state = State.Hover;

    Rigidbody2D rb;
    Vector2 anchor;
    Vector2 diveTarget;
    Transform player;
    float stateTime;
    float nextDiveTime;
    float phase;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        anchor = transform.position;
        phase = Random.value * 10f;
    }

    void Start()
    {
        var pm = FindFirstObjectByType<PlayerMovement>();
        if (pm != null) player = pm.transform;
    }

    void FixedUpdate()
    {
        stateTime += Time.fixedDeltaTime;
        Vector2 pos = rb.position;

        switch (state)
        {
            case State.Hover:
            {
                Vector2 target = anchor + new Vector2(0f, Mathf.Sin((Time.time + phase) * hoverSpeed) * hoverAmplitude);
                rb.MovePosition(Vector2.MoveTowards(pos, target, 3f * Time.fixedDeltaTime));

                if (aggressive && player != null && Time.time >= nextDiveTime &&
                    Vector2.Distance(player.position, pos) < detectRange)
                {
                    Enter(State.Telegraph);
                    Sfx.Play("telegraph", 0.5f, 1.6f);
                }
                break;
            }
            case State.Telegraph:
            {
                // Drży w miejscu, żeby gracz zdążył zauważyć.
                rb.MovePosition(pos + Random.insideUnitCircle * 0.04f);
                if (stateTime >= telegraphTime && player != null)
                {
                    diveTarget = (Vector2)player.position + new Vector2(0f, -0.2f);
                    Enter(State.Dive);
                    Sfx.Play("dive", 0.6f);
                }
                break;
            }
            case State.Dive:
            {
                rb.MovePosition(Vector2.MoveTowards(pos, diveTarget, diveSpeed * Time.fixedDeltaTime));
                if (Vector2.Distance(pos, diveTarget) < 0.15f || stateTime > 0.8f)
                    Enter(State.Return);
                break;
            }
            case State.Return:
            {
                rb.MovePosition(Vector2.MoveTowards(pos, anchor, 5f * Time.fixedDeltaTime));
                if (Vector2.Distance(pos, anchor) < 0.1f)
                {
                    Enter(State.Hover);
                    nextDiveTime = Time.time + diveCooldown;
                }
                break;
            }
        }

        if (sprite != null && player != null)
            sprite.flipX = player.position.x < transform.position.x;
    }

    void Enter(State s)
    {
        state = s;
        stateTime = 0f;
    }
}
