using System;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    public static OrderManager instance { get; private set; }

    public event EventHandler OnRecipeSpawned;

        
    private float spawnRecipeTimer;
    private float spawnRecipeTimerMax = 4f;
    private int waitingRecipeMax = 4;

    [SerializeField] private RecipeListSO recipeListSO;


    private void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        /////////////////////////////////////////////////////////// Generate recipe waiting list
        spawnRecipeTimer -= Time.deltaTime;
        if (spawnRecipeTimer <= 0f)
        {
            spawnRecipeTimer = spawnRecipeTimerMax;

            if (KitchenGameManager.Instance.IsGamePlaying() &&
                DeliveryManager.instance.GetWaitingRecipeSOList().Count < waitingRecipeMax)
            {
                RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count)];

                RecipeSO recipe = GetRandomRecipe();
                if (recipe != null)
                {
                    DeliveryManager.instance.AddWaitingRecipe(recipe);

                    OnRecipeSpawned?.Invoke(this, EventArgs.Empty);

                }

            }
        }
    }

    private RecipeSO GetRandomRecipe()
    {
        if (recipeListSO.recipeSOList.Count == 0) return null;
        return recipeListSO.recipeSOList[UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count)];
    }
}
