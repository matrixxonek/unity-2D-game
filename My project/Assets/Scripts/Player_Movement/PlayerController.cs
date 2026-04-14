using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerMovement : MonoBehaviour
{
    [Header("Parametry Ruchu")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;

    [Header("Wykrywanie Ziemi")]
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float left = Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed ? 1 : 0;
        float right = Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed ? 1 : 0;
        moveInput = right - left;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        Debug.Log(isGrounded);

        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        FlipSprite();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    void FlipSprite()
    {
        if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);
    }


bool IsPlayerGrounded()
{
    CapsuleCollider2D collider = GetComponent<CapsuleCollider2D>(); 
    
    RaycastHit2D hit = Physics2D.BoxCast(collider.bounds.center, collider.bounds.size, 0f, Vector2.down, 0.1f, groundLayer);
    
    return hit.collider != null;
}
}