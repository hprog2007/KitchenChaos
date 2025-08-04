using System;
using UnityEngine;

public class PlatesCounter : BaseCounter {

    public event EventHandler OnPlateSpawned;
    public event EventHandler OnPlateRemoved;
    
    [SerializeField] private KitchenObjectSO plateKitchenObjectSO;

    private float spawnPlateTimer;
    private float spawnPlateTimerMax = 2f;
    private int platesSpawnedAmount;
    private int platesSpawnedAmountMax = 4;

    void Update()
    {
        spawnPlateTimer += Time.deltaTime;
        if (spawnPlateTimer > spawnPlateTimerMax) {
            spawnPlateTimer = 0f;

            if (KitchenGameManager.Instance.IsGamePlaying() && platesSpawnedAmount < platesSpawnedAmountMax) {
                platesSpawnedAmount++;

                OnPlateSpawned?.Invoke(this, EventArgs.Empty);
            }
        }
        
    }

    public override void Interact(Player player) {
        if (!player.HasKitchenObject()) {
            //Player is empty handed
            if (platesSpawnedAmount > 0 ) {
                //There's at least on plate here
                platesSpawnedAmount--;

                KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, player);

                OnPlateRemoved?.Invoke(this, EventArgs.Empty);
            }

        }
    }
}
