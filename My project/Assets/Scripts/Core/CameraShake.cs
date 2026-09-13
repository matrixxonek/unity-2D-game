using UnityEngine;

/// Wstrząs kamery. CameraFollow dodaje CurrentOffset do swojej pozycji.
/// Liczony w czasie nieskalowanym, więc trzęsie też podczas hit-stopu.
public class CameraShake : MonoBehaviour
{
    static CameraShake instance;
    public static Vector2 CurrentOffset { get; private set; }

    float amplitude, duration, remaining;

    void Awake() => instance = this;

    void OnDestroy()
    {
        if (instance != this) return;
        instance = null;
        CurrentOffset = Vector2.zero;
    }

    public static void Shake(float amp, float dur)
    {
        if (instance == null) return;
        if (amp < instance.amplitude && instance.remaining > 0f) return;

        instance.amplitude = amp;
        instance.duration = dur;
        instance.remaining = dur;
    }

    void Update()
    {
        if (remaining <= 0f)
        {
            CurrentOffset = Vector2.zero;
            return;
        }

        remaining -= Time.unscaledDeltaTime;
        float k = Mathf.Clamp01(remaining / duration);
        CurrentOffset = Random.insideUnitCircle * amplitude * k * k;
    }
}
