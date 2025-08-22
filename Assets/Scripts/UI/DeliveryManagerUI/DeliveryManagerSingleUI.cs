using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Collections.AllocatorManager;

public class DeliveryManagerSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI recipeNameText;
    [SerializeField] private Transform iconContainer;
    [SerializeField] private Transform iconTemplate;
    [SerializeField] private Transform clockHandleRed;
    [SerializeField] private TextMeshProUGUI remainingTimeText;
    private OrderTicket ticket;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private bool isShaking = false;

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
        remainingTimeText.text = ((int)orderTicket.RemainingTime).ToString() ;
        // Rotate the clock hand each frame based on the time passed (1 second = full tick)
        float rotationAmount = 360f / 60f * Time.deltaTime;
        clockHandleRed.Rotate(0f, 0f, rotationAmount);
    }


    // Handle expiration (e.g., set expired UI state)
    private void HandleExpiredOrder(OrderTicket orderTicket)
    {       

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
