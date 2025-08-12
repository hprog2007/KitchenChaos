using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryManagerSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI recipeNameText;
    [SerializeField] private Transform iconContainer;
    [SerializeField] private Transform iconTemplate;
    [SerializeField] private TextMeshProUGUI timeRemainingText; // New: Text to show the remaining time
    [SerializeField] private Image timerRing; // New: Image for a radial timer
    private OrderTicket ticket;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private bool isShaking = false;

    [Header("Timer Effects")]
    [SerializeField] private Gradient timerColorGradient;  // Green -> Yellow -> Red
    [SerializeField] private float lowTimeThreshold = 10f;  // When to start pulsing/shaking
    [SerializeField] private float shakeMagnitude = 6f;  // Shake strength
    [SerializeField] private float shakeSpeed = 40f;  // Speed of shaking effect
    [SerializeField] private float pulseSpeed = 4f;  // Pulse speed for scaling effect
    [SerializeField] private float pulseScale = 1.1f;  // Scaling factor for pulsing

    public void Awake()
    {
        iconTemplate.gameObject.SetActive(false);
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
    }

    public void Bind(OrderTicket orderTicket)
    {
        ticket = orderTicket;
        SetRecipeSO(orderTicket.Recipe);

        // Subscribe to ticket events
        ticket.OnTick += OnTick;
        ticket.OnExpired += HandleExpiredOrder;

        // Set initial values
        OnTick(ticket);  // Update immediately with the current time values
    }


    // Method to update the timer visuals (called on every tick)
    public void OnTick(OrderTicket orderTicket)
    {
        // Update the time text
        if (timeRemainingText != null)
        {
            timeRemainingText.text = Mathf.CeilToInt(orderTicket.Remaining).ToString();
        }

        // Update the radial timer
        if (timerRing != null)
        {
            timerRing.fillAmount = orderTicket.Remaining / orderTicket.Duration;
            timerRing.color = timerColorGradient.Evaluate(orderTicket.Remaining / orderTicket.Duration);  // Color shift
        }

        // Pulse and shake near the end
        if (orderTicket.Remaining <= lowTimeThreshold)
        {
            if (!isShaking)
            {
                isShaking = true;
            }

            // Pulsing effect
            float pulse = 1f + (pulseScale - 1f) * (0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed));
            transform.localScale = originalScale * pulse;

            // Shaking effect
            Vector3 shakeOffset = new Vector3(
                Mathf.Sin(Time.time * shakeSpeed),
                Mathf.Cos(Time.time * shakeSpeed * 1.2f),
                0f
            ) * shakeMagnitude;

            transform.localPosition = originalPosition + shakeOffset;
        }
        else
        {
            // Reset scaling and position if we're not near the end
            if (isShaking)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, originalScale, 10f * Time.deltaTime);
                transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, 10f * Time.deltaTime);
                isShaking = false;
            }
        }
    }


    // Handle expiration (e.g., set expired UI state)
    private void HandleExpiredOrder(OrderTicket orderTicket)
    {
        // You can change the UI to indicate this order has expired, such as:
        if (timerRing != null)
        {
            timerRing.color = Color.red; // Example: Set to red when expired
        }
        if (timeRemainingText != null)
        {
            timeRemainingText.text = "Expired"; // Set text to "Expired" when the timer ends
        }

        // Handle further expired logic, such as a failure message or removing the order from the list.
    }

    // Clears the subscription when removed from the UI to avoid memory leaks
    private void OnDestroy()
    {
        if (ticket != null)
        {
            ticket.OnTick -= OnTick;
            ticket.OnExpired -= HandleExpiredOrder;
        }
    }

    // Set the recipe UI (unchanged from your original method)
    public void SetRecipeSO(RecipeSO recipeSO)
    {
        recipeNameText.text = recipeSO.recipeName;

        // Clear any existing icons
        foreach (Transform child in iconContainer)
        {
            if (child == iconTemplate) continue;
            Destroy(child.gameObject);
        }

        // Add the icons for each ingredient
        foreach (KitchenObjectSO kitchenObjectSO in recipeSO.kitchenObjectSOList)
        {
            Transform iconTranform = Instantiate(iconTemplate, iconContainer);
            iconTranform.gameObject.SetActive(true);
            iconTranform.GetComponent<Image>().sprite = kitchenObjectSO.sprite;
        }
    }
}
