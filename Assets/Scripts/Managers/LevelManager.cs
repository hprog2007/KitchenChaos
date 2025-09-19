using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class GameSnapshot
{
    public string version = "2.1.0";
    public SceneType scene;
    public int coins;
    public List<GridItemDTO> GridItems;
    public List<UpgradeDTO> UpgradeDTOList;
}


public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance {  get; private set; }

    [SerializeField] private Transform handBuildCounters;
    [SerializeField] private SceneConfigSO SceneConfigSO;

    private void Awake()
    {
        if (Instance != null && Instance != this) { 
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        LoadLevel();
    }
    public void SaveLevel()
    {
        var gameSnapShot = new GameSnapshot();
        gameSnapShot.scene = (SceneType)System.Enum.Parse(typeof(SceneType), SceneManager.GetActiveScene().name);
        gameSnapShot.coins = CurrencyManager.Instance.Coins;
        gameSnapShot.GridItems = GridManager.Instance.BuildSnapshot();
        gameSnapShot.UpgradeDTOList = UpgradeManager.Instance.BuildUpgradeDTOList();
        SaveLoadManager.SaveGame(gameSnapShot);
    }

    public void LoadLevel()
    {
        var gameSnapShot = SaveLoadManager.LoadGame();
        
        //if no save exists then build the grid based on current scene
        if (gameSnapShot == null)
        {
            GridManager.Instance.FillGridBySceneCounters();
            return;
        }

        if (gameSnapShot.GridItems.Count == 0)
        {
            GridManager.Instance.FillGridBySceneCounters();
        }
        else
        {
            GridManager.Instance.RestoreFromSnapshot(gameSnapShot.GridItems);
            Destroy(handBuildCounters.gameObject); // destroy counters that are manually placed in editor
        }


        CurrencyManager.Instance.SetCoins(gameSnapShot.coins);

        //Restore upgrade current levels
        UpgradeManager.Instance.RestoreFromSnapshot(gameSnapShot.UpgradeDTOList);

    }    

    public SceneType GetActiveSceneType()
    {
        return (SceneType)Enum.Parse(typeof(SceneType), SceneManager.GetActiveScene().name);
    }

    public int GetMinRequired(CounterType counterTypeParam)
    {
         var min = SceneConfigSO.MinRequiredCounters.FirstOrDefault(m => m.CounterType == counterTypeParam);
         return min.CounterCount;
    }
}
