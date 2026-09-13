using UnityEngine;

/// Ławka — punkt odpoczynku i odrodzenia. Uzupełnia HP i ładunek.
[RequireComponent(typeof(Collider2D))]
public class Bench : MonoBehaviour
{
    public bool PlayerInRange { get; private set; }

    PlayerInputReader input;

    void Reset() => GetComponent<Collider2D>().isTrigger = true;

    void OnTriggerEnter2D(Collider2D other)
    {
        var pm = other.GetComponentInParent<PlayerMovement>();
        if (pm == null) return;
        PlayerInRange = true;
        input = pm.GetComponent<PlayerInputReader>();
        HUD.Prompt("E — odpocznij (pełne HP, ładunek, punkt odrodzenia)");
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerMovement>() == null) return;
        PlayerInRange = false;
        HUD.HidePrompt();
    }

    void Update()
    {
        if (!PlayerInRange || input == null || !input.InteractPressed) return;
        Rest(input.gameObject);
    }

    void Rest(GameObject player)
    {
        player.GetComponent<Health>()?.RestoreFull();
        player.GetComponent<ChargeMeter>()?.RestoreFull();
        player.GetComponent<PlayerRespawn>()?.SetCheckpoint(transform.position + Vector3.up * 0.5f);

        Sfx.Play("bench");
        Particles.Burst(transform.position + Vector3.up * 0.5f, new Color(0.5f, 0.95f, 1f), 16, 2.5f, 0.8f, -3f, 0.1f);
        HUD.Message("Odpoczynek — punkt odrodzenia ustawiony", 1.8f);
        HUD.HidePrompt();
        PlayerEvents.RaiseRested();
    }
}
