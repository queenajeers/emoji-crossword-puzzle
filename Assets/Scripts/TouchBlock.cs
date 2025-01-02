using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TouchBlock : MonoBehaviour
{
    public Vector2Int blockLocation;

    public TextMeshProUGUI attatchedLetter;
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
        hintImage.gameObject.SetActive(false);
        MakeAsNormalLetter();
    }

    private void ActivateImage()
    {
        hintImage.gameObject.SetActive(true);

    }

}
