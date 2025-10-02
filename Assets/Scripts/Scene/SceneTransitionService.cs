using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;

[DefaultExecutionOrder(-1000)]
public class SceneTransitionService : MonoBehaviour
{
    public static SceneTransitionService Instance { get; private set; }

    // ------------ PUBLIC CONFIG ------------
    [Header("Loading Scene")]
    [SerializeField] private SceneType loadingSceneType = SceneType.L0_Loading_Scene;   // set in Inspector or keep default

    // ------------ MAILBOX (as you have) ------------
    private readonly Dictionary<Type, object> _mailbox = new();

    // ------------ PENDING REQUEST ------------
    private SceneType _pendingTargetScene;
    private Action<float> _loadingProgressCallback; // set by Loading scene
    private float _minShowSeconds = 0.35f;          // small polish pause to reach 100%
    public float MinShowSeconds => _minShowSeconds;

    void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
            return; 
        }

        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { Instance = null; }

    // --------- SYNC LOAD (kept, if you still need it) ---------
    public void Load(string sceneName, object payload = null)
    {
        if (payload != null) _mailbox[payload.GetType()] = payload;
        //SceneManager.LoadScene(sceneName);
        SceneLoader.Load((SceneLoader.Scene)Enum.Parse(typeof(SceneLoader.Scene), sceneName)); //temporary
    }

    // --------- NEW: ASYNC FLOW WITH LOADING SCENE ---------
    public void LoadWithLoading(SceneType targetScene, object payload = null, float minShowSeconds = 0.35f)
    {
        if (payload != null) _mailbox[payload.GetType()] = payload;        
        _pendingTargetScene = targetScene;
        _minShowSeconds = Mathf.Max(0f, minShowSeconds);

        // Enter the Loading scene (the controller there will call BeginAsyncLoad)
        SceneManager.LoadScene(loadingSceneType.ToString(), LoadSceneMode.Single);
    }

    /// Called by LoadingSceneController.Start()
    public void BeginAsyncLoad(Action<float> onProgress)
    {
        if (_pendingTargetScene == SceneType.None)
        {
            Debug.LogError("[SceneTransitionService] No pending target scene. Did you call LoadWithLoading?");
            return;
        }

        _loadingProgressCallback = onProgress;
        StartCoroutine(Co_LoadTarget());
    }


    // SceneTransitionService.cs (core coroutine)
    private IEnumerator Co_LoadTarget()
    {
        var op = SceneManager.LoadSceneAsync(_pendingTargetScene.ToString(), LoadSceneMode.Single);
        op.allowSceneActivation = false;

        float minSeconds = Mathf.Max(0f, _minShowSeconds);   // set this via LoadWithLoading(..., 10f)
        float start = Time.unscaledTime;

        // drive UI with a timed fake curve 0..0.99
        while (true)
        {
            float elapsed = Time.unscaledTime - start;
            float t01 = minSeconds > 0f ? Mathf.Clamp01(elapsed / minSeconds) : 1f;
            float eased = t01 * t01 * (3f - 2f * t01);     // smoothstep
            float fake = Mathf.Lerp(0f, 0.99f, eased);

            _loadingProgressCallback?.Invoke(fake);

            // activate only when both conditions are met
            if (op.progress >= 0.9f && elapsed >= minSeconds)
                break;

            yield return null;
        }

        // show 100% for one frame so it’s visible
        _loadingProgressCallback?.Invoke(1f);
        yield return null;


        MusicService.Instance.PlayMusicForLevel(_pendingTargetScene);

        op.allowSceneActivation = true;  // Single mode: instantly switches, no manual unload needed
        _pendingTargetScene = SceneType.None;
    }



    // ------------ MAILBOX HELPERS ------------
    public void Store<T>(T payload) where T : class
    { if (payload != null) _mailbox[typeof(T)] = payload; }

    public T Consume<T>() where T : class
    {
        if (_mailbox.TryGetValue(typeof(T), out var obj))
        { _mailbox.Remove(typeof(T)); return obj as T; }
        return null;
    }
}
