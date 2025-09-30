using UnityEngine;
using TMPro;
using System;

public class CoinUI : MonoBehaviour
{
    [SerializeField] private TMP_Text coinText; // drag in Inspector
    [SerializeField] private ParticleSystem Coins_Burst;

    private void Start()
    {
        // Initialize
        CurrencyManager_OnCoinsChanged(CurrencyManager.Instance != null ? CurrencyManager.Instance.GetCoinsBalance() : 0);

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCoinsChanged += CurrencyManager_OnCoinsChanged;
            CurrencyManager.Instance.OnCoinsAdded += CoinsAdded;

        }
    }

    private void CoinsAdded(int obj)
    {
        Coins_Burst.Play();
    }

    void OnEnable()
    {
        //if (CurrencyManager.Instance != null)
        //    CurrencyManager.Instance.OnCoinsChanged += HandleChanged;
        
    }

    void OnDisable()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCoinsChanged -= CurrencyManager_OnCoinsChanged;
    }

    void CurrencyManager_OnCoinsChanged(int value)
    {
        if (coinText != null) coinText.text = $"{value}";
    }
}
