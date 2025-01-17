using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class KeyboardButton : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI buttonTxt;
    public char attatchedLetter;

    public static Action<char> OnCharacterTyped;
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


}
