using System;
using System.Collections.Generic;
using DG.Tweening;
using EnhancedOnScreenControls;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

[Serializable]
public struct RectStruct {    
    public Vector3 AnchoredPosition;
    public Vector3 Rotation;
    public Vector2 SizeDelta;
    public string TutorialText;
    public Vector2 TutorialTextPos;
}

public class MyTutorial : MonoBehaviour
{
    [SerializeField] private GameObject mobileHUD;
    public Animator animator;
    public CanvasGroup tutorialCanvasGroup;
    //public RectTransform reverseMaskRect;
    //public Image CutOutImage;
    public TextMeshProUGUI textUGUI;
    public GameObject skipButton;

    private void OnEnable()
    {
        ToggleInteractionEnabled(false);

        GameInput.Instance.ClickDownEvent += GameInput_ClickDownEvent;
        Time.timeScale = 1f;
        animator.Play("L1_Containers", 0, 0f);
        animator.Update(0f);
    }

    private void OnDisable()
    {
        GameInput.Instance.ClickDownEvent -= GameInput_ClickDownEvent;
    }

    private void ToggleInteractionEnabled(bool enableParam)
    {
        skipButton.SetActive(enableParam);

        tutorialCanvasGroup.blocksRaycasts = enableParam;
        tutorialCanvasGroup.interactable = enableParam;

        if (enableParam)
        {
            GameInput.Instance.EnablePlayerInput();
        }
        else
        {
            GameInput.Instance.DisablePlayerInput();
        }
            

        //disable action buttons click event
        var events = mobileHUD.GetComponentsInChildren<EventTrigger>();
        foreach (EventTrigger e in events)
        {
            e.enabled = enableParam;
        }

        //disable buttons
        var screenButtons = mobileHUD.GetComponentsInChildren<OnScreenButton>();
        foreach (OnScreenButton screenButton in screenButtons)
        {
            screenButton.enabled = enableParam;
        }

        //disable joystick
        var joy = mobileHUD.GetComponentInChildren<EnhancedOnScreenStick>();
        joy.enabled = enableParam;
    }

    private void GameInput_ClickDownEvent(Vector2 clickPos)
    {
        
        Debug.Log("Click");

        if (animator.IsInTransition(0))
            return;

        animator.SetTrigger("NextState");
    }

    public void EnableSkipTutorial()
    {
        ToggleInteractionEnabled(true);
    }

    public void SkipTutorial()
    {
        //mobileHUD.SetActive(true);
        animator.StopPlayback();
        gameObject.SetActive(false);        
    }


    /*
    public void MoveAndResize(Vector2 targetPos, float targetWidth, float targetHeight, float duration = 0.4f)
    {
        var seq = DOTween.Sequence();

        // move in UI space
        seq.Join(reverseMaskRect.DOAnchorPos(targetPos, duration));

        // change width & height (sizeDelta is width/height when not stretched)
        seq.Join(reverseMaskRect.DOSizeDelta(new Vector2(targetWidth, targetHeight), duration));

        // optional polish
        seq.SetEase(Ease.InOutQuad);
        // seq.SetUpdate(true); // run while game is paused / unscaled time
    }
    */
    public void SetTutorialText(string s)
    {
        textUGUI.text = s;
    }
}
