using UnityEngine;
using System.IO;

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
    private const string DirName = "Saves";
    private const string FileName = "KitchenChaos_V1";
    private static string SaveDir => System.IO.Path.Combine(Application.persistentDataPath, DirName);
    private static string SavePathAndFileName => System.IO.Path.Combine(SaveDir, $"{FileName}.bin");

    public static void SaveGame(GameSnapshot gameSnapshot)
    {
        if (!Directory.Exists(SaveDir))
        {
            Directory.CreateDirectory(SaveDir);
        }

        string json = JsonUtility.ToJson(gameSnapshot);

        File.WriteAllText(SavePathAndFileName, json);

        Debug.Log("Game saved");
    }

    public static GameSnapshot LoadGame()
    {
        if (!File.Exists(SavePathAndFileName))
            return null;

        var json = File.ReadAllText(SavePathAndFileName);
        var data = JsonUtility.FromJson<GameSnapshot>(json);

        Debug.Log("Game loaded");
        return data;
    }
}
