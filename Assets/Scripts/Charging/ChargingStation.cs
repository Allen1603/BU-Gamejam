using UnityEngine;

public class ChargingStation : MonoBehaviour
{
    [Header("Charging Settings")]
    public float chargeRate = 25f;            // How fast it charges the player's flashlight
    public float detectionRadius = 2f;        // Radius for detecting player
    public KeyCode interactKey = KeyCode.E;

    [Header("Station Battery Settings")]
    [Range(0, 100)] public float stationBattery = 100f;  // Station’s internal power
    public float drainRate = 20f;                         // How fast the station loses power when charging
    public float rechargeRate = 5f;                       // Optional: auto-recharge rate (if desired)
    public bool autoRecharge = false;                     // If true, it slowly recharges itself over time

    [Header("References")]
    public FlashlightController flashlight; // Assign player flashlight
    public Light stationLight;              // Optional indicator light
    public LayerMask playerLayer;           // Assign Player layer

    private bool isPlayerNearby;
    private bool isCharging;

    private void Update()
    {
        DetectPlayer();

        bool hasPower = stationBattery > 0f;
        bool flashlightNotFull = flashlight != null && flashlight.battery < 100f;

        // Start charging only if player is near, holding E, station has power, and flashlight not full
        if (isPlayerNearby && Input.GetKey(interactKey) && hasPower && flashlightNotFull)
            StartCharging();
        else
            StopCharging();

        if (isCharging && flashlight != null && hasPower && flashlight.battery < 100f)
        {
            flashlight.RechargeBattery(chargeRate * Time.deltaTime);
            stationBattery -= drainRate * Time.deltaTime;
            stationBattery = Mathf.Max(0f, stationBattery);
        }

        // Optional self-recharge logic
        if (autoRecharge && !isCharging && stationBattery < 100f)
        {
            stationBattery += rechargeRate * Time.deltaTime;
            stationBattery = Mathf.Min(100f, stationBattery);
        }

        UpdateStationLight();
    }

    private void DetectPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);
        isPlayerNearby = hits.Length > 0;
    }

    private void StartCharging()
    {
        if (isCharging || flashlight == null) return;
        isCharging = true;
        flashlight.SetRecharging(true);
    }

    private void StopCharging()
    {
        if (!isCharging || flashlight == null) return;
        isCharging = false;
        flashlight.SetRecharging(false);
    }

    private void UpdateStationLight()
    {
        if (stationLight == null) return;

        if (stationBattery <= 0f)
        {
            stationLight.color = Color.gray;
            stationLight.intensity = 1.5f;
        }
        else if (flashlight != null && flashlight.battery >= 100f)
        {
            stationLight.color = Color.cyan; // indicate flashlight full
            stationLight.intensity = 3f;
        }
        else if (isCharging)
        {
            stationLight.color = Color.green;
            stationLight.intensity = 5f;
        }
        else if (isPlayerNearby)
        {
            stationLight.color = Color.yellow;
            stationLight.intensity = 3f;
        }
        else
        {
            stationLight.color = Color.red;
            stationLight.intensity = 2f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
