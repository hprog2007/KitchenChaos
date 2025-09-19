using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    public static AnimationManager Instance;    

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    //Pointer that is above counters in shop buy mode
    public void PlayCounterPointer(Transform counterPointer, int x , int z)
    {
        var arrowTransform = counterPointer;


        arrowTransform.localPosition = new Vector3(-4.2f + x, 4.67f, -6.2f + z );

        arrowTransform.DOLocalMoveY(5.5f, .5f, false)
            .SetEase(Ease.InOutFlash)
            .SetLoops(-1, LoopType.Yoyo)
            .From(arrowTransform.localPosition)
            .SetRelative(true);
    }

    //Magic replace used to replace a counter bought with the old one
    public IEnumerator PlayMagicReplaceByRotation(Transform oldTransform, Transform newTransform)
    {
        // Scale down
        oldTransform.DOScale(0f, 2f);

        // Rotate 180° around Y over 1s
        oldTransform.DOPunchRotation(new Vector3(0, 360, 0), 2f);

        yield return new WaitForSeconds(1f);

        // Scale up 
        newTransform.DOScale(1f, 2f);

        // Rotate 180° around Y over 1s
        newTransform.DOPunchRotation(new Vector3(0, -360, 0), 2f);

        yield return new WaitForSeconds(2.5f);
    }

    //used for map icons
    public void PlayFloatingUpDown(List<Transform> transformListParam)
    {
        foreach (Transform t in transformListParam)
        {
            var rt = t.GetComponent<RectTransform>();
            rt.DOLocalMove(rt.anchoredPosition + Vector2.up * Mathf.Sin(Time.time * 2f) * 10f, 2f);
            //yield return new WaitForSeconds(1f);
            rt.DOLocalMove(rt.anchoredPosition + Vector2.up * Mathf.Sin(Time.time * 2f) * 10f, 2f);
        }
        
    }

    public void PlayScaleY(Transform floatingPanel, float endValue, float duration = 3f, Action onCompleteAction = null)
    {
        floatingPanel.DOScaleY(endValue, duration)            
            .onComplete = () => { onCompleteAction?.Invoke(); };
    }
}
