// 7/14/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ShopMode
{
    None,
    Buy,
    Upgrade,
    Helpers,
    Cosmetics,
    CoinPacks
}

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }
    public ShopMode CurrentMode { get; private set; } = ShopMode.None;    


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ExitToSelectionMap()
    {
        CurrentMode = ShopMode.None;
        SceneManager.LoadScene(SceneType.Level_Selection_Map.ToString());
    }

    public void SetToBuyMode()
    {
        CurrentMode = ShopMode.Buy;
    }

    public void SetToUpgradeMode()
    {
        CurrentMode = ShopMode.Upgrade;
    }

    public void SetToHelperMode()
    {
        CurrentMode = ShopMode.Helpers;
    }

    public void SetToCosmeticsMode()
    {
        CurrentMode = ShopMode.Cosmetics;
    }

    public void SetToCoinPackMode()
    {
        CurrentMode = ShopMode.CoinPacks;
    }

    public bool IsInMode(ShopMode mode)
    {
        return CurrentMode == mode;
    }


    public void ConfirmPurchase(int cost)
    {
        if (CurrencyManager.Instance.Spend(cost))
        {
            Debug.Log("Purchase successful!");
            // Finalize purchase logic
        }
        else
        {
            Debug.Log("Not enough coins!");
        }
    }
}
