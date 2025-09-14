using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

[DefaultExecutionOrder(-1000)] // init very early
public class SceneTransitionService : MonoBehaviour
{
    public static SceneTransitionService Instance { get; private set; }

    // mailbox keyed by type (supports multiple payload kinds)
    private readonly Dictionary<Type, object> _mailbox = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);   // kill duplicates without touching Instance
            return;
        }

        Instance = this;
        transform.SetParent(null);           // keep it at root
        DontDestroyOnLoad(gameObject);

        // IMPORTANT: don't auto-clear on sceneLoaded; let consumers Consume() explicitly.
        // If you insist on auto-clear, do it AFTER first Update in the new scene, not here.
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null; // only clear if we are the active instance
    }

    // Optional: make behavior consistent across domain reloads
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { Instance = null; }

    public void Load(string sceneName, object payload = null)
    {
        if (payload != null)
            _mailbox[payload.GetType()] = payload;

        SceneManager.LoadScene(sceneName);
    }

    public void Store<T>(T payload) where T : class
    {
        if (payload != null) _mailbox[typeof(T)] = payload;
    }

    public T Consume<T>() where T : class
    {
        if (_mailbox.TryGetValue(typeof(T), out var obj))
        {
            _mailbox.Remove(typeof(T));
            return obj as T;
        }
        return null;
    }
}
