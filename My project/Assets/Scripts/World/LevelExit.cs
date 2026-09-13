using UnityEngine;

/// Koniec poziomu — ekran zwycięstwa.
[RequireComponent(typeof(Collider2D))]
public class LevelExit : MonoBehaviour
{
    bool used;

    void Reset() => GetComponent<Collider2D>().isTrigger = true;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (used || other.GetComponentInParent<PlayerMovement>() == null) return;
        used = true;
        if (GameFlow.I != null) GameFlow.I.Victory();
    }
}
