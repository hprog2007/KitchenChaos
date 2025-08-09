using UnityEngine;

[CreateAssetMenu(fileName ="ToastingRecipeSO", menuName = "RecipeSOs/ToastingRecipeSO", order = 50)]
public class ToastingRecipeSO : ScriptableObject {
    public KitchenObjectSO input;
    public KitchenObjectSO output;
    public float toastingTimerMax;
}
