using UnityEngine;

/// Wejście na arenę: zamyka bramę za graczem, budzi bossa, pokazuje pasek.
/// Po śmierci bossa otwiera wyjście. Po śmierci gracza resetuje arenę,
/// żeby dało się wrócić i spróbować jeszcze raz.
[RequireComponent(typeof(Collider2D))]
public class BossArena : MonoBehaviour
{
    public Boss boss;
    public Gate entryGate;
    public Gate exitGate;

    public bool Started { get; private set; }

    Health playerHealth;

    void Reset() => GetComponent<Collider2D>().isTrigger = true;

    void Start()
    {
        var player = FindFirstObjectByType<PlayerMovement>();
        if (player == null) return;
        playerHealth = player.GetComponent<Health>();
        if (playerHealth != null) playerHealth.Died += OnPlayerDied;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (Started || boss == null) return;
        if (other.GetComponentInParent<PlayerMovement>() == null) return;

        Started = true;
        entryGate?.Close();
        boss.Health.Died += OnBossDied;
        boss.Activate();

        if (HUD.I != null) HUD.I.ShowBoss(boss.bossName);
        HUD.Message(boss.bossName, 1.8f);
        Sfx.Play("telegraph", 1f, 0.45f);
        CameraShake.Shake(0.25f, 0.5f);
    }

    void OnBossDied()
    {
        if (HUD.I != null) HUD.I.HideBoss();
        HUD.Message("Droga wolna", 2.4f);
        exitGate?.Open();
        Sfx.Play("victory", 0.8f);
    }

    void OnPlayerDied()
    {
        if (!Started || boss == null || boss.Health.IsDead) return;

        boss.Health.Died -= OnBossDied;
        boss.ResetToDormant();
        entryGate?.Open();
        if (HUD.I != null) HUD.I.HideBoss();
        Started = false;
    }

    void OnDestroy()
    {
        if (boss != null && boss.Health != null) boss.Health.Died -= OnBossDied;
        if (playerHealth != null) playerHealth.Died -= OnPlayerDied;
    }
}
