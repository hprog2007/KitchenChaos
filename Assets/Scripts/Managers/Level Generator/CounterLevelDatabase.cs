using UnityEngine;

[CreateAssetMenu(fileName = "New Counter Level Database", menuName = "Counters/Counter Level Database")]
public class CounterLevelDatabase : ScriptableObject
{
    public CounterLevelSO[] allCounterLevels;

    public GameObject GetPrefab(CounterType type, int level)
    {
        foreach (var so in allCounterLevels)
        {
            if (so.counterType == type && so.level == level)
            {
                return so.counterPrefab;
            }
        }

        Debug.LogWarning($"No CounterLevelSO found for {type} at level {level}");
        return null;
    }
}
