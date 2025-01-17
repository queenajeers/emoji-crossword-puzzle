using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PuzzleBlock : MonoBehaviour, IPointerClickHandler
{
    public Image hintImage;
    private Image BG;
    public char correctLetter;
    public TextMeshProUGUI letter;
    RectTransform letterRect;
    public TextMeshProUGUI numberTextIndicator;
    public GameObject FromTop;
    public GameObject FromBottom;
    public GameObject FromLeft;
    public GameObject FromRight;

    Outline outline;
    [SerializeField] Outline hintRoundedOutline;

    public Color idleColor;
    public Color hintBG;
    public Color filledLetterBG;
    public Color filledLetter;

    public Color highlightColor;

    public Vector2Int blockLocation;

    public Color correctColorBG;
    public Color correctColorText;

    public bool filledCorrectly;
    bool markedAsFinished;
    public bool isHint;



    public GameObject starEffect;

    [HideInInspector]
    public RectTransform rectTransform;


    public static Action<PuzzleBlock> OnPuzzleBlockSelected;

    public Color selectColor;
    public Color selectSecondaryColor;

    public Color normalColor;

    public Color wrongColor;

    public List<string> partOfWords = new List<string>();
    int clickIndex = -1;

    void Awake()
    {
        letterRect = letter.gameObject.GetComponent<RectTransform>();
        rectTransform = GetComponent<RectTransform>();
        outline = GetComponent<Outline>();
        BG = GetComponent<Image>();
    }

    void Start()
    {
        numberTextIndicator.fontSize = letter.fontSize / 3f;
    }

    public void LoadAsHintBlock(Sprite sprite)
    {
        isHint = true;
        BG.color = hintBG;
        BG.enabled = false;
        outline.enabled = false;
        hintRoundedOutline.gameObject.SetActive(true);
        hintImage.gameObject.SetActive(true);
        hintImage.sprite = sprite;
    }
    public void LoadAsNormalText(char letter)
    {
        filledCorrectly = true;

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
        hintRoundedOutline.effectDistance = new Vector2(distance, -distance);
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

    public void SetFilledBG()
    {
        if (markedAsFinished) return;
        BG.color = filledLetterBG;
    }

    public void SetWordIndex(int index)
    {
        numberTextIndicator.gameObject.SetActive(true);
        numberTextIndicator.text = $"{index + 1}.";
    }

    public void MarkAsCorrectBlock(int index, bool noAnimation = false)
    {
        markedAsFinished = true;
        if (!noAnimation)
        {
            BG.DOColor(correctColorBG, .2f);
            letter.DOColor(correctColorText, .2f);
            //outline.effectColor = correctColorText + (.15f * Color.white);
            transform.DOScale(.9f, .06f + (index * .03f)).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                starEffect.SetActive(true);
                transform.DOScale(1f, .9f + (index * .03f)).SetEase(Ease.OutElastic);

            });
        }
        else
        {
            BG.color = correctColorBG;
            letter.color = correctColorText;
            //outline.effectColor = correctColorText + (.15f * Color.white);

        }

    }

    bool faded = false;
    public void FadeOut(int index)
    {
        if (!faded)
        {
            faded = true;
            var cg = gameObject.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = gameObject.AddComponent<CanvasGroup>();

            cg.DOFade(0f, .3f).SetTarget(cg).SetDelay(index * .01f);
            transform.DOScale(0.6f, .29f).SetDelay(index * .01f);
        }
    }

    public void Highlight()
    {
        if (markedAsFinished) return;
        BG.color = selectColor;
    }
    public void HighlightSecondary(string word)
    {
        clickIndex = partOfWords.IndexOf(word);
        BG.color = selectSecondaryColor;
    }
    public void Dim()
    {
        if (markedAsFinished) return;
        BG.color = normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (PuzzleBlockSelector.Instance.avoidTouch) return;
        SelectThis();
    }

    public void SelectThis()
    {
        if (!isHint && partOfWords.Count > 0)
        {
            clickIndex++;
            OnPuzzleBlockSelected?.Invoke(this);
        }
    }
    public void SelectThisWithWord(string word)
    {
        if (!isHint && partOfWords.Count > 0)
        {
            clickIndex = partOfWords.IndexOf(word);
            OnPuzzleBlockSelected?.Invoke(this);
        }
    }

    public string CurrentHightWord()
    {
        return partOfWords[clickIndex % partOfWords.Count];
    }

    public bool OnLetterTyped(char letter)
    {
        return ValidateLetter(letter);
    }

    bool ValidateLetter(char letter)
    {
        this.letter.text = letter.ToString();
        if (letter == correctLetter)
        {
            LoadAsNormalText(letter);
            PuzzleLoader.Instance.RemoveClueForCrosswordblock(blockLocation);

            this.letter.transform.DOScale(1.2f, .2f).OnComplete(() =>
            {
                this.letter.transform.DOScale(1f, .14f);
            });

            return true;
        }
        else
        {

            PuzzleBlockSelector.Instance.avoidTouch = true;
            this.letter.gameObject.SetActive(true);
            Color wrongColorNoAlpha = wrongColor;
            wrongColorNoAlpha.a = 0;
            this.letter.color = new Color(wrongColor.r, wrongColor.g, wrongColor.b, 0f);
            this.letter.DOColor(wrongColor, .2f);
            letterRect.localPosition = Vector3.zero;
            letterRect.DOKill();
            letterRect
                .DOShakeAnchorPos(1f, new Vector3(10f, 0f, 0f), 10, 25, true, true)
                .OnComplete(() =>
                {
                    this.letter.DOColor(wrongColorNoAlpha, .05f).OnComplete(() =>
                         {
                             PuzzleBlockSelector.Instance.avoidTouch = false;
                             this.letter.gameObject.SetActive(false);
                         });

                });

            return false;
        }
    }


}
