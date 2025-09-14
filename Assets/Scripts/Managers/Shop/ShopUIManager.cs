// 7/14/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
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
    [SerializeField] private Transform shopTopPanel;
    [SerializeField] private Transform shopItemsPanel;

    [Header("Cursors")]
    [SerializeField] private Texture2D normalCursor;
    [SerializeField] private Texture2D selectCursor;
    [SerializeField] private Texture2D clickedCursor;
    [SerializeField] private Vector2 normalHotspot = new Vector2(0, 0);
    [SerializeField] private Vector2 selectHotspot = new Vector2(6, 2);
    
    
    [SerializeField] private float confirmingTimerMax = 3f;
    [SerializeField] private Transform circleProgressBarUI;

    //****************************** Events
    public event Action<ShopCardUI> OnShopCardClicked;
    public event Action<ShopCardUI, Transform> OnBuyConfirmed;

    [Header("Shop Modes")]
    public UnityEvent OnBuyButtonClicked;
    public UnityEvent OnUpgradeButtonClicked;
    public UnityEvent OnHelpersButtonClicked;
    public UnityEvent OnCosmeticsButtonClicked;
    public UnityEvent OnCoinsButtonClicked;
    public UnityEvent OnOpenShop;

    // Cursor
    private enum CursorState { Normal, Hand, Grabbing }
    private CursorState currentCursor;
    
    private bool replacementMode;
    private BaseCounter selectedCounter;
    private BaseCounter newCounter; //counter player bought
    private float destroyWaitTimer = 3f;
    private float confirmingTimer;
    
    private bool confirmingState;
    private ShopCardUI currentShopCardUI;
    private bool replacementCanceled;
    

    private void Awake()
    {        
        // Ensure there's only one instance of UIManager
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Optionally, make this object persist across scenes
        //DontDestroyOnLoad(gameObject);

        //************  Init
        ApplyCursor(CursorState.Normal);
        replacementMode = false;
        confirmingState = false;

        //****** event bind
        
    }

    private void Start()
    {
        StartCoroutine(WaitAndConsume()); //skip one frame then consume StartGameParams
    }

    private IEnumerator WaitAndConsume()
    {
        // Option 1: wait a frame so Awake() on SceneTransitionService runs
        yield return null;

        // Option 2: if needed, poll until it's ready
        while (SceneTransitionService.Instance == null)
            yield return null;

        var p = SceneTransitionService.Instance.Consume<StartGameParams>();
        if (p != null && p.startInShopMode)
        {
            OpenShop();
        }
    }

    private void Update()
    {
        if (!ShopManager.Instance.IsShopOpen()) 
            return;

        if (Input.GetMouseButtonDown(0)) // left click
        {
            ScreenClick();
        }
        else if (Input.GetMouseButtonUp(0)) {
            ApplyCursor(CursorState.Hand);
        }

        if (confirmingState)
        {
            confirmingTimer -= Time.deltaTime;
            if (confirmingTimer <= 0 && !replacementCanceled)
            {
                confirmingState = false;
                
                //replace counters by animation
                StartCoroutine(ReplaceCounterRoutine());
                
            }
        }
    }

    private IEnumerator ReplaceCounterRoutine()
    {
        var selCounterTransofrm = selectedCounter.transform;

        //instantiate new counter
        Transform newCounterTransform = Instantiate(newCounter.transform, selCounterTransofrm.parent);
        newCounterTransform.transform.position = selCounterTransofrm.transform.position;
        newCounterTransform.transform.rotation = selCounterTransofrm.transform.rotation;
        newCounterTransform.localScale = Vector3.zero;

        //Replace newCounter in cell grid
        GridManager.Instance.FillGridCellByCounter(newCounterTransform.GetComponent<BaseCounter>());

        // run animation and wait for it to finish
        yield return StartCoroutine(AnimationManager.Instance.PlayMagicReplaceByRotation(selCounterTransofrm, newCounterTransform));

        Destroy(selectedCounter.gameObject, destroyWaitTimer);

        ShowItemsPanel();

        OnBuyConfirmed?.Invoke(currentShopCardUI, selCounterTransofrm);
    }

    private void DisableCounterPointer()
    {
        var counterPointer = FindAnyObjectByType<CounterPointer>(FindObjectsInactive.Exclude);
        if (counterPointer != null)
        {
            counterPointer.gameObject.SetActive(false);
        }
    }

    private Transform EnableCounterPointer()
    {
        var counterPointer = FindAnyObjectByType<CounterPointer>(FindObjectsInactive.Include);
        if (counterPointer != null)
        {
            counterPointer.gameObject.SetActive(true);
            return counterPointer.transform;
        }
        return null;
    }

    private void ScreenClick()
    {
        ApplyCursor(CursorState.Grabbing);

        //Detect click on objects using Raycast
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

    public void OpenShop()
    {
        ApplyCursor(CursorState.Hand);

        ShopManager.Instance.SetToBuyMode();

        OnOpenShop?.Invoke();
    }

    public void CloseShop()
    {
        ApplyCursor(CursorState.Normal);

        ShopManager.Instance.SetToNoneMode();
    }

    private void CountersClick(BaseCounter counterParam)
    {
        if (replacementMode)
        { 
            selectedCounter = counterParam;
            if (IsCounterSelected(counterParam))
            {
                confirmingTimer = confirmingTimerMax;
                confirmingState = true;
                replacementCanceled = false;
                DisableCounterPointer();
                circleProgressBarUI.gameObject.SetActive(true);
                CircleProgressBarUI.Instance.Setup(counterParam.transform.position, confirmingTimerMax);
            }
            else
            {
                confirmingState = false;
                // Set selected counter
                Player.Instance.SetSelectedCounter(counterParam);

                // set pointer position
                var gridCell = GridManager.Instance.GetCellFromWorldPosition(counterParam.transform.position);
                var t = EnableCounterPointer();
                AnimationManager.Instance.PlayCounterPointer(t, gridCell.coordinates.x, gridCell.coordinates.y);
            }
        }
        
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
    public void EnterReplacementMode(ShopCardUI cardParam, GameObject newCounterPrefab)
    {        
        replacementMode = true;
        currentShopCardUI = cardParam;
        newCounter = newCounterPrefab.GetComponent<BaseCounter>();
        
        HideShopPanels();
        
        ScreenMessagesUI.Instance.ShowMessage("Select a counter to replace. \\n " +
            "click on the selected counter to confirm replacement. ", 3, false, 40f);

        // Set selected counter
        var gridCell = GridManager.Instance.GetCellAt(1, 6);
        selectedCounter = gridCell.placedObject.GetComponent<BaseCounter>();
        Player.Instance.SetSelectedCounter(selectedCounter);

        var counterPointerTransform = EnableCounterPointer();
        AnimationManager.Instance.PlayCounterPointer(counterPointerTransform, 1, 6);       

        //PlacementManager.Instance.StartPlacement(selectedItem);
    }

    private void HideShopPanels()
    {
        shopOverlayBackground.gameObject.SetActive(false); // hide ovrlay
        shopTopPanel.gameObject.SetActive(false);
        shopItemsPanel.gameObject.SetActive(false); // hide shop items panel
    }

    private void ShowTopPanel()
    {
        shopOverlayBackground.gameObject.SetActive(true); // hide ovrlay
        shopTopPanel.gameObject.SetActive(true);
        shopItemsPanel.gameObject.SetActive(false); // hide shop items panel
    }

    private void ShowItemsPanel()
    {
        shopOverlayBackground.gameObject.SetActive(true); // hide ovrlay
        shopTopPanel.gameObject.SetActive(false);
        shopItemsPanel.gameObject.SetActive(true); // hide shop items panel
    }

    public bool IsReplacementModeActive()
    {
        return replacementMode;
    }

    public bool IsCounterSelected(BaseCounter counterParam)
    {
        return counterParam == Player.Instance.GetSelectedCounter();
    }

    private void ApplyCursor(CursorState state)
    {
        currentCursor = state;
        switch (state)
        {
            case CursorState.Normal:
                Cursor.SetCursor(normalCursor, normalHotspot, CursorMode.Auto);
                break;
            case CursorState.Hand:
                Cursor.SetCursor(selectCursor, selectHotspot, CursorMode.Auto);
                break;
            case CursorState.Grabbing:
                Cursor.SetCursor(clickedCursor, selectHotspot, CursorMode.Auto);
                break;
        }
        Cursor.lockState = CursorLockMode.None; // ensure visible & free
        Cursor.visible = true;
    }

    public void ShowLowBalanceMessage()
    {
        ScreenMessagesUI.Instance.ShowMessage("You don't have enough coins! You need to eighter buy coins or play more. ");
    }
}
