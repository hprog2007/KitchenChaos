// 7/14/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    private int coins;

    public int Coins => coins;

    private void Awake()
    {
        // Ensure there's only one instance of CurrencyManager
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Optionally, make this object persist across scenes
        DontDestroyOnLoad(gameObject);
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        UpdateUI();
    }

    public bool SpendCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            UpdateUI();
            return true;
        }
        return false; // Not enough coins
    }

    private void UpdateUI()
    {
        // Update the coin display in the UI
        ShopUIManager.Instance?.UpdateCoinDisplay(coins);
    }
}