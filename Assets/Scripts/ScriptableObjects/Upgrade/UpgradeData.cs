using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeData", menuName = "Upgrades/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    public BaseCounter Counter;

    public CounterType CounterType;

    public string CounterTitle;
    
    public int CurrentLevel;
    
    [System.Serializable]
    public struct UpgradeLevel
    {
        public float speed;
        public int capacity;
        public int upgradeCost;
        public string description;
        // upgraded prefab;
    }

    public UpgradeLevel[] levels;
}