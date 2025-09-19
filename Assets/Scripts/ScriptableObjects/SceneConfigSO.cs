using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CounterSlotData
{
    public CounterType CounterType;
    public Transform ParentTransform;
    public Transform CounterTransform;
    
}

[CreateAssetMenu(fileName = "New Scene Config", menuName = "Scenes/Scene Config")]
public class SceneConfigSO : ScriptableObject
{
    public SceneType sceneType;
    //public Sprite sceneDisplayImage;
    //public Material shopUITheme;

    public CounterSlotData[] counterSlots; // replaces defaultCounters
    //public CounterType[] newCountersIntroducedHere;
    //public HelperSO[] allowedHelpers;

    //public float priceMultiplier;
    [Serializable]
    public struct RequiredCounters 
    {
        public CounterType CounterType;
        public int CounterCount;
    }

    public List<RequiredCounters> MinRequiredCounters;
}
