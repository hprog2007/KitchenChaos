using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour {

    public event EventHandler OnRecipeCompleted;
    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFailed;

    public static DeliveryManager instance {  get; private set; }

    private List<RecipeSO> waitingRecipeSOList;

    
    private int successfulRecipesAmount;

    private void Awake() {
        instance = this;

        waitingRecipeSOList = new List<RecipeSO> ();
    }

    private void Update() {
        
        
    }

    public void DeliverRecipe(PlateKitchenObject plateKitchenObject) {
        for(int i = 0; i < waitingRecipeSOList.Count; i++) {
            RecipeSO waitingRecipeSO = waitingRecipeSOList[i];

            if (waitingRecipeSO.kitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count) {
                // Has the same number of ingredients
                bool plateContentsMatchesRecipe = true;
                foreach (KitchenObjectSO recipeKitchenObjectSO in waitingRecipeSO.kitchenObjectSOList) {
                    // Cycling through all ingredients in the Recipe
                    bool ingredientFound = false;
                    foreach (KitchenObjectSO plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList()) {
                        // Cycling through all ingredients in the Plate
                        if (plateKitchenObjectSO == recipeKitchenObjectSO) {
                            //Ingredient matches
                            ingredientFound = true;
                            break;
                        }
                    }
                    if (!ingredientFound) {
                        // This Recipe ingredient was not found on the Plate
                        plateContentsMatchesRecipe = false;
                    }
                }

                if (plateContentsMatchesRecipe) {
                    // Player delivered the correct Recipe!

                    successfulRecipesAmount++;
                    
                    waitingRecipeSOList.RemoveAt(i);

                    OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
                    OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
                    return;
                }
            }
        }

        // No matches found!
        // Player did not deliver a correct recipe
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
    }

    public void DeliverRecipeFIFO(PlateKitchenObject plateKitchenObject)
    {
        RecipeSO waitingRecipeSO = plateKitchenObject.GetTargetRecipeSO();

        if (waitingRecipeSO.kitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count)
        {
            // Has the same number of ingredients
            bool plateContentsMatchesRecipe = true;
            foreach (KitchenObjectSO recipeKitchenObjectSO in waitingRecipeSO.kitchenObjectSOList)
            {
                // Cycling through all ingredients in the Recipe
                bool ingredientFound = false;
                foreach (KitchenObjectSO plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList())
                {
                    // Cycling through all ingredients in the Plate
                    if (plateKitchenObjectSO == recipeKitchenObjectSO)
                    {
                        //Ingredient matches
                        ingredientFound = true;
                        break;
                    }
                }
                if (!ingredientFound)
                {
                    // This Recipe ingredient was not found on the Plate
                    plateContentsMatchesRecipe = false;
                }
            }

            if (plateContentsMatchesRecipe)
            {
                // Player delivered the correct Recipe!

                successfulRecipesAmount++;

                waitingRecipeSOList.RemoveAt(0); //first order

                OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
                OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
                return;
            }
        }

        // No matches found!
        // Player did not deliver a correct recipe
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
    }

    public List<RecipeSO> GetWaitingRecipeSOList() {
        return waitingRecipeSOList;
    }

    public int GetSuccessfulRecipesAmount() {
        return successfulRecipesAmount;
    }

    public void AddWaitingRecipe(RecipeSO recipeSO)
    {
        waitingRecipeSOList.Add(recipeSO);
    }

    public RecipeSO GetFirstOrder()
    {
        return waitingRecipeSOList[0];
    }
}
