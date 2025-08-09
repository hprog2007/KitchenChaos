using System;
using System.Collections.Generic;
using UnityEngine;

public class PlateKitchenObject : KitchenObject {

    public event EventHandler<OnIngredientsAddedEventArgs> OnIngredientsAdded;
    public class OnIngredientsAddedEventArgs : EventArgs {
        public KitchenObjectSO kitchenObjectSO;
    }

    private List<KitchenObjectSO> kitchenObjectSOList;

    private RecipeSO targetRecipe;

    [SerializeField] private Transform plateContent;

    private void Awake() {
        kitchenObjectSOList = new List<KitchenObjectSO>();         
    }

    public void SetTargetRecipe(RecipeSO recipe)
    {
        targetRecipe = recipe;
    }

    public RecipeSO GetTargetRecipeSO()
    {
        return targetRecipe;
    }
    
    public bool TryAddIngredient(KitchenObjectSO kitchenObjectSO) {
        //set target recipe of plate
        if (targetRecipe == null || targetRecipe != DeliveryManager.instance.GetFirstOrder())
        {
            SetTargetRecipe(DeliveryManager.instance.GetFirstOrder());
            AddPlateCompleteVisual();
        }

        if (!targetRecipe.kitchenObjectSOList.Contains(kitchenObjectSO))
        {
            //Not a valid ingredient
            return false;
        }

        if (kitchenObjectSOList.Contains(kitchenObjectSO)) {
            //Already has this type
            return false;
        } else {
            kitchenObjectSOList.Add(kitchenObjectSO);
            OnIngredientsAdded?.Invoke(this, new OnIngredientsAddedEventArgs { 
                kitchenObjectSO = kitchenObjectSO 
            });

            return true;
        }

    }

    public List<KitchenObjectSO> GetKitchenObjectSOList() {
        return kitchenObjectSOList;
    }

    private void AddPlateCompleteVisual()
    {
        //clear PlateContent children 
        foreach(Transform t in plateContent)
        {
            Destroy(t.gameObject);
        }

        Instantiate(targetRecipe.plateCompleteVisual, plateContent);
    }
}
