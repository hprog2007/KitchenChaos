using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class PlateCompleteVisual : MonoBehaviour
{
    [Serializable]
    public struct KitchenObjectSO_GameObject {
        public KitchenObjectSO kitchenObjectSO;
        public GameObject gameObject;
    }

    private PlateKitchenObject plateKitchenObject;
    [SerializeField] private List<KitchenObjectSO_GameObject> kitchenObjectSO_GameObjectList;

    public void Awake()
    {
        plateKitchenObject = GetComponentInParent<PlateKitchenObject>();

        plateKitchenObject.OnIngredientsAdded += PlateKitchenObject_OnIngredientsAdded;

    }

    private void Start() {
        foreach (KitchenObjectSO_GameObject kitchenObjectSO_GameObject in kitchenObjectSO_GameObjectList) {
            kitchenObjectSO_GameObject.gameObject.SetActive(false);
        }
    }

    private void PlateKitchenObject_OnIngredientsAdded(object sender, PlateKitchenObject.OnIngredientsAddedEventArgs e) {
        StartCoroutine(ActivateIngredientNextFrame(e.kitchenObjectSO));
        //foreach (KitchenObjectSO_GameObject kitchenObjectSO_GameObject in kitchenObjectSO_GameObjectList) {
        //    if(kitchenObjectSO_GameObject.kitchenObjectSO == e.kitchenObjectSO) {
        //        kitchenObjectSO_GameObject.gameObject.SetActive(true);
        //    }
        //}
    }

    private IEnumerator ActivateIngredientNextFrame(KitchenObjectSO targetSO)
    {
        yield return null; // wait one frame

        foreach (KitchenObjectSO_GameObject kitchenObjectSO_GameObject in kitchenObjectSO_GameObjectList)
        {
            if (kitchenObjectSO_GameObject.kitchenObjectSO == targetSO)
            {
                kitchenObjectSO_GameObject.gameObject.SetActive(true);
            }
        }
    }
}
