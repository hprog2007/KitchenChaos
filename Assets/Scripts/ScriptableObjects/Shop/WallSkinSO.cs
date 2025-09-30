using UnityEngine;

[CreateAssetMenu(menuName = "Shop/WallSkinSO")]
public class WallSkinSO : ScriptableObject
{
    [Header("Identity")]
    public string posterId;          // unique, stable (e.g., "P_L1_002")
    public string displayName;
    [TextArea] public string description;

    [Header("Economy")]
    public int priceCoins;

    [Header("Assets")]
    public AssetReferenceSprite thumbSprite;       // UI
    public AssetReferenceTexture2D posterTexture;  // in-world material
    public Vector2 desiredSizeMeters = new(1.0f, 1.5f);

    [Header("Optional")]
    public Color frameTint = Color.white;
}
