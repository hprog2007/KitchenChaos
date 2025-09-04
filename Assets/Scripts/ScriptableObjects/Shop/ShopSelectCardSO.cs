using UnityEngine;

[CreateAssetMenu(fileName = "New Shop Select Card", menuName = "Shop/Shop Select Card")]
public class ShopSelectCardSO : ScriptableObject
{
    public CounterType counterType;
    public string CardTitle;
    public Sprite CardImage;
    public string CardDescription;
    public int CardPriceInCoins;
    public GameObject prefab;
}