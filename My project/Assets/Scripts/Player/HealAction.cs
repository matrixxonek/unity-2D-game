using UnityEngine;

/// Leczenie jako sekwencja — odpowiednik Focusa z Hollow Knight, wyrażony
/// językiem sterowania tej gry.
///
/// Kosztuje ładunek (a ładunek bierze się wyłącznie z trafień próbnikiem),
/// wykonuje się stojąc, i można je przerwać. Ładunek schodzi dopiero przy
/// ukończeniu, więc przerwany zabieg nic nie kosztuje poza czasem i ryzykiem.
public class HealAction : MonoBehaviour
{
    [Header("Zabieg")]
    public int healAmount = 2;

    [Tooltip("Napełnij strzykawkę dwa razy, potem wstrzyknij.")]
    public SeqInput[] pattern = { SeqInput.Down, SeqInput.Down, SeqInput.Up };

    public bool IsHealing { get; private set; }

    PlayerInputReader input;
    SequenceRunner sequences;
    ChargeMeter charge;
    Health health;

    void Awake()
    {
        input = GetComponent<PlayerInputReader>();
        sequences = GetComponent<SequenceRunner>();
        charge = GetComponent<ChargeMeter>();
        health = GetComponent<Health>();
    }

    void Update()
    {
        if (!input.HealPressed) return;

        if (IsHealing)
        {
            sequences.Cancel();
            return;
        }

        Begin();
    }

    void Begin()
    {
        if (health.Current >= health.maxHealth)
        {
            HUD.Message("Pełne zdrowie", 1f);
            return;
        }
        if (!charge.CanAffordHeal)
        {
            HUD.Message($"Za mało ładunku ({charge.Current}/{charge.healCost}) — dźgnij coś próbnikiem", 1.6f);
            Sfx.Play("seq_fail", 0.6f);
            return;
        }
        if (!sequences.CanBegin())
        {
            HUD.Message("Zabieg wymaga stania na ziemi", 1.2f);
            return;
        }

        IsHealing = true;
        sequences.Begin(new SequenceRequest
        {
            label = "ZABIEG",
            pattern = pattern,
            resetOnMistake = true,
            onComplete = () =>
            {
                IsHealing = false;
                if (!charge.TrySpend(charge.healCost)) return;

                health.Heal(healAmount);
                Sfx.Play("heal");
                Particles.Burst(transform.position, new Color(0.45f, 1f, 0.55f), 14, 3f, 0.6f, -4f, 0.1f);
                PlayerEvents.RaiseHealed();
            },
            onCancel = () => IsHealing = false,
        });
    }
}
