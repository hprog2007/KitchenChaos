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
        HandleChanged(CurrencyManager.Instance != null ? CurrencyManager.Instance.Coins : 0);

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCoinsChanged += HandleChanged;
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
            CurrencyManager.Instance.OnCoinsChanged -= HandleChanged;
    }

    void HandleChanged(int value)
    {
        if (coinText != null) coinText.text = $"{value}";
    }
}
