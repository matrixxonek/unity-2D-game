using UnityEngine;

/// Zamrożenie czasu na kilka klatek przy trafieniu. Najtańszy juice, jaki istnieje.
public class HitStop : MonoBehaviour
{
    static HitStop instance;
    float resumeAt;
    bool frozen;

    public static void Do(float seconds)
    {
        if (GameFlow.IsPaused) return;
        if (instance == null) instance = new GameObject("HitStop").AddComponent<HitStop>();

        Time.timeScale = 0f;
        instance.frozen = true;
        instance.resumeAt = Mathf.Max(instance.resumeAt, Time.unscaledTime + seconds);
    }

    void Update()
    {
        if (!frozen || Time.unscaledTime < resumeAt) return;
        frozen = false;
        if (!GameFlow.IsPaused) Time.timeScale = 1f;
    }
}
