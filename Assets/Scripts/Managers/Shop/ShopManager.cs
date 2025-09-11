// 7/14/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ShopMode
{
    None,
    Buy,
    Upgrades,
    Helpers,
    Cosmetics,
    Coins
}

public class ShopManager : MonoBehaviour
{
    
    public static ShopManager Instance { get; private set; }
    public ShopMode CurrentMode { get; private set; } = ShopMode.None;

    [SerializeField] private List<ShopAvailableCardList> shopAvailableCardList;
    
    private ShopSelectCardSO currentShopCardSO;

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

    private void Start()
    {
        ShopUIManager.Instance.OnShopCardClicked += ShopUIManager_OnShopCardClicked;
        ShopUIManager.Instance.OnBuyConfirmed += ShopUIManager_OnBuyConfirmed;
    }

    private void ShopUIManager_OnShopCardClicked(ShopCardUI cardParam)
    {
        var cardParam_shopSelectedCard = cardParam.GetShopSelectCardSO();
        switch (CurrentMode)
        {
            case ShopMode.Buy: 
                if (CurrencyManager.Instance.CanAfford(cardParam_shopSelectedCard.CardPriceInCoins))
                {
                    ShopUIManager.Instance.EnterReplacementMode(cardParam, cardParam_shopSelectedCard.prefab);
                } else
                {
                    ShopUIManager.Instance.ShowLowBalanceMessage();
                }
                break;
            case ShopMode.Upgrades:
                if (CurrencyManager.Instance.CanAfford(cardParam_shopSelectedCard.CardPriceInCoins))
                {
                    UpgradeManager.Instance.ApplyNextUpgrade(cardParam.CounterType);
                    ShopUIManager.Instance.UpgradeButtonClick(); //reset upgrade card list
                } else
                {
                    ShopUIManager.Instance.ShowLowBalanceMessage();
                }
                break;
            case ShopMode.Helpers: 
                break;
            case ShopMode.Cosmetics:
                break;
            case ShopMode.Coins: 
                break;
        }
    }

    // Load level selection map
    public void ExitToSelectionMap()
    {
        CurrentMode = ShopMode.None;
        SceneManager.LoadScene(SceneType.Level_Selection_Map.ToString());
    }

    #region Set Shop Mode
    public void SetToNoneMode() => CurrentMode = ShopMode.None;

    public void SetToBuyMode() => CurrentMode = ShopMode.Buy;

    public void SetToUpgradeMode() => CurrentMode = ShopMode.Upgrades;

    public void SetToHelperMode() => CurrentMode = ShopMode.Helpers;

    public void SetToCosmeticsMode() => CurrentMode = ShopMode.Cosmetics;

    public void SetToCoinPackMode() => CurrentMode = ShopMode.Coins;

    public bool IsInMode(ShopMode mode) => CurrentMode == mode;

    public bool IsShopOpen() => CurrentMode != ShopMode.None;
    #endregion


    public ShopAvailableCardList GetShopCardsList(ShopMode shopModeParam)
    {
        switch(shopModeParam)
        {
            case ShopMode.Buy:
                return shopAvailableCardList.FirstOrDefault(a => a.shopMode == shopModeParam);
                break;
            case ShopMode.Upgrades:
                var buyList = shopAvailableCardList.FirstOrDefault(a => a.shopMode == ShopMode.Buy);
                var upgradeCardList = shopAvailableCardList.FirstOrDefault(a => a.shopMode == ShopMode.Upgrades);
                upgradeCardList.shopCardList = new List<ShopSelectCardSO>();

                foreach (var card in buyList.shopCardList)
                {
                    if (UpgradeManager.Instance.UpgradeLevelExist(card.counterType))
                    {
                        upgradeCardList.shopCardList.Add(card);
                    }
                }
                return upgradeCardList;
                break;
            case ShopMode.Helpers:
                return shopAvailableCardList.FirstOrDefault(a => a.shopMode == shopModeParam);
                break;
            case ShopMode.Cosmetics:
                return shopAvailableCardList.FirstOrDefault(a => a.shopMode == shopModeParam);
                break;
            case ShopMode.Coins:
                return shopAvailableCardList.FirstOrDefault(a => a.shopMode == shopModeParam);
                break;
            default:
                return shopAvailableCardList.FirstOrDefault(a => a.shopMode == ShopMode.Buy); //default
        }

        
    }

    public void ShopUIManager_OnBuyConfirmed(ShopCardUI cardParam, Transform selectedCounterTransform)
    {
        var shopCardSO = cardParam.GetShopSelectCardSO();
        CurrencyManager.Instance.Spend(shopCardSO.CardPriceInCoins);
        //var GridManager.Instance.GetGameObjectFromWorldPositin(selectedCounter.transform.position);
        

    }


}
