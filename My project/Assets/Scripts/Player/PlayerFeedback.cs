using UnityEngine;

/// Reakcja postaci na obrażenia: hit-stop, wstrząs, odrzut, błysk, miganie.
[RequireComponent(typeof(Health))]
public class PlayerFeedback : MonoBehaviour
{
    [Header("Wizualizacja")]
    public SpriteRenderer sprite;
    public Color hurtColor = new Color(1f, 0.35f, 0.3f);

    [Header("Odrzut")]
    public float knockbackX = 7f;
    public float knockbackY = 6f;
    public float controlLock = 0.18f;

    Health health;
    PlayerMovement movement;
    Color baseColor = Color.white;
    float flashUntil;

    void Awake()
    {
        health = GetComponent<Health>();
        movement = GetComponent<PlayerMovement>();
        if (sprite != null) baseColor = sprite.color;
        health.Damaged += OnDamaged;
    }

    void OnDestroy() => health.Damaged -= OnDamaged;

    void OnDamaged(DamageInfo info)
    {
        flashUntil = Time.unscaledTime + 0.12f;

        HitStop.Do(0.09f);
        CameraShake.Shake(0.28f, 0.28f);
        Sfx.Play("hurt");
        Particles.Burst(transform.position, hurtColor, 10, 5f, 0.4f, 12f, 0.11f);

        float dir = info.hasSource ? Mathf.Sign(transform.position.x - info.source.x) : -movement.Facing;
        if (dir == 0f) dir = -movement.Facing;
        movement.Knockback(new Vector2(dir * knockbackX, knockbackY), controlLock);
    }

    void Update()
    {
        if (sprite == null) return;

        if (Time.unscaledTime < flashUntil)
        {
            sprite.color = hurtColor;
            return;
        }

        if (health.IsInvulnerable && !health.IsDead)
        {
            bool visible = Mathf.FloorToInt(Time.unscaledTime * 14f) % 2 == 0;
            sprite.color = new Color(baseColor.r, baseColor.g, baseColor.b, visible ? 1f : 0.35f);
            return;
        }

        sprite.color = baseColor;
    }
}
