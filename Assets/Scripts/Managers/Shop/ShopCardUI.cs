using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class ShopCardUI : MonoBehaviour
{
    [SerializeField] private Button button;

    public Image NewBadge;
    public Image UpgradeBadge;
    public TMP_Text CardTitle;
    public Image CardImage;
    public TMP_Text CardDescription;
    public Image CoinImage;
    public TMP_Text CardPrice;
    public CounterType CounterType;


    private ShopSelectCardSO shopSelectCardSO;
    
    private CosmeticItemSO cosmeticItemSO;

    private void Awake()
    {
        if (button ==  null) button = GetComponent<Button>();
        button.onClick.AddListener(() => ShopUIManager.Instance.ShopCardClick(this));
    }

    public ShopSelectCardSO GetShopSelectCardSO() => shopSelectCardSO;

    public CosmeticItemSO GetCosmeticItemSO() => cosmeticItemSO;

    public void SetupNew(ShopSelectCardSO cardSO, ShopMode shopModeParam)
    {
        shopSelectCardSO = cardSO;
        CounterType = cardSO.counterType;
        CardTitle.text = cardSO.CardTitle;
        CardImage.sprite = cardSO.CardImage;
        CardDescription.text = cardSO.CardDescription;

        if (shopModeParam == ShopMode.Coins)
        {
            CardPrice.text = CurrencyManager.Instance.GetActivePriceUnit() + cardSO.CardPriceInCoins.ToString();
            CoinImage.enabled = false;
        } else
        {
            CardPrice.text = cardSO.CardPriceInCoins.ToString();
            CoinImage.enabled = true;
        }
        //CardImage.preserveAspect = true;
        //CardImage.type = Image.Type.Simple;
        //CardTitle.fontSize = 22;
        //CardTitle.fontStyle = FontStyles.Bold;

        NewBadge.enabled = true;
        UpgradeBadge.enabled = false;
    }

    public void SetupUpgrade(ShopSelectCardSO cardSO, CounterType counterType)
    {
        shopSelectCardSO = cardSO;
        CounterType = counterType;
        NewBadge.enabled = false;
        UpgradeBadge.enabled = true;
        this.CardTitle.text = UpgradeManager.Instance.GetNextLevelTitle(counterType) 
                              + " Lvl " + UpgradeManager.Instance.GetNextLevel(counterType);
        CardImage.sprite = cardSO.CardImage;
        this.CardDescription.text = UpgradeManager.Instance.GetNextUpgradeDescription(counterType);
        CardPrice.text = UpgradeManager.Instance.GetNextUpgradePrice(counterType).ToString();
    }

    public void SetupNew(CosmeticItemSO cosmeticParam)
    {
        cosmeticItemSO = cosmeticParam;

        CardTitle.text = cosmeticParam.displayName;
        CardDescription.text = cosmeticParam.description;

        // Price formatting
        switch (cosmeticParam.currency)
        {
            case CurrencyType.Coins:
                CardPrice.text = cosmeticParam.price.ToString();
                CoinImage.enabled = true; // already showing unit, hide coin icon
                break;
            case CurrencyType.Gems:
                CardPrice.text = cosmeticParam.price.ToString();
                CoinImage.enabled = true;  // use this Image as "gem" icon if you want
                break;
            default:
                CardPrice.text = "Free";
                CoinImage.enabled = false;
                break;
        }

        // Load icon
        if (cosmeticParam.icon.RuntimeKeyIsValid())
        {
            cosmeticParam.icon.LoadAssetAsync().Completed += OnIconLoaded;
        }

        NewBadge.enabled = true;
        UpgradeBadge.enabled = false;
    }

    private void OnIconLoaded(AsyncOperationHandle<Sprite> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            CardImage.sprite = handle.Result;
            CardImage.preserveAspect = true;
        }
    }

}
