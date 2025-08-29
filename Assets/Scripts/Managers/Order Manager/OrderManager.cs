using System;
using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    public static OrderManager instance { get; private set; }

    // New, payloaded events (UI should use these)
    public event Action<OrderTicket> OnOrderSpawned;
    public event Action<OrderTicket> OnOrderRemoved;   // completed OR expired remove
    public event Action<OrderTicket> OnOrderTick;      // per-frame-ish (can be throttled)
    public event Action<OrderTicket> OnOrderExpired;   // auto-fail due to timer

    // --- Tunables (expose in inspector)
    [Header("Time Config")]
    [SerializeField] private float baseSeconds = 32f;
    [SerializeField] private float secondsPerIngredient = 6f;
    [SerializeField]
    private AnimationCurve complexityMultiplier =
        AnimationCurve.Linear(1, 1f, 5, 1.6f);
    [SerializeField] private float minSeconds = 10f;
    [SerializeField] private float maxSeconds = 120f;

    private readonly List<OrderTicket> waitingOrderTickets = new List<OrderTicket>();

    private float spawnRecipeTimer;
    private float spawnRecipeTimerMax = 4f;
    public int waitingRecipeMax = 4;

    private float currentTimer = 1f;

    
    [SerializeField] private RecipeListSO recipeListSO; //list of all availlable recipes for this level


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
                GetWaitingRecipeSOList().Count < waitingRecipeMax)
            {
                RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count)];

                RecipeSO recipe = GetRandomRecipe();
                if (recipe != null)
                {
                    AddWaitingRecipe(recipe);
                }

            }
        }

        currentTimer -= Time.deltaTime;
        if (currentTimer <= 0f)
        {
            currentTimer = 1f;

            // Tick all orders; expire those that hit zero
            // (Iterate backwards so we can remove safely)
            for (int i = waitingOrderTickets.Count - 1; i >= 0; i--)
            {
                var ticket = waitingOrderTickets[i];
                bool justExpired = ticket.Update();
                OnOrderTick?.Invoke(ticket); // UI can update its ring/text
                if (justExpired)
                {
                    //order time expired => the recipe failed
                    OnOrderExpired?.Invoke(ticket); // Expired order event
                    waitingOrderTickets.RemoveAt(i);
                    OnOrderRemoved?.Invoke(ticket); // Removed after expiration
                }
            }
        }
    }

    private RecipeSO GetRandomRecipe()
    {
        if (recipeListSO.recipeSOList.Count == 0) return null;
        return recipeListSO.recipeSOList[UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count)];
    }

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

        Debug.Log("Order added to waiting with manager " + GetInstanceID());
        OnOrderSpawned?.Invoke(ticket);

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

    private float CalculateCumulativeDuration()
    {
        float totalDuration = 0f;
        foreach (var ticket in waitingOrderTickets)
        {
            totalDuration += ticket.RemainingTime;
        }
        return totalDuration;
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

    public int GetWaitingCount() => waitingOrderTickets.Count;

    public void RemoveOrderTicket(OrderTicket ticket)
    {
        foreach (var t in waitingOrderTickets)
        {
            if (ticket.Id == t.Id)
            {
                OnOrderRemoved?.Invoke(ticket);
                waitingOrderTickets.Remove(t);
            }
        }
    }
}
