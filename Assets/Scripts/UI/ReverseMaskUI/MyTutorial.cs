using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
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
    public CanvasGroup tutorialUI;
    public RectTransform reverseMaskRect;
    public Image CutOutImage;
    public TextMeshProUGUI textUGUI;

    private void Start()
    {
        //disable click while in tutorial
        //mobileHUD.SetActive(false);
        tutorialUI.blocksRaycasts = false;
        tutorialUI.interactable = false;
        GameInput.Instance.DisablePlayerInput();
        
        GameInput.Instance.ClickDownEvent += GameInput_ClickDownEvent;
        animator.Play("L1_Containers");
    }

    private void OnDisable()
    {
        GameInput.Instance.ClickDownEvent -= GameInput_ClickDownEvent;
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
        tutorialUI.blocksRaycasts = true;
        tutorialUI.interactable = true;
        GameInput.Instance.EnablePlayerInput();
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
