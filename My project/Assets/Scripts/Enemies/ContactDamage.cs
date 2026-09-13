using UnityEngine;

/// Rani gracza przy dotknięciu. Działa i na kolizjach, i na triggerach.
public class ContactDamage : MonoBehaviour
{
    public int damage = 1;
    public bool active = true;

    void OnCollisionEnter2D(Collision2D c) => Hurt(c.collider);
    void OnCollisionStay2D(Collision2D c) => Hurt(c.collider);
    void OnTriggerEnter2D(Collider2D c) => Hurt(c);
    void OnTriggerStay2D(Collider2D c) => Hurt(c);

    void Hurt(Collider2D other)
    {
        if (!active) return;
        if (other.GetComponentInParent<PlayerMovement>() == null) return;

        var targetHealth = other.GetComponentInParent<Health>();
        targetHealth?.TakeDamage(damage, transform.position);
    }
}
