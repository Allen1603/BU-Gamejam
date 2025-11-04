using UnityEngine;

/// <summary>
/// Handles coin pickup logic. When the player collides, it adds currency and destroys itself.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Coin : MonoBehaviour
{
    [Header("Coin Settings")]
    [Tooltip("How much this coin adds to the player's currency.")]
    [SerializeField] private int coinValue = 1;

    private bool collected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;

        // Check if the colliding object has the PlayerCurrency component
        PlayerCurrency playerCurrency = other.GetComponent<PlayerCurrency>();
        if (playerCurrency == null) return;

        collected = true;

        // Add currency to player
        playerCurrency.AddCurrency(coinValue);

        // Destroy the coin
        Destroy(gameObject);
    }
}
