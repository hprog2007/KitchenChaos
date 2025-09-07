using DG.Tweening;
using UnityEngine;

public class CounterPointer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ShopUIManager.Instance.OpenShop();

        //ShopUIManager.Instance.EnterPlacementMode(null);


        //StartCoroutine( AnimationManager.Instance.PlayMagicReplaceByRotation(a.transform, a.transform));
    }

}
