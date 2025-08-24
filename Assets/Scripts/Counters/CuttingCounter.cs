using System;
using UnityEngine;
public class CuttingCounter : BaseCounter, IHasProgress {

    public static event EventHandler OnAnyCut;

    new public static void ResetStaticData() {
        OnAnyCut = null;
    }

    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler OnCut;


    [SerializeField] private CuttingRecipeSO[] cutKitchenObjectSOArray;
    [SerializeField] private ParticleSystem upgradeParticleEffect;

    private int cuttingProgress;

    private Upgrade upgrade;
                

    private void Start()
    {
        upgrade = GetComponent<Upgrade>();
        upgrade.LoadUpgradeState("CuttingCounter");
        if (upgrade == null)
        {
            Debug.LogError("Upgrade component not found on " + gameObject.name);
        }
        // Subscribe to the upgrade event
        Upgrade.OnAnyUpgradeApplied += OnUpgradeApplied;
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        Upgrade.OnAnyUpgradeApplied -= OnUpgradeApplied;
    }

    public void UpgradeCuttingCounter()
    {
        if (upgrade != null && upgrade.CanUpgrade && CurrencyManager.Instance.Coins >= upgrade.CurrentUpgradeCost)
        {
            CurrencyManager.Instance.Spend(upgrade.CurrentUpgradeCost);
            upgrade.ApplyNextUpgrade(); // This triggers OnAnyUpgradeApplied
        }
    }

    private void OnUpgradeApplied(Upgrade upgraded)
    {
        if (upgrade != null)
        {
            upgrade.SaveUpgradeState("CuttingCounter");
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
        }
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
            // Use speed from Upgrade to scale progress
            if (upgrade != null)
            {
                float speed = upgrade.CurrentSpeed;
                if (speed <= 0f)
                {
                    Debug.LogWarning($"Upgrade speed is {speed} on {gameObject.name}. Using default speed of 1.");
                    speed = 1f; // Fallback to prevent issues
                }
                cuttingProgress += Mathf.RoundToInt(speed); // Scale progress by speed
            }
            else
            {
                cuttingProgress++; // Fallback if no Upgrade component
            }

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
