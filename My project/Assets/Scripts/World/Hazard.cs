using System.Collections;
using UnityEngine;

/// Kolce, kwas, dno przepaści. Jak w Hollow Knight: obrażenia + powrót na ostatnie
/// bezpieczne miejsce, zamiast śmierci.
[RequireComponent(typeof(Collider2D))]
public class Hazard : MonoBehaviour
{
    public int damage = 1;
    public bool returnToSafeSpot = true;

    void Reset() => GetComponent<Collider2D>().isTrigger = true;

    void OnTriggerEnter2D(Collider2D other) => Touch(other);
    void OnTriggerStay2D(Collider2D other) => Touch(other);

    void Touch(Collider2D other)
    {
        var respawn = other.GetComponentInParent<PlayerRespawn>();
        if (respawn == null) return;

        var health = other.GetComponentInParent<Health>();
        if (health == null || health.IsDead) return;

        bool hurt = health.TakeDamage(damage, transform.position, ignoreInvulnerability: false);
        if (!hurt && health.IsInvulnerable && !returnToSafeSpot) return;

        if (returnToSafeSpot) StartCoroutine(ReturnAfterFreeze(respawn, health));
    }

    IEnumerator ReturnAfterFreeze(PlayerRespawn respawn, Health health)
    {
        Sfx.Play("spikes", 0.7f);
        HitStop.Do(0.14f);
        yield return new WaitForSecondsRealtime(0.16f);
        if (health.IsDead) yield break;
        respawn.ReturnToSafeSpot();
    }
}
