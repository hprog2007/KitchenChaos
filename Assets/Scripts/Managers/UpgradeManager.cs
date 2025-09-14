using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static UpgradeData;
using Unity.VisualScripting;


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

            // Save upgrade state
            LevelManager.Instance.SaveLevel();

            ScreenMessagesUI.Instance.ShowMessage($"All {upgradeData.CounterTitle}s upgraded to level {upgradeData.CurrentLevel}", 3);

            // Notify all listeners (e.g., CuttingCounter, UI) about the upgrade
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
            switch (upgradeData.CounterType)
            {
                case CounterType.CuttingCounter:
                    FindAnyObjectByType<CuttingCounter>().SetSpeed(upgradeData.levels[upgradeData.CurrentLevel].speed);
                    FindAnyObjectByType<CuttingCounter>().SetCapacity(upgradeData.levels[upgradeData.CurrentLevel].capacity);
                    break;
                case CounterType.Containers:
                    FindAnyObjectByType<ContainerCounter>().SetSpeed(upgradeData.CurrentLevel);
                    FindAnyObjectByType<ContainerCounter>().SetCapacity(upgradeData.CurrentLevel);
                    break;
            }
        }
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
    
    /* moved to LevelManager
     * 
    // Save load
    public void SaveUpgradeState(CounterType counterType)
    {
        var upgradeData = upgradeDataList.FirstOrDefault(a => a.CounterType == counterType);

        PlayerPrefs.SetInt(counterType.ToString() + "_UpgradeLevel", upgradeData.CurrentLevel);
    }

    public void LoadUpgradeState(CounterType counterType)
    {
        var upgradeData = upgradeDataList.FirstOrDefault(a => a.CounterType == counterType);

        upgradeData.CurrentLevel = PlayerPrefs.GetInt(counterType.ToString() + "_UpgradeLevel", 0);

        ApplyUpgrade(upgradeData);
    }
    */

    //used for Save/Load
    public List<UpgradeData> GetUpgradeDataList() => upgradeDataList;

    //used for Save/Load
    public void SetUpgradeDataList(List<UpgradeData> upgradeDataListParam) => upgradeDataList = upgradeDataListParam;



}
