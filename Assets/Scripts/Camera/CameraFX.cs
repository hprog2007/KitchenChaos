using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CinemachineCamera))]
public class CameraFX : MonoBehaviour
{
    [Header("Defaults (used by ShakeOnce)")]
    [SerializeField] float defaultAmplitude = 1.0f;
    [SerializeField] float defaultFrequency = 10f;
    [SerializeField] float defaultDuration = 0.25f;
    [SerializeField]
    AnimationCurve fadeCurve =
        AnimationCurve.EaseInOut(0f, 1f, 1f, 0f); // amplitude multiplier over time

    CinemachineCamera vcam;
    CinemachineBasicMultiChannelPerlin noise;
    Coroutine shakeRoutine;
    float baseAmp, baseFreq;

    void Awake()
    {
        vcam = GetComponent<CinemachineCamera>();
        noise = vcam.GetComponent<CinemachineBasicMultiChannelPerlin>();
        

        if (noise == null)
        {
            Debug.LogWarning(
                "CameraFX: Add 'Cinemachine Basic Multi-Channel Perlin' to this vcam (Add Extension => Basic Multi-Channel Perlin).");
            return;
        }

        baseAmp = noise.AmplitudeGain;
        baseFreq = noise.FrequencyGain;
        noise.enabled = false;
    }

    private void Start()
    {
        OrderManager.instance.OnOrderExpired += OrderManager_OnOrderExpired;
    }

    private void OnDestroy()
    {
        OrderManager.instance.OnOrderExpired -= OrderManager_OnOrderExpired;
    }


    private void OrderManager_OnOrderExpired(OrderTicket obj)
    {
        Shake(4f, 10f, 2f);
    }

    /// <summary>Quick shake with default values.</summary>
    public void ShakeOnce() => Shake(defaultAmplitude, defaultFrequency, defaultDuration);

    /// <summary>Shake the camera. Amplitude = strength, Frequency = speed.</summary>
    public void Shake(float amplitude, float frequency, float duration)
    {
        if (noise == null) return;
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(Co_Shake(amplitude, frequency, duration));
    }

    IEnumerator Co_Shake(float amplitude, float frequency, float duration)
    {
        noise.enabled = true;
        noise.FrequencyGain = frequency;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float n = Mathf.Clamp01(t / duration);
            float mult = fadeCurve.Evaluate(n);      // 1=>0
            noise.AmplitudeGain = amplitude * mult;
            yield return null;
        }

        noise.AmplitudeGain = baseAmp;
        noise.FrequencyGain = baseFreq;
        noise.enabled = false;
        shakeRoutine = null;
        
    }
    

    
}
