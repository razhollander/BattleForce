using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UILongPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    private bool _isNowPressed = false;
    public event Action OnPress;
    public event Action OnPointerDownEvent;
    
    public void OnPointerDown(PointerEventData eventData)
    {
        _isNowPressed = true;
        OnPointerDownEvent?.Invoke();
    }
 
    public void OnPointerUp(PointerEventData eventData)
    {
        _isNowPressed = false;
    }
 
    public void OnPointerExit(PointerEventData eventData)
    {
        _isNowPressed = false;
    }

    private void FixedUpdate()
    {
        if (_isNowPressed)
        {
            OnPress?.Invoke();
        }
    }
}