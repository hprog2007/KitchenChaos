using UnityEngine;

[CreateAssetMenu(fileName = "New Shop Select Card", menuName = "Shop/Shop Select Card")]
public class ShopSelectCardSO : ScriptableObject
{
    public bool IsUpgrade;
    public string CardTitle;
    public Sprite CardImage;
    public string CardDescription;
    public int CardPriceInCoins;
}