using System;

[Serializable]
public class OrderTicket
{
    public string Id;
    public RecipeSO Recipe;
    public float Duration;       // total seconds
    public float RemainingTime { get; private set;}  // seconds left

    public OrderTicket(string id, RecipeSO recipe, float duration)
    {
        Id = id;
        Recipe = recipe;
        Duration = MathF.Max(0.01f, duration);
        RemainingTime = duration;
    }

    public bool Update()
    {
        if (RemainingTime <= 0f) 
            return false; //expired
        
        //decrease RemainingTime one second
        RemainingTime = MathF.Max(0f, RemainingTime - 1f);

        if (RemainingTime <= 0f)
        {
            return true;
        }
        return false;
    }

    public void SetRemainingTime(float time)
    {
        RemainingTime = time;
    }
}
