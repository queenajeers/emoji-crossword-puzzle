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
    Outline outline;
    [SerializeField] Color selectColor;
    Color bgDefaultColor;
    Color outlineDefault;

    void Awake()
    {
        BG = GetComponent<Image>();
        outline = GetComponent<Outline>();
        outlineDefault = outline.effectColor;
        bgDefaultColor = BG.color;
    }
    private void Start()
    {
        ClearBox();
    }
    public void SetLocation(Vector2Int blockLocation)
    {
        this.blockLocation = blockLocation;
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

    public void MakeABox(float borderWidth)
    {
        outline.effectDistance = new Vector2(borderWidth, -borderWidth);
        outline.effectColor = Color.black;
    }
    public void ClearBox()
    {
        outline.effectDistance = new Vector2(0, 0);
        outline.effectColor = outlineDefault;
        attatchedLetter.text = ' '.ToString();
    }

}
