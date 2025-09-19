using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenePopupController : MonoBehaviour
{
    public GameObject popupPanel; // Assign in Inspector
    public RectTransform popupRectTransform;

    private LevelSelectorIconData levelSelectorIconData;

    private bool shopMode;

    public void ShowPopup(Vector3 position)
    {
        popupPanel.SetActive(true);
        popupRectTransform.position = position;
        popupRectTransform.localScale = Vector3.zero;
        //popupRectTransform.LeanScale(Vector3.one, 0.3f).setEaseOutBack(); // If using LeanTween
    }

    public void ShowPopupAtIcon(GameObject icon)
    {
        levelSelectorIconData = icon.GetComponent<LevelSelectorIconData>();

        RectTransform iconRect = icon.GetComponent<RectTransform>();
        RectTransform popupRect = popupPanel.GetComponent<RectTransform>();

        if (iconRect != null && popupRect != null)
        {
            // Convert icon position to world position and then to local position
            Vector3 worldPos = iconRect.transform.position;

            // Set popup position to match icon world position
            popupRect.position = worldPos + new Vector3(0, 100f, 0); // Adjust Y offset upwards

            popupPanel.SetActive(true);
        }
    }




    public void HidePopup()
    {
        popupPanel.SetActive(false);
    }

    public void OnStartGame()
    {
        shopMode = false;
        HidePopup();
        //SceneManager.LoadScene(levelSelectorIconData.sceneType.ToString());
        if (levelSelectorIconData.sceneType == SceneType.L1_Classic_City_Scene)
        {
            SceneTransitionService.Instance.Load(levelSelectorIconData.sceneType.ToString(), new StartGameParams
            {
                startInShopMode = false
            });
        } else
        {
            //Locked for now
        }
    }

    public void OnShop()
    {
        shopMode=true;
        HidePopup();
        //SceneManager.LoadScene(levelSelectorIconData.sceneType.ToString());

        if (levelSelectorIconData.sceneType == SceneType.L1_Classic_City_Scene)
        {
            SceneTransitionService.Instance.Load(levelSelectorIconData.sceneType.ToString(), new StartGameParams
            {
                startInShopMode = true
            });
        } else
        {
            //locked
        }

    }

    public void OnObjectives()
    {
        Debug.Log("Objectives clicked");
        // Show objectives UI
    }
}
