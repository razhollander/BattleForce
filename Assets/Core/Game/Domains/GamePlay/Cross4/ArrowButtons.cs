using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArrowButtons : MonoBehaviour
{
    [SerializeField] public UILongPress LeftButton;
    [SerializeField] public UILongPress RightButton;
    [SerializeField] public UILongPress DownButton;
    [SerializeField] public UILongPress UpButton;
    [SerializeField] public List<Image> _keysImages;
    
    public void SetColor(Color playerColor)
    {
        foreach (var keysImage in _keysImages)
        {
            keysImage.color = playerColor;
        }
    }
}
