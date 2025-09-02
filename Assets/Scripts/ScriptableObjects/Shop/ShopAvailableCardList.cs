using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopAvailableCardList", menuName = "Shop/Shop Available Card List")]
public class ShopAvailableCardList : ScriptableObject
{
    public SceneType sceneType;
    public ShopMode shopMode;
    public List<ShopSelectCardSO> shopCardList;

}