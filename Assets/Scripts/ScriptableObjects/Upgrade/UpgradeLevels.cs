using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeLevels", menuName = "Upgrades/UpgradeLevels")]
public class UpgradeLevels : ScriptableObject
{
    [System.Serializable]
    public struct UpgradeLevel
    {
        public float speed;
        public int capacity;
        public int upgradeCost;
        public string description;
    }

    public UpgradeLevel[] levels;
}