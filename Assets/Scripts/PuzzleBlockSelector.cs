using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleBlockSelector : MonoBehaviour
{
    public static PuzzleBlockSelector Instance { get; private set; }
    PuzzleBlock currentBlockSelected;
    List<PuzzleBlock> allHighlightedPuzzleBlocks = new List<PuzzleBlock>();
    public RectTransform selectorRect;
    int currentBlockIndex;
    string currentHighlightWord;

    public bool avoidTouch;

    GameObject prevActiveHint;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        PuzzleBlock.OnPuzzleBlockSelected += SelectBlock;
        KeyboardButton.OnCharacterTyped += KeyBoardTyped;
    }

    void OnDestroy()
    {
        PuzzleBlock.OnPuzzleBlockSelected -= SelectBlock;
        KeyboardButton.OnCharacterTyped -= KeyBoardTyped;
    }

    public void SetSelectorDimensions(float size, Color borderColor, float borderSize)
    {
        selectorRect.sizeDelta = new Vector2(size + (2 * borderSize), size + (2 * borderSize));
        selectorRect.GetComponent<Outline>().effectColor = borderColor;

        selectorRect.GetComponent<Outline>().effectDistance = new Vector2(borderSize / 2f, -borderSize / 2f);
        selectorRect.GetComponent<Image>().color = borderColor;

    }

    public void SelectBlock(PuzzleBlock puzzleBlock)
    {
        if (currentBlockSelected != null)
        {
            if (currentBlockSelected.filledCorrectly)
            {
                currentBlockSelected.SetFilledBG();
            }
            else
            {
                currentBlockSelected.Dim();
            }



        }
        if (allHighlightedPuzzleBlocks.Count > 0)
        {
            foreach (PuzzleBlock item in allHighlightedPuzzleBlocks)
            {
                if (!item.filledCorrectly)
                {
                    item.Dim();
                }
                else
                {
                    item.SetFilledBG();
                }
            }
        }

        currentBlockSelected = puzzleBlock;
        currentBlockSelected.Highlight();
        currentHighlightWord = currentBlockSelected.CurrentHightWord();
        selectorRect.localPosition = puzzleBlock.rectTransform.localPosition;
        HighlightOtherBlocks();
    }

    void HighlightOtherBlocks()
    {
        if (currentBlockSelected != null)
        {
            Debug.Log("CURRENT HIGGLIGHT WORD " + currentBlockSelected.CurrentHightWord());
            List<PuzzleBlock> otherPuzzleBlocks = new List<PuzzleBlock>();
            allHighlightedPuzzleBlocks = otherPuzzleBlocks;
            var puzzleBlocks = PuzzleLoader.Instance.GetPuzzleBlocksLinkedForWord(currentBlockSelected.CurrentHightWord());

            if (prevActiveHint != null && (prevActiveHint != puzzleBlocks[0].gameObject))
            {
                prevActiveHint.SetActive(false);
            }

            prevActiveHint = puzzleBlocks[0].gameObject;

            prevActiveHint.SetActive(true);
            PuzzleBlocksCentrify.Instance.CenterRects();
            puzzleBlocks = puzzleBlocks.Skip(1).ToList();

            for (int i = 0; i < puzzleBlocks.Count; i++)
            {
                var pb = puzzleBlocks[i];
                if (!pb.filledCorrectly && !pb.isHint)
                {
                    pb.HighlightSecondary(currentHighlightWord);
                    otherPuzzleBlocks.Add(pb);
                }
            }
            for (int i = 0; i < otherPuzzleBlocks.Count; i++)
            {
                if (currentBlockSelected == otherPuzzleBlocks[i])
                {
                    otherPuzzleBlocks[i].Highlight();
                    currentBlockIndex = i;
                }
            }
            if (currentBlockSelected.filledCorrectly && otherPuzzleBlocks.Count > 0)
            {
                otherPuzzleBlocks[0].SelectThisWithWord(currentHighlightWord);
            }

        }
    }

    public void KeyBoardTyped(char letter)
    {
        if (avoidTouch) return;

        if (currentBlockSelected != null)
        {
            if (currentBlockSelected.OnLetterTyped(letter))
            {
                var nextBlock = allHighlightedPuzzleBlocks[(currentBlockIndex + 1) % allHighlightedPuzzleBlocks.Count];
                if (!nextBlock.filledCorrectly)
                {
                    allHighlightedPuzzleBlocks[(currentBlockIndex + 1) % allHighlightedPuzzleBlocks.Count].SelectThisWithWord(currentHighlightWord);
                }
                else
                {
                    var finishedWord = currentBlockSelected.CurrentHightWord();
                    Debug.Log($"WORD {finishedWord} is FINISHED!");
                    PuzzleLoader.Instance.SetWordAsFinished(finishedWord);
                    PuzzleLoader.Instance.HighlightNextWord();
                    if (PuzzleLoader.Instance.AllWordsFinished())
                    {
                        selectorRect.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    public void GoToPosition()
    {
        if (currentBlockSelected != null)
        {
            var goToPosition = currentBlockSelected.goToPosition;
            avoidTouch = true;
            //selectorRect.DOKill();
            selectorRect.DOLocalMove(goToPosition, 0.3f).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                avoidTouch = false;
            });
        }
    }
}
