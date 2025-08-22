using UnityEngine;
using System;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }
    public event Action<int> OnCoinsChanged;
    public event Action<int> OnCoinsAdded;

    [SerializeField] private int startingCoins = 0;
    [SerializeField] private bool autoSave = true;

    const string PP_COINS = "coins";
    int coins;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        coins = PlayerPrefs.HasKey(PP_COINS) ? PlayerPrefs.GetInt(PP_COINS) : startingCoins;
        OnCoinsChanged?.Invoke(coins);
    }

    public int Coins => coins;

    public void Add(int amount)
    {
        if (amount <= 0) return;
        coins += amount;
        OnCoinsChanged?.Invoke(coins);
        OnCoinsAdded?.Invoke(coins);
        if (autoSave) Save();
    }

    public bool CanAfford(int cost) => coins >= cost;

    public bool Spend(int cost)
    {
        if (!CanAfford(cost)) return false;
        coins -= cost;
        OnCoinsChanged?.Invoke(coins);
        if (autoSave) Save();
        return true;
    }

    public void Save()
    {
        PlayerPrefs.SetInt(PP_COINS, coins);
        PlayerPrefs.Save();
    }

    // Optional: for debugging in Editor
    [ContextMenu("Reset Coins")]
    void ResetCoinsCtx()
    {
        coins = 0;
        OnCoinsChanged?.Invoke(coins);
        Save();
    }
}
