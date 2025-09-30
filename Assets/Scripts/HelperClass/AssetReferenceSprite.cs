using UnityEngine;
using UnityEngine.AddressableAssets;

[System.Serializable]
public class AssetReferenceSprite : AssetReferenceT<Sprite>
{
    public AssetReferenceSprite(string guid) : base(guid) { }
}
