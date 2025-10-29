using UnityEngine;
using Cinemachine;

public class CameraDollyManager : MonoBehaviour
{
    [Header("References")]
    public CinemachineVirtualCamera dollyCamera; // Assign Dolly Camera with Track
    private CinemachineTrackedDolly trackedDolly;

    [Header("Movement Settings")]
    public bool isAutoMode = true;
    public float manualSpeed = 0.2f;  // Manual dolly speed when Auto is off
    public float moveSpeed = 2f;      // Smooth move speed toward target point

    [Header("Target Point")]
    public float targetPathPosition = 0f; // Absolute value along the track
    public bool lockToTarget = false;

    private float minPos;
    private float maxPos;

    private void Start()
    {
        if (dollyCamera == null)
        {
            Debug.LogError("Dolly Camera not assigned in CameraDollyManager.");
            return;
        }

        trackedDolly = dollyCamera.GetCinemachineComponent<CinemachineTrackedDolly>();
        if (trackedDolly == null)
        {
            Debug.LogError("This Virtual Camera does not have a Tracked Dolly component.");
            return;
        }

        minPos = trackedDolly.m_Path.MinPos;
        maxPos = trackedDolly.m_Path.MaxPos;

        SetAutoMode(isAutoMode);
    }

    private void Update()
    {
        if (trackedDolly == null) return;

        if (isAutoMode)
            return;

        if (lockToTarget)
        {
            trackedDolly.m_PathPosition = Mathf.Lerp(
                trackedDolly.m_PathPosition,
                targetPathPosition,
                Time.deltaTime * moveSpeed
            );
        }
        else
        {
            float move = Input.GetAxis("Horizontal");
            trackedDolly.m_PathPosition += move * manualSpeed * Time.deltaTime;
            trackedDolly.m_PathPosition = Mathf.Clamp(trackedDolly.m_PathPosition, minPos, maxPos);
        }
    }

    public void SetAutoMode(bool state)
    {
        isAutoMode = state;
        var autoDolly = trackedDolly.m_AutoDolly;
        autoDolly.m_Enabled = state;
        trackedDolly.m_AutoDolly = autoDolly;
        Debug.Log("Auto Dolly mode set to: " + state);
    }

    public void MoveToPathPosition(float targetIndex)
    {
        // Clamp to actual path bounds, not normalized
        targetPathPosition = targetIndex;
        lockToTarget = true;

        SetAutoMode(false);
    }

    public void ReleaseFromTarget()
    {
        lockToTarget = false;
        SetAutoMode(true);
    }
}
