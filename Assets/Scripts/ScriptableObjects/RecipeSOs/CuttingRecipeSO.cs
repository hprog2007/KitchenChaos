using UnityEngine;

[CreateAssetMenu(fileName = "CuttingRecipeSO", menuName = "RecipeSOs/CuttingRecipeSO", order = 40)]
public class CuttingRecipeSO : ScriptableObject {
    public KitchenObjectSO input;
    public KitchenObjectSO output;
    public int cuttingProgressMax;
}
