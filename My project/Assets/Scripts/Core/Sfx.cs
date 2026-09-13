using UnityEngine;

/// Odtwarzacz efektów. Tworzy się sam przy pierwszym użyciu.
public class Sfx : MonoBehaviour
{
    public static float MasterVolume = 0.7f;

    static Sfx instance;
    AudioSource[] sources;
    int next;

    static Sfx Instance
    {
        get
        {
            if (instance == null)
                instance = new GameObject("Sfx").AddComponent<Sfx>();
            return instance;
        }
    }

    void Awake()
    {
        sources = new AudioSource[12];
        for (int i = 0; i < sources.Length; i++)
        {
            var a = gameObject.AddComponent<AudioSource>();
            a.playOnAwake = false;
            a.spatialBlend = 0f;
            sources[i] = a;
        }
    }

    public static void Play(string key, float volume = 1f, float pitch = 1f, float jitter = 0.06f)
    {
        var i = Instance;
        if (i.sources == null) return;

        var src = i.sources[i.next];
        i.next = (i.next + 1) % i.sources.Length;

        src.pitch = pitch * (1f + Random.Range(-jitter, jitter));
        src.PlayOneShot(ProceduralAudio.Get(key), volume * MasterVolume);
    }
}
