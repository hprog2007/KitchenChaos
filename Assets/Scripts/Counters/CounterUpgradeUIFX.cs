using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// Add this to your counter GameObject (or its parent).
/// Works on any Transform; no text/UI required.
/// - Pop/rotate/shake the target
/// - Flash material color/emission on selected renderers
/// - Spawn a local-space radial spark burst parented to the target
[ExecuteAlways]
public class CounterUpgradeFXObject : MonoBehaviour
{
    // -------------------- Configuration --------------------

    [Header("Target")]
    [Tooltip("The object to animate. Defaults to this GameObject's transform.")]
    public Transform animationTarget;

    [Tooltip("Renderers whose color/emission will flash during the upgrade effect.")]
    public List<Renderer> renderersToFlash = new List<Renderer>();

    [Header("Counter (for testing)")]
    [Tooltip("Logical counter value (not required for effect).")]
    public int counterValue = 0;

    [Header("Pop / Rotate / Shake")]
    [Tooltip("Peak scale multiplier during the pop.")]
    public float popScaleMultiplier = 1.35f;

    [Tooltip("Degrees to rotate around Z at pop peak (good for 2D/top-down).")]
    public float popRotateZDegrees = 14f;

    [Tooltip("Time (seconds) to reach the pop peak (ease-out).")]
    public float popRiseDuration = 0.12f;

    [Tooltip("Time (seconds) to settle back to normal (ease).")]
    public float settleDuration = 0.18f;

    [Tooltip("Small random shake distance (world/local units) during pop rise.")]
    public float shakeMagnitudeUnits = 0.05f;

    [Header("Flash (Material)")]
    [Tooltip("Highlight color during the flash.")]
    public Color flashColor = new Color(1f, 0.85f, 0.3f, 1f);

    [Tooltip("Duration for the flash to fade back to the original look.")]
    public float flashFadeDuration = 0.25f;

    [Tooltip("Also push emission color if available on the shader.")]
    public bool affectEmission = true;

    [Tooltip("How intense the emission starts (fades to 0 over flash time).")]
    public float emissionBoost = 2.0f;

    [Header("Spark Burst (Child Objects)")]
    [Tooltip("How many sparks to spawn.")]
    public int sparkCount = 12;

    [Tooltip("How far sparks travel (local units).")]
    public float sparkTravelRadius = 0.8f;

    [Tooltip("Lifetime of each spark.")]
    public float sparkLifetimeSeconds = 0.45f;

    [Tooltip("Random local scale range for sparks.")]
    public Vector2 sparkScaleRange = new Vector2(0.06f, 0.12f);

    // -------------------- Shader Property IDs --------------------

    static readonly int PROP_COLOR = Shader.PropertyToID("_Color");             // Built-in/Standard
    static readonly int PROP_BASECOLOR = Shader.PropertyToID("_BaseColor");     // URP/HDRP Lit
    static readonly int PROP_EMISSION = Shader.PropertyToID("_EmissionColor");

    // -------------------- Internal State --------------------

    private bool isAnimating;
    private float elapsedPopRise;
    private float elapsedSettle;
    private float elapsedFlash;

    private Vector3 baseLocalScale;
    private Quaternion baseLocalRotation;
    private Vector3 baseLocalPosition;

    private struct Spark
    {
        public Transform transformRef;                 // Spark object
        public Vector3 startLocalPosition;
        public Vector3 destinationLocalPosition;
        public float lifetimeSeconds;                  // Total lifetime
        public float elapsedTime;                      // Timer
        public float startLocalScaleValue;
        public float endLocalScaleValue;
        public float spinDegreesPerSecond;
        public MaterialPropertyBlock materialPropertyBlock;
        public Renderer rendererRef;
    }

    private readonly List<Spark> activeSparks = new List<Spark>();

    // -------------------- Unity Lifecycle --------------------

    private void Reset()
    {
        animationTarget = transform;

        // Try to auto-collect a renderer on this object for flashing
        var firstRenderer = GetComponentInChildren<Renderer>();
        if (firstRenderer)
        {
            renderersToFlash = new List<Renderer> { firstRenderer };
        }
    }

    private void OnEnable()
    {
        if (!animationTarget) animationTarget = transform;

#if UNITY_EDITOR
        // Drive animations in the Scene view too (Edit Mode)
        EditorApplication.update -= EditorUpdateTick;
        EditorApplication.update += EditorUpdateTick;
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        EditorApplication.update -= EditorUpdateTick;
#endif
    }

    private void Update()
    {
        // Play Mode uses regular Update with unscaled time
        if (Application.isPlaying)
        {
            float deltaTime = Time.unscaledDeltaTime;
            TickAnimation(deltaTime);
        }
    }

#if UNITY_EDITOR
    // Runs in Edit Mode to animate in the Scene view
    private double _lastEditorTime;
    private void EditorUpdateTick()
    {
        if (Application.isPlaying) return;

        double now = EditorApplication.timeSinceStartup;
        if (_lastEditorTime == 0) _lastEditorTime = now;
        float deltaTime = (float)(now - _lastEditorTime);
        _lastEditorTime = now;

        if (isAnimating || activeSparks.Count > 0)
        {
            TickAnimation(Mathf.Min(deltaTime, 0.05f));
            SceneView.RepaintAll();
        }
    }
#endif

