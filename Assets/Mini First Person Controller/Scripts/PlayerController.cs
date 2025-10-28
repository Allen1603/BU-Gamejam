using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;

    [Header("Running")]
    public bool canRun = true;
    public bool IsRunning { get; private set; }
    public float runSpeed = 9f;
    public KeyCode runningKey = KeyCode.LeftShift;

    [Header("Rotation")]
    public float rotationSpeed = 120f; // degrees per second

    private Rigidbody rb;
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // prevents physics from rotating the body
    }

    void FixedUpdate()
    {
        // Determine if running
        IsRunning = canRun && Input.GetKey(runningKey);

        // Determine movement speed
        float targetSpeed = IsRunning ? runSpeed : speed;
        if (speedOverrides.Count > 0)
            targetSpeed = speedOverrides[speedOverrides.Count - 1]();

        // --- ROTATION ---
        float turnInput = Input.GetAxis("Horizontal"); // A/D keys
        if (Mathf.Abs(turnInput) > 0.01f)
        {
            float turnAmount = turnInput * rotationSpeed * Time.fixedDeltaTime;
            transform.Rotate(Vector3.up * turnAmount);
        }

        // --- FORWARD/BACKWARD MOVEMENT ---
        float moveInput = Input.GetAxis("Vertical"); // W/S keys
        Vector3 move = transform.forward * moveInput * targetSpeed;
        rb.velocity = new Vector3(move.x, rb.velocity.y, move.z);
    }
}
