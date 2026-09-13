using System.Collections;
using UnityEngine;

/// ALFA — większy zmutowany. Boss samouczka.
///
/// Trzy ataki, każdy z czytelnym telegrafem:
///   - szarża przez arenę (kończy się ogłuszeniem o ścianę = okno na rewolwer)
///   - uderzenie w ziemię z dwiema falami na stronę
///   - skok na gracza z falą przy lądowaniu
/// Przy połowie HP wchodzi faza 2: szybciej i przyzywa dwie muchy.
[RequireComponent(typeof(Rigidbody2D), typeof(Health))]
public class Boss : MonoBehaviour
{
    [Header("Boss")]
    public string bossName = "ALFA";
    public float arenaLeft = 158f;
    public float arenaRight = 182f;

    [Header("Ruch")]
    public float walkSpeed = 2.2f;
    public float chargeSpeed = 12f;
    public float leapVx = 9f;
    public float leapVy = 16f;

    [Header("Czasy")]
    public float idleTime = 1.3f;
    public float telegraphTime = 0.75f;
    public float stunTime = 2.3f;
    public float recoverTime = 0.8f;

    [Header("Wizualizacja")]
    public SpriteRenderer sprite;

    enum State { Dormant, Idle, ChargeTelegraph, Charge, Stunned, SlamTelegraph, LeapTelegraph, Leap, Recover, Dead }
    State state = State.Dormant;

    public bool IsActive => state != State.Dormant && state != State.Dead;
    public bool IsStunned => state == State.Stunned;
    public bool IsCharging => state == State.Charge || state == State.Leap;
    public bool IsTelegraphing => state == State.ChargeTelegraph || state == State.SlamTelegraph || state == State.LeapTelegraph;
    public Health Health => health;

