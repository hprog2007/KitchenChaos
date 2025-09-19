using System;
using UnityEngine;
public class CuttingCounter : BaseCounter, IHasProgress {

    public CounterType CurrentCounterType { get { return CounterType.CuttingCounter; } }

    [SerializeField] private ParticleSystem upgradeParticleEffect;
    [SerializeField] private CounterUpgradeFXObject counterUpgradeFXObject; //scale and rotation animation

    public static event EventHandler OnAnyCut;

    new public static void ResetStaticData() {
        OnAnyCut = null;
    }

    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler OnCut;


    [SerializeField] private CuttingRecipeSO[] cutKitchenObjectSOArray;
    

    private int cuttingProgress;


    private void OnEnable()
    {
        UpgradeManager.OnAnyUpgradeApplied += Upgrade_OnAnyUpgradeApplied;
    }

    private void OnDestroy()
    {
        UpgradeManager.OnAnyUpgradeApplied -= Upgrade_OnAnyUpgradeApplied;
    }

    private void Upgrade_OnAnyUpgradeApplied(CounterType counterType)
    {
        if (counterType != CounterType.CuttingCounter)
            return;

        // set speed and capacity
        //SetSpeed(UpgradeManager.Instance.GetCurrentSpeed(counterType));
        //SetCapacity(UpgradeManager.Instance.GetCurrentCapacity(counterType));

        // Play particle effect
        if (upgradeParticleEffect != null)
        {
            // Spawn particle effect slightly above the counter
            Vector3 spawnPosition = transform.position + Vector3.up * 2f; // Adjust height as needed
            ParticleSystem effect = Instantiate(upgradeParticleEffect, spawnPosition, Quaternion.identity);
            effect.Play();
            // Destroy the particle system after it finishes (optional)
            Destroy(effect.gameObject, effect.main.duration);
        }

        // visual effect for counter or ...
        counterUpgradeFXObject.UpgradeAnimation();
    }

    public void UpgradeCuttingCounter()
    {
        UpgradeManager.Instance.ApplyNextUpgrade(CounterType.CuttingCounter);        
    }

    //player puts something like tomato on cutting counter or pick up it
    public override void Interact(Player player) {
        if (!HasKitchenObject()) {
            //There is no kitchen object here
            if (player.HasKitchenObject()) {
                //The player is carrying something
                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO())) {
                    //The player is carrying something that can be cut
                    player.GetKitchenObject().SetKitchenObjectParent(this);
                    cuttingProgress = 0;

                    CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                        progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax
                    });
                }
            } else {
                //The player not carrying anything
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
                }
            } else {
                //The player not carrying anything
                GetKitchenObject().SetKitchenObjectParent(player);
            }

        }
    }

    //player cut something like cabbage into multiple slice
    public override void InteractAlternate(Player player) {
        if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO()) ) {
            // There is a kitchen object that can be cut            
            cuttingProgress += Mathf.RoundToInt(GetSpeed()); // Scale progress by speed

            OnCut?.Invoke(this, EventArgs.Empty);
            OnAnyCut?.Invoke(this, EventArgs.Empty);

            CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax
            });

            if (cuttingProgress >= cuttingRecipeSO.cuttingProgressMax) {
                //spawn sliced kitchen object 
                KitchenObjectSO outputKitchenObjectSO = GetOutputForInput(GetKitchenObject().GetKitchenObjectSO());

                GetKitchenObject().DestroySelf();

                KitchenObject.SpawnKitchenObject(outputKitchenObjectSO, this);
            }
        }
    }

    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO) {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);

        return cuttingRecipeSO != null;
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO) {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);

        if (cuttingRecipeSO != null) {
            return cuttingRecipeSO.output;
        } else {
            return null;
        }
    }

    private CuttingRecipeSO GetCuttingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO) {
        foreach (CuttingRecipeSO cuttingRecipeSO in cutKitchenObjectSOArray) {
            if (cuttingRecipeSO.input == inputKitchenObjectSO) {
                return cuttingRecipeSO;
            }
        }

        return null;
    }
}
