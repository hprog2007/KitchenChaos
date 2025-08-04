using UnityEngine;

public class ShopBuyCardData
{
    public string ID; // Unique identifier for this shop card (e.g. "CuttingCounter_Lv2")

    public CounterType CounterType; // e.g. CuttingCounter, OvenCounter (enum)

    public SceneType SceneType; // The kitchen or scene this card belongs to (enum)

    public int CurrentLevel; // Current level of this counter (0 if buying a new counter)

    public int NextLevel; // The level this card is offering (e.g. +1 for upgrades or 1 for first purchase)

    public string Title; // Display title, e.g. "Cutting Counter Lv2"

    public Sprite Image; // UI sprite for the card

    public int Price; // Price in coins

    public bool IsNew; // Shows NEW badge in UI

    public bool IsUpgrade; // Shows UPGRADE badge in UI

    public string Description; // Optional: summary of effect or upgrade benefit

    public bool IsLocked; // If locked, disables Buy/Upgrade button
}
