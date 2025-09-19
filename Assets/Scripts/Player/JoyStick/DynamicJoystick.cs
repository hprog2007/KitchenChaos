using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DynamicJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("UI References")]
    public RectTransform joystickBase;   // background circle
    public RectTransform joystickHandle; // handle circle
    public CanvasGroup canvasGroup;      // for fade in/out

    [Header("Settings")]
    public float radius = 100f;          // max distance for handle
    public float handleReturnSpeed = 12f;
    public float fadeSpeed = 10f;
    [Range(0f, 1f)] public float outputScale = 1f;
    public bool leftHalfOnly = true;     // restrict to left half of screen

    [HideInInspector] public Vector2 Output; // movement vector (-1..1)

    private bool isHeld = false;
    private int activePointerId = -1;

    void Awake()
    {
        if (canvasGroup)
            canvasGroup.alpha = 0f; // hidden at start
        if (joystickHandle)
            joystickHandle.anchoredPosition = Vector2.zero;
    }

    void Update()
    {
        // Return handle to center if released
        if (!isHeld && joystickHandle)
        {
            joystickHandle.anchoredPosition = Vector2.Lerp(
                joystickHandle.anchoredPosition,
                Vector2.zero,
                Time.deltaTime * handleReturnSpeed
            );
        }

        // Fade in/out
        if (canvasGroup)
        {
            float targetAlpha = isHeld ? 1f : 0f;
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (activePointerId != -1) return;

        // Restrict to left half of screen
        if (leftHalfOnly && eventData.position.x > Screen.width / 2f) return;

        activePointerId = eventData.pointerId;
        isHeld = true;

        // Move joystick base to finger position
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)transform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        joystickBase.anchoredPosition = localPoint;
        joystickHandle.anchoredPosition = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBase,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        // Clamp handle movement inside circle
        Vector2 clamped = Vector2.ClampMagnitude(localPoint, radius);
        joystickHandle.anchoredPosition = clamped;

        // Normalize for output
        Output = (clamped / radius) * outputScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId) return;

        isHeld = false;
        activePointerId = -1;
        Output = Vector2.zero;
    }
}
