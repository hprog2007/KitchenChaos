using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScreenActionButtons : MonoBehaviour,IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Image interactPressedImage;
    [SerializeField] private Image altInteractPressedImage;
    
    
    public void InteractDown()
    {
        interactPressedImage.gameObject.SetActive(true);
        Debug.Log("InteractDown");
    }

    public void InteractUp()
    {
        interactPressedImage.gameObject.SetActive(false);
        Debug.Log("InteractUp");
    }
    public void AltInteractDown()
    {
        altInteractPressedImage.gameObject.SetActive(true);
    }

    public void AltInteractUp()
    {
        altInteractPressedImage.gameObject.SetActive(false);
    }




    private void GameInput_ClickDownEvent(Vector2 clickPosition)
    {
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }
}
