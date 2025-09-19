using System.Collections.Generic;
using UnityEngine;

public class LevelSelectionMap : MonoBehaviour
{
    [SerializeField] private List<Transform> mapIconsList;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        AnimationManager.Instance.PlayFloatingUpDown(mapIconsList);
    }
}
