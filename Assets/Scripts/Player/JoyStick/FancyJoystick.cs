using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;

[DisallowMultipleComponent]
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class FancyJoystick : OnScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler, ICanvasRaycastFilter
{
    // === Bind this to your Move action via <Gamepad>/leftStick ===
    [SerializeField] private string m_ControlPath = "<Gamepad>/leftStick";
    protected override string controlPathInternal
    {
        get => m_ControlPath;
        set => m_ControlPath = value;
    }

    [Header("References (assign in inspector)")]
    [Tooltip("Optional parent for visuals (not required).")]
    [SerializeField] RectTransform visualRoot;
    [SerializeField] RectTransform baseRect;
    [SerializeField] RectTransform handleRect;
    [SerializeField] Image baseImg;
    [SerializeField] Image glowImg;
    [SerializeField] Image handleImg;

    [Header("Behavior")]
    [Tooltip("If true, base pops to finger on press; else it stays fixed.")]
    [SerializeField] bool dynamicBase = true;
    [Tooltip("Joystick radius in pixels.")]
    [SerializeField] float radius = 120f;
    [Range(0f, 0.5f)]
    [SerializeField] float deadZone = 0.12f;
    [Tooltip("Global output scaler to tame speed (0.6–0.85 feels great).")]
    [Range(0.3f, 1.0f)]
    [SerializeField] float outputScale = 0.75f;
    [Tooltip("Eases the vector for steadier motion.")]
    [SerializeField] float smoothTime = 0.06f;
    [Tooltip("Visual return speed for the handle when released.")]
    [SerializeField] float handleReturnSpeed = 12f;
    [Tooltip("Fade speed for dynamic base show/hide.")]
    [SerializeField] float showHideFadeSpeed = 10f;
    [Tooltip("Slight pop on press.")]
    [SerializeField] float pressPopScale = 1.06f;
    [Tooltip("Balances diagonal strength so down-left etc. feel right.")]
    [SerializeField] bool correctDiagonals = true;

    [Header("Optional: Limit touch to left side so buttons work")]
    [SerializeField] bool limitRaycastRegion = true;
    [Tooltip("Normalized screen rect (0..1). Example: x=0,y=0,w=0.55,h=1 for left 55%.")]
    [SerializeField] Rect region01 = new Rect(0f, 0f, 0.55f, 1f);

    Canvas rootCanvas;
    Camera uiCamera;
    CanvasGroup cg;
    RectTransform selfRT;

    Vector2 output;               // smoothed
    Vector2 outputVel;            // SmoothDamp velocity
    Vector2 targetOutput;         // after deadzone & scaling
    Vector2 pointerLocal;         // pointer pos in local space
    int activePointerId = -99;
    bool isHeld = false;
    Vector2 baseStartAnchoredPos;

    void Reset()
    {
        dynamicBase = true;
        radius = 120f;
        deadZone = 0.12f;
        outputScale = 0.75f;
        smoothTime = 0.06f;
        handleReturnSpeed = 12f;
        showHideFadeSpeed = 10f;
        pressPopScale = 1.06f;
        correctDiagonals = true;
        limitRaycastRegion = true;
        region01 = new Rect(0f, 0f, 0.55f, 1f);
    }

    void Awake()
    {
        selfRT = (RectTransform)transform;
        cg = GetComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>();
        uiCamera = rootCanvas && rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay ? rootCanvas.worldCamera : null;

        // Start hidden if dynamic
        if (cg) cg.alpha = dynamicBase ? 0f : 1f;

        if (baseRect) baseStartAnchoredPos = baseRect.anchoredPosition;

        // Make sure images don't block raycasts unnecessarily
        var img = GetComponent<Image>();
        if (img) img.raycastTarget = true;
        if (baseImg) baseImg.raycastTarget = false;
        if (glowImg) glowImg.raycastTarget = false;
        if (handleImg) handleImg.raycastTarget = false;
    }


    void Start() => ForceCenterLayout();

    void ForceCenterLayout()
    {
        if (visualRoot)
        {
            visualRoot.anchorMin = visualRoot.anchorMax = new Vector2(0.5f, 0.5f);
            visualRoot.pivot = new Vector2(0.5f, 0.5f);
            visualRoot.anchoredPosition = Vector2.zero;
        }

        if (baseRect)
        {
            baseRect.anchorMin = baseRect.anchorMax = new Vector2(0.5f, 0.5f);
            baseRect.pivot = new Vector2(0.5f, 0.5f);
            // Only reset if fixed base
            if (!dynamicBase)
                baseRect.anchoredPosition = Vector2.zero;
        }

        if (handleRect)
        {
            handleRect.anchorMin = handleRect.anchorMax = new Vector2(0.5f, 0.5f);
            handleRect.pivot = new Vector2(0.5f, 0.5f);
            handleRect.anchoredPosition = Vector2.zero;
        }
    }


    void Update()
    {
        // Smooth towards target
        output = Vector2.SmoothDamp(output, targetOutput, ref outputVel, smoothTime);
        SendValueToControl(output);

        // Return handle to center when released
        if (!isHeld && handleRect)
        {
            handleRect.anchoredPosition = Vector2.Lerp(handleRect.anchoredPosition, Vector2.zero, Time.deltaTime * handleReturnSpeed);
        }

        // Fade show/hide for dynamic base
        if (dynamicBase && cg)
        {
            float targetAlpha = isHeld ? 1f : 0f;
            cg.alpha = Mathf.MoveTowards(cg.alpha, targetAlpha, Time.deltaTime * showHideFadeSpeed);
        }
    }
    /*
    public void OnPointerDown(PointerEventData eventData)
    {
        if (activePointerId != -99) return; // track only one finger
        activePointerId = eventData.pointerId;
        isHeld = true;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(selfRT, eventData.position, uiCamera, out localPoint);

        if (dynamicBase && baseRect)
        {
            baseRect.anchoredPosition = localPoint;
            if (handleRect) handleRect.anchoredPosition = Vector2.zero;
        }

        // Pop & glow
        if (visualRoot) visualRoot.localScale = Vector3.one * pressPopScale;
        if (glowImg) glowImg.enabled = true;

        UpdateDrag(eventData);
    }
    */

    public void OnPointerDown(PointerEventData eventData)
    {
        if (activePointerId != -99) return;
        if (!IsRaycastLocationValid(eventData.position, uiCamera)) return;

        activePointerId = eventData.pointerId;
        isHeld = true;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(selfRT, eventData.position, uiCamera, out localPoint);

        if (dynamicBase && baseRect)
        {
            baseRect.anchoredPosition = localPoint;
            if (handleRect) handleRect.anchoredPosition = Vector2.zero;
        }

        if (visualRoot) visualRoot.localScale = Vector3.one * pressPopScale;
        if (glowImg) glowImg.enabled = true;

        UpdateDrag(eventData);
    }



    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId) return;
        UpdateDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId) return;

        isHeld = false;
        activePointerId = -99;
        targetOutput = Vector2.zero; // smooth to zero

        // Reset visuals
        if (visualRoot) visualRoot.localScale = Vector3.one;
        if (glowImg) glowImg.enabled = false;

        // If fixed base mode, return base to original spot
        if (!dynamicBase && baseRect)
            baseRect.anchoredPosition = baseStartAnchoredPos;
    }

    void UpdateDrag(PointerEventData eventData)
    {
        if (!baseRect || !handleRect) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(selfRT, eventData.position, uiCamera, out pointerLocal);

        // The center is the base position (moved for dynamic base).
        Vector2 center = baseRect.anchoredPosition;
        Vector2 delta = pointerLocal - center;

        // Clamp the handle within a circle of radius
        Vector2 clamped = Vector2.ClampMagnitude(delta, radius);
        handleRect.anchoredPosition = clamped;

        // Normalize to -1..1 range
        Vector2 norm = clamped / Mathf.Max(radius, 1f);

        // Deadzone + diagonal correction
        float mag = norm.magnitude;
        Vector2 processed;
        if (mag < deadZone)
        {
            processed = Vector2.zero;
        }
        else
        {
            float remapped = Mathf.InverseLerp(deadZone, 1f, mag);
            processed = norm.normalized * remapped;
            if (correctDiagonals) processed = SquareToCircle(processed);
        }

        // Global scale to tame speed
        targetOutput = processed * outputScale;
    }

    static Vector2 SquareToCircle(Vector2 v)
    {
        // Equalizes diagonal strength; common virtual joystick mapping
        float x = v.x, y = v.y;
        float x2 = x * x, y2 = y * y;
        return new Vector2(
            x * Mathf.Sqrt(1f - 0.5f * y2),
            y * Mathf.Sqrt(1f - 0.5f * x2)
        );
    }

    // Limit where this object accepts raycasts (keeps right-side buttons clickable)
        //public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        //{
        //    if (!limitRaycastRegion) return true;
        //    float nx = sp.x / Screen.width;
        //    float ny = sp.y / Screen.height;
        //    return region01.Contains(new Vector2(nx, ny));
        //}

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        // Only allow touches in the left 50% of the screen
        float normalizedX = screenPoint.x / Screen.width;
        return normalizedX <= 0.5f;
    }


#if UNITY_EDITOR
    void OnValidate()
    {
        radius = Mathf.Max(20f, radius);
        deadZone = Mathf.Clamp01(deadZone);
        outputScale = Mathf.Clamp(outputScale, 0.3f, 1f);
        if (region01.width < 0f) region01.width = Mathf.Abs(region01.width);
        if (region01.height < 0f) region01.height = Mathf.Abs(region01.height);
    }
#endif
}
