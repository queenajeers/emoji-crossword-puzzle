using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public enum PuzzleCreatorBrush
{
    None,
    BoxCreation,
    AddLetter,
    MaskLetter,
    AddImage,
    Clear,
    Arrow

}

public enum PuzzleDifficulty
{
    Easy,
    Hard,
    Expert
}

public class PuzzleCreator : MonoBehaviour
{
    public Vector2Int defaultGridSize;
    public string puzzleName;
    public PuzzleDifficulty puzzleDifficulty;

    [Space(20)]
    public GameObject puzzleCreateTouchBlock;

    public EmojiCrossWord emojiCrossWord = new EmojiCrossWord();

    public List<ToolBarButton> toolBarButtons;
    public PuzzleCreatorBrush currentSelectedBrush;
    private Dictionary<Vector2Int, TouchBlock> touchBlocks = new Dictionary<Vector2Int, TouchBlock>();
    private TouchBlock currentSelectedTouchBlock;

    public Transform touchBlocksParent;

    private void Start()
    {
        emojiCrossWord = emojiCrossWord.LoadFromSavedPath(GetSavePath());
        if (emojiCrossWord == null)
        {
            emojiCrossWord = new EmojiCrossWord();
            emojiCrossWord.gridSize = defaultGridSize;
        }
        else
        {
            defaultGridSize = emojiCrossWord.gridSize;
        }

        StartCoroutine(CreateTouchBase());
        SelectBrush(PuzzleCreatorBrush.AddLetter);

        ToolBarButton.OnBrushSelected += SelectBrush;
        TouchBlock.OnTouchBlockClicked += TouchBlockClicked;
        HintSelector.OnHintSelected += OnHintSelected;
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

            GridLayer.Instance.CreateBaseGrid(defaultGridSize.x, defaultGridSize.y);
            yield return new WaitForSeconds(.4f);


            GetComponent<RectTransform>().sizeDelta = GridLayer.Instance.GetComponent<RectTransform>().sizeDelta;
            GetComponent<RectTransform>().offsetMax = GridLayer.Instance.GetComponent<RectTransform>().offsetMax;

            for (int row = 0; row < defaultGridSize.x; row++)
            {
                for (int col = 0; col < defaultGridSize.y; col++)
                {
                    var gridTouchBlock = Instantiate(puzzleCreateTouchBlock, touchBlocksParent);
                    var gridTouchRect = gridTouchBlock.GetComponent<RectTransform>();
                    gridTouchBlock.GetComponent<TouchBlock>().SetLocation(new Vector2Int(row, col));
                    gridTouchRect.localPosition = GridLayer.Instance.GetPositionOfAGridBlock(row, col, transform);
                    gridTouchRect.sizeDelta = Vector2.one * GridLayer.Instance.GetCellSize();

                    touchBlocks[new Vector2Int(row, col)] = gridTouchBlock.GetComponent<TouchBlock>();
                }
            }
            yield return new WaitForSeconds(.4f);
            LoadPreSavedBlocksData();
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

        if (currentSelectedBrush == PuzzleCreatorBrush.Arrow)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Vector2Int currentSelectedBox = currentSelectedTouchBlock.blockLocation;
                Vector2Int leftBoxLocation = currentSelectedBox - new Vector2Int(0, 1);
                if (touchBlocks.ContainsKey(leftBoxLocation))
                {
                    var db = emojiCrossWord.dataBlocks.Find(db => db.blockLocation == leftBoxLocation);
                    if (db != null)
                    {
                        db.AddDirection(ArrowIndication.FromRight);
                        touchBlocks[leftBoxLocation].SetHintArrowIndication(db.arrowIndications);
                    }
                }
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                Vector2Int currentSelectedBox = currentSelectedTouchBlock.blockLocation;
                Vector2Int rightBoxLocation = currentSelectedBox + new Vector2Int(0, 1);
                if (touchBlocks.ContainsKey(rightBoxLocation))
                {
                    var db = emojiCrossWord.dataBlocks.Find(db => db.blockLocation == rightBoxLocation);
                    if (db != null)
                    {
                        db.AddDirection(ArrowIndication.FromLeft);
                        touchBlocks[rightBoxLocation].SetHintArrowIndication(db.arrowIndications);
                    }
                }
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                Vector2Int currentSelectedBox = currentSelectedTouchBlock.blockLocation;
                Vector2Int topBoxLocation = currentSelectedBox - new Vector2Int(1, 0);
                if (touchBlocks.ContainsKey(topBoxLocation))
                {
                    var db = emojiCrossWord.dataBlocks.Find(db => db.blockLocation == topBoxLocation);
                    if (db != null)
                    {
                        db.AddDirection(ArrowIndication.FromBottom);
                        touchBlocks[topBoxLocation].SetHintArrowIndication(db.arrowIndications);
                    }
                }
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                Vector2Int currentSelectedBox = currentSelectedTouchBlock.blockLocation;
                Vector2Int bottomBoxLocation = currentSelectedBox + new Vector2Int(1, 0);
                if (touchBlocks.ContainsKey(bottomBoxLocation))
                {
                    var db = emojiCrossWord.dataBlocks.Find(db => db.blockLocation == bottomBoxLocation);
                    if (db != null)
                    {
                        db.AddDirection(ArrowIndication.FromTop);
                        touchBlocks[bottomBoxLocation].SetHintArrowIndication(db.arrowIndications);
                    }
                }
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
        DataBlock target = null;
        switch (currentSelectedBrush)
        {

            case PuzzleCreatorBrush.Clear:

                target = emojiCrossWord.dataBlocks.Find(db => db.blockLocation == currentSelectedTouchBlock.blockLocation);
                if (target != null)
                {
                    emojiCrossWord.dataBlocks.Remove(target);
                    currentSelectedTouchBlock.ClearBox();
                }


                break;
            case PuzzleCreatorBrush.MaskLetter:

                target = emojiCrossWord.dataBlocks.Find(db => db.blockLocation == currentSelectedTouchBlock.blockLocation);
                if (target != null && (target is LetterBox letterBox))
                {
                    letterBox.isClue = !letterBox.isClue;
                    if (letterBox.isClue)
                    {
                        currentSelectedTouchBlock.MakeAsClueLetter();
                    }
                    else
                    {
                        currentSelectedTouchBlock.MakeAsNormalLetter();
                    }
                }

                break;

            case PuzzleCreatorBrush.AddImage:

                HintsLoader.Instance.OpenHintsLoader();

                break;
        }

    }



