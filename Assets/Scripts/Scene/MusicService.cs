using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>
/// This class used to play a music in level selection map and resume in loading screen
/// but for main scene we have different music
/// </summary>

[Serializable]
public class SceneMusicPair
{
    public SceneType SceneType;
    public AudioClip MusicForScene;
}

[RequireComponent(typeof(AudioSource))]
public class MusicService : MonoBehaviour
{
    private const string PLAYER_PREFS_MUSIC_VOLUME = "MusicVolume";

    public static MusicService Instance { get; private set; }

    [Header("Default music (optional)")]
    public List<SceneMusicPair> sceneAudioClips;
    public float _volume = 0.3f;


    [Header("Fades")]
    public float fadeInSeconds = 0.75f;
    public float fadeOutSeconds = 0.5f;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
            return; 
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); //put this on a root object and manage music for all scenes 

        //set specs
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.ignoreListenerPause = true;   // keeps playing even if AudioListener.pause = true
        audioSource.playOnAwake = false;

        //load from prefs
        _volume = PlayerPrefs.GetFloat(PLAYER_PREFS_MUSIC_VOLUME, _volume);

        var audioClip = GetSceneMusic(SceneManager.GetActiveScene().name);
        if (audioClip != null && !audioSource.isPlaying)
            PlayIfNotPlaying(audioClip);
    }

    public void PlayMusicForLevel(SceneType sceneTypeParam)
    {
        PlayIfNotPlaying(GetSceneMusic(sceneTypeParam));
    }

    public void PlayIfNotPlaying(AudioClip clip )
    {
        // return if playing
        if (audioSource.clip == clip && audioSource.isPlaying)
        {
            return;
        }

        StartCoroutine(CoPlayCrossfade(clip));
    }

    IEnumerator CoPlayCrossfade(AudioClip newClip)
    {
        // fade out current
        if (audioSource.isPlaying && audioSource.volume > 0f && fadeOutSeconds > 0f)
        {
            float start = audioSource.volume;
            for (float t = 0; t < fadeOutSeconds; t += Time.unscaledDeltaTime)
            {
                audioSource.volume = Mathf.Lerp(start, 0f, t / fadeOutSeconds);
                yield return null;
            }
        }

        int preservedSamples = audioSource.isPlaying ? audioSource.timeSamples : 0;

        audioSource.clip = newClip;
        audioSource.volume = 0f;

        // Optional: if you want to continue the *same* track when scenes change:
        // comment out the next line if you ALWAYS start new tracks at the beginning.
        if (preservedSamples > 0 && newClip == audioSource.clip && preservedSamples < newClip.samples)
            audioSource.timeSamples = preservedSamples;  // continue at same position

        audioSource.Play();

        // fade in
        if (fadeInSeconds > 0f)
        {
            for (float t = 0; t < fadeInSeconds; t += Time.unscaledDeltaTime)
            {
                audioSource.volume = Mathf.Lerp(0f, _volume, t / fadeInSeconds);
                yield return null;
            }
        }
        audioSource.volume = _volume;
    }

    public void SetVolume(float volumeParam)
    {
        audioSource.volume = volumeParam;

        PlayerPrefs.SetFloat(PLAYER_PREFS_MUSIC_VOLUME, volumeParam);
        PlayerPrefs.Save();
    }
    public void Pause() => audioSource.Pause();
    public void UnPause() => audioSource.UnPause();

    public void ChangeVolume()
    {
        _volume += .1f;
        if (_volume > 1f)
        {
            _volume = 0f;
        }
        audioSource.volume = _volume;

        PlayerPrefs.SetFloat(PLAYER_PREFS_MUSIC_VOLUME, _volume);
        PlayerPrefs.Save();
    }

    public float GetVolume()
    {
        return _volume;
    }

    public AudioClip GetSceneMusic(string sceneNameParam)
    {
        return GetSceneMusic((SceneType) Enum.Parse(typeof(SceneType), sceneNameParam));

    }
    public AudioClip GetSceneMusic(SceneType sceneTypeParam)
    {
       var sceneMusic = sceneAudioClips.Where(k => k.SceneType == sceneTypeParam).FirstOrDefault();
        if (sceneMusic == null)
        {
            return null;
        } else
        {
            return sceneMusic.MusicForScene;
        }
    }
}
