using System.Collections;
using UnityEngine;

/// Kryształowa platforma, która kruszy się pod ciężarem gracza i odrasta po chwili.
[RequireComponent(typeof(Collider2D))]
public class CrumblingPlatform : MonoBehaviour
{
    public float delay = 0.45f;
    public float respawnAfter = 2.6f;

    Collider2D col;
    SpriteRenderer[] sprites;
    Vector3 basePos;
    bool triggered;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        sprites = GetComponentsInChildren<SpriteRenderer>();
        basePos = transform.position;
    }

    void OnCollisionEnter2D(Collision2D c) => Check(c);
    void OnCollisionStay2D(Collision2D c) => Check(c);

    void Check(Collision2D c)
    {
        if (triggered) return;
        if (c.collider.GetComponentInParent<PlayerMovement>() == null) return;
        if (c.collider.bounds.min.y < col.bounds.max.y - 0.3f) return;   // tylko z góry

        triggered = true;
        StartCoroutine(Crumble());
    }

    IEnumerator Crumble()
    {
        Sfx.Play("crumble", 0.8f);
        float t = 0f;
        while (t < delay)
        {
            t += Time.deltaTime;
            transform.position = basePos + (Vector3)(Random.insideUnitCircle * 0.05f);
            yield return null;
        }

        transform.position = basePos;
        Particles.Burst(transform.position, new Color(0.6f, 0.55f, 0.45f), 12, 3f, 0.5f, 14f, 0.1f);
        col.enabled = false;
        foreach (var s in sprites) s.enabled = false;

        yield return new WaitForSeconds(respawnAfter);

        col.enabled = true;
        foreach (var s in sprites) s.enabled = true;
        Particles.Burst(transform.position, new Color(0.8f, 0.75f, 0.6f), 6, 1.5f, 0.3f, 0f, 0.08f);
        triggered = false;
    }
}
