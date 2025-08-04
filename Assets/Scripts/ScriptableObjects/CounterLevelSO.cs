using UnityEngine;

[CreateAssetMenu(fileName = "New Counter Level", menuName = "Counters/Counter Level")]
public class CounterLevelSO : ScriptableObject
{
    public CounterType counterType;
    public int level;
    public int upgradePrice;

    // Counter-specific properties
    public float cuttingSpeed;      // CuttingCounter only
    public float fryingTimeMax;     // OvenCounter only
    public int capacity;            // ContainerCounter only

    public GameObject counterPrefab;
}
