using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float runSpeed = 5f;
    public float jumpForce = 6f;
    public float crouchSpeedMultiplier = 0.5f;
    public float zLimit = 1f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody rb;
    private PlayerControls controls;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private bool isGrounded;
    private bool isCrouching;
    private bool runHeld;
    private bool jumpPressed;
    private bool crouchPressed;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        controls = new PlayerControls();

        // Input bindings
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        controls.Player.Run.performed += ctx => runHeld = true;
        controls.Player.Run.canceled += ctx => runHeld = false;

        //controls.Player.Jump.performed += ctx => jumpPressed = true;

        //controls.Player.Crouch.performed += ctx => crouchPressed = !isCrouching;
    }

    void OnEnable() => controls.Player.Enable();
    void OnDisable() => controls.Player.Disable();

    void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleJump();
        HandleCrouch();
        LimitDepth();
    }

    void HandleMovement()
    {
        float currentSpeed = runHeld ? runSpeed : walkSpeed;
        if (isCrouching) currentSpeed *= crouchSpeedMultiplier;

        Vector3 input = new Vector3(moveInput.x, 0, moveInput.y);
        Vector3 targetVel = input * currentSpeed;

        rb.velocity = new Vector3(targetVel.x, rb.velocity.y, targetVel.z);
    }

    void HandleRotation()
    {
        float rotateAmount = lookInput.x;

        if (Mathf.Abs(rotateAmount) > 0.1f)
        {
            transform.Rotate(Vector3.up, rotateAmount * 100f * Time.deltaTime);
        }
    }

    void HandleJump()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        if (jumpPressed && isGrounded && !isCrouching)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        jumpPressed = false;
    }

    void HandleCrouch()
    {
        if (crouchPressed)
        {
            isCrouching = !isCrouching;
            float yScale = isCrouching ? 0.5f : 1f;
            transform.localScale = new Vector3(transform.localScale.x, yScale, 1f);
        }
        crouchPressed = false;
    }

    void LimitDepth()
    {
        float z = Mathf.Clamp(transform.position.z, -zLimit, zLimit);
        transform.position = new Vector3(transform.position.x, transform.position.y, z);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
