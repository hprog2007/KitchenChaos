using UnityEngine;

[CreateAssetMenu(fileName = "MixingRecipeSO", menuName = "RecipeSOs/MixingRecipeSO", order = 40)]
public class MixingRecipeSO : ScriptableObject {
    public KitchenObjectSO[] input;
    public KitchenObjectSO output;
    public int mixingProgressMax;
}
