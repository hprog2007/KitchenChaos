using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopCardUI : MonoBehaviour
{
    public TMP_Text Title;
    public Image CardImage;
    public TMP_Text Description;
    public TMP_Text Price;

    public void Setup(ShopSelectCardSO cardSO)
    {
        Title.text = cardSO.CardTitle;
        CardImage.sprite = cardSO.CardImage;
        Description.text = cardSO.CardDescription;
        Price.text = cardSO.CardPriceInCoins.ToString();
        CardImage.preserveAspect = true;
        CardImage.type = Image.Type.Simple;
        Title.fontSize = 22;
        Title.fontStyle = FontStyles.Bold;
    }
}
