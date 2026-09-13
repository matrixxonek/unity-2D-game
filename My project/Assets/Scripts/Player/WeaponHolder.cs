using UnityEngine;

/// Rozdziela wejścia do aktywnej broni i pilnuje, że aktywna może być tylko
/// broń, którą gracz już posiada.
public class WeaponHolder : MonoBehaviour
{
    [Header("Bronie (wszystkie, w kolejności przełączania)")]
    public Weapon[] weapons;

    public int ActiveIndex { get; private set; } = -1;

    public Weapon Active =>
        weapons != null && ActiveIndex >= 0 && ActiveIndex < weapons.Length && weapons[ActiveIndex].IsOwned
            ? weapons[ActiveIndex]
            : null;

    PlayerInputReader input;
    SequenceRunner sequences;
    PlayerAbilities abilities;

    void Awake()
    {
        input = GetComponent<PlayerInputReader>();
        sequences = GetComponent<SequenceRunner>();
        abilities = GetComponent<PlayerAbilities>();
        if (abilities != null) abilities.Unlocked += OnUnlocked;
    }

    void Start() => SelectFirstOwned();

    void OnDestroy()
    {
        if (abilities != null) abilities.Unlocked -= OnUnlocked;
    }

    void OnUnlocked(Ability ability)
    {
        if (Active != null) return;
        SelectFirstOwned();
    }

    public void SelectFirstOwned()
    {
        if (weapons == null) return;
        for (int i = 0; i < weapons.Length; i++)
        {
            if (!weapons[i].IsOwned) continue;
            ActiveIndex = i;
            return;
        }
        ActiveIndex = -1;
    }

    void Update()
    {
        var active = Active;
        if (active == null) return;

        if (input.SwitchWeaponPressed) SwitchNext();

        active = Active;
        if (input.ReloadPressed) active.BeginReload();

        if (input.FirePressed)
        {
            // Strzał w trakcie ładowania bębna jest legalny: przerywasz sekwencję
            // i strzelasz tym, co zdążyłeś załadować.
            if (sequences.IsRunning) sequences.Cancel();
            active.PullTrigger();
        }
    }

    public void SwitchNext()
    {
        if (weapons == null || weapons.Length < 2) return;

        for (int step = 1; step < weapons.Length; step++)
        {
            int candidate = (ActiveIndex + step) % weapons.Length;
            if (!weapons[candidate].IsOwned) continue;

            Active?.OnHolstered();
            ActiveIndex = candidate;
            Sfx.Play("reload", 0.6f, 1.3f);
            HUD.Message(weapons[candidate].displayName, 0.9f);
            PlayerEvents.RaiseWeaponSwitched();
            return;
        }
    }
}
