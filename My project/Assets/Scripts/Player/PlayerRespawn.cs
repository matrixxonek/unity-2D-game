using UnityEngine;

/// Punkty powrotu: ławka (po śmierci) i ostatnie bezpieczne miejsce (po kolcach).
public class PlayerRespawn : MonoBehaviour
{
    [Tooltip("Ile trzeba stać na ziemi, żeby miejsce uznać za bezpieczne.")]
    public float safeDelay = 0.2f;

    public Vector3 Checkpoint { get; private set; }
    public Vector3 LastSafePosition { get; private set; }
    public int Deaths { get; private set; }

    Health health;
    ChargeMeter charge;
    PlayerMovement movement;
    SequenceRunner sequences;
    float groundedSince = -1f;

    void Awake()
    {
        health = GetComponent<Health>();
        charge = GetComponent<ChargeMeter>();
        movement = GetComponent<PlayerMovement>();
        sequences = GetComponent<SequenceRunner>();

        Checkpoint = transform.position;
        LastSafePosition = transform.position;
        health.Died += OnDied;
    }

    void OnDestroy() => health.Died -= OnDied;

    void Update()
    {
        if (!movement.IsGrounded)
        {
            groundedSince = -1f;
            return;
        }

        if (groundedSince < 0f) groundedSince = Time.time;

        // Kruszące się platformy nie są bezpiecznym miejscem powrotu.
        bool stable = movement.GroundCollider != null &&
                      movement.GroundCollider.GetComponentInParent<CrumblingPlatform>() == null;

        if (stable && Time.time - groundedSince >= safeDelay)
            LastSafePosition = transform.position;
    }

    public void SetCheckpoint(Vector3 position) => Checkpoint = position;

    void OnDied()
    {
        Deaths++;
        if (GameFlow.I != null) GameFlow.I.OnPlayerDied(this);
        else RespawnAtCheckpoint();
    }

    public void RespawnAtCheckpoint()
    {
        if (sequences != null && sequences.IsRunning) sequences.Cancel();

        transform.position = Checkpoint;
        LastSafePosition = Checkpoint;
        movement.ResetMotion();

        health.RestoreFull();
        charge?.RestoreFull();
    }

    public void ReturnToSafeSpot()
    {
        if (health.IsDead) return;
        if (sequences != null && sequences.IsRunning) sequences.Cancel();

        transform.position = LastSafePosition;
        movement.ResetMotion();
    }
}
