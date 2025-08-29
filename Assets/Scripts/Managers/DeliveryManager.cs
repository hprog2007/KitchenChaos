using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DeliveryManager : MonoBehaviour
{

    // Existing events (kept)
    public event EventHandler OnRecipeCompleted;
    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFailed;

    public static DeliveryManager instance { get; private set; }

    private int successfulRecipesAmount;

    
    

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);   // kill duplicates
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject); // if you intend it to persist across scenes
    }

    private void Update()
    {
        
    }

    public int GetSuccessfulRecipesAmount() => successfulRecipesAmount;

    // Player can deliver any recipe (unordered)
    public void DeliverRecipe(PlateKitchenObject plateKitchenObject)
    {
        /* not needed for now
        for (int i = 0; i < waitingOrderTickets.Count; i++)
        {
            var ticket = waitingOrderTickets[i];
            if (MatchesPlate(ticket.Recipe, plateKitchenObject))
            {
                CompleteAndRemoveAt(i, ticket);
                return;
            }
        }
        // No matches
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
        */
    }

    // Player must deliver the top (FIFO)
    public void DeliverRecipeFIFO(PlateKitchenObject plateKitchenObject)
    {
        if (OrderManager.instance.GetWaitingCount() == 0) { 
            return; 
        }

        var ticket = OrderManager.instance.GetFirstTicket();
        if (MatchesPlate(ticket.Recipe, plateKitchenObject))
        {
            CompleteAndRemoveAt(0, ticket);
        }
        else
        {
            OnRecipeFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    private void CompleteAndRemoveAt(int index, OrderTicket ticket)
    {
        successfulRecipesAmount++;
        CalcAndAddReward(ticket);
        OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
        OnRecipeSuccess?.Invoke(this, EventArgs.Empty);

        OrderManager.instance.RemoveOrderTicket(ticket);
    }

    private bool MatchesPlate(RecipeSO recipe, PlateKitchenObject plate)
    {
        var plateList = plate.GetKitchenObjectSOList();
        if (recipe.kitchenObjectSOList.Count != plateList.Count) return false;

        // unordered compare
        foreach (KitchenObjectSO recipeIng in recipe.kitchenObjectSOList)
        {
            bool found = false;
            foreach (KitchenObjectSO plateIng in plateList)
            {
                if (plateIng == recipeIng) { found = true; break; }
            }
            if (!found) return false;
        }
        return true;
    }

    private void CalcAndAddReward(OrderTicket ticket)
    {
        ///// reward coins

        int baseReward = ticket.Recipe != null ? ticket.Recipe.rewardCoins : 10; // fallback fixed value
                                                                                 // Optional tip based on time remaining (0..1)
        int tip = Mathf.RoundToInt(baseReward * 0.5f * Mathf.Clamp01(ticket.RemainingTime));
        int total = baseReward + tip;

        CurrencyManager.Instance.Add(total);
        ////// end of reward coins
    }
}
