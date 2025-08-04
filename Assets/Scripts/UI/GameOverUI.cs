using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI recipesDeliveredText;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        KitchenGameManager.Instance.OnStateChanged += KitchenGameManager_OnStateChanged;

        Hide();
    }

    private void KitchenGameManager_OnStateChanged(object sender, System.EventArgs e) {
        if (KitchenGameManager.Instance.IsGameOver()) {
            Show();

            recipesDeliveredText.text = DeliveryManager.instance.GetSuccessfulRecipesAmount().ToString();
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
