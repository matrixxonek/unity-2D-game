using UnityEngine;

/// Reakcja przeciwnika na obrażenia i śmierć: błysk, odrzut, cząsteczki, drop.
[RequireComponent(typeof(Health))]
public class EnemyFeedback : MonoBehaviour
{
    [Header("Wizualizacja")]
    public SpriteRenderer sprite;
    public Color deathColor = new Color(0.8f, 0.2f, 0.2f);
    public Color baseTint = Color.white;
    public bool big;

    [Header("Odrzut")]
    public float knockbackForce = 5f;
    [Range(0f, 1f)] public float knockbackResistance;
    public float stunOnHit = 0.2f;

    [Header("Drop")]
    public AmmoPickup.Kind dropKind = AmmoPickup.Kind.Pistol;
    public int dropAmount = 4;
    [Range(0f, 1f)] public float dropChance = 0.3f;

    public float StunUntil { get; private set; }

    Health health;
    Rigidbody2D rb;
    Color baseColor = Color.white;
    float flashUntil;

    void Awake()
    {
        health = GetComponent<Health>();
        rb = GetComponent<Rigidbody2D>();
        if (sprite != null) baseColor = sprite.color;
        health.Damaged += OnDamaged;
        health.Died += OnDied;
    }

    void OnDestroy()
    {
        health.Damaged -= OnDamaged;
        health.Died -= OnDied;
    }

    void OnDamaged(DamageInfo info)
    {
        flashUntil = Time.unscaledTime + 0.1f;
        StunUntil = Time.time + stunOnHit;

        if (big) Sfx.Play("boss_hit", 0.9f);
        Particles.Burst(transform.position, baseColor * deathColor, 5, 3.5f, 0.3f, 12f, 0.1f);

        if (rb == null || rb.bodyType != RigidbodyType2D.Dynamic || knockbackResistance >= 1f || !info.hasSource)
            return;

        float dir = Mathf.Sign(transform.position.x - info.source.x);
        if (dir == 0f) dir = 1f;
        float k = 1f - knockbackResistance;
        rb.linearVelocity = new Vector2(dir * knockbackForce * k, 3f * k);
    }

    void OnDied()
    {
        Particles.Burst(transform.position, deathColor, big ? 48 : 14, big ? 9f : 6f, 0.6f, 12f, big ? 0.18f : 0.12f);
        Sfx.Play(big ? "boss_die" : "enemy_die");

        if (big)
        {
            HitStop.Do(0.25f);
            CameraShake.Shake(0.7f, 0.6f);
        }
        else CameraShake.Shake(0.1f, 0.12f);

        if (dropAmount > 0 && Random.value < dropChance)
            Pickup.SpawnAmmo(transform.position, dropKind, dropAmount);

        Destroy(gameObject);
    }

    void Update()
    {
        if (sprite == null) return;
        sprite.color = Time.unscaledTime < flashUntil ? Color.white : baseColor * baseTint;
    }
}
