using UnityEngine;
using UnityEngine.AddressableAssets;

[System.Serializable]
public class AssetReferenceTexture2D : AssetReferenceT<Texture2D>
{
    public AssetReferenceTexture2D(string guid) : base(guid) { }
}
