using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class ScreenMessagesUI : MonoBehaviour
{
    public static ScreenMessagesUI Instance { get; private set; }

    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TextMeshProUGUI textMesh; 

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

    public void ShowMessage(string message, int duration)
    {
       
        textMesh.text = message;
        messagePanel.SetActive(true);

        StartCoroutine(DisplayMessageForDuration(duration));
    }

    private IEnumerator DisplayMessageForDuration(int duration)
    {
        yield return new WaitForSeconds(duration);

        HideMessage();

    }

    public void HideMessage()
    {
        messagePanel.SetActive(false);
    }
}
