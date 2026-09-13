using UnityEngine;

/// Baza pickupów: unosi się, a przy dotknięciu gracza aplikuje efekt i znika.
[RequireComponent(typeof(Collider2D))]
public abstract class Pickup : MonoBehaviour
{
    public float bobAmplitude = 0.12f;
    public float bobSpeed = 3f;

    Vector3 basePos;
    float phase;

    protected virtual void Awake()
    {
        basePos = transform.position;
        phase = Random.value * 10f;
    }

    void Update()
    {
        transform.position = basePos + new Vector3(0f, Mathf.Sin((Time.time + phase) * bobSpeed) * bobAmplitude, 0f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponentInParent<PlayerMovement>();
        if (player == null) return;
        if (!Apply(player.gameObject)) return;

        Sfx.Play(SfxKey);
        Particles.Burst(transform.position, ParticleColor, 12, 3.5f, 0.45f, 4f, 0.1f);
        Destroy(gameObject);
    }

    protected abstract bool Apply(GameObject player);
    protected virtual string SfxKey => "pickup";
    protected virtual Color ParticleColor => new Color(1f, 0.9f, 0.5f);

    /// Amunicja spawnowana w runtime (drop z przeciwnika).
    public static AmmoPickup SpawnAmmo(Vector2 pos, AmmoPickup.Kind kind, int amount)
    {
        var lib = SpriteLibrary.I;
        var go = new GameObject("AmmoDrop");
        go.transform.position = pos + Vector2.up * 0.3f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = lib == null ? Visuals.Dot
                  : kind == AmmoPickup.Kind.Pistol ? lib.ammoPistol : lib.ammoRevolver;
        sr.sharedMaterial = Visuals.Unlit;
        sr.sortingOrder = 3;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.6f, 0.5f);

        var pickup = go.AddComponent<AmmoPickup>();
        pickup.kind = kind;
        pickup.amount = amount;
        return pickup;
    }
}
