using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class FlashlightController : MonoBehaviour
{
    [Header("Flashlight Settings")]
    public GameObject flashlightObject;
    public Light flashlightLight;

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
    [Range(3, 30)] public int rayCount = 10;
    [Range(5f, 90f)] public float spreadAngle = 25f;

    [Header("UI Settings")]
    public Slider batterySlider;

    private PlayerControls input;
    private bool flashlightHeld = false;
    private bool isOn = false;
    public bool IsOn => isOn;

    private void Awake()
    {
        input = new PlayerControls();

        input.Player.Flashlight.performed += ctx => flashlightHeld = true;
        input.Player.Flashlight.canceled += ctx => flashlightHeld = false;
    }

    private void OnEnable()
    {
        input.Player.Enable();
    }

    private void OnDisable()
    {
        input.Player.Disable();
    }

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
        if (flashlightHeld && battery > 0f)
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

    private void DazzleEnemies()
    {
        Vector3 origin = flashlightLight.transform.position;
        Vector3 forward = flashlightLight.transform.forward;

        for (int i = 0; i < rayCount; i++)
        {
            Vector3 randomDirection = RandomDirectionInCone(forward, spreadAngle);

            if (Physics.Raycast(origin, randomDirection, out RaycastHit hit, maxDistance, enemyMask))
            {
                EnemyAI enemy = hit.collider.GetComponentInParent<EnemyAI>();
                if (enemy != null)
                {
                    enemy.ApplyDazzle(dazzleRate * Time.deltaTime);
                }
            }

#if UNITY_EDITOR
            Debug.DrawRay(origin, randomDirection * maxDistance, Color.yellow, 0.02f);
#endif
        }
    }

    private Vector3 RandomDirectionInCone(Vector3 forward, float angle)
    {
        float randomYaw = Random.Range(-angle * 0.5f, angle * 0.5f);
        float randomPitch = Random.Range(-angle * 0.5f, angle * 0.5f);
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
}
