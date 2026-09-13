using UnityEngine;

/// Fala uderzeniowa po uderzeniu w ziemię: jedzie po podłodze, rani, zatrzymuje się na ścianie.
public class Shockwave : MonoBehaviour
{
    public int direction = 1;
    public float speed = 8f;
    public float life = 1.4f;
    public int damage = 1;

    float age;
    float dustTimer;

    public static Shockwave Spawn(Vector2 pos, int direction, float speed, float life, int damage, float height = 0.7f)
    {
        var lib = SpriteLibrary.I;
        var go = new GameObject("Shockwave");
        go.transform.position = pos;
        go.layer = Layers.Enemy;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = lib != null && lib.shockwave != null ? lib.shockwave : Visuals.Dot;
        sr.sharedMaterial = Visuals.Unlit;
        sr.sortingOrder = 5;
        sr.flipX = direction < 0;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.8f, height);
        col.offset = new Vector2(0f, height * 0.5f - 0.1f);

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        var wave = go.AddComponent<Shockwave>();
        wave.direction = direction;
        wave.speed = speed;
        wave.life = life;
        wave.damage = damage;
        return wave;
    }

    void Update()
    {
        age += Time.deltaTime;
        if (age >= life)
        {
            Destroy(gameObject);
            return;
        }

        // Ściana zatrzymuje falę.
        if (Physics2D.Raycast(transform.position + Vector3.up * 0.3f, new Vector2(direction, 0f), 0.6f, Layers.GroundMask))
        {
            Destroy(gameObject);
            return;
        }

        transform.position += new Vector3(direction * speed * Time.deltaTime, 0f, 0f);

        dustTimer -= Time.deltaTime;
        if (dustTimer <= 0f)
        {
            dustTimer = 0.04f;
            Particles.Burst(transform.position, new Color(0.75f, 0.68f, 0.55f), 2, 2.5f, 0.3f, 6f, 0.1f, 80f, 90f);
        }
    }

    void OnTriggerEnter2D(Collider2D other) => Hurt(other);
    void OnTriggerStay2D(Collider2D other) => Hurt(other);

    void Hurt(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerMovement>() == null) return;
        other.GetComponentInParent<Health>()?.TakeDamage(damage, transform.position);
    }
}
