using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CosmeticApplier : MonoBehaviour
{
    public void Apply(CosmeticItemSO cosmeticItemSO)
    {
        switch (cosmeticItemSO.category)
        {
            case CosmeticCategory.Poster:
                ApplyPoster(cosmeticItemSO);
                break;
            case CosmeticCategory.WallSkin:
                ApplyMaterialToTagged(cosmeticItemSO.wallMaterial, cosmeticItemSO.targetTags, cosmeticItemSO.targetMaterialSlots);
                break;
            case CosmeticCategory.CounterSkin:
                ApplyMaterialToTagged(cosmeticItemSO.counterMaterial, cosmeticItemSO.targetTags, cosmeticItemSO.targetMaterialSlots);
                break;
            case CosmeticCategory.PlayerSkin:
                ApplyPlayerSkin(cosmeticItemSO);
                break;
            default:
                Debug.LogWarning($"Unhandled category: {cosmeticItemSO.category}");
                break;
        }
    }

    private void ApplyPoster(CosmeticItemSO cosmeticItemSO)
    {
        if (!cosmeticItemSO.posterTexture.RuntimeKeyIsValid()) return;

        cosmeticItemSO.posterTexture.LoadAssetAsync().Completed += h =>
        {
            if (h.Status != AsyncOperationStatus.Succeeded) return;

            var targets = FindTargets(cosmeticItemSO.targetTags);
            foreach (var rend in targets.Select(t => t.GetComponentInChildren<Renderer>(true)).Where(r => r))
            {
                var mat = rend.material; // instance
                mat.SetTexture(cosmeticItemSO.materialTextureProperty, h.Result);
                // Optionally set scale via material or adjust mesh scale by def.posterSizeMeters
            }
        };
    }

    private void ApplyMaterialToTagged(AssetReferenceMaterial matRef, string[] tags, string[] materialSlots)
    {
        if (!matRef.RuntimeKeyIsValid()) return;

        matRef.LoadAssetAsync().Completed += h =>
        {
            if (h.Status != AsyncOperationStatus.Succeeded) return;

            var targets = FindTargets(tags);
            foreach (var rend in targets.Select(t => t.GetComponentInChildren<Renderer>(true)).Where(r => r))
            {
                var mats = rend.sharedMaterials; // shared to replace slots consistently
                if (materialSlots != null && materialSlots.Length > 0)
                {
                    for (int i = 0; i < mats.Length; i++)
                    {
                        var nameNoInstance = mats[i] ? mats[i].name.Replace(" (Instance)", "") : "";
                        if (materialSlots.Contains(nameNoInstance))
                            mats[i] = h.Result;
                    }
                }
                else
                {
                    for (int i = 0; i < mats.Length; i++) mats[i] = h.Result;
                }
                rend.sharedMaterials = mats;
            }
        };
    }

    private void ApplyPlayerSkin(CosmeticItemSO cosmeticItemSo)
    {
        // Option A: swap whole prefab
        if (cosmeticItemSo.playerPrefab.RuntimeKeyIsValid())
        {
            cosmeticItemSo.playerPrefab.InstantiateAsync().Completed += h =>
            {
                if (h.Status != AsyncOperationStatus.Succeeded) return;
                // Disable/replace existing player avatar here, parent & position new one.
            };
            return;
        }

        // Option B: swap materials on the existing rig
        if (cosmeticItemSo.playerMaterials != null && cosmeticItemSo.playerMaterials.Length > 0)
        {
            // Load all materials then apply
            foreach (var matRef in cosmeticItemSo.playerMaterials)
            {
                if (!matRef.RuntimeKeyIsValid()) continue;
                matRef.LoadAssetAsync(); // you can track completions and then assign to skinned mesh renderers
            }
        }
    }

    private GameObject[] FindTargets(string[] tags)
    {
        if (tags == null || tags.Length == 0) return new GameObject[0];
        var list = new System.Collections.Generic.List<GameObject>();
        foreach (var t in tags)
            list.AddRange(GameObject.FindGameObjectsWithTag(t));
        return list.ToArray();
    }
}
