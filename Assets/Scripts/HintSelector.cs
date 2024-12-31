using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HintSelector : MonoBehaviour, IPointerClickHandler
{
    public Image hintImage;
    public string imageLocalPath;

    public static Action<string> OnHintSelected;

    public void LoadImage(string path)
    {
        imageLocalPath = path;
        Sprite sprite = Resources.Load<Sprite>(imageLocalPath);
        if (sprite != null)
        {
            hintImage.preserveAspect = true;
            hintImage.sprite = sprite;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnHintSelected?.Invoke(imageLocalPath);
    }

}
