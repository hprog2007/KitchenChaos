using System;
using UnityEngine;

public class BaseCounter : MonoBehaviour, IKitchenObjectParent {
    
    protected float speed = 1.0f; // Default speed (multiplier)
    protected int capacity = 1;    // Default capacity (e.g., items held)

    // Virtual method for setting speed
    public virtual void SetSpeed(float newSpeed)
    {
        if (newSpeed >= 0)
        {
            speed = newSpeed;
            // Default: No behavior (suitable for container/utility counters)
            // Functional counters will override this
        }
        else
        {
            Debug.LogWarning($"Invalid speed value {newSpeed} for {gameObject.name}");
        }
    }

    // Virtual method for setting capacity
    public virtual void SetCapacity(int newCapacity)
    {
        if (newCapacity >= 0)
        {
            capacity = newCapacity;
            // Default: No behavior (suitable for counters that don’t use capacity)
            // Functional counters will override this
        }
        else
        {
            Debug.LogWarning($"Invalid capacity value {newCapacity} for {gameObject.name}");
        }
    }

    // Expose speed and capacity for other systems
    public float GetSpeed() => speed;
    public int GetCapacity() => capacity;

    public event Action<InvalidInteractionReason, KitchenObjectSO> OnInvalidInteraction;

    protected void RaiseInvalid(InvalidInteractionReason reason, KitchenObjectSO so = null)
        => OnInvalidInteraction?.Invoke(reason, so);

    public static event EventHandler OnAnyObjectPlacedHere;

    public static void ResetStaticData() {
        OnAnyObjectPlacedHere = null;
    }

    [SerializeField] private Transform counterTopPoint;

    private KitchenObject kitchenObject;

    public virtual void Interact(Player player) {
        Debug.LogError("BaseCounter.Interact();");
    }

    public virtual void InteractAlternate(Player player) {
        //Debug.LogError("BaseCounter.InteractAlternate();");
    }

    public Transform GetKitchenObjectFollowTransform() {
        return counterTopPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject) {
        this.kitchenObject = kitchenObject;

        if (kitchenObject != null) {
            OnAnyObjectPlacedHere?.Invoke(this, EventArgs.Empty);
        }
    }

    public KitchenObject GetKitchenObject() {
        return kitchenObject;
    }

    public void ClearKitchenObject() {
        kitchenObject = null;
    }

    public bool HasKitchenObject() {
        return kitchenObject != null;
    }
}
