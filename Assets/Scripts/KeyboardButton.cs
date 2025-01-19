using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class KeyboardButton : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI buttonTxt;
    public char attatchedLetter;

    public static Action<char> OnCharacterTyped;

    RectTransform myRect;

    bool clicked;

    void Start()
    {
        myRect = GetComponent<RectTransform>();
    }
    public void LoadChar(char c)
    {
        attatchedLetter = char.ToUpper(c);
        buttonTxt.text = char.ToUpper(c).ToString();
    }


    // public void OnPointerDown(PointerEventData eventData)
    // {
    //     if (!clicked)
    //     {
    //         OnCharacterTyped?.Invoke(attatchedLetter);
    //         clicked = true;
    //         myRect.DOKill();
    //         myRect.DOScale(.8f, .1f);
    //     }
    // }

    // public void OnPointerExit(PointerEventData eventData)
    // {
    //     clicked = false;
    //     myRect.DOKill(true);
    //     myRect.DOScale(1f, .1f);
    // }

    // public void OnPointerUp(PointerEventData eventData)
    // {
    //     clicked = false;
    //     myRect.DOKill(true);
    //     myRect.DOScale(1f, .1f);
    // }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnCharacterTyped?.Invoke(attatchedLetter);
    }
}
