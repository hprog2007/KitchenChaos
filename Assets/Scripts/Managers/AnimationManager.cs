using System.Collections;
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

    public IEnumerator PlayMagicReplaceByRotation(Transform oldTransform, Transform newTransform)
    {
        // Scale down to 0.5 over 0.5s
        oldTransform.DOScale(0f, 2f);

        // Rotate 180° around Y over 1s
        oldTransform.DOPunchRotation(new Vector3(0, 360, 0), 2f);

        yield return new WaitForSeconds(1f);

        // Scale down to 0.5 over 0.5s
        newTransform.DOScale(1f, 2f);

        // Rotate 180° around Y over 1s
        newTransform.DOPunchRotation(new Vector3(0, -360, 0), 2f);

        yield return new WaitForSeconds(2.5f);
    }
}
