using UnityEngine;

/// Ruch postaci. Wejścia przychodzą z PlayerInputReader — ten skrypt nie czyta
/// klawiatury sam.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Parametry Ruchu")]
    public float moveSpeed = 8f;
    public float jumpForce = 15f;

    [Header("Wykrywanie Ziemi")]
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Wygoda sterowania")]
    [Tooltip("Ile jeszcze można skoczyć po zejściu z krawędzi.")]
    public float coyoteTime = 0.1f;

    [Tooltip("Ile wcześniej można wcisnąć skok przed dotknięciem ziemi.")]
    public float jumpBufferTime = 0.12f;

    [Tooltip("Puszczenie skoku w locie ścina wznoszenie — daje kontrolę nad wysokością.")]
    [Range(0f, 1f)] public float jumpCutMultiplier = 0.45f;

    [Header("Dynamika skoku")]
    public float riseGravity = 3.6f;
    public float fallMultiplier = 1.9f;
    public float apexThreshold = 2.5f;
    [Range(0.1f, 1f)] public float apexGravityMultiplier = 0.55f;
    public float maxFallSpeed = 26f;

    [Header("Squash & stretch")]
    public Transform spriteTransform;
    public float squashAmount = 0.22f;
    public float squashRecovery = 12f;

    public bool IsGrounded { get; private set; }
    public Collider2D GroundCollider { get; private set; }
    public int Facing { get; private set; } = 1;
    public bool DoubleJumpUnlocked => abilities == null || abilities.doubleJump;

    Rigidbody2D rb;
    PlayerInputReader input;
    SequenceRunner sequences;
    PlayerAbilities abilities;

    float lastGroundedTime = -99f;
    float lastJumpPressedTime = -99f;
    float controlLockedUntil = -99f;
    bool doubleJumpAvailable;
    bool jumpHeld;
    bool wasGrounded;
    float peakFallSpeed;
    Vector2 squash = Vector2.one;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        input = GetComponent<PlayerInputReader>();
        sequences = GetComponent<SequenceRunner>();
        abilities = GetComponent<PlayerAbilities>();
    }

    void Update()
    {
        GroundCollider = groundCheck != null
            ? Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer)
            : null;
        IsGrounded = GroundCollider != null;

        if (!IsGrounded) peakFallSpeed = Mathf.Min(peakFallSpeed, rb.linearVelocity.y);

        if (IsGrounded)
        {
            lastGroundedTime = Time.time;
            doubleJumpAvailable = DoubleJumpUnlocked;
            if (!wasGrounded) Land();
        }
        wasGrounded = IsGrounded;

        if (input.JumpPressed)
        {
            lastJumpPressedTime = Time.time;
            jumpHeld = true;
        }

        RecoverSquash();

        bool locked = (sequences != null && sequences.IsRunning) || Time.time < controlLockedUntil;
        if (locked) return;

        if (input.Move > 0.01f) Facing = 1;
        else if (input.Move < -0.01f) Facing = -1;
        transform.localScale = new Vector3(Facing, 1f, 1f);

        if (IsGrounded && Mathf.Abs(input.Move) > 0.01f) PlayerEvents.RaiseMoved();

        TryJump();
        ApplyJumpCut();
    }

    void TryJump()
    {
        if (Time.time - lastJumpPressedTime > jumpBufferTime) return;

        bool coyote = Time.time - lastGroundedTime <= coyoteTime;

        if (IsGrounded || coyote)
        {
            Jump();
            lastGroundedTime = -99f;
            Sfx.Play("jump");
            PlayerEvents.RaiseJumped();
        }
        else if (doubleJumpAvailable)
        {
            doubleJumpAvailable = false;
            Jump();
            Sfx.Play("double_jump");
            Particles.Burst(groundCheck != null ? groundCheck.position : transform.position,
                            new Color(0.5f, 0.95f, 1f), 10, 3.5f, 0.35f, 4f, 0.1f, 140f, -90f);
            PlayerEvents.RaiseDoubleJumped();
        }
    }

    void Jump()
    {
        lastJumpPressedTime = -99f;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        squash = new Vector2(1f - squashAmount, 1f + squashAmount);
    }

    void Land()
    {
        PlayerEvents.RaiseLanded();
        if (peakFallSpeed < -6f)
        {
            float k = Mathf.Clamp01(-peakFallSpeed / 22f);
            squash = new Vector2(1f + squashAmount * (0.6f + k), 1f - squashAmount * (0.6f + k));
            Sfx.Play("land", 0.5f + k * 0.5f, 1f - k * 0.2f);
            Particles.Burst(groundCheck != null ? groundCheck.position : transform.position,
                            new Color(0.7f, 0.7f, 0.65f), 4 + (int)(k * 8), 2.5f, 0.3f, 6f, 0.09f, 120f, 90f);
        }
        peakFallSpeed = 0f;
    }

    void ApplyJumpCut()
    {
        if (!jumpHeld) return;
        if (input.JumpHeld) return;

        jumpHeld = false;
        if (rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
    }

    void RecoverSquash()
    {
        if (spriteTransform == null) return;
        squash = Vector2.Lerp(squash, Vector2.one, Time.deltaTime * squashRecovery);
        spriteTransform.localScale = new Vector3(squash.x, squash.y, 1f);
    }

    void FixedUpdate()
    {
        ApplyGravityFeel();

        bool locked = (sequences != null && sequences.IsRunning) || Time.time < controlLockedUntil;
        if (sequences != null && sequences.IsRunning)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }
        if (locked) return;

        rb.linearVelocity = new Vector2(input.Move * moveSpeed, rb.linearVelocity.y);
    }

    /// Trzy różne grawitacje: wznoszenie, szczyt, opadanie. To one, a nie sama siła
    /// skoku, decydują o tym, czy skok czyta się jako dynamiczny.
    void ApplyGravityFeel()
    {
        float verticalSpeed = rb.linearVelocity.y;

        if (!IsGrounded && Mathf.Abs(verticalSpeed) < apexThreshold)
            rb.gravityScale = riseGravity * apexGravityMultiplier;
        else if (verticalSpeed < -0.01f)
            rb.gravityScale = riseGravity * fallMultiplier;
        else
            rb.gravityScale = riseGravity;

        if (verticalSpeed < -maxFallSpeed)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
    }

    /// Wywoływane przez pogo — łańcuch odbić utrzymuje gracza w powietrzu.
    public void RefreshDoubleJump() => doubleJumpAvailable = DoubleJumpUnlocked;

    /// Odrzut od obrażeń: nadaje prędkość i na chwilę zabiera kontrolę, żeby
    /// FixedUpdate nie nadpisał jej od razu wejściem.
    public void Knockback(Vector2 velocity, float lockTime)
    {
        rb.linearVelocity = velocity;
        controlLockedUntil = Time.time + lockTime;
    }

    public void ResetMotion()
    {
        rb.linearVelocity = Vector2.zero;
        controlLockedUntil = -99f;
        peakFallSpeed = 0f;
        squash = Vector2.one;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
    }
}
