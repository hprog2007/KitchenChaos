// 7/14/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ShopUIManager : MonoBehaviour
{
    public static ShopUIManager Instance { get; private set; }

    [SerializeField] private Transform shopOverlayBackground;
    [SerializeField] private LayerMask counterLayerMask; // assign "Counter" layer in Inspector

    [Header("Shop Panel")]
    [SerializeField] private TextMeshProUGUI headerTitle;
    [SerializeField] private Transform shopCardsParent; //scrollview content   
    [SerializeField] private Transform shopCardPrefab;
    [SerializeField] private Transform itemsPanel;

    [Header("Cursors")]
    [SerializeField] private Texture2D normalCursor;
    [SerializeField] private Texture2D selectCursor;
    [SerializeField] private Vector2 normalHotspot = new Vector2(0, 0);
    [SerializeField] private Vector2 selectHotspot = new Vector2(6, 2);

    //****************************** Events
    public event Action<ShopCardUI> OnShopCardClicked;

    [Header("Shop Modes")]
    public UnityEvent OnBuyButtonClicked;
    public UnityEvent OnUpgradeButtonClicked;
    public UnityEvent OnHelpersButtonClicked;
    public UnityEvent OnCosmeticsButtonClicked;
    public UnityEvent OnCoinsButtonClicked;
    public UnityEvent OnOpenShop;

    // Cursor
    private enum CursorState { Normal, Select }
    private CursorState currentCursor;

    private void Awake()
    {
        ApplyCursor(CursorState.Normal);

        // Ensure there's only one instance of UIManager
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Optionally, make this object persist across scenes
        DontDestroyOnLoad(gameObject);        
    }

    private void Update()
    {
        if (!ShopManager.Instance.IsShopOpen()) 
            return;

        if (Input.GetMouseButtonDown(0)) // left click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, counterLayerMask))
            {
                if (hit.transform.TryGetComponent(out BaseCounter counter))
                {
                    Debug.Log("Clicked counter: " + counter.name);
                    // do your replacement/selection
                    CountersClick(counter);
                }
            }
        }
    }

    public void OpenShop()
    {
        ApplyCursor(CursorState.Select);

        ShopManager.Instance.SetToBuyMode();

        OnOpenShop?.Invoke();
    }

    public void CloseShop()
    {
        ApplyCursor(CursorState.Normal);

        ShopManager.Instance.SetToNoneMode();
    }

    private void CountersClick(BaseCounter baseCounter)
    {
        // Set selected counter
        Player.Instance.SetSelectedCounter(baseCounter);

        // set pointer position
        var gridCell = GridManager.Instance.GetCellFromWorldPosition(baseCounter.transform.position);
        AnimationManager.Instance.PlayCounterPointer(gridCell.coordinates.x, gridCell.coordinates.y);
    }

    // Clear shop panel cards
    private void ClearContent()
    {
        foreach (Transform child in shopCardsParent)
        {
            Destroy(child.gameObject);
        }
    }    

    private bool FillShopCardsPanel(ShopMode shopModeParam)
    {
        var currentShopCardList = ShopManager.Instance.GetShopCardsList(shopModeParam);
        if (currentShopCardList == null)
        {
            Debug.Log("shopAvailableCardList for "+ shopModeParam.ToString() + " isn't defined! Or all upgrade levels are done");
            return false;
        }

        //Instaniate cards
        foreach (ShopSelectCardSO cardSO in currentShopCardList.shopCardList)
        {
            Transform shopCard = Instantiate(shopCardPrefab, shopCardsParent);
            ShopCardUI shopCardUI = shopCard.GetComponent<ShopCardUI>();

            if (shopModeParam == ShopMode.Upgrades)
            {
                shopCardUI.SetupUpgrade(cardSO, cardSO.counterType);
            } else
            {
                shopCardUI.SetupNew(cardSO, shopModeParam);
            }

        }
        return true;
    }

    public void BuyButtonClick()
    {
        headerTitle.text = "Buy";
        ClearContent();
        if (!FillShopCardsPanel(ShopMode.Buy))
        {
            return;
        }

        OnBuyButtonClicked?.Invoke();    
        
    }

    public void UpgradeButtonClick()
    {
        headerTitle.text = "Upgrades";
        ClearContent();
        if (!FillShopCardsPanel(ShopMode.Upgrades))
        {
            return;
        }

        OnUpgradeButtonClicked?.Invoke();
    }

    public void HelpersButtonClick()
    {
        headerTitle.text = "Helpers";
        ClearContent();
        if (!FillShopCardsPanel(ShopMode.Helpers))
        {
            return;
        }

        OnHelpersButtonClicked?.Invoke();
    }

    public void CosmeticsButtonClicked()
    {
        headerTitle.text = "Cosmetics";
        ClearContent();
        if (!FillShopCardsPanel(ShopMode.Cosmetics))
        {
            return;
        }

        OnCosmeticsButtonClicked?.Invoke();
    }

    public void CoinsButtonClick()
    {
        headerTitle.text = "Coins";
        ClearContent();
        if (!FillShopCardsPanel(ShopMode.Coins))
        {
            return;
        }

        OnCoinsButtonClicked?.Invoke();
    }

    public void ShopCardClick(ShopCardUI card)
    {
        OnShopCardClicked?.Invoke(card);
    }

    //When player buys a counter he goes to Placement Mode
    public void EnterPlacementMode(GameObject selectedItem)
    {        

        shopOverlayBackground.gameObject.SetActive(false);

       itemsPanel.gameObject.SetActive(false);

        ScreenMessagesUI.Instance.ShowMessage("Select a counter to replace. \\n click on the selected counter to confirm replacement. ", 3, false, 40f);

        // Set selected counter
        var gridCell = GridManager.Instance.GetCellAt(1, 6);
        var d = gridCell.placedObject.GetComponent<BaseCounter>();
        Player.Instance.SetSelectedCounter(d);

        AnimationManager.Instance.PlayCounterPointer(1, 6);

        //PlacementManager.Instance.StartPlacement(selectedItem);
    }

    private void ApplyCursor(CursorState state)
    {
        currentCursor = state;
        switch (state)
        {
            case CursorState.Normal:
                Cursor.SetCursor(normalCursor, normalHotspot, CursorMode.Auto);
                break;
            case CursorState.Select:
                Cursor.SetCursor(selectCursor, selectHotspot, CursorMode.Auto);
                break;
        }
        Cursor.lockState = CursorLockMode.None; // ensure visible & free
        Cursor.visible = true;
    }
}
