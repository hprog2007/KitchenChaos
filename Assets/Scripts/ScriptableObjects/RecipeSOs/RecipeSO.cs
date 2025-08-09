using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="RecipeSO", menuName = "RecipeSOs/RecipeSO", order = 10)]
public class RecipeSO : ScriptableObject {
    public PlateCompleteVisual plateCompleteVisual;
    public string recipeName;
    public List<KitchenObjectSO> kitchenObjectSOList;
}
