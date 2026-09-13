using System;
using UnityEngine;

public struct DamageInfo
{
    public int amount;
    public bool hasSource;
    public Vector2 source;
}

/// Wspólne HP dla gracza i przeciwników.
public class Health : MonoBehaviour
{
    [Header("Zdrowie")]
    public int maxHealth = 5;

    [Tooltip("Czas nietykalności po otrzymaniu obrażeń. Przeciwnikom dajemy krótki, " +
             "żeby dało się ich zasypać ciosami próbnika.")]
    public float invulnerabilityTime = 0.6f;

    /// Pełna nietykalność — używane po zwycięstwie, żeby nic nie zabiło gracza w napisach.
    public bool Invulnerable { get; set; }

    public int Current { get; private set; }
    public bool IsDead => Current <= 0;
    public bool IsInvulnerable => Time.time < invulnerableUntil;

    public event Action<DamageInfo> Damaged;
    public event Action Died;
    public event Action Changed;

    float invulnerableUntil;

    void Awake() => Current = maxHealth;

    public bool TakeDamage(int amount) => TakeDamage(amount, null);

    public bool TakeDamage(int amount, Vector2? source, bool ignoreInvulnerability = false)
    {
        if (IsDead || amount <= 0 || Invulnerable) return false;
        if (IsInvulnerable && !ignoreInvulnerability) return false;

        Current = Mathf.Max(0, Current - amount);
        invulnerableUntil = Time.time + invulnerabilityTime;

        Damaged?.Invoke(new DamageInfo
        {
            amount = amount,
            hasSource = source.HasValue,
            source = source ?? Vector2.zero,
        });
        Changed?.Invoke();

        if (IsDead) Died?.Invoke();
        return true;
    }

    public void Heal(int amount)
    {
        if (IsDead || amount <= 0) return;
        Current = Mathf.Min(maxHealth, Current + amount);
        Changed?.Invoke();
    }

    public void RestoreFull()
    {
        Current = maxHealth;
        invulnerableUntil = 0f;
        Changed?.Invoke();
    }

    public void Kill()
    {
        if (IsDead) return;
        Current = 0;
        Changed?.Invoke();
        Died?.Invoke();
    }
}
