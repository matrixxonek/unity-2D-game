using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Brama: lity kolider, który podjeżdża do góry. Adresowana po id z triggerów.
public class Gate : MonoBehaviour
{
    public string id;
    public float height = 4f;
    public float speed = 5f;
    public bool startOpen;

    public bool IsOpen { get; private set; }

    static readonly Dictionary<string, Gate> registry = new Dictionary<string, Gate>();

    Vector3 closedPos, openPos;
    Collider2D col;
    Coroutine moving;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        closedPos = transform.position;
        openPos = closedPos + Vector3.up * height;
        if (!string.IsNullOrEmpty(id)) registry[id] = this;

        if (startOpen)
        {
            transform.position = openPos;
            if (col != null) col.enabled = false;
            IsOpen = true;
        }
    }

    void OnDestroy()
    {
        if (!string.IsNullOrEmpty(id) && registry.TryGetValue(id, out var g) && g == this) registry.Remove(id);
    }

    public static Gate Find(string id) =>
        !string.IsNullOrEmpty(id) && registry.TryGetValue(id, out var g) ? g : null;

    public void Open()
    {
        if (IsOpen) return;
        IsOpen = true;
        Sfx.Play("gate");
        CameraShake.Shake(0.08f, 0.3f);
        if (moving != null) StopCoroutine(moving);
        moving = StartCoroutine(MoveTo(openPos, false));
    }

    public void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;
        Sfx.Play("gate", 1f, 0.8f);
        CameraShake.Shake(0.12f, 0.3f);
        if (col != null) col.enabled = true;
        if (moving != null) StopCoroutine(moving);
        moving = StartCoroutine(MoveTo(closedPos, true));
    }

    IEnumerator MoveTo(Vector3 target, bool solid)
    {
        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = target;
        if (col != null) col.enabled = solid;
        if (!solid) Particles.Burst(closedPos, new Color(0.6f, 0.65f, 0.75f), 8, 2f, 0.4f, 8f, 0.1f);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => registry.Clear();
}
