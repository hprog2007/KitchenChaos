using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionService : MonoBehaviour
{
    public static SceneTransitionService Instance { get; private set; }
    object _payload;

    void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
            return; 
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += (_, __) => _payload = null; // auto-clear after delivery if you want
    }

    public void Load(string sceneName, object payload = null)
    {
        _payload = payload;
        SceneManager.LoadScene(sceneName);
    }

    public T Consume<T>() where T : class
    {
        var p = _payload as T;
        _payload = null;
        return p;
    }
}