    Rigidbody2D rb;
    Health health;
    EnemyFeedback feedback;
    ContactDamage contact;
    Transform player;
    float stateTime;
    int dir = -1;
    bool phase2;
    bool airborne;
    Vector3 startPos;
    readonly System.Collections.Generic.List<GameObject> spawned = new System.Collections.Generic.List<GameObject>();

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
        feedback = GetComponent<EnemyFeedback>();
        contact = GetComponent<ContactDamage>();
        health.Died += OnDied;
        startPos = transform.position;
    }

    void OnDestroy() => health.Died -= OnDied;

    /// Po śmierci gracza arena wraca do stanu wyjściowego.
    public void ResetToDormant()
    {
        if (state == State.Dead) return;
        StopAllCoroutines();
        foreach (var go in spawned) if (go != null) Destroy(go);
        spawned.Clear();
        phase2 = false;
        transform.position = startPos;
        rb.linearVelocity = Vector2.zero;
        health.RestoreFull();
        if (contact != null) contact.active = true;
        if (feedback != null) feedback.baseTint = Color.white;
        state = State.Dormant;
        stateTime = 0f;
    }

    void Start()
    {
        var pm = FindFirstObjectByType<PlayerMovement>();
        if (pm != null) player = pm.transform;
    }

    public void Activate()
    {
        if (state != State.Dormant) return;
        Enter(State.Idle);
    }

    void Enter(State s)
    {
        state = s;
        stateTime = 0f;
        if (sprite != null) sprite.transform.localPosition = Vector3.zero;

        switch (s)
        {
            case State.Idle:
            case State.Recover:
                if (contact != null) contact.active = true;
                if (feedback != null) feedback.baseTint = Color.white;
                break;

            case State.ChargeTelegraph:
            case State.SlamTelegraph:
            case State.LeapTelegraph:
                Sfx.Play("telegraph", 0.8f, 0.7f);
                if (feedback != null) feedback.baseTint = new Color(1f, 0.6f, 0.55f);
                break;

            case State.Charge:
                Sfx.Play("dive", 0.8f, 0.55f);
                if (feedback != null) feedback.baseTint = Color.white;
                rb.linearVelocity = new Vector2(dir * chargeSpeed * SpeedMult, rb.linearVelocity.y);
                break;

            case State.Stunned:
                if (contact != null) contact.active = false;
                if (feedback != null) feedback.baseTint = new Color(0.6f, 0.85f, 1f);
                Sfx.Play("slam", 1f, 0.8f);
                CameraShake.Shake(0.5f, 0.4f);
                HitStop.Do(0.06f);
                Particles.Burst(transform.position + new Vector3(dir * 1f, 0f, 0f), new Color(0.9f, 0.85f, 0.7f), 18, 6f, 0.5f, 12f, 0.13f);
                rb.linearVelocity = new Vector2(-dir * 3f, 5f);
                HUD.Message("ALFA ogłuszony — teraz!", 1.2f);
                break;

            case State.Leap:
                if (feedback != null) feedback.baseTint = Color.white;
                rb.linearVelocity = new Vector2(dir * leapVx * SpeedMult, leapVy);
                airborne = false;
                Sfx.Play("jump", 0.8f, 0.5f);
                break;

            case State.Dead:
                if (contact != null) contact.active = false;
                rb.linearVelocity = Vector2.zero;
                break;
        }
    }

    float SpeedMult => phase2 ? 1.3f : 1f;
    float TimeMult => phase2 ? 0.7f : 1f;

    void Update()
    {
        if (!IsActive) return;
        stateTime += Time.deltaTime;

        if (HUD.I != null) HUD.I.SetBoss(health.Current / (float)health.maxHealth);

        if (!phase2 && health.Current <= health.maxHealth / 2)
        {
            phase2 = true;
            HUD.Message("ALFA szaleje!", 1.6f);
            Sfx.Play("telegraph", 1f, 0.5f);
            spawned.Add(EnemyFactory.SpawnFlyer(new Vector2(arenaLeft + 3f, 6.5f), true));
            spawned.Add(EnemyFactory.SpawnFlyer(new Vector2(arenaRight - 3f, 6.5f), true));
        }

        bool bouncing = state == State.Charge || state == State.Leap;
        if (!bouncing && player != null)
            dir = player.position.x > transform.position.x ? 1 : -1;
        if (sprite != null) sprite.flipX = dir < 0;
    }

    void FixedUpdate()
    {
        if (!IsActive) return;

        switch (state)
        {
            case State.Idle:
                rb.linearVelocity = new Vector2(dir * walkSpeed * SpeedMult, rb.linearVelocity.y);
                if (stateTime >= idleTime * TimeMult) ChooseAttack();
                break;

            case State.ChargeTelegraph:
            case State.SlamTelegraph:
            case State.LeapTelegraph:
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                if (sprite != null)
                    sprite.transform.localPosition = new Vector3(Random.Range(-0.06f, 0.06f), 0.08f * Mathf.Sin(stateTime * 40f), 0f);
                if (stateTime >= telegraphTime * TimeMult)
                {
                    if (state == State.ChargeTelegraph) Enter(State.Charge);
                    else if (state == State.SlamTelegraph) { Slam(2); Enter(State.Recover); }
                    else Enter(State.Leap);
                }
                break;

            case State.Charge:
            {
                rb.linearVelocity = new Vector2(dir * chargeSpeed * SpeedMult, rb.linearVelocity.y);
                bool wall = Physics2D.Raycast(transform.position, new Vector2(dir, 0f), 1.5f, Layers.GroundMask);
                bool edge = transform.position.x <= arenaLeft + 1.4f || transform.position.x >= arenaRight - 1.4f;
                if (wall || edge || stateTime > 2.5f) Enter(State.Stunned);
                break;
            }

            case State.Stunned:
                rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.85f, rb.linearVelocity.y);
                if (stateTime >= stunTime) Enter(State.Idle);
                break;

            case State.Leap:
            {
                if (rb.linearVelocity.y < 0f) airborne = true;
                bool grounded = Physics2D.Raycast(transform.position, Vector2.down, 1.4f, Layers.GroundMask);
                if (airborne && grounded && stateTime > 0.3f)
                {
                    Slam(1);
                    Enter(State.Recover);
                }
                else if (stateTime > 3f) Enter(State.Recover);
                break;
            }

            case State.Recover:
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                if (stateTime >= recoverTime * TimeMult) Enter(State.Idle);
                break;
        }
    }

    void ChooseAttack()
    {
        float dist = player != null ? Mathf.Abs(player.position.x - transform.position.x) : 0f;
        float r = Random.value;

        if (dist > 5f) Enter(r < 0.65f ? State.ChargeTelegraph : State.LeapTelegraph);
        else Enter(r < 0.55f ? State.SlamTelegraph : State.LeapTelegraph);
    }

    void Slam(int waves)
    {
        Sfx.Play("slam", 1f, 0.75f);
        CameraShake.Shake(0.5f, 0.35f);
        Vector2 feet = (Vector2)transform.position + new Vector2(0f, -1.1f);
        Particles.Burst(feet, new Color(0.8f, 0.72f, 0.58f), 22, 5f, 0.45f, 10f, 0.13f, 170f, 90f);

        Shockwave.Spawn(feet, 1, 8.5f, 1.5f, 1, 0.8f);
        Shockwave.Spawn(feet, -1, 8.5f, 1.5f, 1, 0.8f);
        if (waves > 1) StartCoroutine(DelayedWave(feet, 0.32f));
    }

    IEnumerator DelayedWave(Vector2 feet, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (state == State.Dead) yield break;
        Sfx.Play("slam", 0.7f, 0.9f);
        Shockwave.Spawn(feet, 1, 8.5f, 1.5f, 1, 0.8f);
        Shockwave.Spawn(feet, -1, 8.5f, 1.5f, 1, 0.8f);
    }

    void OnDied()
    {
        Enter(State.Dead);
        StopAllCoroutines();
        foreach (var go in spawned)
        {
            if (go == null) continue;
            Particles.Burst(go.transform.position, new Color(0.7f, 0.4f, 0.9f), 10, 4f, 0.4f, 0f, 0.1f);
            Destroy(go);
        }
        spawned.Clear();
    }
}
