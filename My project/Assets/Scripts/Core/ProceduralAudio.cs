using System.Collections.Generic;
using UnityEngine;

/// Dźwięki syntezowane w locie — zero plików audio w projekcie.
/// Placeholdery, ale takie, które dają feedback: skok, trafienie, sekwencja, strzał.
public static class ProceduralAudio
{
    const int Rate = 22050;
    enum Wave { Sine, Square, Saw, Triangle }

    static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();

    public static AudioClip Get(string key)
    {
        if (cache.TryGetValue(key, out var clip) && clip != null) return clip;

        float[] samples = Build(key);
        clip = AudioClip.Create(key, samples.Length, 1, Rate, false);
        clip.SetData(samples, 0);
        cache[key] = clip;
        return clip;
    }

    static float[] Build(string key)
    {
        switch (key)
        {
            case "jump":        return Sweep(280f, 640f, 0.13f, Wave.Square, 0.28f, 5f);
            case "double_jump": return Mix(Sweep(500f, 1000f, 0.15f, Wave.Square, 0.25f, 5f), Noise(0.06f, 12f, 0.15f));
            case "land":        return Noise(0.07f, 10f, 0.35f, 0.25f);
            case "swing":       return Sweep(1100f, 350f, 0.07f, Wave.Saw, 0.22f, 6f);
            case "hit":         return Mix(Noise(0.09f, 9f, 0.6f), Sweep(220f, 70f, 0.11f, Wave.Square, 0.35f, 7f));
            case "pogo":        return Sweep(220f, 760f, 0.16f, Wave.Sine, 0.4f, 5f);
            case "hurt":        return Mix(Noise(0.18f, 5f, 0.7f), Sweep(160f, 55f, 0.22f, Wave.Saw, 0.45f, 5f));
            case "seq_step":    return Tone(880f, 0.06f, Wave.Square, 0.2f, 10f);
            case "seq_fail":    return Sweep(320f, 140f, 0.16f, Wave.Square, 0.3f, 6f);
            case "seq_done":    return Arp(new[] { 660f, 880f, 1320f }, 0.06f, 0.25f);
            case "heal":        return Arp(new[] { 523f, 659f, 784f, 1047f }, 0.09f, 0.28f);
            case "shoot":       return Mix(Noise(0.06f, 14f, 0.55f), Sweep(700f, 180f, 0.07f, Wave.Saw, 0.35f, 8f));
            case "revolver":    return Mix(Noise(0.28f, 4f, 0.9f), Sweep(130f, 35f, 0.32f, Wave.Sine, 0.8f, 4f));
            case "click":       return Tone(2400f, 0.018f, Wave.Square, 0.25f, 12f);
            case "reload":      return Arp(new[] { 420f, 560f }, 0.05f, 0.25f);
            case "pickup":      return Arp(new[] { 784f, 1047f, 1319f }, 0.05f, 0.25f);
            case "sample":      return Arp(new[] { 523f, 784f, 1047f, 1568f }, 0.07f, 0.28f);
            case "bench":       return Arp(new[] { 392f, 523f, 659f, 784f }, 0.13f, 0.25f, Wave.Triangle);
            case "enemy_die":   return Mix(Noise(0.22f, 5f, 0.7f), Sweep(420f, 45f, 0.26f, Wave.Saw, 0.4f, 5f));
            case "boss_hit":    return Mix(Noise(0.12f, 6f, 0.7f), Sweep(110f, 40f, 0.16f, Wave.Square, 0.6f, 6f));
            case "boss_die":    return Mix(Noise(0.9f, 1.6f, 0.9f), Sweep(220f, 25f, 0.9f, Wave.Saw, 0.7f, 2.5f));
            case "crumble":     return Noise(0.35f, 3f, 0.4f, 0.15f);
            case "gate":        return Mix(Sweep(70f, 150f, 0.45f, Wave.Square, 0.3f, 3f), Noise(0.4f, 3f, 0.25f, 0.1f));
            case "unlock":      return Arp(new[] { 523f, 659f, 784f, 1047f, 1319f }, 0.09f, 0.3f, Wave.Triangle);
            case "slam":        return Mix(Noise(0.22f, 4f, 0.9f), Sweep(95f, 28f, 0.32f, Wave.Sine, 0.9f, 4f));
            case "telegraph":   return Sweep(180f, 420f, 0.32f, Wave.Sine, 0.28f, 2f);
            case "dive":        return Sweep(1300f, 320f, 0.26f, Wave.Saw, 0.3f, 4f);
            case "spikes":      return Mix(Noise(0.1f, 8f, 0.6f), Sweep(900f, 200f, 0.1f, Wave.Saw, 0.3f, 8f));
            case "message":     return Tone(1200f, 0.05f, Wave.Sine, 0.18f, 12f);
            case "victory":     return Arp(new[] { 523f, 659f, 784f, 1047f, 784f, 1047f, 1319f, 1568f }, 0.11f, 0.3f, Wave.Triangle);
            default:            return Tone(440f, 0.1f, Wave.Sine, 0.3f, 8f);
        }
    }

    static float[] Sweep(float f0, float f1, float duration, Wave wave, float amp, float decay)
    {
        int n = Mathf.Max(1, (int)(duration * Rate));
        var s = new float[n];
        float phase = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            phase += Mathf.Lerp(f0, f1, t) / Rate;
            float env = Mathf.Exp(-decay * t) * (1f - t);
            s[i] = Osc(wave, phase) * amp * env;
        }
        return s;
    }

    static float[] Tone(float f, float duration, Wave wave, float amp, float decay) =>
        Sweep(f, f, duration, wave, amp, decay);

    static float[] Noise(float duration, float decay, float amp, float lowpass = 0.35f)
    {
        int n = Mathf.Max(1, (int)(duration * Rate));
        var s = new float[n];
        var rng = new System.Random(1337);
        float last = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            float white = (float)rng.NextDouble() * 2f - 1f;
            last += (white - last) * lowpass;
            s[i] = last * amp * Mathf.Exp(-decay * t);
        }
        return s;
    }

    static float[] Arp(float[] freqs, float step, float amp, Wave wave = Wave.Square)
    {
        int per = Mathf.Max(1, (int)(step * Rate));
        var s = new float[per * freqs.Length];
        for (int j = 0; j < freqs.Length; j++)
        {
            float phase = 0f;
            for (int i = 0; i < per; i++)
            {
                float t = (float)i / per;
                phase += freqs[j] / Rate;
                s[j * per + i] = Osc(wave, phase) * amp * (1f - t * 0.6f);
            }
        }
        // Ogon, żeby ostatnia nuta nie urywała się kliknięciem.
        for (int i = s.Length - per / 3; i < s.Length; i++)
            s[i] *= (float)(s.Length - i) / (per / 3f);
        return s;
    }

    static float[] Mix(params float[][] parts)
    {
        int n = 0;
        foreach (var p in parts) n = Mathf.Max(n, p.Length);
        var s = new float[n];
        foreach (var p in parts)
            for (int i = 0; i < p.Length; i++) s[i] += p[i];
        for (int i = 0; i < n; i++) s[i] = Mathf.Clamp(s[i], -1f, 1f);
        return s;
    }

    static float Osc(Wave w, float phase)
    {
        float p = phase - Mathf.Floor(phase);
        switch (w)
        {
            case Wave.Square:   return p < 0.5f ? 0.6f : -0.6f;
            case Wave.Saw:      return (2f * p - 1f) * 0.7f;
            case Wave.Triangle: return 1f - 4f * Mathf.Abs(p - 0.5f);
            default:            return Mathf.Sin(p * Mathf.PI * 2f);
        }
    }
}
