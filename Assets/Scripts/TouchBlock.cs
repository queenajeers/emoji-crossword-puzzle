using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TouchBlock : MonoBehaviour
{
    public Vector2Int blockLocation;

    public TextMeshProUGUI attatchedLetter;

    public static Action<TouchBlock> OnTouchBlockClicked;

    Image BG;
    [SerializeField] Color selectColor;
    Color bgDefaultColor;

    void Awake()
    {
        BG = GetComponent<Image>();
        bgDefaultColor = BG.color;
    }
    public void LoadData(Vector2Int blockLocation, char attatchedLetter)
    {
        this.blockLocation = blockLocation;
        this.attatchedLetter.text = attatchedLetter.ToString();
    }

    public void SetLetter(char letter)
    {
        attatchedLetter.text = letter.ToString();
    }

    public void BlockClicked()
    {
        OnTouchBlockClicked?.Invoke(this);
    }

    public void SelectBlock()
    {
        BG.color = selectColor;
    }
    public void DeSelectBlock()
    {
        BG.color = bgDefaultColor;
    }

}
