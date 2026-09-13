using System.Collections;
using UnityEngine;

/// Kryształ radioaktywny: trafiony próbnikiem daje ładunek i odrasta po chwili.
/// Zawsze jest powierzchnią do pogo, niezależnie od tego, czy jest "pełny".
public class ChargeCrystal : MonoBehaviour
{
    public int charge = 3;
    public float regrowTime = 5f;

    public bool Ready { get; private set; } = true;

    SpriteRenderer sprite;
    Color baseColor;

    void Awake()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite != null) baseColor = sprite.color;
    }

    public bool TryHarvest(ChargeMeter meter)
    {
        if (!Ready || meter == null) return false;

        Ready = false;
        meter.Add(charge);
        Sfx.Play("pickup", 0.8f, 1.2f);
        Particles.Burst(transform.position, new Color(0.4f, 0.9f, 1f), 12, 4f, 0.45f, 6f, 0.1f);
        if (sprite != null) sprite.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.3f);
        StartCoroutine(Regrow());
        return true;
    }

    IEnumerator Regrow()
    {
        yield return new WaitForSeconds(regrowTime);
        Ready = true;
        if (sprite != null) sprite.color = baseColor;
        Particles.Burst(transform.position, new Color(0.4f, 0.9f, 1f), 5, 1.5f, 0.3f, 0f, 0.08f);
    }
}
