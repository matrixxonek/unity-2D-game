using UnityEngine;

/// Zwykły zombie: patroluje półkę, zawraca przy ścianie i przy krawędzi.
/// Z włączonym pościgiem idzie w stronę gracza, ale nadal nie schodzi z półki.
[RequireComponent(typeof(Rigidbody2D), typeof(Health))]
public class EnemyWalker : MonoBehaviour
{
    [Header("Ruch")]
    public float moveSpeed = 2.2f;
    public float edgeProbeAhead = 0.5f;
    public float edgeProbeDepth = 1.2f;
    public LayerMask groundLayer;

    [Header("Pościg")]
    public bool chasePlayer;
    public float chaseRange = 6f;

    [Header("Wizualizacja")]
    public SpriteRenderer sprite;

    int direction = -1;
    Rigidbody2D rb;
    EnemyFeedback feedback;
    Transform player;
    float turnCooldown;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        feedback = GetComponent<EnemyFeedback>();
        if (groundLayer.value == 0) groundLayer = Layers.GroundMask;
    }

    void Start()
    {
        var pm = FindFirstObjectByType<PlayerMovement>();
        if (pm != null) player = pm.transform;
    }

    void FixedUpdate()
    {
        if (feedback != null && Time.time < feedback.StunUntil) return;

        turnCooldown -= Time.fixedDeltaTime;

        if (chasePlayer && player != null && turnCooldown <= 0f)
        {
            float dx = player.position.x - transform.position.x;
            if (Mathf.Abs(dx) < chaseRange && Mathf.Abs(player.position.y - transform.position.y) < 2.5f)
            {
                int wanted = dx > 0 ? 1 : -1;
                if (wanted != direction) { direction = wanted; turnCooldown = 0.4f; }
            }
        }

        Vector2 ahead = (Vector2)transform.position + new Vector2(direction * edgeProbeAhead, 0f);
        bool groundAhead = Physics2D.Raycast(ahead, Vector2.down, edgeProbeDepth, groundLayer);
        bool wallAhead = Physics2D.Raycast(transform.position, new Vector2(direction, 0f), edgeProbeAhead + 0.1f, groundLayer);

        if ((!groundAhead || wallAhead) && turnCooldown <= 0f)
        {
            direction = -direction;
            turnCooldown = 0.25f;
        }

        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
        if (sprite != null) sprite.flipX = direction < 0;
    }
}
