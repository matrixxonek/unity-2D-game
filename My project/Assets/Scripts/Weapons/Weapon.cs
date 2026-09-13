using UnityEngine;

/// Baza dla broni palnej. Bronie są dziećmi gracza i same nie czytają klawiatury —
/// wejścia rozdziela WeaponHolder.
public abstract class Weapon : MonoBehaviour
{
    [Header("Opis")]
    public string displayName = "Broń";
    public Ability requiredAbility = Ability.Pistol;

    protected PlayerMovement Owner { get; private set; }
    protected AmmoPouch Pouch { get; private set; }
    protected SequenceRunner Sequences { get; private set; }
    protected PlayerAbilities Abilities { get; private set; }

    public bool IsOwned => Abilities == null || Abilities.Has(requiredAbility);

    /// Linijka do HUD-u.
    public abstract string StatusLine { get; }

    public abstract void PullTrigger();
    public abstract void BeginReload();

    /// Wywoływane przy chowaniu broni — przerywa trwające przeładowanie.
    public virtual void OnHolstered()
    {
        if (Sequences != null && Sequences.IsRunning) Sequences.Cancel();
    }

    protected virtual void Awake()
    {
        Owner = GetComponentInParent<PlayerMovement>();
        Pouch = GetComponentInParent<AmmoPouch>();
        Sequences = GetComponentInParent<SequenceRunner>();
        Abilities = GetComponentInParent<PlayerAbilities>();
    }

    /// Strzał hitscan w kierunku, w którym patrzy postać. Zwraca trafione HP albo null.
    /// Pocisk zatrzymuje się na pierwszej litej przeszkodzie — ściana blokuje strzał.
    protected Health FireHitscan(float range, Color tracerColor, out Vector2 endPoint)
    {
        Vector2 origin = transform.position;
        Vector2 dir = new Vector2(Owner.Facing, 0f);
        endPoint = origin + dir * range;

        var hits = Physics2D.RaycastAll(origin, dir, range);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        Health hitHealth = null;
        foreach (var hit in hits)
        {
            if (hit.collider.transform.IsChildOf(Owner.transform)) continue;

            var candidate = hit.collider.GetComponentInParent<Health>();

            // Triggery bez HP (złoża, ławka, strefy) nie zatrzymują pocisku.
            if (hit.collider.isTrigger && candidate == null) continue;

            endPoint = hit.point;
            hitHealth = candidate;
            break;
        }

        Tracer.Spawn(origin, endPoint, tracerColor);
        Particles.Burst(origin + dir * 0.3f, new Color(1f, 0.9f, 0.5f), 4, 3f, 0.12f, 0f, 0.08f, 60f, Owner.Facing > 0 ? 0f : 180f);
        if (hitHealth != null) Tracer.Impact(endPoint, new Color(1f, 0.55f, 0.3f));
        else Particles.Burst(endPoint, new Color(0.8f, 0.8f, 0.75f), 3, 2f, 0.2f, 8f, 0.07f);

        return hitHealth != null && !hitHealth.IsDead ? hitHealth : null;
    }
}
