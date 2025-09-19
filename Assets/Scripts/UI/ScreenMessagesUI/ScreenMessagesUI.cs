using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScreenMessagesUI : MonoBehaviour
{
    public static ScreenMessagesUI Instance { get; private set; }

    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TextMeshProUGUI textMesh;
    [SerializeField] private Button btnYes;
    [SerializeField] private Button btnNo;
    [SerializeField] private RectTransform floatingMessagePanel;
    [SerializeField] private TextMeshProUGUI floatingMessage;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        messagePanel.SetActive(false);
        floatingMessagePanel.gameObject.SetActive(false);

    }

    public void ShowMessage(string message, int duration = 3, bool HideOnClick = false, float fontSize = 40, float messagePanelHeight = 200f )
    {
        textMesh.text = message;
        textMesh.fontSizeMax = fontSize;
        var messageRect = messagePanel.GetComponent<RectTransform>();
        messageRect.sizeDelta = new Vector2(messageRect.sizeDelta.x, messagePanelHeight);
        messagePanel.SetActive(true);

        if (!HideOnClick)
        {
            StartCoroutine(DisplayMessageForDuration(duration));
        } else
        {
            BindClickToHide();
        }
    }   

    public void ShowFloatingMessage(string messageParam, Vector3 messagePositionParam, float duration)
    {
        floatingMessage.text = messageParam;
        floatingMessagePanel.anchoredPosition = messagePositionParam;
        floatingMessagePanel.localScale = new Vector3(floatingMessagePanel.localScale.x, 0, floatingMessagePanel.localScale.z);
        floatingMessagePanel.gameObject.SetActive(true);
        AnimationManager.Instance.PlayScaleY(floatingMessagePanel, 1f, duration, () => 
            {
                AnimationManager.Instance.PlayScaleY(floatingMessagePanel, 0f , duration, () => 
                    { floatingMessagePanel.gameObject.SetActive(false); });
            });
    }

    public void HideFloatingMessage()
    {
        
    }
    private void BindClickToHide()
    {
        var button = messagePanel.GetComponent<Button>();
        if (button.onClick != null)
        {
            button.onClick.RemoveAllListeners();
        }
        button.onClick.AddListener(HideMessagePanel);
    }

    private IEnumerator DisplayMessageForDuration(int duration)
    {
        yield return new WaitForSeconds(duration);

        HideMessagePanel();

    }

    public void HideMessagePanel()
    {
        transform.DOScale(0f, .5f).SetEase(Ease.OutQuad).onComplete = 
            () => {
                    messagePanel.SetActive(false);
                    transform.localScale = Vector3.one;
                  };
        
    }

    public bool ShowConfirmMessage(string message)
    {
        textMesh.text = message;
        messagePanel.SetActive(true);

        BindClickToHide();

        return true;
    }

    private void BindToYesNoButtons()
    {
        var buttonYes = btnYes.GetComponent<Button>();
        var buttonNo = btnNo.GetComponent<Button>();
        buttonYes.onClick.RemoveAllListeners();
        buttonNo.onClick.RemoveAllListeners();

        //if (button.onClick != null)
        //{
        //    button.onClick.RemoveAllListeners();
        //}
        //buttonYes.onClick.AddListener(() => { return true; });
    }
}
