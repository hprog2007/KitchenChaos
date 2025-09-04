using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShopCardUI : MonoBehaviour
{
    [SerializeField] private Button button;

    public Image NewBadge;
    public Image UpgradeBadge;
    public TMP_Text CardTitle;
    public Image CardImage;
    public TMP_Text CardDescription;
    public TMP_Text CardPrice;
    public CounterType CounterType;

    private void Awake()
    {
        if (button ==  null) button = GetComponent<Button>();
        button.onClick.AddListener(() => ShopUIManager.Instance.ShopCardClick(this));
    }

    public void SetupNew(ShopSelectCardSO cardSO, ShopMode shopModeParam)
    {
        CounterType = cardSO.counterType;
        CardTitle.text = cardSO.CardTitle;
        CardImage.sprite = cardSO.CardImage;
        CardDescription.text = cardSO.CardDescription;
        CardPrice.text = cardSO.CardPriceInCoins.ToString();
        //CardImage.preserveAspect = true;
        //CardImage.type = Image.Type.Simple;
        //CardTitle.fontSize = 22;
        //CardTitle.fontStyle = FontStyles.Bold;

        NewBadge.enabled = true;
        UpgradeBadge.enabled = false;
    }

    public void SetupUpgrade(ShopSelectCardSO cardSO, CounterType counterType)
    {
        CounterType = counterType;
        NewBadge.enabled = false;
        UpgradeBadge.enabled = true;
        this.CardTitle.text = UpgradeManager.Instance.GetNextLevelTitle(counterType) 
                              + " Lvl " + UpgradeManager.Instance.GetNextLevel(counterType);
        CardImage.sprite = cardSO.CardImage;
        this.CardDescription.text = UpgradeManager.Instance.GetNextUpgradeDescription(counterType);
        CardPrice.text = UpgradeManager.Instance.GetNextUpgradePrice(counterType).ToString();
    }
}
