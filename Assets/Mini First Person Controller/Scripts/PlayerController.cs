using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 9f;
    public bool canRun = true;
    public KeyCode runningKey = KeyCode.LeftShift;

    [Header("Rotation")]
    public float rotationSpeed = 120f;

    private Rigidbody rb;
    private Animator anim;
    private FlashlightController flashlight;

    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    public bool IsRunning { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        flashlight = GetComponentInChildren<FlashlightController>();
        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        HandleMovement();
        UpdateAnimator();
    }

    private void HandleMovement()
    {
        IsRunning = canRun && Input.GetKey(runningKey);

        float targetSpeed = IsRunning ? runSpeed : walkSpeed;
        if (speedOverrides.Count > 0)
            targetSpeed = speedOverrides[speedOverrides.Count - 1]();

        // Rotation
        float turnInput = Input.GetAxis("Horizontal");
        if (Mathf.Abs(turnInput) > 0.01f)
        {
            float turnAmount = turnInput * rotationSpeed * Time.fixedDeltaTime;
            transform.Rotate(Vector3.up * turnAmount);
        }

        // Movement
        float moveInput = Input.GetAxis("Vertical");
        Vector3 move = transform.forward * moveInput * targetSpeed;
        rb.velocity = new Vector3(move.x, rb.velocity.y, move.z);
    }

    private void UpdateAnimator()
    {
        // Get current movement speed (0–1 normalized)
        Vector3 horizontalVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        float normalizedSpeed = Mathf.Clamp01(horizontalVel.magnitude / runSpeed);

        // Pass values to animator
        anim.SetFloat("Speed", normalizedSpeed, 0.1f, Time.deltaTime);
        anim.SetBool("HasPhone", flashlight != null && flashlight.IsOn);
    }
}
