using System.Collections.Generic;
using UnityEngine;

public class LevelSelectionMap : MonoBehaviour
{
    [SerializeField] private List<Transform> mapIconsList;
    bool _started;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_started) return;
        _started = true;
        Time.timeScale = 1f;
        AnimationManager.Instance.PlayFloatingUpDown(mapIconsList);
    }
}
