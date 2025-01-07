using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleBlock : MonoBehaviour
{
    public Image hintImage;
    private Image BG;
    public TextMeshProUGUI letter;
    public TextMeshProUGUI numberTextIndicator;
    public GameObject FromTop;
    public GameObject FromBottom;
    public GameObject FromLeft;
    public GameObject FromRight;

    Outline outline;

    public Color idleColor;
    public Color hintBG;
    public Color filledLetterBG;
    public Color filledLetter;

    public Color highlightColor;

    public Vector2Int blockLocation;

    void Awake()
    {
        outline = GetComponent<Outline>();
        BG = GetComponent<Image>();
    }

    public void LoadAsHintBlock(Sprite sprite)
    {
        BG.color = hintBG;
        hintImage.gameObject.SetActive(true);
        hintImage.sprite = sprite;
    }
    public void LoadAsNormalText(char letter)
    {
        BG.color = filledLetterBG;
        this.letter.gameObject.SetActive(true);
        this.letter.color = filledLetter;
        this.letter.text = letter.ToString();
    }

    public void SetAsIdleBox()
    {
        BG.color = idleColor;
    }

    public void SetOutline(Color color, float distance)
    {
        outline.effectColor = color;
        outline.effectDistance = new Vector2(distance, -distance);
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

    public void HighlightBG()
    {
        BG.color = highlightColor;
    }

    public void NormaliseBG()
    {
        BG.color = Color.white;
    }

    public void SetWordIndex(int index)
    {
        numberTextIndicator.gameObject.SetActive(true);
        numberTextIndicator.text = $"{index + 1}.";
    }


}
