using UnityEngine;

[System.Serializable]
public class CounterSlotData
{
    public CounterType counterType;
    public Vector3 position;
    public Quaternion rotation;
}

[CreateAssetMenu(fileName = "New Scene Config", menuName = "Scenes/Scene Config")]
public class SceneConfigSO : ScriptableObject
{
    public string sceneName;
    public Sprite sceneDisplayImage;
    public Material shopUITheme;

    public CounterSlotData[] counterSlots; // replaces defaultCounters
    public CounterType[] newCountersIntroducedHere;
    public HelperSO[] allowedHelpers;

    public float priceMultiplier;
}
