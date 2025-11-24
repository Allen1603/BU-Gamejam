using UnityEngine;
using UnityEngine.InputSystem;

public class ChargingStation : MonoBehaviour
{
    [Header("Charging Settings")]
    public float chargeRate = 25f;
    public float detectionRadius = 2f;

    [Header("Station Battery Settings")]
    [Range(0, 100)] public float stationBattery = 100f;
    public float drainRate = 20f;
    public float rechargeRate = 5f;
    public bool autoRecharge = false;

    [Header("Currency Cost Settings")]
    public bool useCurrencyCost = true;
    public int currencyCost = 10;

    [Header("References")]
    public FlashlightController flashlight;
    public Light stationLight;
    public LayerMask playerLayer;

    private bool isPlayerNearby;
    private bool isCharging;
    private bool hasPaid;

    private PlayerController playerController;
    private PlayerCurrency playerCurrency;

    // Input System
    private PlayerControls input;
    private InputAction chargeAction;

    private void Awake()
    {
        input = new PlayerControls();
        chargeAction = input.Player.Charge;

        chargeAction.performed += OnChargePressed;
    }

    private void OnEnable()
    {
        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }

    private void Update()
    {
        DetectPlayer();

        bool hasPower = stationBattery > 0f;
        bool flashlightNotFull = flashlight != null && flashlight.battery < 100f;

        if (isCharging && flashlightNotFull && hasPower)
        {
            flashlight.RechargeBattery(chargeRate * Time.deltaTime);

            stationBattery -= drainRate * Time.deltaTime;
            stationBattery = Mathf.Max(0f, stationBattery);

            if (flashlight.battery >= 100f)
                StopCharging();
        }
        else if (isCharging && (!hasPower || !flashlightNotFull))
        {
            StopCharging();
        }

        if (isCharging && !isPlayerNearby)
            StopCharging();

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

        if (!isPlayerNearby) return;

        foreach (var hit in hits)
        {
            if (playerController == null)
                playerController = hit.GetComponent<PlayerController>();

            if (playerCurrency == null)
                playerCurrency = hit.GetComponent<PlayerCurrency>();

            if (flashlight == null)
                flashlight = hit.GetComponentInChildren<FlashlightController>();
        }
    }

    private void OnChargePressed(InputAction.CallbackContext ctx)
    {
        if (!isPlayerNearby) return;
        if (isCharging) return;
        if (flashlight == null) return;
        if (flashlight.battery >= 100f) return;
        if (stationBattery <= 0f) return;

        if (useCurrencyCost && playerCurrency != null && !hasPaid)
        {
            if (playerCurrency.DeductCurrency(currencyCost))
                hasPaid = true;
            else
                return;
        }

        StartCharging();
    }

    private void StartCharging()
    {
        if (isCharging) return;

        if (AudioManager.instance != null)
            AudioManager.instance.Playsfx(AudioManager.instance.chargingSFX);

        isCharging = true;

        if (playerController != null)
            playerController.enabled = false;

        if (flashlight != null)
            flashlight.SetRecharging(true);
    }

    private void StopCharging()
    {
        if (!isCharging) return;

        isCharging = false;

        if (playerController != null)
            playerController.enabled = true;

        if (flashlight != null)
            flashlight.SetRecharging(false);

        hasPaid = false;
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
            stationLight.color = Color.cyan;
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
