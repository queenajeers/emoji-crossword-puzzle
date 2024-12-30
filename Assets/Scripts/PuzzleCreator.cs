using System.Collections;
using System.Collections.Generic;
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

public class PuzzleCreator : MonoBehaviour
{
    public GameObject puzzleCreateTouchBlock;

    public EmojiCrossWord emojiCrossWord;
    public string levelName;
    public List<ToolBarButton> toolBarButtons;
    public PuzzleCreatorBrush currentSelectedBrush;
    public TouchBlock currentSelectedTouchBlock;

    private void Start()
    {
        StartCoroutine(CreateTouchBase());
        SelectBrush(PuzzleCreatorBrush.None);


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
                    var cellBorderSize = GridLayer.Instance.GetCellBorderSize();
                    gridTouchBlock.GetComponent<Outline>().effectDistance = new Vector2(cellBorderSize, -cellBorderSize);
                    gridTouchRect.localPosition = GridLayer.Instance.GetPositionOfAGridBlock(row, col, transform);
                    gridTouchRect.sizeDelta = Vector2.one * GridLayer.Instance.GetCellSize();
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
    }

    public void SavePuzzle()
    {
        Debug.Log("SAVE PUZZLE");
    }

}
