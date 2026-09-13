using System.Collections;
using UnityEngine;

/// Zmutowany zombie: wolny, dużo HP, uderza w ziemię i puszcza falę w obie strony.
/// Wręcz jest niebezpieczny — to wróg, który uczy pistoletu.
[RequireComponent(typeof(Rigidbody2D), typeof(Health))]
public class EnemyHeavy : MonoBehaviour
{
    [Header("Ruch")]
    public float moveSpeed = 1.4f;
    public float detectRange = 7.5f;
    public float edgeProbeAhead = 0.7f;
    public float edgeProbeDepth = 1.6f;

    [Header("Uderzenie")]
    public float slamRange = 2.4f;
    public float telegraphTime = 0.7f;
    public float recoverTime = 0.9f;
    public float slamCooldown = 2.4f;
    public float waveSpeed = 7f;
    public float waveLife = 0.7f;

    [Header("Wizualizacja")]
    public SpriteRenderer sprite;

    enum State { Idle, Walk, Telegraph, Recover }
    State state = State.Idle;

    Rigidbody2D rb;
    EnemyFeedback feedback;
    Transform player;
    int direction = -1;
    float stateTime;
    float nextSlamTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        feedback = GetComponent<EnemyFeedback>();
    }

    void Start()
    {
        var pm = FindFirstObjectByType<PlayerMovement>();
        if (pm != null) player = pm.transform;
    }

    void FixedUpdate()
    {
        stateTime += Time.fixedDeltaTime;
        float dx = player != null ? player.position.x - transform.position.x : 0f;
        float dist = player != null ? Vector2.Distance(player.position, transform.position) : 999f;

        switch (state)
        {
            case State.Idle:
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                if (player != null && dist < detectRange) Enter(State.Walk);
                break;

            case State.Walk:
            {
                if (dist > detectRange * 1.3f) { Enter(State.Idle); break; }
                if (dist < slamRange && Time.time >= nextSlamTime)
                {
                    Enter(State.Telegraph);
                    Sfx.Play("telegraph", 0.7f, 0.8f);
                    if (feedback != null) feedback.baseTint = new Color(1f, 0.65f, 0.55f);
                    break;
                }

                direction = dx > 0 ? 1 : -1;
                Vector2 ahead = (Vector2)transform.position + new Vector2(direction * edgeProbeAhead, 0f);
                bool groundAhead = Physics2D.Raycast(ahead, Vector2.down, edgeProbeDepth, Layers.GroundMask);
                bool wallAhead = Physics2D.Raycast(transform.position, new Vector2(direction, 0f), edgeProbeAhead + 0.1f, Layers.GroundMask);

                float vx = (!groundAhead || wallAhead) ? 0f : direction * moveSpeed;
                if (feedback != null && Time.time < feedback.StunUntil) vx = rb.linearVelocity.x;
                rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y);
                break;
            }

            case State.Telegraph:
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                if (sprite != null)
                    sprite.transform.localPosition = new Vector3(Random.Range(-0.05f, 0.05f), 0.1f * Mathf.Sin(stateTime * 30f), 0f);
                if (stateTime >= telegraphTime) Slam();
                break;

            case State.Recover:
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                if (stateTime >= recoverTime) Enter(State.Walk);
                break;
        }

        if (sprite != null) sprite.flipX = direction < 0;
    }

    void Enter(State s)
    {
        state = s;
        stateTime = 0f;
        if (sprite != null) sprite.transform.localPosition = Vector3.zero;
        if (feedback != null && s != State.Telegraph) feedback.baseTint = Color.white;
    }

    void Slam()
    {
        nextSlamTime = Time.time + slamCooldown;
        Enter(State.Recover);

        Sfx.Play("slam", 0.9f);
        CameraShake.Shake(0.35f, 0.3f);
        Vector2 feet = (Vector2)transform.position + new Vector2(0f, -0.6f);
        Particles.Burst(feet, new Color(0.75f, 0.68f, 0.55f), 14, 4f, 0.4f, 10f, 0.11f, 160f, 90f);

        Shockwave.Spawn(feet, 1, waveSpeed, waveLife, 1);
        Shockwave.Spawn(feet, -1, waveSpeed, waveLife, 1);
    }
}
