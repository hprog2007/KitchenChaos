using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class TrayCompleteVisual : MonoBehaviour
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
        if (plateKitchenObject == null)
        {
            Debug.LogError($"{nameof(PlateCompleteVisual)}: No {nameof(PlateKitchenObject)} found in parents.", this);
        }
    }

    void OnEnable()
    {
        if (plateKitchenObject == null) return;
        plateKitchenObject.OnIngredientsAdded += PlateKitchenObject_OnIngredientsAdded;
    }

    void OnDisable()
    {
        if (plateKitchenObject == null) return;
        plateKitchenObject.OnIngredientsAdded -= PlateKitchenObject_OnIngredientsAdded;

        // Optional: if you start longer-running coroutines, cancel them here
        StopAllCoroutines();
    }

    private void Start() {
        foreach (var pair in kitchenObjectSO_GameObjectList)
        {
            if (pair.gameObject != null)
                pair.gameObject.SetActive(false);
        }       
    }

    private void PlateKitchenObject_OnIngredientsAdded(object sender, PlateKitchenObject.OnIngredientsAddedEventArgs e) {
        // Guard against being destroyed or disabled between event fire and handler run
        if (!this || !isActiveAndEnabled) return;
        if (e == null || e.kitchenObjectSO == null) return;

        StartCoroutine(ActivateIngredientNextFrame(e.kitchenObjectSO));
    }

    

    private IEnumerator ActivateIngredientNextFrame(KitchenObjectSO targetSO)
    {
        // If we get disabled/destroyed before the next frame, Unity will stop this coroutine automatically.
        yield return null;

        // Double-check we’re still alive/enabled after the frame
        if (!this || !isActiveAndEnabled) yield break;

        foreach (var pair in kitchenObjectSO_GameObjectList)
        {
            if (pair.kitchenObjectSO == targetSO && pair.gameObject != null)
            {
                pair.gameObject.SetActive(true);
            }
        }
    }

}
