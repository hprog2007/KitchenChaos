using UnityEngine;

[CreateAssetMenu(fileName ="FryingRecipeSO", menuName = "RecipeSOs/FryingRecipeSO", order = 20)]
public class FryingRecipeSO : ScriptableObject {
    public KitchenObjectSO input;
    public KitchenObjectSO output;
    public float fryingTimerMax;
}
