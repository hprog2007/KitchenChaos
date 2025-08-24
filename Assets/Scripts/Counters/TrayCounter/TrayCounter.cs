using System;
using UnityEngine;

public class TrayCounter : BaseCounter {

    public event EventHandler OnTraySpawned;
    public event EventHandler OnTrayRemoved;
    
    [SerializeField] private KitchenObjectSO trayKitchenObjectSO;

    private float spawnTrayTimer;
    private float spawnTrayTimerMax = 2f;
    private int traysSpawnedAmount;
    private int traysSpawnedAmountMax = 4;

    void Update()
    {
        //////////////////////////////////////////////////////// Generate Plates
        spawnTrayTimer += Time.deltaTime;
        if (spawnTrayTimer > spawnTrayTimerMax) {
            spawnTrayTimer = 0f;

            if (KitchenGameManager.Instance.IsGamePlaying() && traysSpawnedAmount < traysSpawnedAmountMax) {
                traysSpawnedAmount++;

                OnTraySpawned?.Invoke(this, EventArgs.Empty);
            }
        }
        
    }

    public override void Interact(Player player) {
        if (!player.HasKitchenObject()) {
            //Player is empty handed
            if (traysSpawnedAmount > 0 ) {
                //There's at least on plate here
                traysSpawnedAmount--;

                
                //spawn a plate and parent it to player
                PlateKitchenObject plateKitchenObject = KitchenObject.SpawnKitchenObject(trayKitchenObjectSO, player) as PlateKitchenObject;

                OnTrayRemoved?.Invoke(this, EventArgs.Empty);
            }

        }
    }
}
