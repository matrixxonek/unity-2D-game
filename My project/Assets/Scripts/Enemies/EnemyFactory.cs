using UnityEngine;

/// Przeciwnicy spawnowani w runtime (np. przez bossa). Poziom buduje ich edytorowo,
/// więc tu jest tylko to, czego potrzebuje rozgrywka.
public static class EnemyFactory
{
    public static GameObject SpawnFlyer(Vector2 pos, bool aggressive)
    {
        var lib = SpriteLibrary.I;
        var go = new GameObject("Flyer");
        go.transform.position = pos;
        go.layer = Layers.Enemy;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = lib != null && lib.flyer != null ? lib.flyer : Visuals.Dot;
        sr.sharedMaterial = Visuals.Unlit;
        sr.sortingOrder = 4;

        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.3f;
        col.isTrigger = true;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var hp = go.AddComponent<Health>();
        hp.maxHealth = 2;
        hp.invulnerabilityTime = 0.1f;
        hp.RestoreFull();

        var fb = go.AddComponent<EnemyFeedback>();
        fb.sprite = sr;
        fb.deathColor = new Color(0.7f, 0.4f, 0.9f);
        fb.knockbackResistance = 1f;
        fb.dropChance = 0.2f;

        var cd = go.AddComponent<ContactDamage>();
        cd.damage = 1;

        var fl = go.AddComponent<EnemyFlyer>();
        fl.aggressive = aggressive;
        fl.sprite = sr;

        Particles.Burst(pos, new Color(0.7f, 0.4f, 0.9f), 10, 4f, 0.4f, 0f, 0.1f);
        return go;
    }
}
