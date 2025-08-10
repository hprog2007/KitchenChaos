using System;
using Unity.VisualScripting;
using UnityEngine;

public class ClearCounter : BaseCounter {

    //[SerializeField] private KitchenObjectSO[] kitchenObjectSO;

    public override void Interact(Player player) {
        if (!HasKitchenObject()) {
            //There is no kitchen object here
            if (player.HasKitchenObject()) {
                //The player is carrying something
                player.GetKitchenObject().SetKitchenObjectParent(this);
            } else {
                //The player not carrying anything
                RaiseInvalid(InvalidInteractionReason.PlayerHandsEmpty);
            }
        } else {
            //There is a kitchen object here
            if (player.HasKitchenObject()) {
                //The player carrying something
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject)) {
                    //Player is holding a plate                    
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO())) {
                        GetKitchenObject().DestroySelf();
                    }
                } else {
                    //Player is not carrying Plate but something else
                    if (GetKitchenObject().TryGetPlate(out plateKitchenObject)) {
                        //Counter is holding a plate
                        if (plateKitchenObject.TryAddIngredient(player.GetKitchenObject().GetKitchenObjectSO())) {
                            player.GetKitchenObject().DestroySelf();
                        }
                    } 

                }

            } else {
                //The player not carrying anything
                //give kitchenObject to player
                GetKitchenObject().SetKitchenObjectParent(player);
            }

        }
        
    }

    public override void InteractAlternate(Player player)
    {
        //if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO()))
        //{
        //    //There is a kitchen object here and it can be cut 
        //    cuttingProgress++;

        //    OnCut?.Invoke(this, EventArgs.Empty);
        //    OnAnyCut?.Invoke(this, EventArgs.Empty);

        //    CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

        //    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        //    {
        //        progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax
        //    });

        //    if (cuttingProgress >= cuttingRecipeSO.cuttingProgressMax)
        //    {
        //        //spawn sliced kitchen object 
        //        KitchenObjectSO outputKitchenObjectSO = GetOutputForInput(GetKitchenObject().GetKitchenObjectSO());

        //        GetKitchenObject().DestroySelf();

        //        KitchenObject.SpawnKitchenObject(outputKitchenObjectSO, this);
        //    }
        //}
    }


}
