using System;
using UnityEngine;

[Serializable]
public class OrderTicket
{
    public string Id;
    public RecipeSO Recipe;
    public float Duration;     // total seconds
    public float Remaining;    // seconds left

    public event Action<OrderTicket> OnTick;
    public event Action<OrderTicket> OnExpired;

    public OrderTicket(string id, RecipeSO recipe, float duration)
    {
        Id = id;
        Recipe = recipe;
        Duration = Mathf.Max(0.01f, duration);
        Remaining = Duration;
    }

    public bool Update(float dt)
    {
        if (Remaining <= 0f) return false;
        Remaining = Mathf.Max(0f, Remaining - dt);
        OnTick?.Invoke(this);
        if (Remaining <= 0f) { OnExpired?.Invoke(this); return true; }
        return false;
    }
}
