using System;
using System.Collections.Generic;
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
    public List<UpgradeData> UpgradeDataList;
}


public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance {  get; private set; }

    [SerializeField] private Transform handBuildCounters;

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
        gameSnapShot.UpgradeDataList = UpgradeManager.Instance.GetUpgradeDataList();
        SaveLoadManager.SaveGame(gameSnapShot);
    }

    public void LoadLevel()
    {
        var gameSnapShot = SaveLoadManager.LoadGame();
        if (gameSnapShot == null)
        {
            GridManager.Instance.FillGridBySceneCounters();
            return;
        }


        CurrencyManager.Instance.SetCoins(gameSnapShot.coins);
        if (gameSnapShot.GridItems.Count == 0)
        {
            GridManager.Instance.FillGridBySceneCounters();
        }
        else
        {
            GridManager.Instance.RestoreFromSnapshot(gameSnapShot.GridItems);
            Destroy(handBuildCounters.gameObject);
        }
        
        UpgradeManager.Instance.SetUpgradeDataList(gameSnapShot.UpgradeDataList);

        

        //InstantiateCounter(gameSnapShot.grid);
    }    

    public void uuu()
    {

    }

    /*
    private void InstantiateCounter(GridCell[,] gridCells)
    {
        foreach (var cell in gridCells)
        {
            var selCounterTransofrm = cell.placedObject.transform;

            //instantiate new counter
            Transform newCounterTransform = Instantiate(selCounterTransofrm.transform, selCounterTransofrm.parent);
            newCounterTransform.transform.position = selCounterTransofrm.transform.position;
            newCounterTransform.transform.rotation = selCounterTransofrm.transform.rotation;
            newCounterTransform.localScale = selCounterTransofrm.transform.localScale;

            //Replace newCounter in cell grid
            GridManager.Instance.FillGridCellByCounter((BaseCounter)cell.placedObject.GetComponent<BaseCounter>());
        }
    }
    */
    
}
