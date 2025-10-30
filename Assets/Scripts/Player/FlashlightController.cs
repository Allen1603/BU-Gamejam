using UnityEngine;
using System.Collections.Generic;

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
    public float maxDistance = 10f;     // Light range
    public float dazzleRate = 20f;      // How fast enemies get "burned" by light
    public LayerMask enemyMask;         // Assign your "Enemy" layer

    private bool isOn = false;
    public bool IsOn => isOn;


    private void Start()
    {
        if (flashlightObject != null)
            flashlightObject.SetActive(false);
        if (flashlightLight != null)
            flashlightLight.enabled = false;
    }

    private void Update()
    {
        HandleInput();
        HandleBattery();

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
        Ray ray = new Ray(flashlightLight.transform.position, flashlightLight.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, enemyMask))
        {
            EnemyAI enemy = hit.collider.GetComponentInParent<EnemyAI>();
            if (enemy != null)
            {
                enemy.ApplyDazzle(dazzleRate * Time.deltaTime);
            }
        }
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