    // -------------------- Public API --------------------

    [ContextMenu("Test Upgrade")]
    public void UpgradeAnimation()
    {
        counterValue++;
        StartAnimation();
        SpawnSparkBurst();
    }

    [ContextMenu("Flash Only")]
    public void FlashOnly()
    {
        elapsedFlash = 0f;
    }

    // -------------------- Animation Core --------------------

    private void StartAnimation()
    {
        isAnimating = true;
        elapsedPopRise = 0f;
        elapsedSettle = 0f;
        elapsedFlash = 0f;

        baseLocalScale = animationTarget.localScale;
        baseLocalRotation = animationTarget.localRotation;
        baseLocalPosition = animationTarget.localPosition;
    }

    private void TickAnimation(float deltaTime)
    {
        // --- Pop / Rotate / Shake ---
        if (isAnimating)
        {
            if (elapsedPopRise < popRiseDuration)
            {
                // Rise phase
                elapsedPopRise += deltaTime;
                float normalized = Mathf.Clamp01(elapsedPopRise / popRiseDuration);
                float easeOut = 1f - Mathf.Pow(1f - normalized, 3f);

                animationTarget.localScale = Vector3.LerpUnclamped(baseLocalScale, baseLocalScale * popScaleMultiplier, easeOut);
                animationTarget.localRotation = Quaternion.Euler(0, 0, Mathf.LerpUnclamped(0, popRotateZDegrees, easeOut)) * baseLocalRotation;

                // Diminishing shake
                Vector3 shakeOffset = (Vector3)(Random.insideUnitCircle * shakeMagnitudeUnits * (1f - normalized));
                animationTarget.localPosition = baseLocalPosition + shakeOffset;
            }
            else
            {
                // Settle phase
                if (elapsedSettle < settleDuration)
                {
                    elapsedSettle += deltaTime;
                    float normalized = Mathf.Clamp01(elapsedSettle / settleDuration);
                    float ease = 1f - Mathf.Pow(1f - normalized, 2f);

                    float slightOvershoot = Mathf.Sin(normalized * Mathf.PI) * 0.03f;

                    animationTarget.localScale =
                        Vector3.LerpUnclamped(animationTarget.localScale, baseLocalScale, ease) + Vector3.one * slightOvershoot;

                    animationTarget.localRotation = Quaternion.Slerp(animationTarget.localRotation, baseLocalRotation, ease);
                    animationTarget.localPosition = Vector3.LerpUnclamped(animationTarget.localPosition, baseLocalPosition, ease);
                }
                else
                {
                    animationTarget.localScale = baseLocalScale;
                    animationTarget.localRotation = baseLocalRotation;
                    animationTarget.localPosition = baseLocalPosition;
                    isAnimating = false;
                }
            }
        }

        // --- Flashing on renderers ---
        if (elapsedFlash < flashFadeDuration)
        {
            elapsedFlash += deltaTime;
            float t = Mathf.Clamp01(elapsedFlash / flashFadeDuration);

            foreach (var renderer in renderersToFlash)
            {
                if (!renderer) continue;

                // Determine a reasonable base color from the material (if present)
                Color baseColor = Color.white;
                var sharedMat = renderer.sharedMaterial;
                if (sharedMat)
                {
                    if (sharedMat.HasProperty(PROP_COLOR)) baseColor = sharedMat.GetColor(PROP_COLOR);
                    else if (sharedMat.HasProperty(PROP_BASECOLOR)) baseColor = sharedMat.GetColor(PROP_BASECOLOR);
                }

                // Interpolate from flash to base over time
                Color lerpedColor = Color.Lerp(flashColor, baseColor, t);

                // MaterialPropertyBlock to avoid touching the material asset
                var mpb = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(mpb);

                // Try set both _Color and _BaseColor for compatibility
                mpb.SetColor(PROP_COLOR, lerpedColor);
                mpb.SetColor(PROP_BASECOLOR, lerpedColor);

                if (affectEmission)
                {
                    Color emissiveColor = flashColor * Mathf.Lerp(emissionBoost, 0f, t);
                    mpb.SetColor(PROP_EMISSION, emissiveColor);
                }

                renderer.SetPropertyBlock(mpb);
            }
        }

        // --- Sparks ---
        if (activeSparks.Count > 0)
        {
            for (int i = activeSparks.Count - 1; i >= 0; i--)
            {
                var spark = activeSparks[i];
                if (!spark.transformRef)
                {
                    activeSparks.RemoveAt(i);
                    continue;
                }

                spark.elapsedTime += deltaTime;
                float k = Mathf.Clamp01(spark.elapsedTime / spark.lifetimeSeconds);
                float easeOut = 1f - Mathf.Pow(1f - k, 3f);

                spark.transformRef.localPosition =
                    Vector3.LerpUnclamped(spark.startLocalPosition, spark.destinationLocalPosition, easeOut);

                float currentScale = Mathf.LerpUnclamped(spark.startLocalScaleValue, spark.endLocalScaleValue, k);
                spark.transformRef.localScale = Vector3.one * Mathf.Max(currentScale, 0.0001f);

                spark.transformRef.localRotation = Quaternion.Euler(0, 0, spark.spinDegreesPerSecond * k);

                // Fade alpha if shader supports _Color/_BaseColor via MPB
                if (spark.rendererRef)
                {
                    spark.rendererRef.GetPropertyBlock(spark.materialPropertyBlock);

                    // We reduce alpha channel regardless; shaders that ignore alpha will just not show the fade.
                    if (spark.materialPropertyBlock.GetVector(PROP_COLOR) is Vector4 colorVec1)
                    {
                        var c = (Color)colorVec1; c.a = 1f - k;
                        spark.materialPropertyBlock.SetColor(PROP_COLOR, c);
                    }
                    if (spark.materialPropertyBlock.GetVector(PROP_BASECOLOR) is Vector4 colorVec2)
                    {
                        var c2 = (Color)colorVec2; c2.a = 1f - k;
                        spark.materialPropertyBlock.SetColor(PROP_BASECOLOR, c2);
                    }

                    spark.rendererRef.SetPropertyBlock(spark.materialPropertyBlock);
                }

                if (k >= 1f)
                {
                    if (spark.transformRef) DestroyImmediate(spark.transformRef.gameObject);
                    activeSparks.RemoveAt(i);
                }
                else
                {
                    activeSparks[i] = spark;
                }
            }
        }
    }

