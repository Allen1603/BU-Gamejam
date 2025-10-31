using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Smoothly dissolves this GameObject and all its child renderers using a Shader Graph dissolve material.
/// Triggered only when StartDissolve() is called (e.g. on death).
/// </summary>
public class EnemyDissolve : MonoBehaviour
{
    [Header("Dissolve Settings")]
    [Tooltip("Time in seconds for the dissolve effect to complete.")]
    [SerializeField] private float dissolveTime = 1.5f;

    [Tooltip("Shader property controlling dissolve amount (float).")]
    [SerializeField] private string dissolveProperty = "_Dissolve";

    [Tooltip("Use alpha fade if shader has no dissolve property.")]
    [SerializeField] private bool useSimpleFade = true;

    [Header("Material Settings")]
    [Tooltip("Assign your dissolve Shader Graph material here (optional). If not assigned, will use original materials with dissolve effect.")]
    [SerializeField] private Material dissolveMaterial;

    private List<Material> originalMaterials = new List<Material>();
    private List<Renderer> renderers = new List<Renderer>();
    private bool isDissolving = false;
    private float currentDissolveValue = 0f;
    private bool materialsSwapped = false;

    private void Start()
    {
        // Cache renderers and original materials like DissolveChilds
        var allRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in allRenderers)
        {
            renderers.Add(rend);

            // Store original materials
            foreach (Material mat in rend.materials)
            {
                originalMaterials.Add(mat);
            }
        }
    }

    /// <summary>
    /// Starts the dissolve effect. Call this from EnemyAI when the enemy dies.
    /// </summary>
    public void StartDissolve()
    {
        if (isDissolving) return;
        StartCoroutine(DissolveRoutine());
    }

    private IEnumerator DissolveRoutine()
    {
        isDissolving = true;

        // Swap to dissolve materials only when dissolve starts
        SwapToDissolveMaterials();

        float elapsed = 0f;

        while (elapsed < dissolveTime)
        {
            elapsed += Time.deltaTime;

            // Smooth animation from 0 to 1
            float value = Mathf.Clamp01(elapsed / dissolveTime);
            SetValue(value);
            yield return null;
        }

        if (AudioManager.instance != null)
            AudioManager.instance.Playsfx(AudioManager.instance.enemyDisolve);
        // Final state (fully dissolved)
        SetValue(1f);

        // Small delay to ensure the final dissolve state is visible
        yield return new WaitForSeconds(0.1f);

        Destroy(gameObject);
    }

    /// <summary>
    /// Swaps all renderers to use dissolve materials (called only when dissolve starts)
    /// </summary>
    private void SwapToDissolveMaterials()
    {
        if (materialsSwapped) return;

        foreach (Renderer rend in renderers)
        {
            Material[] newMats = new Material[rend.materials.Length];

            for (int i = 0; i < rend.materials.Length; i++)
            {
                Material materialToUse;

                if (dissolveMaterial != null)
                {
                    // Use the assigned dissolve material
                    materialToUse = new Material(dissolveMaterial);

                    // Copy main texture and color from original material to preserve appearance
                    Material originalMat = originalMaterials[i];
                    if (originalMat.HasProperty("_MainTex") && materialToUse.HasProperty("_MainTex"))
                    {
                        materialToUse.SetTexture("_MainTex", originalMat.GetTexture("_MainTex"));
                    }
                    if (originalMat.HasProperty("_Color") && materialToUse.HasProperty("_Color"))
                    {
                        materialToUse.SetColor("_Color", originalMat.GetColor("_Color"));
                    }
                    if (originalMat.HasProperty("_BaseColor") && materialToUse.HasProperty("_BaseColor"))
                    {
                        materialToUse.SetColor("_BaseColor", originalMat.GetColor("_BaseColor"));
                    }
                }
                else
                {
                    // Use cloned original material (like DissolveChilds)
                    materialToUse = new Material(rend.materials[i]);
                }

                // Initialize dissolve value (fully visible)
                if (materialToUse.HasProperty(dissolveProperty))
                {
                    materialToUse.SetFloat(dissolveProperty, 0f);
                }

                newMats[i] = materialToUse;
            }

            rend.materials = newMats;
        }

        materialsSwapped = true;
    }

    /// <summary>
    /// Sets the dissolve value on all materials (same approach as DissolveChilds)
    /// </summary>
    /// <param name="value">Dissolve amount from 0 (fully visible) to 1 (fully dissolved)</param>
    public void SetValue(float value)
    {
        if (!materialsSwapped) return;

        currentDissolveValue = value;

        foreach (Renderer rend in renderers)
        {
            foreach (Material mat in rend.materials)
            {
                if (mat.HasProperty(dissolveProperty))
                {
                    mat.SetFloat(dissolveProperty, value);
                }
                else if (useSimpleFade && mat.HasProperty("_Color"))
                {
                    Color c = mat.color;
                    c.a = Mathf.Lerp(1f, 0f, value);
                    mat.color = c;
                }
                // No grayscale loss - keep albedo like in DissolveChilds
            }
        }
    }

    /// <summary>
    /// Immediately sets the dissolve state without animation
    /// </summary>
    public void SetDissolveState(float value)
    {
        if (!materialsSwapped)
        {
            SwapToDissolveMaterials();
        }
        SetValue(value);
    }

    private void OnDestroy()
    {
        // Clean up any dissolve materials we created
        if (materialsSwapped)
        {
            foreach (Renderer rend in renderers)
            {
                if (rend != null)
                {
                    foreach (Material mat in rend.materials)
                    {
                        if (mat != null && originalMaterials.Contains(mat) == false)
                        {
                            if (Application.isPlaying)
                                Destroy(mat);
                            else
                                DestroyImmediate(mat);
                        }
                    }
                }
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Test Dissolve")]
    private void TestDissolve()
    {
        StartDissolve();
    }

    [ContextMenu("Reset Dissolve")]
    private void ResetDissolve()
    {
        if (materialsSwapped)
        {
            // Revert to original materials
            int materialIndex = 0;
            foreach (Renderer rend in renderers)
            {
                Material[] originalMats = new Material[rend.materials.Length];
                for (int i = 0; i < rend.materials.Length; i++)
                {
                    originalMats[i] = originalMaterials[materialIndex + i];
                }
                rend.materials = originalMats;
                materialIndex += rend.materials.Length;
            }
            materialsSwapped = false;
        }
        isDissolving = false;
    }

    [ContextMenu("Set Half Dissolved")]
    private void SetHalfDissolved()
    {
        SetDissolveState(0.5f);
    }
#endif
}