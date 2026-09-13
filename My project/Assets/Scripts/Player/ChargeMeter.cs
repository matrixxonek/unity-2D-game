using System;
using UnityEngine;

/// Ładunek pobierany próbnikiem z trafionych celów. Zasila wyłącznie leczenie.
///
/// Konsekwencja projektowa: żeby się wyleczyć, trzeba najpierw kogoś dźgnąć.
/// Nie da się uciec i odespać — trzeba wejść w walkę, żeby z niej wyjść.
public class ChargeMeter : MonoBehaviour
{
    [Header("Ładunek")]
    public int maxCharge = 9;

    [Tooltip("Koszt jednego zabiegu leczniczego.")]
    public int healCost = 3;

    public int Current { get; private set; }
    public bool CanAffordHeal => Current >= healCost;

    public event Action Changed;

    public void Add(int amount)
    {
        if (amount <= 0) return;
        Current = Mathf.Min(maxCharge, Current + amount);
        Changed?.Invoke();
    }

    public bool TrySpend(int amount)
    {
        if (Current < amount) return false;
        Current -= amount;
        Changed?.Invoke();
        return true;
    }

    public void RestoreFull()
    {
        Current = maxCharge;
        Changed?.Invoke();
    }
}
