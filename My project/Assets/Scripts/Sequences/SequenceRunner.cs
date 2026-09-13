using System;
using UnityEngine;

public enum SeqInput { Up, Down, Left, Right }

/// Opis jednej sekwencji do wykonania. Inicjator (broń, leczenie) wypełnia to
/// i oddaje do SequenceRunner.
public class SequenceRequest
{
    public string label;
    public SeqInput[] pattern;

    /// Czy pomyłka kasuje postęp w bieżącym wzorze. Rewolwer ma false — każdy krok
    /// commituje jedną komorę, więc nie ma czego cofać (zacinanie się to dopiero moduł).
    public bool resetOnMistake = true;

    /// Wywoływane po każdym poprawnym kroku. Argument = indeks kroku.
    public Action<int> onStep;
    public Action onComplete;
    public Action onCancel;
}

/// Silnik sekwencji wejść — hook całej gry.
///
/// Zasady projektowe (patrz docs/GDD.md, sekcja 3), zaimplementowane tutaj:
///  1. Sekwencja dzieje się w świecie i kosztuje bezbronność — ruch jest zablokowany.
///  2. Przerwanie nie kasuje tego, co już zacommitowane przez onStep.
///  4. Nigdy w trakcie parkouru — start wymaga ziemi, ruch i skok przerywają.
[DefaultExecutionOrder(-50)]
[RequireComponent(typeof(PlayerInputReader))]
public class SequenceRunner : MonoBehaviour
{
    public bool IsRunning => active != null;
    public string Label => active?.label;
    public SeqInput[] Pattern => active?.pattern;
    public int Progress { get; private set; }

    /// Ustawiane na true na jedną klatkę, gdy gracz się pomylił — HUD może mrugnąć.
    public bool MistakeThisFrame { get; private set; }
    public float LastMistakeTime { get; private set; } = -99f;

    SequenceRequest active;
    PlayerInputReader input;
    PlayerMovement movement;
    Health health;

    void Awake()
    {
        input = GetComponent<PlayerInputReader>();
        movement = GetComponent<PlayerMovement>();
        health = GetComponent<Health>();
        if (health != null) health.Damaged += OnDamaged;
    }

    void OnDestroy()
    {
        if (health != null) health.Damaged -= OnDamaged;
    }

    public bool CanBegin() => movement == null || movement.IsGrounded;

    public void Begin(SequenceRequest request)
    {
        if (request == null || request.pattern == null || request.pattern.Length == 0) return;
        if (IsRunning) Cancel();

        active = request;
        Progress = 0;
    }

    public void Cancel()
    {
        if (active == null) return;

        var finished = active;
        active = null;
        Progress = 0;
        finished.onCancel?.Invoke();
    }

    // Zasada 2: obrażenia przerywają bieżący wzór, ale to, co onStep już
    // zacommitował (załadowane komory), zostaje.
    void OnDamaged(DamageInfo info)
    {
        if (!IsRunning) return;
        Cancel();
    }

    void Update()
    {
        MistakeThisFrame = false;
        if (active == null) return;

        // Zasada 4: ruch i skok wybijają z sekwencji. Gracz zawsze może się wycofać.
        if (Mathf.Abs(input.Move) > 0.01f || input.JumpPressed || !CanBegin())
        {
            Cancel();
            return;
        }

        if (!input.TryConsumeArrow(out SeqInput pressed)) return;

        if (pressed == active.pattern[Progress])
        {
            Progress++;
            Sfx.Play("seq_step", 0.8f, 1f + Progress * 0.07f, 0f);

            var current = active;
            current.onStep?.Invoke(Progress - 1);

            // onStep mógł sam anulować sekwencję (np. skończyła się amunicja).
            if (active != current) return;

            if (Progress >= active.pattern.Length)
            {
                active = null;
                Progress = 0;
                Sfx.Play("seq_done", 0.8f);
                current.onComplete?.Invoke();
            }
        }
        else
        {
            MistakeThisFrame = true;
            LastMistakeTime = Time.unscaledTime;
            Sfx.Play("seq_fail", 0.6f);
            if (active.resetOnMistake) Progress = 0;
        }
    }
}
