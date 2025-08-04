using UnityEngine;

[CreateAssetMenu(fileName = "New Helper", menuName = "Helpers/Helper")]
public class HelperSO : ScriptableObject
{
    public string helperName;
    public Sprite icon;
    public int unlockSceneNumber;

    public HelperLevelData[] levels;
}

[System.Serializable]
public class HelperLevelData
{
    public int level;
    public int upgradePrice;
    public float efficiency;  // e.g. speed or automation quality
}
