// 7/14/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ShopUIManager : MonoBehaviour
{
    public static ShopUIManager Instance { get; private set; }

    public event Action<ShopCardUI> OnShopCardClicked;

    [SerializeField] private TextMeshProUGUI headerTitle;

    [SerializeField] private Transform shopCardsParent;    

    [SerializeField] private Transform shopCardPrefab;

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

    private void ClearContent()
    {
        foreach (Transform child in shopCardsParent)
        {
            Destroy(child.gameObject);
        }
    }

    private bool FillShopCardsPanel(ShopMode shopModeParam)
    {
        var currentShopCardList = ShopManager.Instance.GetShopCardsList(shopModeParam);
        if (currentShopCardList == null)
        {
            Debug.Log("shopAvailableCardList for "+ shopModeParam.ToString() + " isn't defined! Or all upgrade levels are done");
            return false;
        }

        //Instaniate cards
        foreach (ShopSelectCardSO cardSO in currentShopCardList.shopCardList)
        {
            Transform shopCard = Instantiate(shopCardPrefab, shopCardsParent);
            ShopCardUI shopCardUI = shopCard.GetComponent<ShopCardUI>();

            if (shopModeParam == ShopMode.Upgrades)
            {
                shopCardUI.SetupUpgrade(cardSO, cardSO.counterType);
            } else
            {
                shopCardUI.SetupNew(cardSO, shopModeParam);
            }

        }
        return true;
    }

    public void BuyButtonClick()
    {
        headerTitle.text = "Buy";
        ClearContent();
        if (!FillShopCardsPanel(ShopMode.Buy))
        {
            return;
        }

        OnBuyButtonClicked?.Invoke();    
        
    }

    public void UpgradeButtonClick()
    {
        headerTitle.text = "Upgrades";
        ClearContent();
        if (!FillShopCardsPanel(ShopMode.Upgrades))
        {
            return;
        }

        OnUpgradeButtonClicked?.Invoke();
    }

    public void HelpersButtonClick()
    {
        headerTitle.text = "Helpers";
        ClearContent();
        if (!FillShopCardsPanel(ShopMode.Helpers))
        {
            return;
        }

        OnHelpersButtonClicked?.Invoke();
    }

    public void CosmeticsButtonClicked()
    {
        headerTitle.text = "Cosmetics";
        ClearContent();
        if (!FillShopCardsPanel(ShopMode.Cosmetics))
        {
            return;
        }

        OnCosmeticsButtonClicked?.Invoke();
    }

    public void CoinsButtonClick()
    {
        headerTitle.text = "Coins";
        ClearContent();
        if (!FillShopCardsPanel(ShopMode.Coins))
        {
            return;
        }

        OnCoinsButtonClicked?.Invoke();
    }

    public void ShopCardClick(ShopCardUI card)
    {
        OnShopCardClicked?.Invoke(card);
    }

    public void EnterPlacementMode(GameObject selectedItem)
    {
        PlacementManager.Instance.StartPlacement(selectedItem);
    }    
}
