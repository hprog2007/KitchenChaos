using UnityEngine;

public class ToasterBurnWarningUI : MonoBehaviour
{
    [SerializeField] private ToasterCounter toasterCounter;

    private void Start() {
        toasterCounter.OnProgressChanged += ToasterCounter_OnProgressChanged;

        Hide();
    }

    private void ToasterCounter_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e) {
        float burnShowProgressAmount = .5f;
        bool show = toasterCounter.IsPoped() && e.progressNormalized >= burnShowProgressAmount;

        if (show) {
            Show();
        } else {
            Hide();
        }

    }

    private void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }
}
