using UnityEngine;
using System.Collections;

public class EnemyDissolve : MonoBehaviour
{
    [Header("Dissolve Settings")]
    public float dissolveTime = 1.5f;
    public string dissolveProperty = "_DissolveAmount"; // use if you have a dissolve shader
    public bool useSimpleFade = true; // fallback if no dissolve shader

    private Renderer[] renderers;
    private Material[] materials;
    private bool isDissolving;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        materials = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            materials[i] = renderers[i].material;
    }

    public void StartDissolve()
    {
        if (isDissolving) return;
        StartCoroutine(DissolveRoutine());
    }

    private IEnumerator DissolveRoutine()
    {
        isDissolving = true;
        float elapsed = 0f;

        while (elapsed < dissolveTime)
        {
            float t = elapsed / dissolveTime;

            foreach (Material mat in materials)
            {
                if (mat.HasProperty(dissolveProperty))
                {
                    mat.SetFloat(dissolveProperty, t);
                }
                else if (useSimpleFade && mat.HasProperty("_Color"))
                {
                    Color c = mat.color;
                    c.a = Mathf.Lerp(1f, 0f, t);
                    mat.color = c;
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
