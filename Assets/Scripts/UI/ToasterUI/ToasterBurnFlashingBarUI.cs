using UnityEngine;

public class ToasterBurnFlashingBarUI : MonoBehaviour
{
    private const string IS_FLASHING = "IsFlashing";

    [SerializeField] private ToasterCounter toasterCounter;

    private Animator animator;

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    private void Start() {
        toasterCounter.OnProgressChanged += ToasterCounter_OnProgressChanged;

        animator.SetBool(IS_FLASHING, false);
    }

    private void ToasterCounter_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e) {
        float burnShowProgressAmount = .5f;
        bool show = toasterCounter.IsPoped() && e.progressNormalized >= burnShowProgressAmount;

        animator.SetBool(IS_FLASHING, show);

    }

    
}
