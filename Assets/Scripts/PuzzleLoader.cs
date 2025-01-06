using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleLoader : MonoBehaviour
{
    public string puzzleName;
    public PuzzleDifficulty puzzleDifficulty;
    private EmojiCrossWord emojiCrossWord = new EmojiCrossWord();
    public GameObject puzzleBlockPrefab;
    public Transform puzzleParent;

    public float gridSizeMultiplier;
    public float borderSize;
    public Color borderColor;

    public Transform draggableLettersParent;
    Dictionary<Vector2Int, Vector2> draggablesSnapPositions = new Dictionary<Vector2Int, Vector2>();
    Dictionary<Vector2Int, PuzzleBlock> emptyPuzzleBlocks = new Dictionary<Vector2Int, PuzzleBlock>();

    public DraggableLetter currentDraggingLetter;
    private PuzzleBlock currentNearestPuzzleBlock;
    public float thresholdForNearestPuzzleBlock;

    public int maxDraggableLetters;



    private void Start()
    {
        emojiCrossWord = emojiCrossWord.LoadFromSavedPath(GetSavePath());

        if (emojiCrossWord != null)
        {
            StartCoroutine(LoadPuzzle());
        }
        else
        {
            Debug.LogWarning($"No puzzle with name=> {puzzleName} in difficulty level=> {puzzleDifficulty} found!");
        }

        DraggableLetter.OnLetterDraggingStarted += OnLetterDragableDragStarted;
        DraggableLetter.OnLetterDraggingEnded += OnLetterDragableDragEnded;

    }

    void Update()
    {
        if (currentDraggingLetter != null)
        {
            HighlightNearestPuzzleBlock();
        }
    }

    IEnumerator LoadPuzzle()
    {
        if (GridLayer.Instance != null)
        {
            Vector2Int gridSize = emojiCrossWord.gridSize;
            GridLayer.Instance.SetCellBorderSize(borderSize);
            GridLayer.Instance.CreateBaseGrid(gridSize.x, gridSize.y);
            GridLayer.Instance.gridSizeMultiplier = gridSizeMultiplier;
            DraggableLettersContainer.Instance.InitialiseSlots(maxDraggableLetters);

            yield return new WaitForSeconds(.4f);

            DraggableLettersContainer.Instance.LoadLetters(new char[] { 'A', 'B', 'C' });

            GetComponent<Outline>().effectDistance = new Vector2(borderSize * 2f, -borderSize * 2f);
            GetComponent<Outline>().effectColor = borderColor;

            GetComponent<RectTransform>().sizeDelta = GridLayer.Instance.GetComponent<RectTransform>().sizeDelta;
            GetComponent<RectTransform>().offsetMax = GridLayer.Instance.GetComponent<RectTransform>().offsetMax;
            GetComponent<RectTransform>().offsetMin = GridLayer.Instance.GetComponent<RectTransform>().offsetMin;

            foreach (var dataBlock in emojiCrossWord.dataBlocks)
            {
                Vector2Int blockLocation = dataBlock.blockLocation;

                var puzzleBlock = Instantiate(puzzleBlockPrefab, puzzleParent);

                var puzzleBlockRect = puzzleBlock.GetComponent<RectTransform>();
                puzzleBlockRect.localPosition = GridLayer.Instance.GetPositionOfAGridBlock(blockLocation.x, blockLocation.y, transform);
                puzzleBlockRect.sizeDelta = Vector2.one * GridLayer.Instance.GetCellSize();

                var puzzleBlockComp = puzzleBlock.GetComponent<PuzzleBlock>();
                puzzleBlockComp.SetOutline(borderColor, borderSize);
                puzzleBlockComp.SetHintArrowIndication(dataBlock.arrowIndications);

                if (dataBlock is LetterBox letterBox)
                {
                    if (!letterBox.isClue)
                    {
                        // Not Empty Boxs
                        puzzleBlockComp.LoadAsNormalText(letterBox.letter);
                    }
                    else
                    {
                        // Empty Box
                        draggablesSnapPositions[blockLocation] = GridLayer.Instance.GetPositionOfAGridBlock(blockLocation.x, blockLocation.y, draggableLettersParent);
                        emptyPuzzleBlocks[blockLocation] = puzzleBlockComp;
                    }
                }
                else if (dataBlock is Hint hintBox)
                {
                    Sprite sprite = Resources.Load<Sprite>(hintBox.localPath);
                    puzzleBlockComp.LoadAsHintBlock(sprite);
                }

            }

            draggableLettersParent.SetAsLastSibling();
            yield return null;
        }
    }

    public string GetSavePath()
    {
        string difficultyFolder = "";
        switch (puzzleDifficulty)
        {
            case PuzzleDifficulty.Easy:
                difficultyFolder = "Easy";
                break;
            case PuzzleDifficulty.Hard:
                difficultyFolder = "Hard";
                break;
            case PuzzleDifficulty.Expert:
                difficultyFolder = "Expert";
                break;
        }
        string savePath = Path.Combine(Application.dataPath, "Resources", "Levels", difficultyFolder, $"{puzzleName}.json");
        return savePath;
    }

    public void HighlightNearestPuzzleBlock()
    {
        float minDist = Mathf.Infinity;
        Vector2Int targetGridLocation = new Vector2Int(-1, -1);
        foreach (var blockLocation in draggablesSnapPositions.Keys)
        {
            float dist = Vector2.Distance(draggablesSnapPositions[blockLocation], currentDraggingLetter.CurrentLocalPosition());
            if (dist < minDist && dist < thresholdForNearestPuzzleBlock)
            {
                minDist = dist;
                targetGridLocation = blockLocation;
            }
        }

        if (currentNearestPuzzleBlock != null)
        {
            currentNearestPuzzleBlock.NormaliseBG();
            currentNearestPuzzleBlock = null;
        }

        if (emptyPuzzleBlocks.ContainsKey(targetGridLocation))
        {
            currentNearestPuzzleBlock = emptyPuzzleBlocks[targetGridLocation];

            currentNearestPuzzleBlock.HighlightBG();
            currentDraggingLetter.SetReturnPosition(draggablesSnapPositions[targetGridLocation]);
        }

    }

    private void OnLetterDragableDragStarted(DraggableLetter draggableLetter)
    {
        currentDraggingLetter = draggableLetter;
    }
    private void OnLetterDragableDragEnded(DraggableLetter draggableLetter)
    {
        if (currentNearestPuzzleBlock != null)
        {
            currentNearestPuzzleBlock.NormaliseBG();
            currentNearestPuzzleBlock = null;
        }
        else
        {
            currentDraggingLetter.SetReturnPositionOriginalPosition();
            currentDraggingLetter = null;
        }
    }

}
