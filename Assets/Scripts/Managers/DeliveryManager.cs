using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{

    // Existing events (kept)
    public event EventHandler OnRecipeCompleted;
    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFailed;

    // New, payloaded events (UI should use these)
    public event Action<OrderTicket> OnOrderSpawned;
    public event Action<OrderTicket> OnOrderRemoved;   // completed OR expired remove
    public event Action<OrderTicket> OnOrderTick;      // per-frame-ish (can be throttled)
    public event Action<OrderTicket> OnOrderExpired;   // auto-fail due to timer

    public static DeliveryManager instance { get; private set; }

    // Replace bare recipes with tickets
    private readonly List<OrderTicket> waitingOrderTickets = new List<OrderTicket>();
    private int successfulRecipesAmount;

    // --- Tunables (expose in inspector)
    [Header("Time Config")]
    [SerializeField] private float baseSeconds = 320f;
    [SerializeField] private float secondsPerIngredient = 6f;
    [SerializeField]
    private AnimationCurve complexityMultiplier =
        AnimationCurve.Linear(1, 1f, 5, 1.6f);
    [SerializeField] private float minSeconds = 10f;
    [SerializeField] private float maxSeconds = 120f;
    

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        // Tick all orders; expire those that hit zero
        // (Iterate backwards so we can remove safely)
        for (int i = waitingOrderTickets.Count - 1; i >= 0; i--)
        {
            var ticket = waitingOrderTickets[i];
            bool justExpired = ticket.Update(Time.deltaTime);
            OnOrderTick?.Invoke(ticket); // UI can update its ring/text
            if (justExpired)
            {
                OnRecipeFailed?.Invoke(this, EventArgs.Empty);
                OnOrderExpired?.Invoke(ticket); // Expired order event
                waitingOrderTickets.RemoveAt(i);
                OnOrderRemoved?.Invoke(ticket); // Removed after expiration
            }
        }
    }

    private float CalculateCumulativeDuration()
    {
        float totalDuration = 0f;
        foreach (var ticket in waitingOrderTickets)
        {
            totalDuration += ticket.RemainingTime;
        }
        return totalDuration;
    }

    private float CalcDuration(RecipeSO recipe)
    {
        // If RecipeSO has a Complexity int, use it; else assume 1
        int complexity = recipe.Complexity <= 0 ? 1 : recipe.Complexity;
        float t = baseSeconds + recipe.kitchenObjectSOList.Count * secondsPerIngredient;
        t *= complexityMultiplier.Evaluate(Mathf.Clamp(complexity, 1, 5));
        return Mathf.Clamp(t, minSeconds, maxSeconds);
    }

    // --- Public API ---

    public IReadOnlyList<OrderTicket> GetWaitingOrders() => waitingOrderTickets;

    public List<RecipeSO> GetWaitingRecipeSOList()
    {
        // kept for compatibility with existing UI code
        var list = new List<RecipeSO>(waitingOrderTickets.Count);
        foreach (var t in waitingOrderTickets) list.Add(t.Recipe);
        return list;
    }

    public int GetSuccessfulRecipesAmount() => successfulRecipesAmount;

    public void AddWaitingRecipe(RecipeSO recipeSO)
    {
        string id = Guid.NewGuid().ToString("N");

        // Calculate duration for current order
        float durationForCurrentOrder = CalcDuration(recipeSO);

        // Create and add the new ticket with the calculated duration
        var ticket = new OrderTicket(id, recipeSO, durationForCurrentOrder);
        waitingOrderTickets.Add(ticket);

        // Recalculate the total duration after removing expired/completed tickets
        float cumulativeDuration = RecalculateTicketDurations();

        OnOrderSpawned?.Invoke(ticket);
    }

    private float RecalculateTicketDurations()
    {
        if (waitingOrderTickets.Count == 0) return 0f;

        // Recalculate the duration for all remaining tickets
        float cumulativeDuration = waitingOrderTickets[0].RemainingTime;
        // Iterate from the first to the last item in the list
        for (int i = 1; i < waitingOrderTickets.Count; i++)
        {
            cumulativeDuration += waitingOrderTickets[i].Duration;
            waitingOrderTickets[i].UpdateCumulativeDuration(cumulativeDuration);
        }
        return cumulativeDuration;
    }

    


    public RecipeSO GetFirstOrderRecipe() => waitingOrderTickets.Count > 0 ? waitingOrderTickets[0].Recipe : null;
    public OrderTicket GetFirstTicket() => waitingOrderTickets.Count > 0 ? waitingOrderTickets[0] : null;

    // Player can deliver any recipe (unordered)
    public void DeliverRecipe(PlateKitchenObject plateKitchenObject)
    {
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
    }

    // Player must deliver the top (FIFO)
    public void DeliverRecipeFIFO(PlateKitchenObject plateKitchenObject)
    {
        if (waitingOrderTickets.Count == 0) { OnRecipeFailed?.Invoke(this, EventArgs.Empty); return; }

        var ticket = GetFirstTicket();
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
        waitingOrderTickets.RemoveAt(index);

        CalcAndAddReward(ticket);

        OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
        OnRecipeSuccess?.Invoke(this, EventArgs.Empty);

        OnOrderRemoved?.Invoke(ticket);
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
