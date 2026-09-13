using UnityEngine;

/// Własny, minimalny system cząsteczek na sprite'ach. Jedna pula, zero konfiguracji.
public class Particles : MonoBehaviour
{
    class P
    {
        public Transform t;
        public SpriteRenderer r;
        public Vector2 v;
        public float life, maxLife, size, gravity;
        public Color c;
        public bool alive;
    }

    static Particles instance;
    readonly P[] pool = new P[320];
    int next;

    static Particles Instance
    {
        get
        {
            if (instance == null)
                instance = new GameObject("Particles").AddComponent<Particles>();
            return instance;
        }
    }

    void Awake() => EnsurePool();

    /// Po domain reload w edytorze tablica wraca pusta, a Awake nie odpala się ponownie.
    void EnsurePool()
    {
        if (pool[0] != null && pool[0].t != null) return;
        foreach (Transform child in transform) Destroy(child.gameObject);
        for (int i = 0; i < pool.Length; i++)
        {
            var go = new GameObject("p");
            go.transform.SetParent(transform, false);
            var r = go.AddComponent<SpriteRenderer>();
            r.sprite = Visuals.Dot;
            r.sharedMaterial = Visuals.Unlit;
            r.sortingOrder = 30;
            go.SetActive(false);
            pool[i] = new P { t = go.transform, r = r };
        }
    }

    public static void Burst(Vector2 pos, Color color, int count, float speed, float life,
                             float gravity = 18f, float size = 0.12f, float spreadDeg = 360f, float dirDeg = 90f)
    {
        var inst = Instance;
        inst.EnsurePool();
        for (int n = 0; n < count; n++)
        {
            var p = inst.pool[inst.next];
            inst.next = (inst.next + 1) % inst.pool.Length;

            float a = (dirDeg + Random.Range(-spreadDeg * 0.5f, spreadDeg * 0.5f)) * Mathf.Deg2Rad;
            p.v = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * speed * Random.Range(0.45f, 1.2f);
            p.maxLife = p.life = life * Random.Range(0.7f, 1.3f);
            p.gravity = gravity;
            p.size = size * Random.Range(0.7f, 1.3f);
            p.c = color;
            p.alive = true;

            p.t.position = pos;
            p.t.localScale = Vector3.one * p.size;
            p.r.color = color;
            p.t.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        EnsurePool();
        float dt = Time.deltaTime;
        foreach (var p in pool)
        {
            if (p == null || !p.alive) continue;

            p.life -= dt;
            if (p.life <= 0f)
            {
                p.alive = false;
                p.t.gameObject.SetActive(false);
                continue;
            }

            p.v.y -= p.gravity * dt;
            p.t.position += (Vector3)(p.v * dt);

            float k = p.life / p.maxLife;
            p.t.localScale = Vector3.one * p.size * k;
            p.r.color = new Color(p.c.r, p.c.g, p.c.b, p.c.a * k);
        }
    }
}