    // -------------------- Sparks --------------------

    private void SpawnSparkBurst()
    {
        CleanupExistingSparks();

        for (int i = 0; i < sparkCount; i++)
        {
            // Use a primitive sphere so you don't need any assets; lightweight and visible in Scene
            GameObject sparkObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sparkObject.name = "UpgradeSpark";
            sparkObject.transform.SetParent(animationTarget, false);
            sparkObject.hideFlags = HideFlags.HideAndDontSave; // avoid saving to scene/prefab
            sparkObject.layer = gameObject.layer;

            // Remove collider
            var collider = sparkObject.GetComponent<Collider>();
            if (collider) DestroyImmediate(collider);

            // Random local scale
            float startScale = Random.Range(sparkScaleRange.x, sparkScaleRange.y);
            sparkObject.transform.localScale = Vector3.one * startScale;

            // Random direction and distance in local XY plane
            float angleRadians = Random.value * Mathf.PI * 2f;
            float distance = Random.Range(sparkTravelRadius * 0.5f, sparkTravelRadius);
            Vector3 destinationLocal = new Vector3(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians), 0f) * distance;

            // Color tint via MPB (random hue)
            var renderer = sparkObject.GetComponent<Renderer>();
            var mpb = new MaterialPropertyBlock();
            Color randomHueColor = Color.HSVToRGB(Random.value, 0.8f, 1f);
            mpb.SetColor(PROP_COLOR, randomHueColor);
            mpb.SetColor(PROP_BASECOLOR, randomHueColor);
            renderer.SetPropertyBlock(mpb);

            activeSparks.Add(new Spark
            {
                transformRef = sparkObject.transform,
                startLocalPosition = Vector3.zero,
                destinationLocalPosition = destinationLocal,
                lifetimeSeconds = sparkLifetimeSeconds,
                elapsedTime = 0f,
                startLocalScaleValue = startScale,
                endLocalScaleValue = startScale * 0.3f,
                spinDegreesPerSecond = Random.Range(180f, 540f),
                materialPropertyBlock = mpb,
                rendererRef = renderer
            });
        }
    }

    private void CleanupExistingSparks()
    {
        for (int i = 0; i < activeSparks.Count; i++)
        {
            if (activeSparks[i].transformRef)
                DestroyImmediate(activeSparks[i].transformRef.gameObject);
        }
        activeSparks.Clear();
    }
}

// -------------------- Custom Inspector --------------------
#if UNITY_EDITOR
[CustomEditor(typeof(CounterUpgradeFXObject))]
public class CounterUpgradeFXObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var fx = (CounterUpgradeFXObject)target;

        GUILayout.Space(8);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Test Upgrade", GUILayout.Height(28)))
            {
                fx.UpgradeAnimation();
            }
            if (GUILayout.Button("Flash Only", GUILayout.Height(28)))
            {
                fx.FlashOnly();
            }
        }

        EditorGUILayout.HelpBox(
            "Click 'Test Upgrade' to preview pop/rotate/shake + spark burst.\n" +
            "Works in the Scene view without entering Play Mode.",
            MessageType.Info
        );
    }
}
#endif
