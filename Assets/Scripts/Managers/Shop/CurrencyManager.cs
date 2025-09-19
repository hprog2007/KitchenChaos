using UnityEngine;
using System;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }
    public event Action<int> OnCoinsChanged;
    public event Action<int> OnCoinsAdded;

    [SerializeField] private int startingCoins = 0;

    private int coins;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        coins = startingCoins;
    }

    public int Coins => coins;

    public void Add(int amount)
    {
        if (amount <= 0) 
            return;

        coins += amount;
        OnCoinsChanged?.Invoke(coins);
        OnCoinsAdded?.Invoke(coins);
    }

    public bool CanAfford(int cost) => coins >= cost;

    public bool Spend(int cost)
    {
        if (!CanAfford(cost)) return false;
        coins -= cost;
        OnCoinsChanged?.Invoke(coins);
        return true;
    }

    //used for game load
    public void SetCoins(int coinsParam)
    {
        coins = coinsParam;
        OnCoinsChanged?.Invoke(coins);
    }

    public string GetActivePriceUnit()
    {
        return "$";
    }
}
