using UnityEngine;

[CreateAssetMenu()]
public class ToastingRecipeSO : ScriptableObject {
    public KitchenObjectSO input;
    public KitchenObjectSO output;
    public float toastingTimerMax;
}
