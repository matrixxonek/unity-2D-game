using UnityEngine;

/// Warstwa tła przesuwająca się wolniej niż kamera.
public class ParallaxBackground : MonoBehaviour
{
    [Range(0f, 1f)] public float factor = 0.7f;
    public float verticalFactor = 0.5f;

    Transform cam;
    Vector3 startPos;
    Vector3 camStart;

    void Start()
    {
        cam = Camera.main != null ? Camera.main.transform : null;
        startPos = transform.position;
        if (cam != null) camStart = cam.position;
    }

    void LateUpdate()
    {
        if (cam == null) return;
        Vector3 d = cam.position - camStart;
        transform.position = new Vector3(startPos.x + d.x * factor,
                                         startPos.y + d.y * factor * verticalFactor,
                                         startPos.z);
    }
}
