using UnityEngine;

public class ToasterCounterSound : MonoBehaviour
{
    [SerializeField] private ToasterCounter toasterCounter;
    
    private AudioSource audioSource;
    private float warningSoundTimer;
    private bool playWarningSound;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start() {
        toasterCounter.OnStateChanged += ToasterCounter_OnStateChanged;
        toasterCounter.OnProgressChanged += ToasterCounter_OnProgressChanged;
    }

    private void ToasterCounter_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e) {
        //float burnShowProgressAmount = .5f;
        //playWarningSound = toasterCounter.IsPoped() && e.progressNormalized >= burnShowProgressAmount;

    }

    private void ToasterCounter_OnStateChanged(object sender, ToasterCounter.OnStateChangedEventArgs e) {
        bool playSound = e.state == ToasterCounter.State.Toasting;
        if (playSound) {
            audioSource.Play();
        } else {
            audioSource.Pause();
        }
    }

    private void Update() {
        //if (playWarningSound)
        //{
        //    warningSoundTimer -= Time.deltaTime;
        //    if (warningSoundTimer <= 0)
        //    {
        //        float warningSoundTimerMax = .2f;
        //        warningSoundTimer = warningSoundTimerMax;
        //        SoundManager.Instance.PlayWarningSound(toasterCounter.transform.position);
        //    }
        //}
    }
}
