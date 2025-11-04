using UnityEngine;
using TMPro;

/// <summary>
/// Manages the player's total currency (coins, money, etc.).
/// Updates a TextMeshPro UI element to show the current amount.
/// </summary>
public class PlayerCurrency : MonoBehaviour
{
    [Header("Currency Settings")]
    [SerializeField] private int currentCurrency = 0;

    [Header("UI References")]
    [Tooltip("Assign the TextMeshProUGUI component that will display the currency count.")]
    [SerializeField] private TextMeshProUGUI currencyText;

    public int CurrentCurrency => currentCurrency;

    private void Start()
    {
        UpdateCurrencyUI();
    }

    /// <summary>
    /// Increases the player's currency by the specified amount and updates the UI.
    /// </summary>
    public void AddCurrency(int amount)
    {
        currentCurrency += amount;
        UpdateCurrencyUI();
        Debug.Log($"Currency Added! Total = {currentCurrency}");
    }

    /// <summary>
    /// Decreases the player's currency by the specified amount and updates the UI.
    /// </summary>
    public bool DeductCurrency(int amount)
    {
        if (currentCurrency >= amount)
        {
            currentCurrency -= amount;
            UpdateCurrencyUI();
            Debug.Log($"Currency Deducted: {amount}. Remaining: {currentCurrency}");
            return true;
        }
        else
        {
            Debug.Log($"Not enough currency! Required: {amount}, Available: {currentCurrency}");
            return false;
        }
    }

    /// <summary>
    /// Refreshes the UI text to match the current currency amount.
    /// </summary>
    private void UpdateCurrencyUI()
    {
        if (currencyText != null)
        {
            currencyText.text = $"Coins: {currentCurrency}";
        }
        else
        {
            Debug.LogWarning("Currency Text UI not assigned in PlayerCurrency.");
        }
    }
}