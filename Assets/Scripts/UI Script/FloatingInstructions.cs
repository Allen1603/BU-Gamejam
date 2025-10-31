using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Collider))]
public class FloatingInstruction : MonoBehaviour
{
    [Header("UI Setup")]
    public Canvas instructionCanvas;      // World Space Canvas
    public TMP_Text instructionText;          // Text UI element
    [TextArea] public string message = "Press E to interact";

    [Header("Settings")]
    public bool smoothFade = true;
    public float fadeSpeed = 3f;

    private Transform playerCamera;
    private CanvasGroup canvasGroup;
    private bool isPlayerInRange = false;

    private void Start()
    {
        // Ensure collider is trigger
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        if (instructionCanvas == null)
            instructionCanvas = GetComponentInChildren<Canvas>();

        if (instructionText != null)
            instructionText.text = message;

        // Add CanvasGroup for fade
        canvasGroup = instructionCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = instructionCanvas.gameObject.AddComponent<CanvasGroup>();

        // Hide initially
        canvasGroup.alpha = 0f;

        // Get camera
        if (Camera.main != null)
            playerCamera = Camera.main.transform;
    }

    private void Update()
    {
        // Face the player camera
        if (playerCamera != null && instructionCanvas != null)
        {
            instructionCanvas.transform.rotation = Quaternion.LookRotation(
                instructionCanvas.transform.position - playerCamera.position
            );
        }

        // Fade in/out smoothly
        float targetAlpha = isPlayerInRange ? 1f : 0f;
        if (smoothFade)
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
        else
            canvasGroup.alpha = targetAlpha;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            isPlayerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            isPlayerInRange = false;
    }
}
