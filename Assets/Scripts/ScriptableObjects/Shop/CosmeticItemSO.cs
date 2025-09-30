using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

public enum CosmeticCategory
{
    Poster,        // texture applied to an in-world frame/quad
    WallSkin,      // material/texture applied to walls
    CounterSkin,   // material applied to counters/tables
    PlayerSkin,    // prefab or material set swapped on player rig
    UITheme,       // optional, for UI skinning
    Other
}

public enum CurrencyType { None, Coins, Gems, Dollar, Toman }

[CreateAssetMenu(menuName = "Shop/Cosmetics/CosmeticItemSO")]
public class CosmeticItemSO : ScriptableObject
{
    [Header("Identity")]
    public string itemId;                   // unique, stable e.g. "COS_L3_Wall_Granite_01"
    public string displayName;
    [TextArea] public string description;
    public CosmeticCategory category = CosmeticCategory.Poster;

    [Header("Economy")]
    public CurrencyType currency = CurrencyType.Coins;
    public int price = 0;                   // interpreted based on 'currency'

    [Header("UI/Preview")]
    public AssetReferenceSprite icon;       // shop thumbnail
    public AssetReferenceGameObject previewPrefab; // optional: rotating preview for 3D skins

    [Header("Category Payload")]
    // You’ll use *one* of these depending on category:
    public AssetReferenceTexture2D posterTexture; // Poster
    public Vector2 posterSizeMeters = new(1.0f, 1.5f);
    public Color frameTint = Color.white;

    public AssetReferenceMaterial wallMaterial;   // WallSkin
    public AssetReferenceMaterial counterMaterial;// CounterSkin

    public AssetReferenceGameObject playerPrefab; // PlayerSkin (full outfit or rig)
    public AssetReferenceMaterial[] playerMaterials; // alternative: just swap materials

    [Header("Application Hints (optional)")]
    public string materialTextureProperty = "_BaseMap"; // used if applying a Texture2D
    public string[] targetTags;               // e.g., "Wall", "Counter", "PosterFrame"
    public string[] targetMaterialSlots;      // optional names to map which renderer mats to replace
}
