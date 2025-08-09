using UnityEngine;

[CreateAssetMenu(fileName = "BurningRecipeSO", menuName = "RecipeSOs/BurningRecipeSO", order = 30)]
public class BurningRecipeSO : ScriptableObject {
    public KitchenObjectSO input;
    public KitchenObjectSO output;
    public float burningTimerMax;
}
