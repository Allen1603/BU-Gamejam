using UnityEngine;
using System.Collections.Generic;

namespace DissolveExample
{
    public class DissolveChilds : MonoBehaviour
    {
        private readonly List<Material> materials = new List<Material>();

        void Start()
        {
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in renderers)
            {
                foreach (Material mat in rend.materials)
                {
                    // Clone materials to avoid shared reference issues
                    Material newMat = new Material(mat);
                    rend.material = newMat;
                    materials.Add(newMat);
                }
            }
        }

        private void Update()
        {
            float value = Mathf.PingPong(Time.time * 0.5f, 1f);
            SetValue(value);
        }

        public void SetValue(float value)
        {
            foreach (Material mat in materials)
            {
                if (mat.HasProperty("_Dissolve"))
                    mat.SetFloat("_Dissolve", value);
                // No grayscale loss — keep albedo
            }
        }
    }
}
