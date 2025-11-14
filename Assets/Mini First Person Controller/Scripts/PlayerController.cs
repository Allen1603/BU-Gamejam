using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 9f;
    public bool canRun = true;

    [Header("Rotation")]
    public float rotationSpeed = 120f;

    private Rigidbody rb;
    private Animator anim;
    private FlashlightController flashlight;

    private PlayerControls input;
    private Vector2 moveInput;
    private Vector2 lookInput;

    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    public bool IsRunning { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        flashlight = GetComponentInChildren<FlashlightController>();
        rb.freezeRotation = true;

        input = new PlayerControls();

        // Read continuous inputs
        input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        input.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        input.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        input.Player.Run.performed += ctx => IsRunning = ctx.ReadValue<float>() > 0.5f;
        input.Player.Run.canceled += ctx => IsRunning = false;
    }

    void OnEnable()
    {
        input.Player.Enable();
    }

    void OnDisable()
    {
        input.Player.Disable();
    }

    void FixedUpdate()
    {
        HandleMovement();
        UpdateAnimator();
    }

    private void HandleMovement()
    {
        float targetSpeed = IsRunning && canRun ? runSpeed : walkSpeed;

        if (speedOverrides.Count > 0)
            targetSpeed = speedOverrides[speedOverrides.Count - 1]();

        // Rotation (Right Stick / A-D)
        if (Mathf.Abs(lookInput.x) > 0.01f)
        {
            float turnAmount = lookInput.x * rotationSpeed * Time.fixedDeltaTime;
            transform.Rotate(Vector3.up * turnAmount);
        }

        // Movement (Left Stick / W-S)
        Vector3 move = transform.forward * moveInput.y * targetSpeed;
        rb.velocity = new Vector3(move.x, rb.velocity.y, move.z);
    }

    private void UpdateAnimator()
    {
        Vector3 horizontalVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        float normalizedSpeed = Mathf.Clamp01(horizontalVel.magnitude / runSpeed);

        anim.SetFloat("Speed", normalizedSpeed, 0.1f, Time.deltaTime);
        anim.SetBool("HasPhone", flashlight != null && flashlight.IsOn);
    }
}
