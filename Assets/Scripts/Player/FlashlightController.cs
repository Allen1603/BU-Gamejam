using UnityEngine;
using UnityEngine.UI;

public class FlashlightController : MonoBehaviour
{
    [Header("Flashlight Settings")]
    public GameObject flashlightObject;
    public Light flashlightLight;
    public KeyCode holdKey = KeyCode.E;

    [Header("Battery Settings")]
    [Range(0, 100)] public float battery = 100f;
    public float drainRate = 5f;
    public float rechargeRate = 20f;
    public bool isRecharging = false;

    [Header("Flashlight Beam Settings")]
    public float maxDistance = 10f;
    public float dazzleRate = 20f;
    public LayerMask enemyMask;

    [Header("Beam Spread Settings")]
    [Range(3, 30)] public int rayCount = 10;     // Number of rays within the cone
    [Range(5f, 90f)] public float spreadAngle = 25f; // Width of the cone in degrees

    [Header("UI Settings")]
    public Slider batterySlider;

    private bool isOn = false;
    public bool IsOn => isOn;

    private void Start()
    {
        if (flashlightObject != null)
            flashlightObject.SetActive(false);
        if (flashlightLight != null)
            flashlightLight.enabled = false;

        if (batterySlider != null)
        {
            batterySlider.minValue = 0;
            batterySlider.maxValue = 100;
            batterySlider.value = battery;
        }
    }

    private void Update()
    {
        HandleInput();
        HandleBattery();
        UpdateBatteryUI();

        if (isOn)
            DazzleEnemies();
    }

    private void HandleInput()
    {
        if (Input.GetKey(holdKey) && battery > 0f)
        {
            if (!isOn) TurnOn();
        }
        else
        {
            if (isOn) TurnOff();
        }
    }

    private void HandleBattery()
    {
        if (isOn && battery > 0f)
        {
            battery -= drainRate * Time.deltaTime;
            battery = Mathf.Max(0f, battery);
        }

        if (battery <= 0f)
            TurnOff();

        if (isRecharging && battery < 100f)
        {
            battery += rechargeRate * Time.deltaTime;
            battery = Mathf.Min(battery, 100f);
        }
    }

    private void UpdateBatteryUI()
    {
        if (batterySlider != null)
            batterySlider.value = battery;
    }

    private void TurnOn()
    {
        isOn = true;
        if (flashlightObject != null) flashlightObject.SetActive(true);
        if (flashlightLight != null) flashlightLight.enabled = true;
    }

    private void TurnOff()
    {
        isOn = false;
        if (flashlightObject != null) flashlightObject.SetActive(false);
        if (flashlightLight != null) flashlightLight.enabled = false;
    }

    /// <summary>
    /// Casts multiple rays in a cone to detect enemies and apply dazzle effect.
    /// </summary>
    private void DazzleEnemies()
    {
        Vector3 origin = flashlightLight.transform.position;
        Vector3 forward = flashlightLight.transform.forward;

        for (int i = 0; i < rayCount; i++)
        {
            // Random direction within the cone
            Vector3 randomDirection = RandomDirectionInCone(forward, spreadAngle);
            if (Physics.Raycast(origin, randomDirection, out RaycastHit hit, maxDistance, enemyMask))
            {
                EnemyAI enemy = hit.collider.GetComponentInParent<EnemyAI>();
                if (enemy != null)
                {
                    enemy.ApplyDazzle(dazzleRate * Time.deltaTime);
                }
            }

            // Debug visualization
#if UNITY_EDITOR
            Debug.DrawRay(origin, randomDirection * maxDistance, Color.yellow, 0.02f);
#endif
        }
    }

    /// <summary>
    /// Returns a random direction vector within a cone.
    /// </summary>
    private Vector3 RandomDirectionInCone(Vector3 forward, float angle)
    {
        float randomYaw = Random.Range(-angle / 2f, angle / 2f);
        float randomPitch = Random.Range(-angle / 2f, angle / 2f);
        Quaternion rotation = Quaternion.Euler(randomPitch, randomYaw, 0);
        return rotation * forward;
    }

    public void RechargeBattery(float amount)
    {
        battery = Mathf.Min(battery + amount, 100f);
    }

    public void SetRecharging(bool value)
    {
        isRecharging = value;
    }

    private void OnDrawGizmosSelected()
    {
        if (flashlightLight == null)
            return;

        Gizmos.color = Color.yellow;
        Vector3 start = flashlightLight.transform.position;
        Vector3 forward = flashlightLight.transform.forward;

        // Draw main direction
        Gizmos.DrawLine(start, start + forward * maxDistance);

        // Draw cone bounds
        float halfAngle = spreadAngle * 0.5f;
        Vector3 right = Quaternion.Euler(0, halfAngle, 0) * forward;
        Vector3 left = Quaternion.Euler(0, -halfAngle, 0) * forward;
        Vector3 up = Quaternion.Euler(halfAngle, 0, 0) * forward;
        Vector3 down = Quaternion.Euler(-halfAngle, 0, 0) * forward;

        Gizmos.DrawLine(start, start + right * maxDistance);
        Gizmos.DrawLine(start, start + left * maxDistance);
        Gizmos.DrawLine(start, start + up * maxDistance);
        Gizmos.DrawLine(start, start + down * maxDistance);
    }
}
