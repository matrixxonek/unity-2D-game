using System;
using UnityEngine;

/// Globalne zdarzenia gracza. Samouczek, HUD i statystyki słuchają tutaj,
/// zamiast trzymać referencje do każdego systemu z osobna.
public static class PlayerEvents
{
    public static event Action Moved;
    public static event Action Jumped;
    public static event Action DoubleJumped;
    public static event Action Landed;
    public static event Action ProbeHit;
    public static event Action Pogoed;
    public static event Action Healed;
    public static event Action Fired;
    public static event Action Reloaded;
    public static event Action WeaponSwitched;
    public static event Action RevolverLoaded;
    public static event Action SampleCollected;
    public static event Action Rested;

    public static void RaiseMoved() => Moved?.Invoke();
    public static void RaiseJumped() => Jumped?.Invoke();
    public static void RaiseDoubleJumped() => DoubleJumped?.Invoke();
    public static void RaiseLanded() => Landed?.Invoke();
    public static void RaiseProbeHit() => ProbeHit?.Invoke();
    public static void RaisePogoed() => Pogoed?.Invoke();
    public static void RaiseHealed() => Healed?.Invoke();
    public static void RaiseFired() => Fired?.Invoke();
    public static void RaiseReloaded() => Reloaded?.Invoke();
    public static void RaiseWeaponSwitched() => WeaponSwitched?.Invoke();
    public static void RaiseRevolverLoaded() => RevolverLoaded?.Invoke();
    public static void RaiseSampleCollected() => SampleCollected?.Invoke();
    public static void RaiseRested() => Rested?.Invoke();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        Moved = null; Jumped = null; DoubleJumped = null; Landed = null;
        ProbeHit = null; Pogoed = null; Healed = null; Fired = null; Reloaded = null;
        WeaponSwitched = null; RevolverLoaded = null; SampleCollected = null; Rested = null;
    }
}
