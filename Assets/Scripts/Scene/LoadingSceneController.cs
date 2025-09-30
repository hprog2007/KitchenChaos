// LoadingSceneController.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LoadingSceneController : MonoBehaviour
{
    [Header("Overlay (BLACK ONLY)")]
    public CanvasGroup fadeGroup;        // <-- put this on a black Image object only
    public Image blackImage;             // same object as fadeGroup, optional (raycast target control)

    [Header("UI")]
    public Image revealImage; 
    public Slider progressBar;
    public TextMeshProUGUI percentText;
    public TextMeshProUGUI tipText;

    [Header("Timing")]
    public float fadeIn = 0.25f;

    [Header("Tips")]
    [TextArea]
    public string[] tips = {
        "Optimize your layout in Shop Mode.",
        "Upgrade helpers to speed up deliveries.",
        "Chain actions to reduce walking time."
    };
    public float tipEvery = 3.5f;

    float _targetProgress;
    float _lastTipUnscaled;

    void Awake()
    {
        if (fadeGroup)
        {
            fadeGroup.alpha = 1f;            // start fully black
            fadeGroup.blocksRaycasts = true; // block during first frame
        }
        if (blackImage) blackImage.raycastTarget = true;
        if (progressBar) progressBar.value = 0f;
    }

    IEnumerator Start()
    {
        // show one tip immediately
        if (tipText && tips.Length > 0) tipText.text = tips[Random.Range(0, tips.Length)];

        // let the UI render once so you don't see a blank frame
        yield return null;

        // fade the BLACK overlay only; UI remains visible behind it
        if (fadeGroup) yield return Fade(fadeGroup, 0f, fadeIn);

        // after fade is done, stop the overlay from eating input
        if (fadeGroup) fadeGroup.blocksRaycasts = false;
        if (blackImage) blackImage.raycastTarget = false;

        // start async load now; progress will be faked by the service
        SceneTransitionService.Instance.BeginAsyncLoad(OnProgress);
    }

    void Update()
    {
        if (progressBar)
        {
            // Smoothly chase the target (10s window will make it move gradually)
            progressBar.value = Mathf.MoveTowards(progressBar.value, _targetProgress, 1f * Time.unscaledDeltaTime);
            revealImage.fillAmount = _targetProgress;            
        }

        if (percentText)
        {
            float shown = progressBar ? progressBar.value : _targetProgress;
            percentText.text = Mathf.RoundToInt(shown * 100f) + "%";
        }

        if (tipText && tips.Length > 1 && (Time.unscaledTime - _lastTipUnscaled) > tipEvery)
        {
            _lastTipUnscaled = Time.unscaledTime;
            tipText.text = tips[Random.Range(0, tips.Length)];
        }
    }

    void OnProgress(float p) => _targetProgress = Mathf.Clamp01(p);

    IEnumerator Fade(CanvasGroup cg, float target, float time)
    {
        if (!cg || time <= 0f) { cg.alpha = target; yield break; }
        float start = cg.alpha, t = 0f;
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(start, target, t / time);
            yield return null;
        }
        cg.alpha = target;
    }
}
