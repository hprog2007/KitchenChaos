using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static UpgradeData;
using Unity.VisualScripting;
using System;


[Serializable]
public class UpgradeDTO
{
    public CounterType CounterType;
    
    public string CounterTypeTitle;

    public int CurrentLevel;
}

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance {  get; private set; }

    [SerializeField] private List<UpgradeData> upgradeDataList; // Reference to ScriptableObject    

    // Static event to notify when any upgrade is applied
    public static event System.Action<CounterType> OnAnyUpgradeStarted;
    public static event System.Action<CounterType> OnAnyUpgradeApplied;    

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            Debug.Log("UpgradeManager was created before!");
            return;
        }

        Instance = this;

    }

    // Properties to access current upgrade stats
    public UpgradeData GetNextLevelUpgradeData(CounterType counterType)
    {
        var upgradeData = upgradeDataList.FirstOrDefault(a => a.CounterType == counterType);

        return upgradeData;
    }

    public string GetNextLevelTitle(CounterType counterType)
    {
        var upgradeData = upgradeDataList.FirstOrDefault(a => a.CounterType == counterType);

        return upgradeData.CounterTitle;
    }

    public int GetCurrentLevel(CounterType counterType)
    {
        var upgradeData = upgradeDataList.FirstOrDefault(a => a.CounterType == counterType);

        return upgradeData.CurrentLevel;
    }

    public int GetNextLevel(CounterType counterType)
    {
        var upgradeData = upgradeDataList.FirstOrDefault(a => a.CounterType == counterType);

        return upgradeData.CurrentLevel + 1;
    }

    public float GetCurrentSpeed(CounterType counterType)
    {
        var upgradeData = upgradeDataList.FirstOrDefault(a => a.CounterType == counterType);

        return upgradeData.levels[upgradeData.CurrentLevel].speed;
    }

    public int GetCurrentCapacity(CounterType counterType)
    {
        var upgradeData = upgradeDataList.FirstOrDefault(a => a.CounterType == counterType);

        return upgradeData.levels[upgradeData.CurrentLevel].capacity;
    }

    public int GetNextUpgradePrice(CounterType counterType)
    {
        var upgradeData = upgradeDataList.FirstOrDefault(a => a.CounterType == counterType);

        return upgradeData.CurrentLevel < upgradeData.levels.Length-1 ? upgradeData.levels[upgradeData.CurrentLevel + 1].upgradeCost : 0;
    }

    // Optional: Get upgrade details for UI
    public string GetCurrentUpgradeDescription(CounterType counterType)
    {
        var upgradeData = upgradeDataList.FirstOrDefault(a => a.CounterType == counterType);
        return upgradeData.levels[upgradeData.CurrentLevel].description;
    }

    // Optional: Get upgrade details for UI
    public string GetNextUpgradeDescription(CounterType counterType)
    {
        var upgradeData = upgradeDataList.FirstOrDefault(a => a.CounterType == counterType);

        return upgradeData.levels[upgradeData.CurrentLevel+1].description;
    }

    public bool UpgradeLevelExist(CounterType counterType)
    {
        var upgradeData = upgradeDataList.FirstOrDefault(a => a.CounterType == counterType);

        if (upgradeData == null)
        {
            // no upgrade defined for this counterType
            return false;
        }

        return upgradeData.CurrentLevel < upgradeData.levels.Length-1;
    }

    public void ApplyNextUpgrade(CounterType counterType)
    {
        var upgradeData = upgradeDataList.FirstOrDefault(a => a.CounterType == counterType);
        var currentUpgradeCost = GetNextUpgradePrice(counterType);
        var upgradeLevelExist = UpgradeLevelExist(counterType);
        

        if (upgradeLevelExist)
        {
            OnAnyUpgradeStarted?.Invoke(counterType);

            upgradeData.CurrentLevel++;
            ApplyUpgrade(upgradeData);
            Debug.Log($"Upgraded {upgradeData.CounterTitle} to level {upgradeData.CurrentLevel}");            

            ScreenMessagesUI.Instance.ShowMessage($"All {upgradeData.CounterTitle}s upgraded to level {upgradeData.CurrentLevel}", 3);

            // Notify all listeners (e.g., CuttingCounter, UI) about the upgrade
            //used for particel effect and animation when upgrading
            OnAnyUpgradeApplied?.Invoke(counterType);
        }
        else
        {
            Debug.Log($"Max upgrade level reached for {upgradeData.CounterTitle}");
        }
    }

    public void ApplyUpgrade(UpgradeData upgradeData)
    {
        if (upgradeData.CurrentLevel < upgradeData.levels.Length)
        {
            var matchingCounters = GetCountersByType(upgradeData.CounterType);

            foreach (var counter in matchingCounters)
            {
                counter.SetSpeed(upgradeData.levels[upgradeData.CurrentLevel].speed);
                counter.SetCapacity(upgradeData.levels[upgradeData.CurrentLevel].capacity);
            }
        }
    }   

    //Get Counters By Type
    public BaseCounter[] GetCountersByType(CounterType counterTypeParam)
    {
        BaseCounter[] allCounters = FindObjectsByType<BaseCounter>( FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        return  allCounters
            .Where(c => c.CounterType == counterTypeParam)
            .ToArray();
    }

    // Optional: Reset to base level
    public void ResetUpgrades()
    {
        foreach (var upgradeData in upgradeDataList)
        {
            upgradeData.CurrentLevel = 0;
            ApplyUpgrade(upgradeData);
            LevelManager.Instance.SaveLevel();
        }
    }

    /// <summary>
    /// Builds a list of UpgradeDTOs from the serialized UpgradeData list.
    /// </summary>
    public List<UpgradeDTO> BuildUpgradeDTOList()
    {
        var dtoList = new List<UpgradeDTO>();

        foreach (var data in upgradeDataList)
        {
            if (data == null) continue; // Safety check

            UpgradeDTO dto = new UpgradeDTO
            {
                CounterType = data.CounterType,
                CounterTypeTitle = data.CounterType.ToString(),
                CurrentLevel = data.CurrentLevel,
            };

            dtoList.Add(dto);
        }

        return dtoList;
    }

    /// <summary>
    /// Restores UpgradeData ScriptableObjects from a snapshot of UpgradeDTOs.
    /// </summary>
    public void RestoreFromSnapshot(IEnumerable<UpgradeDTO> items)
    {
        if (items == null) return;

        // Build a lookup for quick CounterType -> DTO mapping
        Dictionary<CounterType, UpgradeDTO> dtoMap =
            items.ToDictionary(dto => dto.CounterType, dto => dto);

        foreach (var data in upgradeDataList)
        {
            if (data == null) continue;

            // Find a matching DTO by CounterType
            if (dtoMap.TryGetValue(data.CounterType, out UpgradeDTO dto))
            {
                // Update simple fields
                data.CurrentLevel = dto.CurrentLevel;   
                ApplyUpgrade(data);

#if UNITY_EDITOR
                // Mark ScriptableObject dirty so Unity saves it in Editor mode
                UnityEditor.EditorUtility.SetDirty(data);
#endif
            }
        } //for



    } //Restore from snapshot

}
