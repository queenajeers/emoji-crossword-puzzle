using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TouchBlock : MonoBehaviour
{
    public Vector2Int blockLocation;

    public TextMeshProUGUI attatchedLetter;
    public TextMeshProUGUI attatchedHintText;

    public Image hintImage;

    public static Action<TouchBlock> OnTouchBlockClicked;

    Image BG;
    Outline outline;
    [SerializeField] Color selectColor;
    Color bgDefaultColor;
    Color outlineDefault;

    public GameObject FromTop;
    public GameObject FromBottom;
    public GameObject FromRight;
    public GameObject FromLeft;


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
        ClearBox();
        attatchedLetter.text = letter.ToString();
        MakeABox(GridLayer.Instance.GetCellBorderSize());
    }
    public void AddLetterToText(char letter)
    {
        attatchedLetter.text = "";
        attatchedHintText.text += letter.ToString();
        MakeABox(GridLayer.Instance.GetCellBorderSize());
    }
    public void SetLetterToText(string text)
    {
        attatchedLetter.text = "";
        attatchedHintText.text += text;
        MakeABox(GridLayer.Instance.GetCellBorderSize());
    }
    public string BackSpaceHintLetter()
    {

        attatchedLetter.text = "";
        var hintText = attatchedHintText.text;
        if (hintText.Length > 0)
        {
            hintText = hintText.Substring(0, hintText.Length - 1);
            attatchedHintText.text = hintText;
        }
        MakeABox(GridLayer.Instance.GetCellBorderSize());
        return hintText;
    }

    public void SetImage(string imageLocalPath)
    {
        ClearBox();
        ActivateImage();
        Sprite sprite = Resources.Load<Sprite>(imageLocalPath);
        hintImage.sprite = sprite;
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

    public void MakeAsClueLetter()
    {
        var c = attatchedLetter.color;
        c.a = .2f;
        attatchedLetter.color = c;
    }

    public void MakeAsNormalLetter()
    {
        var c = attatchedLetter.color;
        c.a = 1f;
        attatchedLetter.color = c;
    }

    private void MakeABox(float borderWidth)
    {
        outline.effectDistance = new Vector2(borderWidth, -borderWidth);
        outline.effectColor = Color.gray;
    }
    public void ClearBox()
    {
        outline.effectDistance = new Vector2(0, 0);
        outline.effectColor = outlineDefault;
        attatchedLetter.text = ' '.ToString();
        attatchedHintText.text = "";
        hintImage.gameObject.SetActive(false);
        MakeAsNormalLetter();
        SetHintArrowIndication(null);
    }

    private void ActivateImage()
    {
        hintImage.gameObject.SetActive(true);
    }

    public void SetHintArrowIndication(List<ArrowIndication> arrowIndications)
    {

        FromTop.SetActive(false);
        FromBottom.SetActive(false);
        FromLeft.SetActive(false);
        FromRight.SetActive(false);
        if (arrowIndications == null) return;
        foreach (ArrowIndication arrowIndication in arrowIndications)
        {
            switch (arrowIndication)
            {
                case ArrowIndication.FromTop:

                    FromTop.SetActive(true);
                    break;
                case ArrowIndication.FromBottom:

                    FromBottom.SetActive(true);
                    break;
                case ArrowIndication.FromLeft:

                    FromLeft.SetActive(true);
                    break;
                case ArrowIndication.FromRight:

                    FromRight.SetActive(true);
                    break;
            }
        }
    }

}
