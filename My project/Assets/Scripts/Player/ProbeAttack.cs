using UnityEngine;

/// Próbnik — teleskopowy pręt z chwytakiem. Jedyne narzędzie walki wręcz i jednocześnie
/// narzędzie misji.
///
/// Ten sam przycisk obsługuje wszystkie konteksty:
///   - przeciwnik              -> dźgnięcie (i pobranie ładunku)
///   - złoże                   -> pobranie próbki
///   - kryształ                -> pobranie ładunku
///   - atak w dół w powietrzu  -> pogo, odbicie od czegokolwiek trafionego
public class ProbeAttack : MonoBehaviour
{
    [Header("Próbnik")]
    public int damage = 2;
    public float range = 1.5f;
    public float width = 1.0f;
    public float cooldown = 0.28f;

    [Tooltip("Ile ładunku daje jedno trafienie w żywy cel.")]
    public int chargePerHit = 1;

    [Header("Pogo")]
    public float pogoForce = 13f;

    [Header("Wizualizacja")]
    public float swingVisualTime = 0.09f;

    public bool CanSwing => Time.time >= nextSwingTime;

    PlayerInputReader input;
    PlayerMovement movement;
    ChargeMeter charge;
    SequenceRunner sequences;
    Rigidbody2D rb;

    Transform visual;
    float nextSwingTime;
    float visualHideTime;

    void Awake()
    {
        input = GetComponent<PlayerInputReader>();
        movement = GetComponent<PlayerMovement>();
        charge = GetComponent<ChargeMeter>();
        sequences = GetComponent<SequenceRunner>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        var lib = SpriteLibrary.I;
        var go = new GameObject("ProbeVisual");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = lib != null && lib.probe != null ? lib.probe : Visuals.Dot;
        sr.sharedMaterial = Visuals.Unlit;
        sr.sortingOrder = 7;
        go.SetActive(false);
        visual = go.transform;
    }

    void OnDestroy()
    {
        if (visual != null) Destroy(visual.gameObject);
    }

    void Update()
    {
        if (visual != null && visual.gameObject.activeSelf && Time.time >= visualHideTime)
            visual.gameObject.SetActive(false);

        if (!input.ProbePressed || !CanSwing) return;
        if (sequences != null && sequences.IsRunning) return;

        Swing();
    }

    void Swing()
    {
        nextSwingTime = Time.time + cooldown;

        bool downward = input.HoldDown && !movement.IsGrounded;
        Vector2 dir = downward ? Vector2.down
                    : input.HoldUp ? Vector2.up
                    : new Vector2(movement.Facing, 0f);

        Vector2 center = (Vector2)transform.position + dir * (range * 0.5f);
        Vector2 size = dir.y != 0f ? new Vector2(width, range) : new Vector2(range, width);

        ShowSwing(dir);
        Sfx.Play("swing", 0.7f);

        bool hitSomething = false;
        foreach (var col in Physics2D.OverlapBoxAll(center, size, 0f))
        {
            if (col.transform.IsChildOf(transform)) continue;

            var crystal = col.GetComponentInParent<ChargeCrystal>();
            if (crystal != null)
            {
                crystal.TryHarvest(charge);
                hitSomething = true;
                continue;
            }

            var targetHealth = col.GetComponentInParent<Health>();
            if (targetHealth != null && !targetHealth.IsDead)
            {
                if (targetHealth.TakeDamage(damage, transform.position))
                    charge?.Add(chargePerHit);
                hitSomething = true;
                continue;
            }

            var sample = col.GetComponentInParent<SampleNode>();
            if (sample != null)
            {
                if (!sample.Collected) sample.Collect();
                hitSomething = true;
                continue;
            }

            if (col.GetComponentInParent<PogoSurface>() != null)
                hitSomething = true;
        }

        if (!hitSomething) return;

        Sfx.Play("hit");
        HitStop.Do(0.045f);
        CameraShake.Shake(0.12f, 0.15f);
        Particles.Burst(center, new Color(1f, 0.95f, 0.8f), 6, 4f, 0.25f, 10f, 0.09f);
        PlayerEvents.RaiseProbeHit();

        // Pogo: odbicie od trafionego celu. Zwraca też double jump, więc łańcuch
        // odbić utrzymuje gracza w powietrzu.
        if (downward && rb != null)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, pogoForce);
            movement.RefreshDoubleJump();
            Sfx.Play("pogo");
            CameraShake.Shake(0.16f, 0.18f);
            PlayerEvents.RaisePogoed();
        }
    }

    void ShowSwing(Vector2 dir)
    {
        if (visual == null) return;

        visual.gameObject.SetActive(true);
        visual.position = transform.position + new Vector3(0f, 0.05f, 0f);
        visual.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        visual.localScale = new Vector3(range, 1f, 1f);
        visualHideTime = Time.time + swingVisualTime;
    }
}
