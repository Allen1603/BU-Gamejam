using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    public CameraDollyManager camManager;
    public float targetPathPosition; // Exact value along the path (not normalized)

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            camManager.MoveToPathPosition(targetPathPosition);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            camManager.ReleaseFromTarget();
        }
    }
}
