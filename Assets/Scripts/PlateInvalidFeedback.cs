using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlateKitchenObject))]
public class PlateInvalidFeedback : MonoBehaviour
{
    [Header("Shake")]
    [SerializeField] private Transform shakeTarget;          // Defaults to this.transform
    [SerializeField] private float shakeDuration = 0.18f;
    [SerializeField] private float shakeStrength = 0.06f;

    [Header("Flash (optional)")]
    [SerializeField] private List<Renderer> flashRenderers = new List<Renderer>();
    [SerializeField] private Color flashColor = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField] private float flashTime = 0.12f;

    [Header("Sound (optional)")]
    [SerializeField] private AudioClip invalidSfx;

    [Header("Spam control")]
    [SerializeField] private float minInterval = 0.08f;

    private PlateKitchenObject plate;
    private Coroutine shakeCo, flashCo;
    private readonly Dictionary<Renderer, Color> _orig = new();
    private float _lastTime;

    void Awake()
    {
        plate = GetComponent<PlateKitchenObject>();
        if (!shakeTarget) shakeTarget = transform;

        foreach (var r in flashRenderers)
        {
            if (r && !_orig.ContainsKey(r))
                _orig[r] = GetColor(r);
        }
    }

    void OnEnable()
    {
        plate.OnInvalidIngredientAttempt += HandleInvalid;
    }

    void OnDisable()
    {
        plate.OnInvalidIngredientAttempt -= HandleInvalid;
        if (shakeCo != null) StopCoroutine(shakeCo);
        if (flashCo != null) StopCoroutine(flashCo);
        RestoreColors();
    }

    private void HandleInvalid(IngredientFailReason reason, KitchenObjectSO so)
    {
        if (!isActiveAndEnabled) return;
        if (Time.unscaledTime - _lastTime < minInterval) return; // debounce
        _lastTime = Time.unscaledTime;

        if (shakeCo != null) StopCoroutine(shakeCo);
        shakeCo = StartCoroutine(Shake());

        if (flashRenderers.Count > 0)
        {
            if (flashCo != null) StopCoroutine(flashCo);
            flashCo = StartCoroutine(Flash());
        }

        if (invalidSfx)
            AudioSource.PlayClipAtPoint(invalidSfx, transform.position);
    }

    private IEnumerator Shake()
    {
        var orig = shakeTarget.localPosition;
        float t = 0f;
        while (t < shakeDuration)
        {
            shakeTarget.localPosition = orig + Random.insideUnitSphere * shakeStrength;
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        shakeTarget.localPosition = orig;
    }

    private IEnumerator Flash()
    {
        foreach (var r in flashRenderers)
            SetColor(r, flashColor);
        yield return new WaitForSecondsRealtime(flashTime);
        RestoreColors();
    }

    private void RestoreColors()
    {
        foreach (var kv in _orig)
            if (kv.Key) SetColor(kv.Key, kv.Value);
    }

    private static Color GetColor(Renderer r)
    {
        if (!r) return Color.white;
        return r.material.HasProperty("_Color") ? r.material.color : Color.white;
    }

    private static void SetColor(Renderer r, Color c)
    {
        if (!r) return;
        if (r.material.HasProperty("_Color")) r.material.color = c;
    }
}
