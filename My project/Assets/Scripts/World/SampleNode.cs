using System;
using UnityEngine;

/// Złoże materiału. Zbierane tym samym przyciskiem co atak — próbnik jest
/// jednocześnie bronią i narzędziem misji, więc nie ma osobnej "interakcji".
public class SampleNode : MonoBehaviour
{
    [Header("Próbka")]
    public string sampleName = "Maź ze ścian";

    [Tooltip("Amunicja dorzucana przy pobraniu — na czas dema, żeby było czym strzelać.")]
    public int pistolRoundsReward = 6;
    public int revolverRoundsReward = 1;

    public bool Collected { get; private set; }

    public static int CollectedCount { get; private set; }
    public static int TotalCount { get; private set; }
    public static event Action Changed;

    SpriteRenderer sprite;
    bool promptShown;

    void Awake()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
        TotalCount++;
    }

    void OnDestroy()
    {
        TotalCount--;
        if (Collected) CollectedCount--;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (Collected || other.GetComponentInParent<PlayerMovement>() == null) return;
        promptShown = true;
        HUD.Prompt("J — pobierz próbkę próbnikiem");
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!promptShown || other.GetComponentInParent<PlayerMovement>() == null) return;
        promptShown = false;
        HUD.HidePrompt();
    }

    public void Collect()
    {
        if (Collected) return;

        Collected = true;
        CollectedCount++;
        if (promptShown) { promptShown = false; HUD.HidePrompt(); }

        var pouch = FindFirstObjectByType<AmmoPouch>();
        if (pouch != null)
        {
            pouch.AddPistol(pistolRoundsReward);
            pouch.AddRevolver(revolverRoundsReward);
        }

        Sfx.Play("sample");
        HitStop.Do(0.08f);
        Particles.Burst(transform.position, new Color(0.45f, 1f, 0.6f), 18, 4f, 0.6f, 4f, 0.11f);
        HUD.Message($"PRÓBKA: {sampleName}", 2.2f);
        if (sprite != null) sprite.color = new Color(0.35f, 0.4f, 0.4f, 0.55f);

        Changed?.Invoke();
        PlayerEvents.RaiseSampleCollected();
    }

    /// Statyki przeżywają wyjście z trybu gry, więc trzeba je zerować ręcznie.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        CollectedCount = 0;
        TotalCount = 0;
        Changed = null;
    }
}
