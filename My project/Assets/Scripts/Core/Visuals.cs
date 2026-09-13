using UnityEngine;

/// Materiały i sprite'y tworzone w runtime (cząsteczki, ślady, obiekty spawnowane).
/// Nieoświetlone, żeby świeciły niezależnie od Light2D.
public static class Visuals
{
    static Material unlit;
    static Sprite dot;

    public static Material Unlit
    {
        get
        {
            if (unlit != null) return unlit;

            var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            unlit = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            return unlit;
        }
    }

    /// Biały kwadrat 1x1 jednostki.
    public static Sprite Dot
    {
        get
        {
            if (dot != null) return dot;

            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color32[16];
            for (int i = 0; i < px.Length; i++) px[i] = new Color32(255, 255, 255, 255);
            tex.SetPixels32(px);
            tex.Apply();
            dot = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            return dot;
        }
    }
}
