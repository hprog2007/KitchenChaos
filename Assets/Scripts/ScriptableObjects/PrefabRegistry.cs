using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PrefabRegistry", menuName = "Game/Prefab Registry")]
public class PrefabRegistry : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public string id;            // e.g. "counter/toaster_v2" or "helper/delivery"
        public GameObject prefab;
    }

    [SerializeField] private List<Entry> entries = new();
    private Dictionary<string, GameObject> _map;

    public GameObject GetPrefab(string id)
    {
        _map ??= BuildMap();
        return _map != null && _map.TryGetValue(id, out var prefab) ? prefab : null;
    }

    private Dictionary<string, GameObject> BuildMap()
    {
        var d = new Dictionary<string, GameObject>();
        foreach (var e in entries)
            if (!string.IsNullOrEmpty(e.id) && e.prefab != null) d[e.id] = e.prefab;
        return d;
    }
}
