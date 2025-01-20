using System.Collections;
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

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        PuzzleBlock.OnPuzzleBlockSelected += SelectBlock;
        KeyboardButton.OnCharacterTyped += KeyBoardTyped;
        PhysicalKeyboard.LetterTyped += KeyBoardTyped;
        PhysicalKeyboard.BackspaceTyped += BackspaceClicked;

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
            if (currentBlockSelected.isLetterfilled)
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
                if (!item.isLetterfilled)
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
            Debug.Log("CURRENT HIGHLIGHT WORD " + currentBlockSelected.CurrentHightWord());
            List<PuzzleBlock> otherPuzzleBlocks = new List<PuzzleBlock>();
            allHighlightedPuzzleBlocks = otherPuzzleBlocks;
            var puzzleBlocks = PuzzleLoader.Instance.GetPuzzleBlocksLinkedForWord(currentBlockSelected.CurrentHightWord());
            puzzleBlocks = puzzleBlocks.Skip(1).ToList();

            for (int i = 0; i < puzzleBlocks.Count; i++)
            {
                var pb = puzzleBlocks[i];
                if (!pb.isLetterfilled && !pb.isHint)
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
            if (currentBlockSelected.isLetterfilled && otherPuzzleBlocks.Count > 0)
            {
                otherPuzzleBlocks[0].SelectThisWithWord(currentHighlightWord);
            }
        }
    }

    public void KeyBoardTyped(char letter)
    {
        if (avoidTouch) return;

        if (currentBlockSelected != null && !currentBlockSelected.isLetterfilledCorrectly)
        {
            currentBlockSelected.OnLetterTyped(letter);
            var nextBlock = allHighlightedPuzzleBlocks[(currentBlockIndex + 1) % allHighlightedPuzzleBlocks.Count];
            if (!nextBlock.isLetterfilled)
            {
                allHighlightedPuzzleBlocks[(currentBlockIndex + 1) % allHighlightedPuzzleBlocks.Count].SelectThisWithWord(currentHighlightWord);
            }
            else
            {
                var currentWord = currentBlockSelected.CurrentHightWord();
                ValidateBlocksForWord(currentWord);
            }
        }
    }

    void HighlightNextWord()
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

    public void BackspaceClicked()
    {
        var currentWord = currentBlockSelected.CurrentHightWord();
        var puzzleBlocks = PuzzleLoader.Instance.GetPuzzleBlocksLinkedForWord(currentWord);
        puzzleBlocks = puzzleBlocks.Skip(1).ToList();
        int currentIndex = puzzleBlocks.IndexOf(currentBlockSelected);
        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex = puzzleBlocks.Count - 1;
        }
        puzzleBlocks[currentIndex].ClearText();
        puzzleBlocks[currentIndex].SelectThisWithWord(currentWord);
    }

    void ValidateBlocksForWord(string word)
    {
        var puzzleBlocks = PuzzleLoader.Instance.GetPuzzleBlocksLinkedForWord(word);
        puzzleBlocks = puzzleBlocks.Skip(1).ToList();
        bool blocksFinished = true;
        foreach (var block in puzzleBlocks)
        {
            if (!block.IsItCorrectLetter())
            {
                blocksFinished = false;
                break;
            }
        }
        if (!blocksFinished)
        {
            // Wrong
            StartCoroutine(WrongCor(puzzleBlocks, word));

        }
        else
        {
            // Correct
            HighlightNextWord();
        }
    }

    IEnumerator WrongCor(List<PuzzleBlock> puzzleBlocks, string word)
    {
        avoidTouch = true;
        areWrongAnimationsDone = false;
        for (int i = 0; i < puzzleBlocks.Count; i++)
        {
            if (!puzzleBlocks[i].isLetterfilledCorrectly)
            {
                puzzleBlocks[i].ValidateBlock();
            }
        }
        yield return new WaitUntil(GetStatusForWrongAnimationsDone);
        avoidTouch = false;
        puzzleBlocks[0].SelectThisWithWord(word);
    }

    bool areWrongAnimationsDone = false;
    public void WrongAnimationsDone()
    {
        areWrongAnimationsDone = true;
    }

    bool GetStatusForWrongAnimationsDone()
    {
        return areWrongAnimationsDone;
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
