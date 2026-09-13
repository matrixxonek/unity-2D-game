using UnityEngine;

/// Widoczny ślad po strzale. Bez tego strzelanie nie daje żadnego feedbacku —
/// Debug.DrawLine widać tylko w Scene View, nie w grze.
public class Tracer : MonoBehaviour
{
    static Material shared;

    public static void Spawn(Vector2 from, Vector2 to, Color color,
                             float width = 0.07f, float life = 0.06f)
    {
        if (shared == null)
        {
            var shader = Shader.Find("Sprites/Default");
            shared = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
        }

        var go = new GameObject("Tracer");
        var line = go.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.SetPosition(0, from);
        line.SetPosition(1, to);
        line.startWidth = line.endWidth = width;
        line.sharedMaterial = shared;
        line.startColor = line.endColor = color;
        line.sortingOrder = 20;
        line.numCapVertices = 2;

        Destroy(go, life);
    }

    /// Krótki błysk w punkcie trafienia.
    public static void Impact(Vector2 at, Color color, float size = 0.35f, float life = 0.08f)
    {
        Spawn(at + Vector2.left * size * 0.5f, at + Vector2.right * size * 0.5f, color, size, life);
    }
}
