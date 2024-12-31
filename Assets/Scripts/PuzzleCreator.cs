using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public enum PuzzleCreatorBrush
{
    None,
    BoxCreation,
    AddLetter,
    MaskLetter,
    AddImage,
    Clear

}

public enum PuzzleDifficulty
{
    Easy,
    Hard,
    Expert
}

public class PuzzleCreator : MonoBehaviour
{
    public GameObject puzzleCreateTouchBlock;

    public EmojiCrossWord emojiCrossWord;
    public string levelName;
    public PuzzleDifficulty puzzleDifficulty;
    public List<ToolBarButton> toolBarButtons;
    public PuzzleCreatorBrush currentSelectedBrush;
    public TouchBlock currentSelectedTouchBlock;

    private void Start()
    {
        StartCoroutine(CreateTouchBase());
        SelectBrush(PuzzleCreatorBrush.AddLetter);


        ToolBarButton.OnBrushSelected += SelectBrush;
        TouchBlock.OnTouchBlockClicked += TouchBlockClicked;
    }


    public void SelectBrush(PuzzleCreatorBrush brushType)
    {
        currentSelectedBrush = brushType;
        foreach (var button in toolBarButtons)
        {
            if (button.brushType == brushType)
            {
                button.SelectButton();
            }
            else
            {
                button.DeSelectButton();
            }
        }
    }

    IEnumerator CreateTouchBase()
    {
        if (GridLayer.Instance != null)
        {
            var gridSize = emojiCrossWord.gridSize;

            GridLayer.Instance.CreateBaseGrid(gridSize.x, gridSize.y);
            yield return new WaitForSeconds(.4f);

            GetComponent<RectTransform>().sizeDelta = GridLayer.Instance.GetComponent<RectTransform>().sizeDelta;
            GetComponent<RectTransform>().offsetMax = GridLayer.Instance.GetComponent<RectTransform>().offsetMax;

            for (int row = 0; row < gridSize.x; row++)
            {
                for (int col = 0; col < gridSize.y; col++)
                {
                    var gridTouchBlock = Instantiate(puzzleCreateTouchBlock, transform);
                    var gridTouchRect = gridTouchBlock.GetComponent<RectTransform>();
                    gridTouchBlock.GetComponent<TouchBlock>().SetLocation(new Vector2Int(row, col));
                    gridTouchRect.localPosition = GridLayer.Instance.GetPositionOfAGridBlock(row, col, transform);
                    gridTouchRect.sizeDelta = Vector2.one * GridLayer.Instance.GetCellSize();
                }
            }

        }

    }


    void Update()
    {
        foreach (char c in Input.inputString)
        {
            char upperC = char.ToUpper(c); // Convert the character to uppercase
            if (upperC >= 'A' && upperC <= 'Z') // Check if it's a capital letter
            {
                Debug.Log($"Character clicked {upperC}");
                CharacterClicked(upperC);
            }
        }
    }


    public void TouchBlockClicked(TouchBlock touchedBlock)
    {
        if (currentSelectedTouchBlock != null)
        {
            currentSelectedTouchBlock.DeSelectBlock();
        }
        touchedBlock.SelectBlock();
        currentSelectedTouchBlock = touchedBlock;
    }

    void CharacterClicked(char c)
    {
        if (currentSelectedBrush == PuzzleCreatorBrush.AddLetter)
        {
            if (currentSelectedTouchBlock != null)
            {
                currentSelectedTouchBlock.MakeABox(GridLayer.Instance.GetCellBorderSize());
                currentSelectedTouchBlock.SetLetter(c);


                var dataBlock = emojiCrossWord.dataBlocks.Find(db => db.blockLocation == currentSelectedTouchBlock.blockLocation);

                if (dataBlock == null)
                {
                    dataBlock = new LetterBox(c)
                    {
                        blockLocation = currentSelectedTouchBlock.blockLocation
                    };
                    emojiCrossWord.dataBlocks.Add(dataBlock);
                }
                else
                {
                    var letterBox = (LetterBox)dataBlock;
                    letterBox.UpdateLeter(c);
                }

            }

        }
    }

    public void SavePuzzle()
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

        string savePath = Path.Combine(Application.dataPath, "Resources", "Levels", difficultyFolder);
        Debug.Log($"Saving to path {savePath}");
        emojiCrossWord.SaveCrossWordPuzzle(savePath);

    }

}
