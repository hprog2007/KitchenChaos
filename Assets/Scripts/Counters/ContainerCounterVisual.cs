using UnityEngine;

public class ContainerCounterVisual : MonoBehaviour
{
    private const string Open_Close = "OpenClose";

    [SerializeField] private ContainerCounter containerCounter;    
    
    private Animator animator;

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    private void Start() {
        containerCounter.OnPlayerGrabbedObject += ContainerCounter_OnPlayerGrabbedObject;    
    }

    private void ContainerCounter_OnPlayerGrabbedObject(object sender, System.EventArgs e) {
        animator.SetTrigger(Open_Close);
    }
}
