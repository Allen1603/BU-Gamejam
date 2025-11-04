using UnityEngine;

public class ChargingStation : MonoBehaviour
{
    [Header("Charging Settings")]
    public float chargeRate = 25f;            // How fast it charges the player's flashlight
    public float detectionRadius = 2f;        // Radius for detecting player
    public KeyCode interactKey = KeyCode.E;   // Key to start charging

    [Header("Station Battery Settings")]
    [Range(0, 100)] public float stationBattery = 100f;  // Station's internal power
    public float drainRate = 20f;                         // How fast the station loses power when charging
    public float rechargeRate = 5f;                       // Optional: auto-recharge rate (if desired)
    public bool autoRecharge = false;                     // If true, it slowly recharges itself over time

    [Header("Currency Cost Settings")]
    public bool useCurrencyCost = true;                   // Whether to charge currency for charging
    public int currencyCost = 10;                         // One-time cost to charge to full

    [Header("References")]
    public FlashlightController flashlight; // Assign player flashlight
    public Light stationLight;              // Optional indicator light
    public LayerMask playerLayer;           // Assign Player layer

    private bool isPlayerNearby;
    private bool isCharging;
    private PlayerController playerController;
    private PlayerCurrency playerCurrency;
    private bool hasPaid = false;

    private void Update()
    {
        DetectPlayer();

        bool hasPower = stationBattery > 0f;
        bool flashlightNotFull = flashlight != null && flashlight.battery < 100f;
        bool hasEnoughCurrency = !useCurrencyCost || (playerCurrency != null && playerCurrency.CurrentCurrency >= currencyCost);

        // Handle charging start with E key
        if (isPlayerNearby && !isCharging && Input.GetKeyDown(interactKey) && hasPower && flashlightNotFull && hasEnoughCurrency)
        {
            StartCharging();
        }

        // Handle charging process
        if (isCharging && flashlight != null && hasPower && flashlightNotFull)
        {
            // Charge flashlight
            flashlight.RechargeBattery(chargeRate * Time.deltaTime);
            stationBattery -= drainRate * Time.deltaTime;
            stationBattery = Mathf.Max(0f, stationBattery);

            // Check if charging is complete
            if (flashlight.battery >= 100f)
            {
                StopCharging();
            }
        }
        else if (isCharging && (!hasPower || !flashlightNotFull))
        {
            // Conditions no longer met, stop charging
            StopCharging();
        }

        // Allow player to cancel charging by moving away
        if (isCharging && !isPlayerNearby)
        {
            StopCharging();
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

        // Get player components if nearby
        if (isPlayerNearby && (playerController == null || playerCurrency == null || flashlight == null))
        {
            foreach (var hit in hits)
            {
                if (playerController == null)
                    playerController = hit.GetComponent<PlayerController>();
                if (playerCurrency == null)
                    playerCurrency = hit.GetComponent<PlayerCurrency>();
                if (flashlight == null)
                    flashlight = hit.GetComponentInChildren<FlashlightController>();

                if (playerController != null && playerCurrency != null && flashlight != null)
                    break;
            }
        }
    }

    private void StartCharging()
    {
        if (isCharging || flashlight == null || playerController == null) return;

        // Deduct currency immediately
        if (useCurrencyCost && playerCurrency != null && !hasPaid)
        {
            if (playerCurrency.DeductCurrency(currencyCost))
            {
                hasPaid = true;
            }
            else
            {
                // Not enough currency, don't start charging
                return;
            }
        }

        isCharging = true;

        // Disable player movement
        playerController.enabled = false;

        // Set recharging state in flashlight (this will handle the flashlight behavior)
        flashlight.SetRecharging(true);

        Debug.Log("Charging started - Player movement disabled, flashlight in recharge mode");
    }

    private void StopCharging()
    {
        if (!isCharging) return;

        isCharging = false;

        // Re-enable player movement
        if (playerController != null)
            playerController.enabled = true;

        // Stop recharging state in flashlight
        if (flashlight != null)
        {
            flashlight.SetRecharging(false);
        }

        // Reset payment status
        hasPaid = false;

        Debug.Log("Charging stopped - Player movement enabled");
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