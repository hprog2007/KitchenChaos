using UnityEngine;

public class ToasterCounterVisual : MonoBehaviour
{
    [SerializeField] private ToasterCounter toasterCounter;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start() {
        toasterCounter.OnStateChanged += ToasterCounter_OnStateChanged;
    }

    private void ToasterCounter_OnStateChanged(object sender, ToasterCounter.OnStateChangedEventArgs e) {
        bool showVisual = e.state == ToasterCounter.State.Toasting || e.state == ToasterCounter.State.Poped;
        if (e.state == ToasterCounter.State.Toasting)
        {
            animator.SetTrigger("PushDown");
        }
        else if (e.state == ToasterCounter.State.Poped)
        {
            animator.SetTrigger("PopUp");
        }
        else if (e.state == ToasterCounter.State.Idle)
        {
            Debug.Log("Anim Idle");
            animator.SetTrigger("Idle");
        }
    }
}
