using UnityEngine;

[CreateAssetMenu(fileName = "New Shop Select Card", menuName = "Shop/Shop Select Card")]
public class ShopSelectCardSO : ScriptableObject
{
    public Sprite CardBadge;
    public string CardTitle;
    public Sprite CardImage;
    public int CardPriceInCoins;
}