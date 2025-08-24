

using UnityEngine;

public class Upgrade : MonoBehaviour
{
    [SerializeField] private UpgradeLevels upgradeData; // Reference to ScriptableObject
    
    private static int currentLevel = 0; // Current upgrade level (0 = base level)
    private BaseCounter counter; // Reference to the counter's logic (modify as per your counter's script)

    // Static event to notify when any upgrade is applied
    public static event System.Action<Upgrade> OnAnyUpgradeApplied;

    // Properties to access current upgrade stats
    public float CurrentSpeed => upgradeData.levels[currentLevel].speed;
    public int CurrentCapacity => upgradeData.levels[currentLevel].capacity;
    public int CurrentUpgradeCost => currentLevel < upgradeData.levels.Length - 1 ? upgradeData.levels[currentLevel + 1].upgradeCost : 0;
    public bool CanUpgrade => currentLevel < upgradeData.levels.Length - 1;

    

    private void Awake()
    {
        // Get the counter component (replace 'Counter' with your actual counter script)
        counter = GetComponent<BaseCounter>();
        if (counter == null)
        {
            Debug.LogError("Counter component not found on " + gameObject.name);
        }
    }

    private void Start()
    {
        // Apply the initial upgrade level
        ApplyUpgrade();
    }

    public void ApplyNextUpgrade()
    {
        if (CanUpgrade)
        {
            currentLevel++;
            ApplyUpgrade();
            Debug.Log($"Upgraded {gameObject.name} to level {currentLevel}");
            // Notify all listeners (e.g., CuttingCounter, UI) about the upgrade
            OnAnyUpgradeApplied?.Invoke(this);
        }
        else
        {
            Debug.Log($"Max upgrade level reached for {gameObject.name}");
        }
    }

    private void ApplyUpgrade()
    {
        if (currentLevel < upgradeData.levels.Length)
        {
            counter.SetSpeed(upgradeData.levels[currentLevel].speed);
            counter.SetCapacity(upgradeData.levels[currentLevel].capacity);
        }
    }

    // Optional: Get upgrade details for UI
    public string GetCurrentUpgradeDescription()
    {
        return upgradeData.levels[currentLevel].description;
    }

    // Optional: Reset to base level
    public void ResetUpgrades()
    {
        currentLevel = 0;
        ApplyUpgrade();
    }

    // Save load
    public void SaveUpgradeState(string counterID)
    {
        PlayerPrefs.SetInt(counterID + "_UpgradeLevel", currentLevel);
    }

    public void LoadUpgradeState(string counterID)
    {
        currentLevel = PlayerPrefs.GetInt(counterID + "_UpgradeLevel", 0);
        ApplyUpgrade();
    }
}