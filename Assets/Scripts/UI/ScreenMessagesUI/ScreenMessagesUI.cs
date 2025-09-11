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


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        messagePanel.SetActive(false);


    }

    public void ShowMessage(string message, int duration = 3, bool HideOnClick = false, float fontSize = 50)
    {
       
        textMesh.text = message;
        textMesh.fontSizeMax = fontSize;
        messagePanel.SetActive(true);

        if (!HideOnClick)
        {
            StartCoroutine(DisplayMessageForDuration(duration));
        } else
        {
            BindClickToHide();
        }
    }   

    private void BindClickToHide()
    {
        var button = messagePanel.GetComponent<Button>();
        if (button.onClick != null)
        {
            button.onClick.RemoveAllListeners();
        }
        button.onClick.AddListener(HideMessage);
    }

    private IEnumerator DisplayMessageForDuration(int duration)
    {
        yield return new WaitForSeconds(duration);

        HideMessage();

    }

    public void HideMessage()
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
