using System;
using System.Collections.Generic;
using UnityEngine;

public class TrayKitchenObject : KitchenObject {

    public event EventHandler<OnContentAddedEventArgs> OnContentAdded;
    public class OnContentAddedEventArgs : EventArgs {
        public KitchenObjectSO kitchenObjectSO;
    }    


    private List<KitchenObjectSO> kitchenObjectSOList;

    [SerializeField] private Transform trayContent;

    private void Awake() {
        kitchenObjectSOList = new List<KitchenObjectSO>();         
    }
    
    public bool TryAddContent(KitchenObjectSO kitchenObjectSO) {
        //Add content to tray event if it exists
        kitchenObjectSOList.Add(kitchenObjectSO);
        OnContentAdded?.Invoke(this, new OnContentAddedEventArgs
        {
            kitchenObjectSO = kitchenObjectSO
        });

        return true;

    }

    public List<KitchenObjectSO> GetKitchenObjectSOList() {
        return kitchenObjectSOList;
    }

}
