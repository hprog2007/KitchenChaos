using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Scene Config")]
    public SceneConfigSO sceneConfig;

    [Header("Database")]
    public CounterLevelDatabase counterLevelDatabase; // See notes below

    void Start()
    {
        GenerateLevel();
    }

    public void GenerateLevel()
    {
        if (sceneConfig == null)
        {
            Debug.LogError("SceneConfigSO is missing from LevelGenerator.");
            return;
        }

        foreach (var slot in sceneConfig.counterSlots)
        {
            // Check if player owns this counter (if you add ownership checks later)
            // For now, assume all slots are populated

            // Get current global level for this counter type
            int level = CounterUpgradeManager.GetLevel(slot.counterType);

            // Get prefab for this counter type and level
            GameObject prefab = counterLevelDatabase.GetPrefab(slot.counterType, level);

            if (prefab == null)
            {
                Debug.LogWarning($"Prefab not found for {slot.counterType} at level {level}");
                continue;
            }

            // Instantiate counter at defined position and rotation
            Instantiate(prefab, slot.position, slot.rotation);
        }

        Debug.Log("Level generation complete.");
    }
}
