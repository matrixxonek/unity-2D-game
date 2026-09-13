using System;
using UnityEngine;

/// Zapas amunicji znajdowanej w świecie. Dwie osobne waluty — pistoletowa jest
/// częsta, rewolwerowa bardzo rzadka i to ona tworzy decyzję "trzymam na bossa
/// czy ratuję się teraz".
public class AmmoPouch : MonoBehaviour
{
    [Header("Zapas")]
    public int pistolRounds = 24;
    public int revolverRounds = 3;

    [Header("Limity")]
    public int maxPistolRounds = 90;
    public int maxRevolverRounds = 12;

    public event Action Changed;

    /// Wyjmuje do `wanted` naboi pistoletowych. Zwraca ile faktycznie było.
    public int TakePistol(int wanted)
    {
        int taken = Mathf.Min(wanted, pistolRounds);
        pistolRounds -= taken;
        if (taken > 0) Changed?.Invoke();
        return taken;
    }

    public bool TakeRevolver(int count = 1)
    {
        if (revolverRounds < count) return false;
        revolverRounds -= count;
        Changed?.Invoke();
        return true;
    }

    public void AddPistol(int amount)
    {
        pistolRounds = Mathf.Min(maxPistolRounds, pistolRounds + amount);
        Changed?.Invoke();
    }

    public void AddRevolver(int amount)
    {
        revolverRounds = Mathf.Min(maxRevolverRounds, revolverRounds + amount);
        Changed?.Invoke();
    }
}
