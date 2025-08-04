// 7/14/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ShopUIManager : MonoBehaviour
{
    public static ShopUIManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI coinText; // Reference to the UI Text element for coins

    public UnityEvent OnBuyButtonClicked;
    public UnityEvent OnUpgradeButtonClicked;
    public UnityEvent OnHelpersButtonClicked;
    public UnityEvent OnCosmeticsButtonClicked;
    public UnityEvent OnCoinsButtonClicked;
    public UnityEvent OnOpenShop;
    
    private void Awake()
    {
        // Ensure there's only one instance of UIManager
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Optionally, make this object persist across scenes
        DontDestroyOnLoad(gameObject);
    }

    public void OpenShop()
    {
        OnOpenShop?.Invoke();
    }

    public void CloseShop()
    {
        
    }

    public void BuyButtonClick()
    {
        OnBuyButtonClicked?.Invoke();        
    }

    public void UpgradeButtonClick()
    {
        OnUpgradeButtonClicked?.Invoke();
    }

    public void HelpersButtonClick()
    {
        OnHelpersButtonClicked?.Invoke();
    }

    public void OnCosmeticButtonClicked()
    {
        OnCosmeticsButtonClicked?.Invoke();
    }

    public void CoinsButtonClick()
    {
        OnCoinsButtonClicked?.Invoke();
    }

    public void EnterPlacementMode(GameObject selectedItem)
    {
        PlacementManager.Instance.StartPlacement(selectedItem);
    }

    public void UpdateCoinDisplay(int coins)
    {
        if (coinText != null)
        {
            coinText.text = $"Coins: {coins}";
        }
        else
        {
            Debug.LogWarning("Coin Text UI element is not assigned in the UIManager.");
        }
    }
}
