using UnityEngine;

/// Kamera podążająca za graczem z martwą strefą, granicami poziomu i wstrząsem.
[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("Cel")]
    public Transform target;

    [Header("Podążanie")]
    public float smoothTime = 0.16f;
    public Vector2 offset = new Vector2(0f, 1.2f);
    public Vector2 deadZone = new Vector2(1.4f, 1.0f);

    [Header("Granice")]
    public bool useBounds;
    public float minX, maxX, minY, maxY;

    Camera cam;
    Vector3 velocity;
    Vector2 basePos;

    void Awake()
    {
        cam = GetComponent<Camera>();
        basePos = transform.position;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector2 desired = (Vector2)target.position + offset;
        Vector2 delta = desired - basePos;

        if (Mathf.Abs(delta.x) < deadZone.x) desired.x = basePos.x;
        if (Mathf.Abs(delta.y) < deadZone.y) desired.y = basePos.y;

        desired = Clamp(desired);

        Vector3 smoothed = Vector3.SmoothDamp(basePos, desired, ref velocity, smoothTime,
                                              Mathf.Infinity, Time.unscaledDeltaTime);
        basePos = smoothed;

        Vector2 shake = CameraShake.CurrentOffset;
        transform.position = new Vector3(basePos.x + shake.x, basePos.y + shake.y, transform.position.z);
    }

    Vector2 Clamp(Vector2 p)
    {
        if (!useBounds || cam == null) return p;

        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        float x = Mathf.Clamp(p.x, minX + halfW, Mathf.Max(minX + halfW, maxX - halfW));
        float y = Mathf.Clamp(p.y, minY + halfH, Mathf.Max(minY + halfH, maxY - halfH));
        return new Vector2(x, y);
    }

    public void SnapToTarget()
    {
        if (target == null) return;
        basePos = Clamp((Vector2)target.position + offset);
        velocity = Vector3.zero;
        transform.position = new Vector3(basePos.x, basePos.y, transform.position.z);
    }
}
