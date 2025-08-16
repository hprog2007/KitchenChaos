using System;

[Serializable]
public class OrderTicket
{
    public string Id;
    public RecipeSO Recipe;
    public float Duration;       // total seconds
    public float CumulativeDuration { get; private set; } 
    public float RemainingTime { get; private set;}  // seconds left

    public event Action<OrderTicket> OnTick;
    public event Action<OrderTicket> OnExpired;

    public OrderTicket(string id, RecipeSO recipe, float duration)
    {
        Id = id;
        Recipe = recipe;
        Duration = MathF.Max(0.01f, duration);
        RemainingTime = duration;
        CumulativeDuration = duration;
    }

    public bool Update(float deltaTime)
    {
        if (RemainingTime <= 0f) return false;
        //RemainingTime = MathF.Max(0f, RemainingTime - dt);
        RemainingTime = CumulativeDuration = MathF.Max(0f, CumulativeDuration - deltaTime);
        OnTick?.Invoke(this);
        if (RemainingTime <= 0f)
        {
            OnExpired?.Invoke(this);
            return true;
        }
        return false;
    }

    public void UpdateCumulativeDuration(float cumulativeDuration)
    {
        this.CumulativeDuration = cumulativeDuration;  // Update total duration for this ticket
    }
}