    void CharacterClicked(char c)
    {
        if (currentSelectedBrush == PuzzleCreatorBrush.AddLetter)
        {
            if (currentSelectedTouchBlock != null)
            {

                var dataBlock = emojiCrossWord.dataBlocks.Find(db => db.blockLocation == currentSelectedTouchBlock.blockLocation);

                if (dataBlock == null)
                {
                    currentSelectedTouchBlock.SetLetter(c);

                    dataBlock = new LetterBox(c)
                    {
                        blockLocation = currentSelectedTouchBlock.blockLocation
                    };
                    emojiCrossWord.dataBlocks.Add(dataBlock);
                }
                else
                {
                    currentSelectedTouchBlock.SetLetter(c);
                    emojiCrossWord.dataBlocks.Remove(dataBlock);
                    var letterBox = new LetterBox(c)
                    {
                        blockLocation = currentSelectedTouchBlock.blockLocation
                    };
                    emojiCrossWord.dataBlocks.Add(letterBox);
                }

            }

        }
    }

    public void OnHintSelected(string imageLocalPath)
    {
        if (currentSelectedTouchBlock != null)
        {
            currentSelectedTouchBlock.SetImage(imageLocalPath);

            var dataBlock = emojiCrossWord.dataBlocks.Find(db => db.blockLocation == currentSelectedTouchBlock.blockLocation);
            if (dataBlock == null)
            {
                var hint = new Hint()
                {
                    blockLocation = currentSelectedTouchBlock.blockLocation,
                    localPath = imageLocalPath
                };
                hint.localPath = imageLocalPath;
                emojiCrossWord.dataBlocks.Add(hint);
            }
            else
            {
                emojiCrossWord.dataBlocks.Remove(dataBlock);
                var hint = new Hint()
                {
                    blockLocation = currentSelectedTouchBlock.blockLocation,
                    localPath = imageLocalPath
                };
                emojiCrossWord.dataBlocks.Add(hint);
            }

            HintsLoader.Instance.CloseHintsLoader();
        }
    }

    public void SavePuzzle()
    {
        string savePath = GetSavePath();
        emojiCrossWord.puzzleName = puzzleName;
        emojiCrossWord.SaveCrossWordPuzzle(savePath);
    }

    private void LoadPreSavedBlocksData()
    {
        foreach (var block in emojiCrossWord.dataBlocks)
        {
            if (block is LetterBox letterBox)
            {
                var blockLocation = letterBox.blockLocation;
                touchBlocks[blockLocation].SetLetter(letterBox.letter);
                if (letterBox.isClue)
                {
                    touchBlocks[blockLocation].MakeAsClueLetter();
                }
                else
                {
                    touchBlocks[blockLocation].MakeAsNormalLetter();
                }
            }
            else if (block is Hint hintBox)
            {
                var blockLocation = hintBox.blockLocation;
                touchBlocks[blockLocation].SetImage(hintBox.localPath);
            }

            touchBlocks[block.blockLocation].SetHintArrowIndication(block.arrowIndications);

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

    public void CenterGrid()
    {
        EmojiCrossWordUtility.CenterDataBlocks(emojiCrossWord);
        foreach (var block in touchBlocks.Values)
        {
            block.ClearBox();
        }

        SavePuzzle();
        LoadPreSavedBlocksData();

    }



}
