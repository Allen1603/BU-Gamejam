using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 5f;
    public float jumpForce = 6f;
    public float crouchSpeedMultiplier = 0.5f;
    public float zLimit = 1f; // limit depth movement

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody rb;
    private bool isGrounded;
    private bool isCrouching;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        HandleMovement();
        HandleJump();
        HandleCrouch();
        LimitDepth();
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical"); // slight depth movement allowed

        // Choose between walk or run
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        if (isCrouching) currentSpeed *= crouchSpeedMultiplier;

        Vector3 targetVelocity = new Vector3(moveX, 0, moveZ) * currentSpeed;
        Vector3 velocity = rb.velocity;
        velocity.x = targetVelocity.x;
        velocity.z = targetVelocity.z;
        rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);

        // Flip character to face movement direction
        if (moveX != 0)
            transform.localScale = new Vector3(Mathf.Sign(moveX), 1, 1);
    }

    void HandleJump()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
        if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z); // reset Y
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = true;
            transform.localScale = new Vector3(transform.localScale.x, 0.5f, 1f);
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            isCrouching = false;
            transform.localScale = new Vector3(transform.localScale.x, 1f, 1f);
        }
    }

    void LimitDepth()
    {
        float clampedZ = Mathf.Clamp(transform.position.z, -zLimit, zLimit);
        transform.position = new Vector3(transform.position.x, transform.position.y, clampedZ);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
