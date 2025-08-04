using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int coins;
    public List<CounterType> countersOwned;
    public Dictionary<CounterType, int> counterLevels;
    public List<string> helpersOwned;
    public Dictionary<string, int> helperLevels;
}

/*
****** Recommended save structure  *******

json
{
  "coins": 1200,
  "countersOwned": ["CuttingCounter", "OvenCounter"],
  "counterLevels": {
    "CuttingCounter": 2,
    "OvenCounter": 1
  },
  "helpersOwned": ["PlateBot"],
  "helperLevels": {
    "PlateBot": 2
  }
}
*/

/*
 
 #### When to call SaveGame?**

 After **every purchase or upgrade**:

* Buying a new counter
* Upgrading a counter type
* Buying or upgrading a helper
* After finishing a scene (if coins are gained)

This prevents progress loss on crashes or closures.


 */


public static class SaveLoadManager
{
    private static string saveKey = "KitchenChaosSave";

    public static void SaveGame(int coins, List<CounterType> countersOwned, List<string> helpersOwned)
    {
        SaveData data = new SaveData();
        data.coins = coins;
        data.countersOwned = countersOwned;
        data.counterLevels = CounterUpgradeManager.GetAllLevels();
        data.helpersOwned = helpersOwned;
        data.helperLevels = HelperManager.GetAllLevels();

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(saveKey, json);
        PlayerPrefs.Save();

        Debug.Log("Game saved");
    }

    public static SaveData LoadGame()
    {
        if (!PlayerPrefs.HasKey(saveKey))
            return null;

        string json = PlayerPrefs.GetString(saveKey);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // Restore counter levels
        foreach (var pair in data.counterLevels)
            CounterUpgradeManager.SetLevel(pair.Key, pair.Value);

        // Restore helper levels
        foreach (var pair in data.helperLevels)
            HelperManager.SetLevel(pair.Key, pair.Value);

        Debug.Log("Game loaded");
        return data;
    }
}
