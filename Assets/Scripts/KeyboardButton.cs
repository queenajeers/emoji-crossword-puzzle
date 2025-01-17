using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class KeyboardButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerExitHandler, IPointerUpHandler
{
    public TextMeshProUGUI buttonTxt;
    public char attatchedLetter;

    public static Action<char> OnCharacterTyped;

    RectTransform myRect;

    void Start()
    {
        myRect = GetComponent<RectTransform>();
    }
    public void LoadChar(char c)
    {
        attatchedLetter = char.ToUpper(c);
        buttonTxt.text = char.ToUpper(c).ToString();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Clicked " + attatchedLetter);
        OnCharacterTyped?.Invoke(attatchedLetter);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        myRect.DOKill();
        myRect.DOScale(.8f, .1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        myRect.DOKill(true);
        myRect.DOScale(1f, .1f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        myRect.DOKill(true);
        myRect.DOScale(1f, .1f);
    }
}
