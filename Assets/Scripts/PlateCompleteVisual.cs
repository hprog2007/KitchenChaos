using System;
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

    [SerializeField] private PlateKitchenObject plateKitchenObject;
    [SerializeField] private List<KitchenObjectSO_GameObject> kitchenObjectSO_GameObjectList;

    private void Start() {
        plateKitchenObject.OnIngredientsAdded += PlateKitchenObject_OnIngredientsAdded;

        foreach (KitchenObjectSO_GameObject kitchenObjectSO_GameObject in kitchenObjectSO_GameObjectList) {
            kitchenObjectSO_GameObject.gameObject.SetActive(false);
        }
    }

    private void PlateKitchenObject_OnIngredientsAdded(object sender, PlateKitchenObject.OnIngredientsAddedEventArgs e) {
        foreach(KitchenObjectSO_GameObject kitchenObjectSO_GameObject in kitchenObjectSO_GameObjectList) {
            if(kitchenObjectSO_GameObject.kitchenObjectSO == e.kitchenObjectSO) {
                kitchenObjectSO_GameObject.gameObject.SetActive(true);
            }
        }
    }
}
